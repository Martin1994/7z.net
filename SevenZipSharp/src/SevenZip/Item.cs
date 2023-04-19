using SevenZip.Native;

namespace SevenZip;

public enum SevenZipItemType
{
    File,
    Directory
}

public unsafe struct SevenZipItem
{
    private readonly IInArchive* _arc;
    private readonly uint _index;

    public SevenZipItem(IInArchive* arc, uint index)
    {
        _arc = arc;
        _index = index;
    }

    public string Path
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidPath, out var prop);
            return prop.ReadString();
        }
    }

    public SevenZipItemType Type
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidIsDir, out var prop);
            return prop.ReadBool() ? SevenZipItemType.Directory : SevenZipItemType.File;
        }
    }
}
