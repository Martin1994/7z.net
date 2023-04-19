using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

[Guid("23170F69-40C1-278A-0000-000300030000")]
public unsafe struct IInStream
{
    public void** lpVtbl;
    // Used by proxy
    public long id;
}

public unsafe struct VTableIInStream
{
    public static ref VTableIInStream FromPointer(void** lpVtbl) => ref *(VTableIInStream*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<IInStream*, byte*, uint, uint*, HRESULT> Read;
    public delegate* unmanaged<IInStream*, long, uint, ulong*, HRESULT> Seek;
}

public unsafe class InStreamProxy : ManagedComProxy<IInStream>
{
    private struct ManagedVTable
    {
        private void* _m1, _m2, _m3, _m4, _m5, _m6, _m7;
    }

    [FixedAddressValueType]
    private static readonly ManagedVTable _lpVtbl;

    static InStreamProxy()
    {
        fixed (void* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.QueryInterface = &VTableIUnknown.NoopQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableIInStream vTableIInStream = ref VTableIInStream.FromPointer(lpVtbl);
            vTableIInStream.Read = &ManagedRead;
            vTableIInStream.Seek = &ManagedSeek;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedRead(IInStream* that, byte* data, uint size, uint* processedSize)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        if (size >= 0x80000000) {
            return HRESULT.E_INVALIDARG;
        }
        *processedSize = (uint)((InStreamProxy)proxy)._implementation.Read(new Span<byte>(data, (int)size));
        return HRESULT.S_OK;
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSeek(IInStream* that, long offset, uint seekOrigin, ulong* newPosition)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        ulong newPos = (ulong)((InStreamProxy)proxy)._implementation.Seek(offset, (SeekOrigin)seekOrigin);
        if (newPosition != null) {
            *newPosition = newPos;
        }
        return HRESULT.S_OK;
    }

    private readonly Stream _implementation;

    public InStreamProxy(Stream implementation)
    {
        _implementation = implementation;
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
