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

        string? name = ob.get("name");
        string? color = ob.get("color");

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

    cl($"[48;5;227;38;5;0;1m{app.Environment.EnvironmentName}[0m");
    await app.RunAsync();
  }
}
