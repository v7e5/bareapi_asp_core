static class User {

  public static IResult Create(
    HttpContext ctx, Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin(ctx)) {
      return Results.BadRequest(new {error = "verboten"});
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

  public static IResult List(
    HttpContext ctx, Auth auth, SqliteConnection conn, JsonElement? o
  ) {
    if(!auth.IsAdmin(ctx)) {
      return Results.BadRequest(new {error = "verboten"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, username from user where 1 ";

    if(o?._long("id") is long id) {
      cmd.CommandText += " and id=:id ";
      cmd.Parameters.AddWithValue("id", id);
    }

    if(o?._str("username") is string username) {
      cmd.CommandText += " and username = :username ";
      cmd.Parameters.AddWithValue("username", username);
    }

    return Results.Ok(cmd.ExecuteReader().ToDictArray());
  }

  public static IResult Delete(
    HttpContext ctx, Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin(ctx)) {
      return Results.BadRequest(new {error = "verboten"});
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
