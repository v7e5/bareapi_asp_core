static class User {

  public static IResult Create(
      HttpContext ctx, SqliteConnection conn, JsonElement o) {

    if(o.ValueKind is not JsonValueKind.Object) {
      return Results.BadRequest(new {error = "not an object"});
    }

    string? username = o._str("username");
    //plain-text here for testing purposes, do not use in production
    string? passwd = o._str("passwd");

    if((username, passwd) is (null, null)) {
      return Results.BadRequest(new {error = "need a name and password"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText
      = "insert into user(username, passwd) values (:username, :passwd)";

    cmd.Parameters.AddWithValue("username", username);
    cmd.Parameters.AddWithValue("passwd", passwd);

    try {
      cmd.ExecuteNonQuery();
    } catch (SqliteException ex) {
      return Results.BadRequest(new {
        error = (ex.SqliteErrorCode == 19)
          ? "username already exists" : ex.Message
      });
    }

    return Results.Ok();
  }

  public static IEnumerable<IDictionary<string, object>> List(
      HttpContext ctx, SqliteConnection conn, JsonElement? o) {

    long? id = null;
    string? name = null;

    if(o?.ValueKind is JsonValueKind.Object) {
      id = o?._long("id");
      name = o?._str("name");
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, username from user where 1 ";

    if(id is not null) {
      cmd.CommandText += " and id=:id ";
      cmd.Parameters.AddWithValue("id", id);
    }

    if(name is not null) {
      cmd.CommandText += " and name = :name ";
      cmd.Parameters.AddWithValue("name", name);
    }

    return cmd.ExecuteReader().ToDictArray();
  }

  public static IResult Delete
    (HttpContext ctx, SqliteConnection conn, JsonElement o) {

      if(o.ValueKind is not JsonValueKind.Object) {
        return Results.BadRequest(new {error = "not an object"});
      }

      long? id = o._long("id");
      if(id is null) {
        return Results.BadRequest(new {error = "need an id"});
      }

      using var cmd = conn.CreateCommand();
      cmd.CommandText = "delete from user where id = :id";
      cmd.Parameters.AddWithValue("id", id);
      cmd.ExecuteNonQuery();

      return Results.Ok();
    }

}
