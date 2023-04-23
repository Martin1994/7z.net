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
    public ref IInArchive Native => ref *_arc;
    private readonly InStreamProxy _streamProxy;
    private bool _disposedValue;
    private readonly Lazy<SevenZipItemTree> _itemTree;
    public SevenZipItemNode ItemTree => _itemTree.Value.Root;

    public uint Count => _arc->GetNumberOfItem();

    public ulong PhysicalSize
    {
        get
        {
            _arc->GetArchiveProperty(PROPID.kpidPhySize, out var prop);
            return prop.ReadUInt64();
        }
    }

    public SevenZipItem this[uint index] => new SevenZipItem(this, index);

    public unsafe SevenZipInArchive(string filename, Stream stream)
    {
        _arc = null;
        _streamProxy = null!;
        SevenZipArchiveFormat? format = SevenZipArchiveFormat.FromPath(filename);
        if (format == null)
        {
            throw new FormatException("Cannot derive archive type from the given file.");
        }
        _arc = SevenZipLibrary.CreateObject<IInArchive>(format.ClassId);
        _streamProxy = new InStreamProxy(stream);
        _arc->Open(in _streamProxy.ComObject);
        _itemTree = new Lazy<SevenZipItemTree>(() => new SevenZipItemTree(this));
    }

    public void Extract(IEnumerable<SevenZipItemNode> nodes, NAskMode mode, in IManagedArchiveExtractCallback callback)
    {
        var indexes = nodes
            .SelectMany(node => node.Traverse())
            .Where(node => node.Type == SevenZipItemType.File)
            .Select(node => node.Id)
            .ToArray();
        Extract(indexes, mode, callback);
    }

    private void Extract(Span<uint> indexes, NAskMode mode, in IManagedArchiveExtractCallback callback)
    {
        indexes.Sort();

        using var callbackProxy = new ArchiveExtractCallbackProxy(callback);
        try
        {
            _arc->Extract(indexes, mode, in callbackProxy.ComObject);
        }
        catch (SevenZipComException ex)
        {
            Exception? inner = callbackProxy.PopPendingException();
            if (inner != null)
            {
                throw new SevenZipComException(ex.Code, ex.Message, inner);
            }
        }
    }

    public void ExtractAll(NAskMode mode, in IManagedArchiveExtractCallback callback)
    {
        using var callbackProxy = new ArchiveExtractCallbackProxy(callback);
        try
        {
            _arc->Extract(mode, in callbackProxy.ComObject);
        }
        catch (SevenZipComException ex)
        {
            Exception? inner = callbackProxy.PopPendingException();
            if (inner != null)
            {
                throw new SevenZipComException(ex.Code, ex.Message, inner);
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _streamProxy?.Dispose();
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
