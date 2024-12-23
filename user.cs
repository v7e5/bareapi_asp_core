static class User {

  public static IResult Create(
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    string? username = o._str("username");
    string? password = o._str("password");

    if((username, password) is (null, null)) {
      return Results.BadRequest(new {error = "need a name and password"});
    }

    using var ex_user = conn.CreateCommand();
    ex_user.CommandText = "select id from user where username=:username";
    ex_user.Parameters.AddWithValue("username", username);

    if(ex_user.ExecuteScalar() is not null) {
      return Results.BadRequest(new {error = "username already exists"});
    }

    byte[] salt = RandomNumberGenerator.GetBytes(16);
    byte[] hash = deriveKey(password: password!, salt: salt);

    using var cmd = conn.CreateCommand();
    cmd.CommandText
      = "insert into user(username, password) values (:username, :password)";

    cmd.Parameters.AddWithValue("username", username);
    cmd.Parameters.AddWithValue("password",
      Convert.ToBase64String(salt) + ':' + Convert.ToBase64String(hash));
    if(cmd.ExecuteNonQuery() == 0) {
      return Results.BadRequest(new {error = "cannot create"});
    }

    return Results.Ok();
  }

  public static IResult List(
    Auth auth, SqliteConnection conn, JsonElement? o
  ) {
    if(!auth.IsAdmin()) {
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
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    if(!auth.IsAdmin()) {
      return Results.BadRequest(new {error = "verboten"});
    }

    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "delete from user where id = :id";
    cmd.Parameters.AddWithValue("id", id);
    if(cmd.ExecuteNonQuery() == 0) {
      return Results.BadRequest(new {error = "cannot delete"});
    }

    return Results.Ok();
  }

  public static IResult ResetPass(
    Auth auth, SqliteConnection conn, JsonElement o
  ) {
    string? password = o._str("password");
    if(password is null) {
      return Results.BadRequest(new {error = "need a password"});
    }

    byte[] salt = RandomNumberGenerator.GetBytes(16);
    byte[] hash = deriveKey(password: password!, salt: salt);

    using var cmd = conn.CreateCommand();
    cmd.CommandText
      = "update user set password = :password where id = :id";
    cmd.Parameters.AddWithValue("id", auth.GetCurrentUser());
    cmd.Parameters.AddWithValue("password",
      Convert.ToBase64String(salt) + ':' + Convert.ToBase64String(hash));

    if(cmd.ExecuteNonQuery() == 0) {
      return Results.BadRequest(new {error = "cannot reset"});
    }

    return Results.Ok();
  }

  public static IResult Profile(
    Auth auth, SqliteConnection conn
  ) {
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, username as name from user where id=:id";
    cmd.Parameters.AddWithValue("id", auth.GetCurrentUser());

    return Results.Ok(new {
      user = cmd.ExecuteReader().ToDictArray().FirstOrDefault()
    });
  }
}
