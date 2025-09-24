class Auth: IMiddleware {
  private static readonly string[] noauth = {
    "login", "hailstone", "ecco", "env", "now", "cg",
    "pdf" , "img"
  };
  private readonly HttpContext? ctx;
  private readonly SqliteConnection conn;

  public Auth(IHttpContextAccessor acx, SqliteConnection conn) =>
    (this.conn, this.ctx) = (conn, acx.HttpContext);

  public long? GetCurrentUser() {
    if(this.ctx?.Request.Cookies.TryGetValue("_id", out var k) ?? false) {
      using var cmd = this.conn.CreateCommand();
      cmd.CommandText = "select userid from session where id=:id";
      cmd.Parameters.AddWithValue("id", k);
      return (long?) cmd.ExecuteScalar();
    }
    return null;
  }

  public bool IsAdmin() => this.GetCurrentUser() == 1;

  public async Task InvokeAsync(HttpContext ctx, RequestDelegate nxt) {
    cl($"[;38;5;27;1m[{ctx.Request.Path}][0m");

    if((this.GetCurrentUser() is not null)
      || (noauth.Contains(ctx.Request.Path.ToString()[1..]))) {
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
    cl(String.Join("", collatz([Random.Shared.Next(1, 79)])
      .Select(e => $"[;38;5;{e % 256};1m|")) + "[0m");

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
      .AddCors()
      .AddHttpContextAccessor()
      .AddScoped(_ => {
        var conn = new SqliteConnection(
          builder.Configuration.GetValue<string>("dbconn"));
        conn.Open();
        return conn;
      })
      .AddScoped<Auth>()
      .AddProblemDetails();

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

    app.MapPost("/ecco", async (JsonElement? o) => {
      if(o?._int("delay") is int n && n > 0) {
        await Task.Delay(n);
      }

      return new {
        t = DateTimeOffset.UtcNow.ToLocalTime().ToString(),
        o = (o?.ValueKind is JsonValueKind.Undefined) ? null : o,
      };
    });

    app.MapPost("/cg", () => new ConGen());

    app.MapPost("/env", () => env());

    app.MapGet("/env", async (int? n) => {
      if(n is not null) {
        await Task.Delay((int) n);
      }

      return env();
    });

    static async Task _pdf(IWebHostEnvironment env, HttpContext ctx) {
      ctx.Response.ContentType = "application/pdf";

      using (var fs = new FileStream(
        Path.Combine(env.WebRootPath, "thinkpad_e14_gen_2_amd_spec.pdf"),
        FileMode.Open, FileAccess.Read
      )) {
        await fs.CopyToAsync(ctx.Response.Body);
      }
    }

    static async Task _img(IWebHostEnvironment env, HttpContext ctx) {
      ctx.Response.ContentType = "image/png";

      using (var fs = new FileStream(
        Path.Combine(env.WebRootPath, "tabriz.png"),
        FileMode.Open, FileAccess.Read
      )) {
        await fs.CopyToAsync(ctx.Response.Body);
      }
    }

    app.MapGet("/pdf", _pdf);
    app.MapPost("/pdf", _pdf);
    app.MapGet("/img", _img);
    app.MapPost("/img", _img);

    app.MapPost("/hailstone", (JsonElement o) =>
      ((o._int("n") is int n) && n > 0) ? collatz([n]) : null); 

    app.MapGet("/hailstone", (int n) => (n > 0) ? collatz([n]) : null); 

    app.MapPost("/now", (
      Auth auth, SqliteConnection conn
    ) => {
      using var cmd = conn.CreateCommand();
      cmd.CommandText =
      """
      select
        datetime('now', 'localtime') as local,
        unixepoch() as unix_timestamp,
        current_timestamp as unix_timestamp_str
      """;

      var ut = DateTimeOffset.UtcNow;

      return new {
        user = auth.GetCurrentUser(),
        server = new {
          local = ut.ToLocalTime().ToString(),
          unix_timestamp = ut.ToUnixTimeSeconds(),
          unix_timestamp_str = ut.ToString()
        },
        database = cmd.ExecuteReader().ToDictArray().FirstOrDefault(),
        ng = new NonGen().Cast<int>(),
        cg = new ConGen()
      };
    });

    app.MapPost("/login", (
      HttpContext ctx, SqliteConnection conn, Auth auth, JsonElement o
    ) => {
      if (auth.GetCurrentUser() is not null) {
        cl("logged in - skip");
        return Results.Ok();
      }

      string? username = o._str("username");
      string? password = o._str("password");

      if((username, password) is (null, null)) {
        return Results.BadRequest(new {error = "need a name and password"});
      }

      using var user_cmd = conn.CreateCommand();
      user_cmd.CommandText
        = "select id, password from user where username=:u";
      user_cmd.Parameters.AddWithValue("u", username);

      var user = user_cmd.ExecuteReader().ToDictArray().FirstOrDefault();

      if(user is null
        || (user["password"]?.ToString()?.Split(':') is string[] arr
          && !CryptographicOperations.FixedTimeEquals(
          deriveKey(
            password: password!,
            salt: Convert.FromBase64String(arr[0])
          ),
          Convert.FromBase64String(arr[1])))
        ) {
        return Results.BadRequest(new {error = "incorrect user/pass"});
      }

      var userid = (long?) user["id"];

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
            + ";path=/;httponly;samesite=lax;max-age=604800"
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
      sess_del.Parameters.AddWithValue("u", auth.GetCurrentUser());
      sess_del.ExecuteNonQuery();

      ctx.Response.Headers.Append(
        "set-cookie", "_id=;path=/;httponly;samesite=lax;max-age=0"
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
    _user.MapPost("/list",      User.List);
    _user.MapPost("/create",    User.Create);
    _user.MapPost("/delete",    User.Delete);
    _user.MapPost("/profile",   User.Profile);
    _user.MapPost("/resetpass", User.ResetPass);

    var _todo = app.MapGroup("/todo");
    _todo.MapPost("/list",   Todo.List);
    _todo.MapPost("/create", Todo.Create);
    _todo.MapPost("/update", Todo.Update);
    _todo.MapPost("/delete", Todo.Delete);

    var _color = app.MapGroup("/color");
    _color.MapPost("/list",   Color.List);
    _color.MapPost("/group",   Color.Group);

    cl($"[48;5;227;38;5;0;1m{app.Environment.EnvironmentName}[0m");
    await app.RunAsync();
  }
}
