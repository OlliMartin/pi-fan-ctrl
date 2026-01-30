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

  public event EventHandler<ReadingChangedEventArgs>? ReadingChanged;
}

public class ReadingChangedEventArgs(string source, IReading reading) : EventArgs
{
  public string Source { get; } = source;
  public IReading Reading { get; } = reading;
}