sealed class NonGen: System.Collections.IEnumerable {
  int[] data = {1, 2, 3};

  public System.Collections.IEnumerator GetEnumerator() => new Rator(this);

  class Rator: System.Collections.IEnumerator, IDisposable {
    NonGen collection;
    int currentIndex = -1;

    public Rator (NonGen items) => this.collection = items;

    public object Current => currentIndex switch {
      _ when currentIndex == -1 =>
        throw new InvalidOperationException ("Enumeration not started!"),
      _ when currentIndex == collection.data.Length =>
        throw new InvalidOperationException ("Past end of list!"),
      _ => collection.data[currentIndex]
    };

    public bool MoveNext() {
      cl($"nongen move {currentIndex}");
      return (currentIndex >= collection.data.Length - 1)
        ? false : ++currentIndex < collection.data.Length;
    }

    public void Reset() => currentIndex = -1;

    void IDisposable.Dispose() {
      cl("nongen dispose"); 
    }
  }
}

sealed class ConGen: IEnumerable<int> {
  int[] data = {3, 2, 1};

  public IEnumerator<int> GetEnumerator() => new Rator(this);

  System.Collections.IEnumerator
    System.Collections.IEnumerable.GetEnumerator() =>
      throw new Exception("non gen n/a");

  class Rator: IEnumerator<int> {
    ConGen collection;
    int currentIndex = -1;

    public Rator (ConGen items) => this.collection = items;

    public bool MoveNext() {
      cl($"congen move {currentIndex}");
      return ++currentIndex < collection.data.Length;
    }

    public int Current => collection.data [currentIndex];

    public void Reset() => currentIndex = -1;

    void IDisposable.Dispose() {
      cl("congen dispose"); 
    }

    object System.Collections.IEnumerator.Current =>
      throw new Exception("non gen n/a");
  }
}
