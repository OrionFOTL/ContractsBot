namespace MafiaContractsBot.Features.ContractsRanking;

public class TemporaryFile : IDisposable
{
    private bool _disposed;

    public FileInfo File { get; }

    public FileStream FileStream { get; }

    public TemporaryFile(string filePath)
    {
        File = new FileInfo(filePath);
        FileStream = File.Open(FileMode.Open, FileAccess.ReadWrite);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            FileStream.Dispose();
        }

        try
        {
            if (File.Exists)
            {
                File.Delete();
            }
        }
        catch
        {
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~TemporaryFile() => Dispose(false);
}
