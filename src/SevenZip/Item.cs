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
            return prop.ReadBool(false) ? SevenZipItemType.Directory : SevenZipItemType.File;
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
    public bool TryGetUnixFileMode(out UnixFileMode mode)
    {
        _arc.Native.GetProperty(_index, PROPID.kpidAttrib, out var prop);
        if (!prop.TryReadUInt32(out uint windowsAttr))
        {
            mode = default;
            return false;
        }

        if ((windowsAttr & FILE_ATTRIBUTE_UNIX_EXTENSION) > 0)
        {
            // Use native Unix file mode information
            uint unixMode = windowsAttr >> 16;
            if ((unixMode & S_IFMT) == S_IFDIR)
            {
                unixMode |= (uint)(UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute);
            }
            mode = (UnixFileMode)(unixMode & 0xFFF);
        }
        else
        {
            // Derive Unix file mode from Windows file attributes as a best effort guess
            FileAttributes parsedAttr = (FileAttributes)(windowsAttr & 0x7FFF);
            mode = UnixFileMode.OtherRead | UnixFileMode.GroupRead | UnixFileMode.UserRead;

            if ((parsedAttr & FileAttributes.Directory) > 0)
            {
                mode |= UnixFileMode.OtherExecute | UnixFileMode.GroupExecute | UnixFileMode.UserExecute;
            }

            if ((parsedAttr & FileAttributes.ReadOnly) == 0)
            {
                mode |= UnixFileMode.UserWrite;
            }
        }
        return true;
    }

    public bool TryGetWindowsFileAttributes(out FileAttributes result)
    {
        _arc.Native.GetProperty(_index, PROPID.kpidAttrib, out var prop);
        if (prop.TryReadUInt32(out uint attr))
        {
            result = (FileAttributes)(attr & 0x7FFF); // 7z only supports attributes up to 0x7FFF
            return true;
        }
        result = default;
        return false;
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
