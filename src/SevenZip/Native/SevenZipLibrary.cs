using System.Runtime.InteropServices;

namespace SevenZip.Native;

public class SevenZipLibrary
{
    private const string LIB = "7z";

    [DllImport(LIB)]
    public extern static HRESULT GetNumberOfMethods(out uint numMethods);

    [DllImport(LIB)]
    public extern static HRESULT GetMethodProperty(uint index, NMethodPropID propID, out PROPVARIANT value);

    [DllImport(LIB)]
    public extern static HRESULT GetNumberOfFormats(out uint numFormats);

    [DllImport(LIB)]
    public extern static HRESULT GetHandlerProperty2(uint index, NHandlerPropID propID, out PROPVARIANT value);

    [DllImport(LIB)]
    public extern static HRESULT CreateObject(in CLSID clsid, in Guid iid, out IntPtr outObject);

    public static unsafe T* CreateObject<T>(in CLSID clsid) where T : unmanaged
    {
        IntPtr obj;
        CreateObject(clsid, typeof(T).GUID, out obj).EnsureSuccess();
        return (T*)obj;
    }

    [DllImport(LIB)]
    public unsafe extern static void* SysAllocStringLen(byte* sz, uint len);

    private static readonly Lazy<string?> _initialized = new(Install);

    /// <summary>
    /// Find the 7-Zip shared library file, using the default file name produced by 7z source code.
    /// Call it before the first P/Invoke.
    /// </summary>
    ///
    /// <remarks>
    /// The 7z source code produces <c>7z.dll</c> on Windows, and <c>7z.so</c> elsewhere. For a
    /// library under any other name, install your own <see cref="NativeLibrary.SetDllImportResolver"/>
    /// instead of calling this.
    ///
    /// The default probing is not enough: on macOS the runtime probes only
    /// <c>lib7z</c>, <c>lib7z.dylib</c> and <c>7z.dylib</c>, never <c>7z.so</c>; and the hardened
    /// runtime strips <c>DYLD_*</c>, so the location cannot come from the environment either.
    /// On Linux, distribution packages install <c>7z.so</c> in private directories outside the
    /// default loader search path. After the application and process directories, probe
    /// <c>/usr/lib/7zip</c> (Ubuntu, Arch) and <c>/usr/libexec/7zip</c> (Fedora).
    /// If none of these directories contains the library, fall back to the runtime's default search.
    /// </remarks>
    ///
    /// <returns>
    /// The path found in the explicit search directories, or <c>null</c> if none was found there.
    /// The runtime's default search is deferred until the first P/Invoke.
    /// </returns>
    public static string? Initialize() => _initialized.Value;

    private static string? Install()
    {
        var path = Find();

        NativeLibrary.SetDllImportResolver(typeof(SevenZipLibrary).Assembly, (libraryName, _, _) =>
        {
            if (!string.Equals(libraryName, LIB, StringComparison.Ordinal))
            {
                return IntPtr.Zero;
            }

            var resolved = path ?? Find();

            return resolved is null
                ? IntPtr.Zero
                : NativeLibrary.Load(resolved);
        });

        return path;
    }

    private static string DefaultFileName => OperatingSystem.IsWindows() ? "7z.dll" : "7z.so";

    private static IEnumerable<string> Directories()
    {
        yield return AppContext.BaseDirectory;

        if (Path.GetDirectoryName(Environment.ProcessPath) is { Length: > 0 } processDirectory)
        {
            yield return processDirectory;
        }

        if (OperatingSystem.IsLinux())
        {
            yield return "/usr/lib/7zip";
            yield return "/usr/libexec/7zip";
        }
    }

    private static string? Find() =>
        Directories()
            .Distinct()
            .Select(directory => Path.Combine(directory, DefaultFileName))
            .FirstOrDefault(File.Exists);
}

public struct CLSID {
    public Guid Guid;
}
