readonly record struct TodoO (
  long? id,
  string? task,
  long? due,
  long[]? categories,
  bool? done
);

static class Todo {

  public static IResult Create(
    HttpContext ctx, Auth auth, SqliteConnection conn, TodoO todo) {
    if(String.IsNullOrEmpty(todo.task?.Trim())) {
      return Results.BadRequest(new {error = "need a task"});
    }

    using var tran = conn.BeginTransaction();
    try {
      using var cmd = conn.CreateCommand();
      cmd.CommandText  =
        """
        insert into todo
          (task, done, due_unix_timestamp, userid)
            values
          (:task, :done, :due, :userid)
        """;

      cmd.Parameters.AddWithValue("task", todo.task);
      cmd.Parameters.AddWithValue("done", todo.done ?? false);
      cmd.Parameters.AddWithValue("due",
        todo.due ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds());
      cmd.Parameters.AddWithValue("userid", auth.GetCurrentUser(ctx));

      cmd.ExecuteNonQuery();

      if(todo.categories is not (null or {Length: 0})) {
        using var last_insert  = conn.CreateCommand();
        last_insert.CommandText = "select last_insert_rowid()";
        long? todoid = (long?) last_insert.ExecuteScalar();

        if(todoid is not null) {
          using var category_todo  = conn.CreateCommand();

          category_todo.CommandText =
            Enumerable.Range(0, todo.categories.Length)
              .Aggregate(
                "insert or ignore into category_todo"
                  + " (categoryid, todoid) values ",
                (a, v) => a + $"(:categoryid_{v}, :todoid),")
              .TrimEnd(',');

          category_todo.Parameters.AddRange(
            todo.categories.Select((e, i) => new SqliteParameter {
              ParameterName = $"categoryid_{i}",
              SqliteType = SqliteType.Integer,
              Value = e
            }));

          category_todo.Parameters.Add(
            new SqliteParameter {
              ParameterName = "todoid",
              SqliteType = SqliteType.Integer,
              Value = todoid
            });

          category_todo.ExecuteNonQuery();
        }
      }

      tran.Commit();
    } catch(SqliteException ex) {
      tran.Rollback();
      return Results.BadRequest(new {error = ex.Message});
    }

    return Results.Ok();
  }

