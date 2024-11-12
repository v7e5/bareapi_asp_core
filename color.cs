static class Color {


  public static IResult List(SqliteConnection conn, JsonElement? o) {
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, grupo, hex, grade, vivid from color";

    return Results.Ok(cmd.ExecuteReader().ToDictArray());
  }

}

