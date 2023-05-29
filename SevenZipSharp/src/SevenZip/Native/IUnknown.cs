using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableIUnknown
{
    public const int UNIX_COM_DESCTRUCTOR_COUNT = 2;

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
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Destructors are ahead of other methods on non-Windows builds
            vTableOffset += UNIX_COM_DESCTRUCTOR_COUNT;
        }
    }

    public delegate* unmanaged<void*, Guid*, void**, HRESULT> QueryInterface;
    public delegate* unmanaged<void*, uint> AddRef;
    public delegate* unmanaged<void*, uint> Release;
}
