static class Category {

  public static IResult Create(
    HttpContext ctx, SqliteConnection conn, JsonElement o) {

    if(o.ValueKind is not JsonValueKind.Array) {
      return Results.BadRequest(new {error = "not an array"});
    }

    var arr = o.EnumerateArray();
    var count = arr.Count();

    if(count == 0) {
      return Results.BadRequest(new {error = "array is empty"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "insert or ignore into category(name, color) values ";

    for(int i = 0; i < count; i++) {
      var ob = arr.ElementAt(i);

      if(ob.ValueKind is not JsonValueKind.Object) {
        continue;
      }

      string? name = ob._str("name");
      string? color = ob._str("color");

      if((name, color) is (null, null)) {
        continue;
      }

      cmd.CommandText += $"(:name_{i}, :color_{i}),";
      cmd.Parameters.AddWithValue($"name_{i}", name);
      cmd.Parameters.AddWithValue($"color_{i}", color);
    }
    cmd.CommandText = cmd.CommandText.TrimEnd(',');
    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

  public static IEnumerable<IDictionary<string, object>> List(
    HttpContext ctx, SqliteConnection conn, JsonElement? o) {

    long? id = null;
    string? name = null;
    string? color = null;

    if(o?.ValueKind is JsonValueKind.Object) {
      id = o?._long("id");
      name = o?._str("name");
      color = o?._str("color");
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "select id, name, color from category where 1 ";

    if(id is not null) {
      cmd.CommandText += " and id=:id ";
      cmd.Parameters.AddWithValue("id", id);
    }

    if(name is not null) {
      cmd.CommandText += " and name like :name ";
      cmd.Parameters.AddWithValue("name", $"%{name}%");
    }

    if(color is not null) {
      cmd.CommandText += " and color = :color ";
      cmd.Parameters.AddWithValue("color", color);
    }

    return cmd.ExecuteReader().ToDictArray();
  }

  public static IResult Update(
    HttpContext ctx, SqliteConnection conn, JsonElement o) {

    if(o.ValueKind is not JsonValueKind.Object) {
      return Results.BadRequest(new {error = "not an object"});
    }

    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    string? name = o._str("name");
    string? color = o._str("color");

    if((name, color) is (null, null)) {
      return Results.BadRequest(new {error = "no field to update"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "update or ignore category set ";

    if(name is not null) {
      cmd.CommandText += " name = :name ,";
      cmd.Parameters.AddWithValue("name", name);
    }

    if(color is not null) {
      cmd.CommandText += " color = :color ,";
      cmd.Parameters.AddWithValue("color", color);
    }

    cmd.CommandText = cmd.CommandText.TrimEnd(',');

    cmd.CommandText += " where id = :id";
    cmd.Parameters.AddWithValue("id", id);

    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

  public static IResult Delete(
    HttpContext ctx, SqliteConnection conn, JsonElement o) {

    if(o.ValueKind is not JsonValueKind.Object) {
      return Results.BadRequest(new {error = "not an object"});
    }

    long? id = o._long("id");
    if(id is null) {
      return Results.BadRequest(new {error = "need an id"});
    }

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "delete from category where id = :id";
    cmd.Parameters.AddWithValue("id", id);
    cmd.ExecuteNonQuery();

    return Results.Ok();
  }

}
