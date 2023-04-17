using SevenZip.Native;
using SevenZip.Native.Com;

namespace SevenZip;

public unsafe class SevenZipInArchive : IDisposable
{
    private readonly IInArchive* _arc;
    private readonly ManagedInStream _stream;
    private bool _disposedValue;

    public uint Count => _arc->GetNumberOfItem();

    public unsafe SevenZipInArchive(string filename, Stream stream)
    {
        _arc = null;
        _stream = null!;
        SevenZipArchiveFormat? format = SevenZipArchiveFormat.FromPath(filename);
        if (format == null)
        {
            throw new FormatException("Cannot derive archive type from the given file.");
        }
        _arc = SevenZipLibrary.CreateObject<IInArchive>(format.ClassId);
        _stream = new ManagedInStream(stream);
        _arc->Open(in _stream.ComObject);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {   
                if (_stream != null)
                {
                    _stream.Dispose();
                }
            }

            if (_arc != null)
            {
                _arc->Release();
            }
            _disposedValue = true;
        }
    }

    ~SevenZipInArchive()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
