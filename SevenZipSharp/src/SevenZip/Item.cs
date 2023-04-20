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

    public SevenZipItemType Type
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidIsDir, out var prop);
            return prop.ReadBool() ? SevenZipItemType.Directory : SevenZipItemType.File;
        }
    }

    public string Path
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidPath, out var prop);
            return prop.ReadString();
        }
    }

    const uint FILE_ATTRIBUTE_UNIX_EXTENSION = 0x8000;
    const uint S_IFMT = 0xF000;
    const uint S_IFDIR = 0x4000;
    public UnixFileMode UnixFileMode
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidAttrib, out var prop);
            uint windowsAttr = prop.ReadUInt32();

            if ((windowsAttr & FILE_ATTRIBUTE_UNIX_EXTENSION) > 0)
            {
                uint unixMode = windowsAttr >> 16;
                if ((unixMode & S_IFMT) == S_IFDIR)
                {
                    unixMode |= (uint)(UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute);
                }
                return (UnixFileMode)(unixMode & 0xFFF);
            }
            else
            {
                FileAttributes parsedAttr = (FileAttributes)(windowsAttr & 0x7FFF);
                UnixFileMode mode = UnixFileMode.OtherRead | UnixFileMode.GroupRead | UnixFileMode.UserRead;

                if ((parsedAttr & FileAttributes.Directory) > 0)
                {
                    mode |= UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute;
                }

                if ((parsedAttr & FileAttributes.ReadOnly) == 0)
                {
                    mode |= UnixFileMode.UserWrite;
                }

                return mode;
            }
        }
    }

    public FileAttributes WindowsFileAttributes
    {
        get
        {
            _arc->GetProperty(_index, PROPID.kpidAttrib, out var prop);
            uint attr = prop.ReadUInt32();
            return (FileAttributes)(attr & 0x7FFF); // 7z only supports attributes up to 0x7FFF
        }
    }
}
