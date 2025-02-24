static class Color {
  public static IResult Group(SqliteConnection conn) {
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
      select c.grupo from color c group by c.grupo
      """;
    var data = cmd.ExecuteReader()
      .ToDictArray()
      .Select(e => e["grupo"]?.ToString()?.Split('-')[0])
      .Distinct()
      .OrderBy(s => s);
    return Results.Ok(new { data });
  }

  public static IResult List(SqliteConnection conn, JsonElement? o) {
    long? cursor_init = o?._long("cursor_init");
    long? cursor_prev = o?._long("cursor_prev");
    long? cursor_next = o?._long("cursor_next");
    string? filter_text = o?._str("filter_text");
    int? limit = o?._int("limit") ?? 10;
    long? cursor = cursor_next ?? cursor_prev;
    bool forward = cursor_prev == null;
    var(op, dir) = forward ? (">", "asc"): ("<", "desc");
    using var cmd = conn.CreateCommand();
    cmd.CommandText = """
      select
      c.id, c.grupo, c.hex, c.grade, c.vivid
      from color c where 1
      """;

    if (filter_text != null) {
      cmd.CommandText += " ";
      cmd.CommandText += """
        and (
        c.id like:filter
        or c.grupo like:filter
        or c.hex like:filter
        or c.grade like:filter
        )
        """;
      cmd.Parameters.AddWithValue("filter", $"%{filter_text}%");
    }

    if (cursor != null) {
      cmd.CommandText += $" and c.id {op}:cursor ";
      cmd.Parameters.AddWithValue("cursor", cursor);
    }

    cmd.CommandText += $" group by c.id order by c.id {dir} limit:limit";
    cmd.Parameters.AddWithValue("limit", limit);
    var data = cmd.ExecuteReader().ToDictArray(!forward);
    cursor_prev = cursor_next = null;

    if (data.Length != 0) {
      cursor_prev = (long?)data[0]["id"];

      if (cursor == null) {
        cursor_init = cursor_prev;
      }

      if (cursor_init == cursor_prev) {
        cursor_prev = null;
      }

      if (data.Length == limit) {
        cursor_next = (long?)data[^1]["id"];
      }
    }

    return Results.Ok(
    new {
      data,
      cursor_init,
      cursor_prev,
      cursor_next,
    }
      );
  }
}
