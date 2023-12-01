using SevenZip.Native;

namespace SevenZip;

public class SevenZipArchiveFormat
{
    private static readonly Dictionary<string, SevenZipArchiveFormat> _nameToHandlerMap = new();
    private static readonly Dictionary<string, List<SevenZipArchiveFormat>> _extToHandlerMap = new();

    unsafe static SevenZipArchiveFormat()
    {
        uint num;
        PROPVARIANT value;

        SevenZipLibrary.GetNumberOfFormats(out num).EnsureSuccess();
        for (uint i = 0; i < num; i++)
        {
            SevenZipLibrary.GetHandlerProperty2(i, NHandlerPropID.kClassID, out value).EnsureSuccess();
            CLSID* classIdPtr = value.ReadPointer<CLSID>();
            if (classIdPtr == null)
            {
                continue;
            }
            CLSID classId = *classIdPtr;

            SevenZipLibrary.GetHandlerProperty2(i, NHandlerPropID.kName, out value).EnsureSuccess();
            string name = value.ReadString();

            SevenZipArchiveFormat format = new(
                name: name,
                classId: classId
            );

            SevenZipLibrary.GetHandlerProperty2(i, NHandlerPropID.kExtension, out value).EnsureSuccess();
            string[] extList = value.ReadString().Split(" ");

            SevenZipLibrary.GetHandlerProperty2(i, NHandlerPropID.kAddExtension, out value).EnsureSuccess();
            string? rawAddExt = value.ReadOptionalString();
            string[] addExtList = rawAddExt == null ? extList.Select(_ => "*").ToArray() : rawAddExt.Split(" ");

            foreach ((string ext, string addExt) in extList.Zip(addExtList))
            {
                RegisterFormatByExtension($".{ext}", format);
                if (addExt != "*")
                {
                    RegisterFormatByExtension($".{addExt}.{ext}", format);
                }
            }
        }
    }

    private static void RegisterFormatByExtension(string ext, SevenZipArchiveFormat format)
    {
        List<SevenZipArchiveFormat>? list;
        if (!_extToHandlerMap.TryGetValue(ext, out list))
        {
            list = new();
            _extToHandlerMap.Add(ext, list);
        }
        list.Add(format);
    }

    public static IReadOnlyList<SevenZipArchiveFormat> FromPath(string path)
    {
        string ext = Path.GetExtension(path);
        string addExt = Path.GetExtension(Path.GetFileNameWithoutExtension(path));

        List<SevenZipArchiveFormat> formats = new();

        if (_extToHandlerMap.TryGetValue(ext, out var knownList))
        {
            formats.AddRange(knownList);
        }

        if (addExt != "" && _extToHandlerMap.TryGetValue(addExt + ext, out knownList))
        {
            formats.AddRange(knownList);
        }

        return formats;
    }

    public string Name { get; }
    public CLSID ClassId { get; }

    public SevenZipArchiveFormat(string name, CLSID classId)
    {
        this.Name = name;
        this.ClassId = classId;
    }
}
