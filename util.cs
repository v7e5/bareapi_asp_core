namespace util;

static class Util {

  public static void cl<T>(T x) {
    Console.WriteLine(x);
  }

  public static void z<T>(this T t) {
    Console.WriteLine(t);
  }

  public static string? _str(this JsonElement j, string k) =>
    (j.ValueKind is JsonValueKind.Object
      && j.TryGetProperty(k, out var v)
      && v.ValueKind is JsonValueKind.String
      && v.GetString()?.Trim() is string s
      && s.Length != 0
    ) ? s : null;

  public static long? _long(this JsonElement j, string k) =>
    (j.ValueKind is JsonValueKind.Object
      && j.TryGetProperty(k, out var v)
      && v.ValueKind is JsonValueKind.Number
      && v.TryGetInt64(out var i)
    ) ? i : null;

  public static int? _int(this JsonElement j, string k) =>
    (j.ValueKind is JsonValueKind.Object
      && j.TryGetProperty(k, out var v)
      && v.ValueKind is JsonValueKind.Number
      && v.TryGetInt32(out var i)
    ) ? i : null;

  public static bool? _bool(this JsonElement j, string k) =>
    (j.ValueKind is JsonValueKind.Object
      && j.TryGetProperty(k, out var v)
      && v.ValueKind is (JsonValueKind.True or JsonValueKind.False)
    ) ? v.GetBoolean() : null;

  public static JsonElement.ArrayEnumerator? _arr(
    this JsonElement j, string k
  ) =>
    (j.ValueKind is JsonValueKind.Object
      && j.TryGetProperty(k, out var v)
      && v.ValueKind is JsonValueKind.Array
    ) ? v.EnumerateArray() : null;

  public static IEnumerable<T> _reverse<T>(
    this IEnumerable<T> source, bool reverse = true
  ) => reverse ? source.Reverse() : source;

  public static IDictionary<string, object>[] ToDictArray(
    this SqliteDataReader reader, bool reverse = false
  ) => reader
    .Cast<IDataRecord>()
    .Select(e => Enumerable.Range(0, e.FieldCount)
      .ToDictionary(e.GetName, e.GetValue))
    ._reverse(reverse)
    .ToArray();

  public static byte[] deriveKey(string password, byte[] salt) =>
    Rfc2898DeriveBytes.Pbkdf2(
      password: password,
      salt: salt,
      iterations: 100000,
      hashAlgorithm: HashAlgorithmName.SHA512,
      outputLength: 32
    );

  public static FrozenDictionary<string, JsonElement> fd(this JsonElement o) =>
    o.EnumerateObject().ToFrozenDictionary(kv => kv.Name, kv => kv.Value);

  public static FrozenDictionary<String, String> env() =>
    Environment
      .GetEnvironmentVariables()
      .Cast<System.Collections.DictionaryEntry>()
      .ToFrozenDictionary(
        kv => kv.Key.ToString() ?? "",
        kv => kv.Value?.ToString() ?? ""
      );

  public static Task<string> env_str() => Task.Run(() => dump(env()));

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

  public static int[] collatz(int[] a) => a[^1] switch {
    1 => a, 
    int n => collatz([..a, n % 2 == 0 ? n/2 : (3*n)+1])
  };

}
