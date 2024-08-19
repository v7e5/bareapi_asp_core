class XXX {
  static async Task Main() {
    cl(
      String.Join("", collatz([Random.Shared.Next(1, 79)])
        .Select(e => $"[;38;5;{e % 256};1m\u25a0")) + "[0m"
    );

  }
}
