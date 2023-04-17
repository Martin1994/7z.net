using System.Runtime.InteropServices;

namespace SevenZip.Native.Com;

public unsafe struct VTableIUnknown
{
    public static readonly int vTableOffset;
    
    public static ref VTableIUnknown FromPointer(void** lpVtbl) => ref *(VTableIUnknown*)(lpVtbl);

    [UnmanagedCallersOnly]
    public static uint NoopAddRef(void* that) => 1;

    [UnmanagedCallersOnly]
    public static uint NoopRelease(void* that) => 0;

    static VTableIUnknown() {
        vTableOffset = sizeof(VTableIUnknown) / sizeof(void**);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Destructors are ahead of other methods on non-Windows builds
            vTableOffset += 2;
        }
    }

    public delegate* unmanaged<void*, Guid*, void**, uint> QueryInterface;
    public delegate* unmanaged<void*, uint> AddRef;
    public delegate* unmanaged<void*, uint> Release;
}
