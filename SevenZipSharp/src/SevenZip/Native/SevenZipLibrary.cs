using SevenZip.Native.Com;
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
}

public enum NMethodPropID
{
    kID,
    kName,
    kDecoder,
    kEncoder,
    kPackStreams,
    kUnpackStreams,
    kDescription,
    kDecoderIsAssigned,
    kEncoderIsAssigned,
    kDigestSize,
    kIsFilter
}

public enum NHandlerPropID
{
    kName = 0,        // VT_BSTR
    kClassID,         // binary GUID in VT_BSTR
    kExtension,       // VT_BSTR
    kAddExtension,    // VT_BSTR
    kUpdate,          // VT_BOOL
    kKeepName,        // VT_BOOL
    kSignature,       // binary in VT_BSTR
    kMultiSignature,  // binary in VT_BSTR
    kSignatureOffset, // VT_UI4
    kAltStreams,      // VT_BOOL
    kNtSecure,        // VT_BOOL
    kFlags,           // VT_UI4
    kTimeFlags        // VT_UI4
}

public struct CLSID {
    public Guid Guid;
}
