using SevenZip.Native;
using System.Runtime.CompilerServices;

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
    public Stream Stream { get; }
    private readonly InStreamProxy _streamProxy;
    private bool _disposedValue;
    private readonly Lazy<SevenZipItemTree> _itemTree;
    public SevenZipItemNode RootNode => _itemTree.Value.Root;

    public uint Count => _arc->GetNumberOfItem();

    static SevenZipInArchive()
    {
        RuntimeHelpers.RunClassConstructor(typeof(ArchiveExtractCallbackProxy).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CompressProgressInfoProxy).TypeHandle);
        RuntimeHelpers.RunClassConstructor(typeof(CryptoGetTextPasswordProxy).TypeHandle);
    }

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
        var formats = SevenZipArchiveFormat.FromPath(filename);
        
        _streamProxy = new InStreamProxy(stream);

        foreach (var format in formats)
        {
            try
            {
                _arc = SevenZipLibrary.CreateObject<IInArchive>(format.ClassId);
                _arc->AddRef();
                _arc->Open(in _streamProxy.ComObject);
                break;
            }
            catch (SevenZipComException ex)
            {
                _arc->Release();
                _arc = null;
                if (ex.Code != HRESULT.S_FALSE)
                {
                    _streamProxy.Dispose();
                    throw;
                }
            }
        }
        
        if (_arc == null)
        {
            _streamProxy.Dispose();
            throw new FormatException("Cannot derive archive type from the given file.");
        }

        Stream = stream;
        _itemTree = new Lazy<SevenZipItemTree>(() => new SevenZipItemTree(this));
    }

    public void Extract(IEnumerable<SevenZipItemNode> nodes, NAskMode mode, in IArchiveExtractCallback callback)
    {
        var indexes = nodes
            .SelectMany(node => node.Traverse())
            .Where(node => node.IsTracked)
            .Select(node => node.Id)
            .ToArray();
        Extract(indexes, mode, callback);
    }

    public void Extract(Span<uint> indexes, NAskMode mode, in IArchiveExtractCallback callback)
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
                throw new SevenZipComException(ex.HResult, ex.Message, inner);
            }
            throw;
        }
    }

    public void ExtractAll(NAskMode mode, in IArchiveExtractCallback callback)
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
                throw new SevenZipComException(ex.HResult, ex.Message, inner);
            }
            throw;
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
