namespace util;

//readonly record struct JSON(JsonElement Body) {
//  public static async ValueTask<JSON> BindAsync(HttpContext ctx) {
//    using(var o = await JsonDocument.ParseAsync(ctx.Request.Body)) {
//      return new JSON(o.RootElement.Clone());
//    }
//  }
//}

static class Util {

  public static string _sator =
    """
    sator
    arepo
    tenet
    opera
    rotas
    """;

  public static string _json =
    """
    {
      "str": "abc",
      "int": 370,
      "double": 370.9999,
      "bool": true,
      "null": null,
      "arr_str": [
        "abc",
        "def",
        "ghi"
      ],
      "arr_mix": [
        1,
        "one",
        1
      ],
      "arr_num": [
        1,
        2,
        3
      ],
      "composite": {
        "str": "abc",
        "int": 370,
        "double": 370.9999,
        "bool": true,
        "null": null,
        "arr_str": [
          "abc",
          "def",
          "ghi"
        ],
        "arr_mix": [
          1,
          "one",
          1
        ],
        "arr_num": [
          1,
          2,
          3
        ]
      }
    }
    """;

  public static void cl<T>(T x) {
    Console.WriteLine(x);
  }

  public static void z<T>(this T t) {
    Console.WriteLine(t);
  }

  public static FrozenDictionary<string, JsonElement> fd(this JsonElement o) {
    return o.EnumerateObject()
      .ToFrozenDictionary(kv => kv.Name, kv => kv.Value);
  }

  public static FrozenDictionary<String, String> env() {
    return Environment
      .GetEnvironmentVariables()
      .Cast<System.Collections.DictionaryEntry>()
      .ToFrozenDictionary(
        kv => kv.Key.ToString() ?? "", kv => kv.Value?.ToString() ?? "");
  }

  public static Task<string> env_str() {
    return Task.Run(() => dump(env()));
  }

  public static string dump(FrozenDictionary<string, string> x) {
    var c1 = x.Keys.Max(k => k.Length);
    var c2 = x.Values.Max(v => v.Length);

    var t_c1 = new String('━', c1);
    var t_c2 = new String('━', c2);
    var s_c2 = new String('─', c2);

    var t = '┏' + t_c1 + '┳' + t_c2 + '┑' + Environment.NewLine + '┃';
    var b = '│' + Environment.NewLine + '┗' + t_c1 + '┻' + t_c2 + '┙';
    var j = '│' + Environment.NewLine
      + '┣' + t_c1 + '┫' + s_c2 + '┤' + Environment.NewLine + '┃';

    return new StringBuilder(t)
      .AppendJoin(j, x.Select(
          kv => kv.Key.PadRight(c1) + '┃' + kv.Value.PadRight(c2)))
      .Append(b).ToString();
  }

  public static int[] collatz(int[] a) {
    var i = a.Last();

    if (i is 1) {
      return a;
    }

    return collatz([
      ..a,
      (((i % 2) == 0) ? (i / 2) : ((3 * i) + 1))
    ]);
  }

  public static IEnumerable<IDictionary<string, object>> dict(
    SqliteDataReader reader) {
    return reader.Cast<IDataRecord>()
      .Select(e => Enumerable.Range(0, e.FieldCount)
        .ToFrozenDictionary(e.GetName, e.GetValue));
  }

}
