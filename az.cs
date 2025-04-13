static class AZ {

  public static IResult List(SqliteConnection conn, JsonElement? o) {
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
        id, word
      from az where 1
      """;

    if(cursor != null) {
      cmd.CommandText += $" and id {op} :cursor ";
      cmd.Parameters.AddWithValue("cursor", cursor);
    }

    cmd.CommandText += $" order by id {dir} limit 10";

    var data = cmd.ExecuteReader().ToDictArray(!forward);

    cursor_prev = cursor_next = null;
    if(data.Length != 0) {
      cursor_prev = (long?) data[0]["id"];

      if(cursor == null) {
        cursor_init = cursor_prev;
      }

      if(cursor_init == cursor_prev) {
        cursor_prev = null;
      }

      if(data.Length == 10) {
        cursor_next = (long?) data[^1]["id"];
      }
    }

    return Results.Ok(new {
      data,
      cursor_init,
      cursor_prev,
      cursor_next,
    });
  }

}
