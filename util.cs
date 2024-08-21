namespace util;

static class Util {

  public static void cl<T>(T x) {
    Console.WriteLine(x);
  }

  public static void z<T>(this T t) {
    Console.WriteLine(t);
  }

  public static string? _str(this JsonElement j, string k) {
    if(j.TryGetProperty(k, out var _v)) {
      var s = _v.GetString()?.Trim();
      return (s?.Length != 0) ? s : null;
    }
    return null;
  }

  public static long? _long(this JsonElement j, string k) {
    if(j.TryGetProperty(k, out var _v) && _v.TryGetInt64(out var _i)) {
      return _i;
    }
    return null;
  }

  public static IEnumerable<IDictionary<string, object>> ToDictArray(
    this SqliteDataReader reader) {
    return reader.Cast<IDataRecord>()
      .Select(e => Enumerable
        .Range(0, e.FieldCount).ToDictionary(e.GetName, e.GetValue))
      .ToArray();
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

}
