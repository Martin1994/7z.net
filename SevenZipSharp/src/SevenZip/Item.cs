using SevenZip.Native;

namespace SevenZip;

public enum SevenZipItemType
{
    File,
    Directory
}

public struct SevenZipItem
{
    private readonly SevenZipInArchive _arc;
    private readonly uint _index;

    public SevenZipItem(SevenZipInArchive arc, uint index)
    {
        _arc = arc;
        _index = index;
    }

    public SevenZipItemType Type
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidIsDir, out var prop);
            return prop.ReadBool() ? SevenZipItemType.Directory : SevenZipItemType.File;
        }
    }

    public string Path
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidPath, out var prop);
            return prop.ReadString();
        }
    }

    public ulong Size
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidSize, out var prop);
            return prop.ReadUInt64();
        }
    }

    public string Name => System.IO.Path.GetFileName(Path);

    const uint FILE_ATTRIBUTE_UNIX_EXTENSION = 0x8000;
    const uint S_IFMT = 0xF000;
    const uint S_IFDIR = 0x4000;
    public UnixFileMode UnixFileMode
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidAttrib, out var prop);
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
            _arc.Native.GetProperty(_index, PROPID.kpidAttrib, out var prop);
            uint attr = prop.ReadUInt32();
            return (FileAttributes)(attr & 0x7FFF); // 7z only supports attributes up to 0x7FFF
        }
    }

    public DateTime ModifiedTime
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidMTime, out var prop);
            return prop.ReadOptionalFileTime();
        }
    }

    public DateTime CreatedTime
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidCTime, out var prop);
            return prop.ReadOptionalFileTime();
        }
    }

    public DateTime AccessedTime
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidATime, out var prop);
            return prop.ReadOptionalFileTime();
        }
    }

    public string? Comment
    {
        get
        {
            _arc.Native.GetProperty(_index, PROPID.kpidComment, out var prop);
            return prop.ReadOptionalString();
        }
    }
}
