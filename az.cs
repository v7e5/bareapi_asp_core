static class AZ {

  public static IResult List(SqliteConnection conn, JsonElement? o) {
    var cursor_init = o?._long("cursor_init");
    var cursor_prev = o?._long("cursor_prev");
    var cursor_next = o?._long("cursor_next");
    var filter_text = o?._str("filter_text");
    var flatten = o?._bool("flatten") ?? false;
    var fields = (o?.TryGetProperty("fields", out var v) ?? false)
      ? v.EnumerateObject()
         .ToFrozenDictionary(kv => kv.Name, kv => kv.Value.GetString()) : null;
    var count = o?._int("count") ?? 10;

    var cursor = cursor_next ?? cursor_prev;
    var forward = cursor_prev == null;
    var(op, dir) = forward ? (">", "asc"): ("<", "desc");
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, word from az where 1 ";

    if (filter_text != null) {
      cmd.CommandText += """
        and (
          word like:filter
        )
        """;
      cmd.Parameters.AddWithValue("filter", $"%{filter_text}%");
    }

    if (cursor != null) {
      cmd.CommandText += $" and id {op}:cursor ";
      cmd.Parameters.AddWithValue("cursor", cursor);
    }

    cmd.CommandText += $" group by id, word order by id {dir} limit:limit";
    cmd.Parameters.AddWithValue("limit", count + 1);
    var reader = cmd.ExecuteReader();
    cursor_prev = cursor_next = null;

    if (!reader.HasRows) {
      return Results.Ok(
        new {
          data = (object?)null,
          cursor_init,
          cursor_prev,
          cursor_next,
        }
      );
    }

    string[] cols;

    if (fields != null) {
      cols = new string[fields.Count];

      using (var enumerator = fields.GetEnumerator()) {
        var i = 0;
        while (enumerator.MoveNext()) {
          cols[i++] = enumerator.Current.Key;
        }
      }
    } else {
      cols = new[] {
        "id",
        "word"
      };
    }

    var data = new object[count][];
    var index = forward ? 0: ((count - 1) * -1);
    var lenCol = cols.Length;
    var more = false;
    var cnt = 0;

    while (reader.Read()) {
      if (++cnt > count) {
        more = true;
        break;
      }

      var k = Math.Abs(index++);
      var row = new Dictionary<string, object>(lenCol);

      for (int i = 0; i < lenCol; i++) {
        var _k = cols[i];
        row[fields?[_k] ?? _k] = reader[_k];
      }

      data[k] = new object[2] { reader["id"], row };
    }

    if (cnt < count) {
      Array.Resize(ref data, cnt);
    }

    var lenData = data.Length;
    cursor_prev = (long)data[0][0];

    if (cursor == null) {
      cursor_init = cursor_prev;
    }

    if (cursor_init == cursor_prev) {
      cursor_prev = null;
    }

    if (!forward || more) {
      cursor_next = (long)data[lenData - 1][0];
    }

    object[]? flat = null;

    if (flatten) {
      flat = new object[lenData];

      for (int i = 0; i < lenData; i++) {
        flat[i] = data[i][1];
      }

      data = null;
    }

    return Results.Ok(
      new {
        data = flat ?? data,
        cursor_init,
        cursor_prev,
        cursor_next,
      }
    );
  }

}
