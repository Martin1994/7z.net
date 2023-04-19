using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

[Guid("23170F69-40C1-278A-0000-000600200000")]
public unsafe struct IArchiveExtractCallback
{
    public void** lpVtbl;
    // Used by proxy
    public long id;
}

public unsafe struct VTableIArchiveExtractCallback
{
    public static ref VTableIArchiveExtractCallback FromPointer(void** lpVtbl) => ref *(VTableIArchiveExtractCallback*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<IArchiveExtractCallback*, ulong, HRESULT> SetTotal;
    public delegate* unmanaged<IArchiveExtractCallback*, ulong*, HRESULT> SetCompleted;
    public delegate* unmanaged<IArchiveExtractCallback*, uint, void**, NAskMode, HRESULT> GetStream;
    public delegate* unmanaged<IArchiveExtractCallback*, NAskMode, HRESULT> PrepareOperation;
    public delegate* unmanaged<IArchiveExtractCallback*, NOperationResult, HRESULT> SetOperationResult;
}

public interface IManagedArchiveExtractCallback
{
    void SetTotal(ulong size);
    void SetCompleted(in ulong size);
    unsafe void GetStream(uint index, out void* outStream, NAskMode askExtractMode);
    void PrepareOperation(NAskMode askExtractMode);
    void SetOperationResult(NOperationResult opRes);
}

public unsafe class ArchiveExtractCallbackProxy : ManagedComProxy<IArchiveExtractCallback>
{
    private struct ManagedVTable
    {
        private void* _m1, _m2, _m3, _m4, _m5, _m6, _m7, _m8, _m9, _m10;
    }

    [FixedAddressValueType]
    private static readonly ManagedVTable _lpVtbl;

    static ArchiveExtractCallbackProxy()
    {
        fixed (void* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.QueryInterface = &VTableIUnknown.NoopQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableIArchiveExtractCallback vTableIArchiveExtractCallback = ref VTableIArchiveExtractCallback.FromPointer(lpVtbl);
            vTableIArchiveExtractCallback.SetTotal = &ManagedSetTotal;
            vTableIArchiveExtractCallback.SetCompleted = &ManagedSetCompleted;
            vTableIArchiveExtractCallback.GetStream = &ManagedGetStream;
            vTableIArchiveExtractCallback.PrepareOperation = &ManagedPrepareOperation;
            vTableIArchiveExtractCallback.SetOperationResult = &ManagedSetOperationResult;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSetTotal(IArchiveExtractCallback* that, ulong size)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        
        try
        {
            ((ArchiveExtractCallbackProxy)proxy)._implementation.SetTotal(size);
            return HRESULT.S_OK;
        }
        catch (NotImplementedException)
        {
            return HRESULT.E_NOTIMPL;
        }
        catch (Exception)
        {
            return HRESULT.E_FAIL;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSetCompleted(IArchiveExtractCallback* that, ulong* size)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        
        try
        {
            ((ArchiveExtractCallbackProxy)proxy)._implementation.SetCompleted(in *size);
            return HRESULT.S_OK;
        }
        catch (NotImplementedException)
        {
            return HRESULT.E_NOTIMPL;
        }
        catch (Exception)
        {
            return HRESULT.E_FAIL;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedGetStream(IArchiveExtractCallback* that, uint index, void** outStream, NAskMode askExtractMode)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        
        try
        {
            ((ArchiveExtractCallbackProxy)proxy)._implementation.GetStream(index, out *outStream, askExtractMode);
            return HRESULT.S_OK;
        }
        catch (NotImplementedException)
        {
            return HRESULT.E_NOTIMPL;
        }
        catch (Exception)
        {
            return HRESULT.E_FAIL;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedPrepareOperation(IArchiveExtractCallback* that, NAskMode askExtractMode)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        
        try
        {
            ((ArchiveExtractCallbackProxy)proxy)._implementation.PrepareOperation(askExtractMode);
            return HRESULT.S_OK;
        }
        catch (NotImplementedException)
        {
            return HRESULT.E_NOTIMPL;
        }
        catch (Exception)
        {
            return HRESULT.E_FAIL;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSetOperationResult(IArchiveExtractCallback* that, NOperationResult opRes)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }
        
        try
        {
            ((ArchiveExtractCallbackProxy)proxy)._implementation.SetOperationResult(opRes);
            return HRESULT.S_OK;
        }
        catch (NotImplementedException)
        {
            return HRESULT.E_NOTIMPL;
        }
        catch (Exception)
        {
            return HRESULT.E_FAIL;
        }
    }

    private readonly IManagedArchiveExtractCallback _implementation;

    public ArchiveExtractCallbackProxy(IManagedArchiveExtractCallback implementation)
    {
        _implementation = implementation;
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
