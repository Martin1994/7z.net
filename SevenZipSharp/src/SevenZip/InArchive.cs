using SevenZip.Native;
namespace SevenZip;

public struct SevenZipProperty
{
    public string Name { get; init; }
    public string Value { get; init; }
}

// Note: Neither 7-zip nor this .NET wrapper is thread safe
public unsafe class SevenZipInArchive : IDisposable
{
    private readonly IInArchive* _arc;
    private readonly InStreamProxy _stream;
    private bool _disposedValue;
    private readonly Lazy<SevenZipItemTree> _fileTree;
    public SevenZipItemTree FileTree => _fileTree.Value;

    private SevenZipItemTree BuildItemTree()
    {
        PROPVARIANT prop;
        uint num = this.Count;
        if (num >= 0x80000000)
        {
            throw new IndexOutOfRangeException($"The archive contains too many items: {num}");
        }

        SevenZipItemTree tree = new((int)num);
        for (uint i = 0; i < num; i++)
        {
            _arc->GetProperty(i, PROPID.kpidIsDeleted, out prop);
            bool deleted = prop.ReadBool(false);
            if (deleted)
            {
                continue;
            }

            _arc->GetProperty(i, PROPID.kpidPath, out prop);
            string path = prop.ReadOptionalString() ?? "";
            _arc->GetProperty(i, PROPID.kpidIsDir, out prop);
            bool isDir = prop.ReadBool();

            tree.Add(i, path, isDir);
        }
        return tree;
    }

    public uint Count => _arc->GetNumberOfItem();

    public ulong PhysicalSize
    {
        get
        {
            _arc->GetArchiveProperty(PROPID.kpidPhySize, out var prop);
            return prop.ReadUInt64();
        }
    }

    public SevenZipItem this[uint index] => new SevenZipItem(_arc, index);

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
        _stream = new InStreamProxy(stream);
        _arc->Open(in _stream.ComObject);
        _fileTree = new Lazy<SevenZipItemTree>(BuildItemTree);
    }

    public void Extract(Span<uint> indexes, NAskMode mode, in IManagedArchiveExtractCallback callback)
    {
        indexes.Sort();

        using var callbackProxy = new ArchiveExtractCallbackProxy(callback);
        _arc->Extract(indexes, mode, in callbackProxy.ComObject);
    }

    public void ExtractAll(NAskMode mode, in IManagedArchiveExtractCallback callback)
    {
        using var callbackProxy = new ArchiveExtractCallbackProxy(callback);
        _arc->Extract(mode, in callbackProxy.ComObject);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _stream?.Dispose();
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
