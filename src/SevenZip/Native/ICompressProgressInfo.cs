using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableICompressProgressInfo
{
    public static ref VTableICompressProgressInfo FromPointer(void** lpVtbl) => ref *(VTableICompressProgressInfo*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ComObject*, ulong*, ulong*, HRESULT> SetRatioInfo;
}

public unsafe class CompressProgressInfoProxy : ComProxy<CompressProgressInfoProxy, ICompressProgressInfo>
{
    private struct ManagedVTable
    {
        private void* _m1, _m2, _m3, _m4, _m5, _m6;
    }

    [FixedAddressValueType]
    private static readonly ManagedVTable _lpVtbl;

    static CompressProgressInfoProxy()
    {
        fixed (void* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.QueryInterface = &ManagedQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableICompressProgressInfo vTableICompressProgressInfo = ref VTableICompressProgressInfo.FromPointer(lpVtbl);
            vTableICompressProgressInfo.SetRatioInfo = &ManagedSetRatioInfo;
        }

        RegisterInterface(implementation => new CompressProgressInfoProxy(implementation));
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
    private static HRESULT ManagedSetRatioInfo(ComObject* that, ulong* inSize, ulong* outSize)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            proxy._implementation.SetRatioInfo(in *inSize, in *outSize);
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return proxy.PersistAndExtractException(e);
        }
    }

    public CompressProgressInfoProxy(ICompressProgressInfo implementation) : base(implementation)
    {
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
