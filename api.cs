class Auth: IMiddleware {
  private readonly SqliteConnection conn;

  public Auth(SqliteConnection _conn) => conn = _conn;

  public long? GetCurrentUser(HttpContext ctx) {
    if(ctx.Request.Cookies.TryGetValue("_id", out var k)) {
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "select userid from session where id=:id";
      cmd.Parameters.AddWithValue("id", k);
      return (long?) cmd.ExecuteScalar();
    }
    return null;
  }

  public bool IsAdmin(HttpContext ctx) => this.GetCurrentUser(ctx) == 1;

  public async Task InvokeAsync(HttpContext ctx, RequestDelegate nxt) {
    cl($"[;38;5;27;1m[{ctx.Request.Path}][0m");

    if((this.GetCurrentUser(ctx) is not null)
      || (ctx.Request.Path.ToString() == "/login")) {
      await nxt(ctx);
    } else {
      ctx.Response.StatusCode = 403;
      ctx.Response.ContentType = "application/json";
      await ctx.Response.WriteAsync("""{"error": "verboten"}""");
      await ctx.Response.CompleteAsync();
    }
  }
}

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
      })
      .AddScoped<Auth>();

    var app = builder.Build();

    app.UseCors(builder => {
      builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
    app.UseMiddleware<Auth>();
    app.UseStaticFiles();
    app.UseRouting();
    app.MapShortCircuit(404, "robots.txt", "favicon.ico", ".well-known");
    app.UseExceptionHandler();
    app.UseDeveloperExceptionPage();

    app.MapPost("/echo", (JsonElement o) => o);

    app.MapPost("/env", () => env());

    app.MapPost("/now", (
      Auth auth, HttpContext ctx, SqliteConnection conn
    ) => {
      using var cmd = conn.CreateCommand();
      cmd.CommandText = "select unixepoch()";

      var db = (long?) cmd.ExecuteScalar();
      var cs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
      var ts = DateTimeOffset.FromUnixTimeSeconds(cs).ToString();

      return new {
        db, cs, ts,
        user = auth.GetCurrentUser(ctx)
      };
    });

    app.MapPost("/login", (
      HttpContext ctx, SqliteConnection conn, Auth auth, JsonElement o
    ) => {
      if (auth.GetCurrentUser(ctx) is not null) {
        cl("logged in - skip");
        return Results.Ok();
      }

      string? username = o._str("username");
      string? passwd = o._str("passwd");

      if((username, passwd) is (null, null)) {
        return Results.BadRequest(new {error = "need a name and password"});
      }

      using var user_cmd = conn.CreateCommand();
      user_cmd.CommandText
        = "select id, passwd from user where username=:u";
      user_cmd.Parameters.AddWithValue("u", username);

      var user = user_cmd.ExecuteReader().ToDictArray().FirstOrDefault();

      if(user is null
        || (((string) user["passwd"]).Split(':') is var arr
          && !CryptographicOperations.FixedTimeEquals(
          deriveKey(
            password: passwd!,
            salt: Convert.FromBase64String(arr[0])
          ),
          Convert.FromBase64String(arr[1])))
        ) {
        return Results.BadRequest(new {error = "incorrect user/pass"});
      }

      var userid = (long) user["id"];

      using var sess_del = conn.CreateCommand();
      sess_del.CommandText = "delete from session where userid=:u";
      sess_del.Parameters.AddWithValue("u", userid);
      sess_del.ExecuteNonQuery();

      static IEnumerable<string> _guid() {
        while(true) {
          yield return Guid.NewGuid().ToString();
        }
      }

      foreach(var g in _guid()) {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "select id from session where id=:id";
        cmd.Parameters.AddWithValue("id", g);

        if(cmd.ExecuteScalar() is null) {
          using var sess_add = conn.CreateCommand();
          sess_add.CommandText
            = "insert into session(id, userid) values (:g, :u)";
          sess_add.Parameters.AddWithValue("g", g);
          sess_add.Parameters.AddWithValue("u", userid);
          sess_add.ExecuteNonQuery();

          ctx.Response.Headers.Append(
            "set-cookie", "_id=" + g
            + ";domain=0.0.0.0;path=/;httponly;samesite=lax;max-age=604800"
          );
          break;
        }
      };

      return Results.Ok();
    });

    app.MapPost("/logout", (
      HttpContext ctx, SqliteConnection conn, Auth auth
    ) => {
      using var sess_del = conn.CreateCommand();
      sess_del.CommandText = "delete from session where userid=:u";
      sess_del.Parameters.AddWithValue("u", auth.GetCurrentUser(ctx));
      sess_del.ExecuteNonQuery();

      ctx.Response.Headers.Append(
        "set-cookie", "_id="
        + ";domain=0.0.0.0;path=/;httponly;samesite=lax;max-age=0"
      );

      return Results.Ok();
    });

    app.MapPost("/az", AZ.List);

    var _category = app.MapGroup("/category");
    _category.MapPost("/list",   Category.List);
    _category.MapPost("/create", Category.Create);
    _category.MapPost("/update", Category.Update);
    _category.MapPost("/delete", Category.Delete);

    var _user = app.MapGroup("/user");
    _user.MapPost("/list",   User.List);
    _user.MapPost("/create", User.Create);
    _user.MapPost("/delete", User.Delete);
    _user.MapPost("/resetpass", User.ResetPass);

    var _todo = app.MapGroup("/todo");
    _todo.MapPost("/list",   Todo.List);
    _todo.MapPost("/create", Todo.Create);
    _todo.MapPost("/update", Todo.Update);
    _todo.MapPost("/delete", Todo.Delete);

    cl($"[48;5;227;38;5;0;1m{app.Environment.EnvironmentName}[0m");
    await app.RunAsync();
  }
}