  public static IResult Update(
    HttpContext ctx, Auth auth, SqliteConnection conn, TodoO todo) {
    if(todo.id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    if(todo with {id = null} == new TodoO()) {
      return Results.BadRequest(new {error = "nothing to update"});
    }

    using var tran = conn.BeginTransaction();
    try {
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "update or ignore todo set ";

      if(todo.task is not null) {
        cmd.CommandText += " task = :task ,";
        cmd.Parameters.AddWithValue("task", todo.task);
      }

      if(todo.due is not null) {
        cmd.CommandText += " due_unix_timestamp = :due ,";
        cmd.Parameters.AddWithValue("due", todo.due);
      }

      if(todo.done is not null) {
        cmd.CommandText += " done = :done ,";
        cmd.Parameters.AddWithValue("done", todo.done);
      }

      cmd.CommandText = cmd.CommandText.TrimEnd(',');
      cmd.CommandText += " where id = :id and userid = :userid";
      cmd.Parameters.AddWithValue("id", todo.id);
      cmd.Parameters.AddWithValue("userid", auth.GetCurrentUser(ctx));

      if(cmd.ExecuteNonQuery() == 0) {
        return Results.BadRequest(new {error = "cannot update"});
      }

      if(todo.categories is not null) {
        using var del = conn.CreateCommand();
        del.CommandText = "delete from category_todo where todoid = :todoid";
        del.Parameters.AddWithValue("todoid", todo.id);
        del.ExecuteNonQuery();

        if(todo.categories.Length != 0) {
          using var category_todo  = conn.CreateCommand();

          category_todo.CommandText =
            Enumerable.Range(0, todo.categories.Length)
              .Aggregate(
                "insert or ignore into category_todo"
                  + " (categoryid, todoid) values ",
                (a, v) => a + $"(:categoryid_{v}, :todoid),")
              .TrimEnd(',');

          category_todo.Parameters.AddRange(
            todo.categories.Select((e, i) => new SqliteParameter {
              ParameterName = $"categoryid_{i}",
              SqliteType = SqliteType.Integer,
              Value = e
            }));

          category_todo.Parameters.Add(
            new SqliteParameter {
              ParameterName = "todoid",
              SqliteType = SqliteType.Integer,
              Value = todo.id
            });

          category_todo.ExecuteNonQuery();
        }
      }

      tran.Commit();
    } catch(SqliteException ex) {
      tran.Rollback();
      return Results.BadRequest(new {error = ex.Message});
    }

    return Results.Ok("update");
  }

  public static IResult List(
    HttpContext ctx, Auth auth, SqliteConnection conn, JsonElement? o) {
    long? cursor_init = o?._long("cursor_init");
    long? cursor_prev = o?._long("cursor_prev");
    long? cursor_next = o?._long("cursor_next");
    long? cursor = cursor_next ?? cursor_prev;

    bool forward = cursor_prev == null;
    var (op, dir) = forward ? (">" , "asc") : ("<" , "desc");

    using var cmd = conn.CreateCommand();
    cmd.CommandText =
      """
      select
        t.id,
        t.task,
        t.done,
        t.due_unix_timestamp,
        json_group_array(
          json_object('id', c.id, 'name', c.name, 'color', c.color)
        ) as categories
      from todo t
        left join category_todo ct on t.id = ct.todoid
        left join category c on c.id = ct.categoryid
      where 1 
      """;

    if(o?._long("id") is long id) {
      cmd.CommandText += " and t.id=:id ";
      cmd.Parameters.AddWithValue("id", id);
    }

    if(o?._str("task") is string task) {
      cmd.CommandText += " and t.task like :task ";
      cmd.Parameters.AddWithValue("task", $"%{task}%");
    }

    if(o?._bool("done") is bool done) {
      cmd.CommandText += " and t.done = :done ";
      cmd.Parameters.AddWithValue("done", done);
    }

    if(o?._long("due_from") is long due_from) {
      cmd.CommandText += " and t.due_unix_timestamp >= :due_from ";
      cmd.Parameters.AddWithValue("due_from", due_from);
    }

    if(o?._long("due_to") is long due_to) {
      cmd.CommandText += " and t.due_unix_timestamp <= :due_to ";
      cmd.Parameters.AddWithValue("due_to", due_to);
    }

    if(o?._arr("categories")?
      .Select(e => (long?) (
        (e.ValueKind is JsonValueKind.Number
          && e.TryGetInt64(out var i)) ? i : null))
      .Where(e => e != null) is var arr
      && arr is not null 
      && String.Join(',', arr) is var categories
      && categories.Length != 0
    ) {
      cmd.CommandText +=
        $" and ct.categoryid in ({String.Join(',', categories)}) ";
    }

    if(cursor != null) {
      cmd.CommandText += $" and t.id {op} :cursor ";
      cmd.Parameters.AddWithValue("cursor", cursor);
    }

    cmd.CommandText +=
      $"and t.userid = :userid group by t.id order by t.id {dir} limit 10";
    cmd.Parameters.AddWithValue("userid", auth.GetCurrentUser(ctx));

    var data = cmd.ExecuteReader().ToDictArray(!forward);

    cursor_prev = cursor_next = null;
    if(data.Length != 0) {
      cursor_prev = (long) data[0]["id"];

      if(cursor == null) {
        cursor_init = cursor_prev;
      }

      if(cursor_init == cursor_prev) {
        cursor_prev = null;
      }

      if(data.Length == 10) {
        cursor_next = (long) data[^1]["id"];
      }
    }

    return Results.Ok(new {
      data,
      cursor_init,
      cursor_prev,
      cursor_next,
    });
  }

  public static IResult Delete(
    HttpContext ctx, Auth auth, SqliteConnection conn, JsonElement o) {
    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "delete from todo where id=:id and userid=:userid";
    cmd.Parameters.AddWithValue("id", id);
    cmd.Parameters.AddWithValue("userid", auth.GetCurrentUser(ctx));

    if(cmd.ExecuteNonQuery() == 0) {
      return Results.BadRequest(new {error = "cannot delete"});
    }

    return Results.Ok();
  }

}
