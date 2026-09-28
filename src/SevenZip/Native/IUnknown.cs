using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableIUnknown
{
    public const int P7ZIP_COM_DESCTRUCTOR_COUNT = 2;

    // No longer supports p7zip, so this is always false.
    public const bool Z7_USE_VIRTUAL_DESTRUCTOR_IN_IUNKNOWN = false;

    public static readonly int vTableOffset;

    public static ref VTableIUnknown FromPointer(void** lpVtbl) => ref *(VTableIUnknown*)(lpVtbl);

    [UnmanagedCallersOnly]
    public static HRESULT NoopQueryInterface(void* that, Guid* iid, void** outObject) => HRESULT.E_NOTIMPL;

    [UnmanagedCallersOnly]
    public static uint NoopAddRef(void* that) => 1;

    [UnmanagedCallersOnly]
    public static uint NoopRelease(void* that) => 0;

    static VTableIUnknown()
    {
        vTableOffset = sizeof(VTableIUnknown) / sizeof(void**);
        if (Z7_USE_VIRTUAL_DESTRUCTOR_IN_IUNKNOWN && !RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Destructors are ahead of other methods on non-Windows builds
            vTableOffset += P7ZIP_COM_DESCTRUCTOR_COUNT;
        }
    }

    public delegate* unmanaged<void*, Guid*, void**, HRESULT> QueryInterface;
    public delegate* unmanaged<void*, uint> AddRef;
    public delegate* unmanaged<void*, uint> Release;
}
