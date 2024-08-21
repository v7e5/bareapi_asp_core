class XXX {
  static async Task Main() {
    cl(
      String.Join("", collatz([Random.Shared.Next(1, 79)])
        .Select(e => $"[;38;5;{e % 256};1m\u25a0")) + "[0m"
    );

    var builder = WebApplication.CreateEmptyBuilder(new(){
      WebRootPath = "static"
    });
    builder.Configuration.Sources.Clear();
    builder.Configuration.AddJsonFile(
      "config.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();

    builder.WebHost.UseKestrelCore().ConfigureKestrel(o => {
      o.AddServerHeader = false;
      o.Limits.MaxRequestBodySize = null;
      o.ListenLocalhost(
        builder.Configuration.GetValue<Int16>("port", 5000));
    });

    builder.Services
      .AddRoutingCore()
      .AddProblemDetails()
      .AddCors()
      .AddScoped(_ => {
        var conn = new SqliteConnection(
          builder.Configuration.GetValue<string>("dbconn"));
        conn.Open();
        return conn;
      });

    var app = builder.Build();

    app.UseCors(builder => {
      builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
    app.UseStaticFiles();
    app.UseRouting();
    app.MapShortCircuit(404, "robots.txt", "favicon.ico", ".well-known");
    app.UseExceptionHandler();
    app.UseDeveloperExceptionPage();

    app.MapPost("/env", () => {
      return env();
    });

    app.MapPost("/echo", (JsonElement o) => {
      return o;
    });

    var _category = app.MapGroup("/category");

    _category.MapPost("/create",
      (HttpContext ctx, SqliteConnection conn, JsonElement o) => {

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

        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "name_" + i,
            SqliteType = SqliteType.Text,
            Value = name
          });

        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "color_" + i,
            SqliteType = SqliteType.Text,
            Value = color
          });
      }
      cmd.CommandText = cmd.CommandText.TrimEnd(',');
      cmd.ExecuteNonQuery();

      return Results.Ok();
    });

    _category.MapPost("/list",
      (HttpContext ctx, SqliteConnection conn, JsonElement? o) => {

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
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "id",
            SqliteType = SqliteType.Integer,
            Value = id
          });
      }

      if(name is not null) {
        cmd.CommandText += " and name like :name ";
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "name",
            SqliteType = SqliteType.Text,
            Value = $"%{name}%"
          });
      }

      if(color is not null) {
        cmd.CommandText += " and color = :color ";
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "color",
            SqliteType = SqliteType.Text,
            Value = color
          });
      }

      return cmd.ExecuteReader().ToDictArray();
    });

    _category.MapPost("/update",
      (HttpContext ctx, SqliteConnection conn, JsonElement o) => {

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
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "name",
            SqliteType = SqliteType.Text,
            Value = name
          });
      }

      if(color is not null) {
        cmd.CommandText += " color = :color ,";
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "color",
            SqliteType = SqliteType.Text,
            Value = color
          });
      }

      cmd.CommandText = cmd.CommandText.TrimEnd(',');

      cmd.CommandText += " where id = :id";
      cmd.Parameters.Add(
        new SqliteParameter {
          ParameterName = "id",
          SqliteType = SqliteType.Integer,
          Value = id
        });

      cmd.ExecuteNonQuery();

      return Results.Ok();
    });

    _category.MapPost("/delete",
      (HttpContext ctx, SqliteConnection conn, JsonElement o) => {

      if(o.ValueKind is not JsonValueKind.Object) {
        return Results.BadRequest(new {error = "not an object"});
      }

      long? id = o._long("id");
      if(id is null) {
        return Results.BadRequest(new {error = "need an id"});
      }

      using var cmd = conn.CreateCommand();
      cmd.CommandText = "delete from category where id = :id";

      cmd.Parameters.Add(
        new SqliteParameter {
          ParameterName = "id",
          SqliteType = SqliteType.Integer,
          Value = id
        });

      cmd.ExecuteNonQuery();

      return Results.Ok();
    });

    var _user = app.MapGroup("/user");

    _user.MapPost("/create",
      (HttpContext ctx, SqliteConnection conn, JsonElement o) => {

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

      cmd.Parameters.Add(
        new SqliteParameter {
          ParameterName = "username",
          SqliteType = SqliteType.Text,
          Value = username
        });

      cmd.Parameters.Add(
        new SqliteParameter {
          ParameterName = "passwd",
          SqliteType = SqliteType.Text,
          Value = passwd
        });

      try {
        cmd.ExecuteNonQuery();
      } catch (SqliteException ex) {
        return Results.BadRequest(new {
          error = (ex.SqliteErrorCode == 19)
            ? "username already exists" : ex.Message
        });
      }

      return Results.Ok();
    });

    _user.MapPost("/list",
      (HttpContext ctx, SqliteConnection conn, JsonElement? o) => {

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
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "id",
            SqliteType = SqliteType.Integer,
            Value = id
          });
      }

      if(name is not null) {
        cmd.CommandText += " and name = :name ";
        cmd.Parameters.Add(
          new SqliteParameter {
            ParameterName = "name",
            SqliteType = SqliteType.Text,
            Value = name
          });
      }

      return cmd.ExecuteReader().ToDictArray();
    });

    _user.MapPost("/delete",
      (HttpContext ctx, SqliteConnection conn, JsonElement o) => {

      if(o.ValueKind is not JsonValueKind.Object) {
        return Results.BadRequest(new {error = "not an object"});
      }

      long? id = o._long("id");
      if(id is null) {
        return Results.BadRequest(new {error = "need an id"});
      }

      using var cmd = conn.CreateCommand();
      cmd.CommandText = "delete from user where id = :id";

      cmd.Parameters.Add(
        new SqliteParameter {
          ParameterName = "id",
          SqliteType = SqliteType.Integer,
          Value = id
        });

      cmd.ExecuteNonQuery();

      return Results.Ok();
    });

    cl($"[48;5;227;38;5;0;1m{app.Environment.EnvironmentName}[0m");
    await app.RunAsync();
  }
}
