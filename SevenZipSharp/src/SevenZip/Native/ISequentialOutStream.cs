using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

[Guid("23170F69-40C1-278A-0000-000300020000")]
public unsafe struct ISequentialOutStream
{
    public void** lpVtbl;
    // Used by proxy
    public long id;
}

public unsafe struct VTableISequentialOutStream
{
    public static ref VTableISequentialOutStream FromPointer(void** lpVtbl) => ref *(VTableISequentialOutStream*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ISequentialOutStream*, byte*, uint, uint*, HRESULT> Write;
}

public unsafe class SequentialOutStreamProxy : ManagedComProxy<SequentialOutStreamProxy, ISequentialOutStream>
{
    private struct ManagedVTable
    {
        private void* _m1, _m2, _m3, _m4, _m5, _m6;
    }

    [FixedAddressValueType]
    private static readonly ManagedVTable _lpVtbl;

    static SequentialOutStreamProxy()
    {
        fixed (void* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.QueryInterface = &VTableIUnknown.NoopQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableISequentialOutStream vTableIInStream = ref VTableISequentialOutStream.FromPointer(lpVtbl);
            vTableIInStream.Write = &ManagedWrite;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedWrite(ISequentialOutStream* that, byte* data, uint size, uint* processedSize)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        if (size >= 0x80000000) {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            proxy._implementation.Write(new Span<byte>(data, (int)size));
            *processedSize = size;
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            *processedSize = 0;
            return (HRESULT)e.HResult;
        }
    }

    private readonly Stream _implementation;

    public SequentialOutStreamProxy(Stream implementation)
    {
        _implementation = implementation;
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
