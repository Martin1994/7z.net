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
}

public struct CLSID {
    public Guid Guid;
}
