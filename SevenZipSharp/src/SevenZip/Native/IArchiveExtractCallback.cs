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
    public delegate* unmanaged<IArchiveExtractCallback*, uint, ISequentialOutStream**, NAskMode, HRESULT> GetStream;
    public delegate* unmanaged<IArchiveExtractCallback*, NAskMode, HRESULT> PrepareOperation;
    public delegate* unmanaged<IArchiveExtractCallback*, NOperationResult, HRESULT> SetOperationResult;
}

public interface IManagedArchiveExtractCallback
{
    void SetTotal(ulong size);
    void SetCompleted(in ulong size);
    Stream? GetStream(uint index, NAskMode askExtractMode);
    void PrepareOperation(NAskMode askExtractMode);
    void SetOperationResult(NOperationResult opRes);
}

public unsafe class ArchiveExtractCallbackProxy : ManagedComProxy<ArchiveExtractCallbackProxy, IArchiveExtractCallback>, IDisposable
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
            proxy._implementation.SetTotal(size);
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return (HRESULT)e.HResult;
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
            proxy._implementation.SetCompleted(in *size);
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return (HRESULT)e.HResult;
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedGetStream(IArchiveExtractCallback* that, uint index, ISequentialOutStream** outStream, NAskMode askExtractMode)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            Stream? stream = proxy._implementation.GetStream(index, askExtractMode);
            if (stream == null)
            {
                *outStream = null;
            }
            else
            {
                proxy._currentStreamProxy?.Dispose();
                proxy._currentStreamProxy = new SequentialOutStreamProxy(stream);
                fixed (ISequentialOutStream* comPtr = &proxy._currentStreamProxy.ComObject)
                {
                    *outStream = comPtr;
                }
            }

            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return (HRESULT)e.HResult;
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
            proxy._implementation.PrepareOperation(askExtractMode);
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return (HRESULT)e.HResult;
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
            proxy._implementation.SetOperationResult(opRes);
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return (HRESULT)e.HResult;
        }
    }

    private readonly IManagedArchiveExtractCallback _implementation;
    private SequentialOutStreamProxy? _currentStreamProxy;
    private bool _disposedValue;

    public ArchiveExtractCallbackProxy(IManagedArchiveExtractCallback implementation)
    {
        _implementation = implementation;
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _currentStreamProxy?.Dispose();
                _currentStreamProxy = null;
            }

            _disposedValue = true;
        }
        base.Dispose(disposing);
    }
}
