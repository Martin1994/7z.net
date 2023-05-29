using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableIInStream
{
    public static ref VTableIInStream FromPointer(void** lpVtbl) => ref *(VTableIInStream*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ComObject*, byte*, uint, uint*, HRESULT> Read;
    public delegate* unmanaged<ComObject*, long, uint, ulong*, HRESULT> Seek;
}

public unsafe class InStreamProxy : ComProxy<InStreamProxy, Stream>
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
            vTableIUnknown.QueryInterface = &ManagedQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableIInStream vTableIInStream = ref VTableIInStream.FromPointer(lpVtbl);
            vTableIInStream.Read = &ManagedRead;
            vTableIInStream.Seek = &ManagedSeek;
        }

        RegisterInterface(new Guid("23170F69-40C1-278A-0000-000300030000"), implementation => new InStreamProxy(implementation));
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedQueryInterface(void* that, Guid* iid, void** outObject)
    {
        if (!TryGetProxy(((ComObject*)that)->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }

        return proxy.QueryInterface(iid, outObject);
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedRead(ComObject* that, byte* data, uint size, uint* processedSize)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        if (size >= 0x80000000)
        {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            *processedSize = (uint)proxy._implementation.Read(new Span<byte>(data, (int)size));
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            *processedSize = 0;
            return proxy.PersistAndExtractException(e);
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSeek(ComObject* that, long offset, uint seekOrigin, ulong* newPosition)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            ulong newPos = (ulong)proxy._implementation.Seek(offset, (SeekOrigin)seekOrigin);
            if (newPosition != null)
            {
                *newPosition = newPos;
            }
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return proxy.PersistAndExtractException(e);
        }
    }

    public InStreamProxy(Stream implementation) : base(implementation)
    {
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
