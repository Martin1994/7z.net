using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableISequentialOutStream
{
    public static ref VTableISequentialOutStream FromPointer(void** lpVtbl) => ref *(VTableISequentialOutStream*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ComObject*, byte*, uint, uint*, HRESULT> Write;
}

public unsafe class SequentialOutStreamProxy : ComProxy<SequentialOutStreamProxy, Stream>
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
            vTableIUnknown.QueryInterface = &ManagedQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableISequentialOutStream vTableIInStream = ref VTableISequentialOutStream.FromPointer(lpVtbl);
            vTableIInStream.Write = &ManagedWrite;
        }

        RegisterInterface(new Guid("23170F69-40C1-278A-0000-000300020000"), implementation => new SequentialOutStreamProxy(implementation));
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
    private static HRESULT ManagedWrite(ComObject* that, byte* data, uint size, uint* processedSize)
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
            proxy._implementation.Write(new Span<byte>(data, (int)size));
            *processedSize = size;
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            *processedSize = 0;
            return proxy.PersistAndExtractException(e);
        }
    }

    public SequentialOutStreamProxy(Stream implementation) : base(implementation)
    {
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
