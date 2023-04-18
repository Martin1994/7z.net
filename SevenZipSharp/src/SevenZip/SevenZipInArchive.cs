using System.Collections.ObjectModel;
using SevenZip.Native;
using SevenZip.Native.Com;

namespace SevenZip;

public struct SevenZipProperty
{
    public string Name { get; init; }
    public string Value { get; init; }
}

// Note: Neither 7z nor its .NET wrapper is thread safe
public unsafe class SevenZipInArchive : IDisposable
{
    private readonly IInArchive* _arc;
    private readonly ManagedInStream _stream;
    private bool _disposedValue;
    private readonly Lazy<SevenZipFileTree> _fileTree;
    public SevenZipFileTree FileTree => _fileTree.Value;

    public uint Count => _arc->GetNumberOfItem();

    private SevenZipFileTree BuildFileTree()
    {
        PROPVARIANT prop;
        uint num = this.Count;
        if (num >= 0x80000000)
        {
            throw new IndexOutOfRangeException($"The archive contains too many items: {num}");
        }

        SevenZipFileTree tree = new((int)num);
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

            tree.Add((int)i, path, isDir);
        }
        return tree;
    }

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
        _fileTree = new Lazy<SevenZipFileTree>(BuildFileTree);
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
