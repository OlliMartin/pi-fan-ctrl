using PiFanCtrl.Model;

namespace PiFanCtrl.Interfaces;

public interface IReadingStore
{
  public Task AddAsync(IReading reading, CancellationToken cancelToken = default);

  public Task AddRangeAsync(
    IEnumerable<IReading> readings,
    CancellationToken cancelToken = default
  );

  public IEnumerable<IReading> GetAll();

  public decimal? GetLatest(string source);

  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;
}

public sealed class ReadingChangedEventArgs : EventArgs
{
  public string Source { get; }
  public IReading Reading { get; }

  public ReadingChangedEventArgs(string source, IReading reading)
  {
    Source = source;
    Reading = reading;
  }
}