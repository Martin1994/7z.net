using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

[Guid("23170F69-40C1-278A-0000-000600200000")]
public interface IArchiveExtractCallback
{
    void SetTotal(ulong size);
    void SetCompleted(in ulong size);
    Stream? GetStream(uint index, NAskMode askExtractMode);
    void PrepareOperation(NAskMode askExtractMode);
    void SetOperationResult(NOperationResult opRes);
}

public unsafe struct VTableIArchiveExtractCallback
{
    public static ref VTableIArchiveExtractCallback FromPointer(void** lpVtbl) => ref *(VTableIArchiveExtractCallback*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ComObject*, ulong, HRESULT> SetTotal;
    public delegate* unmanaged<ComObject*, ulong*, HRESULT> SetCompleted;
    public delegate* unmanaged<ComObject*, uint, ComObject**, NAskMode, HRESULT> GetStream;
    public delegate* unmanaged<ComObject*, NAskMode, HRESULT> PrepareOperation;
    public delegate* unmanaged<ComObject*, NOperationResult, HRESULT> SetOperationResult;
}

public unsafe class ArchiveExtractCallbackProxy : ComProxy<ArchiveExtractCallbackProxy, IArchiveExtractCallback>, IDisposable
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
            vTableIUnknown.QueryInterface = &ManagedQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableIArchiveExtractCallback vTableIArchiveExtractCallback = ref VTableIArchiveExtractCallback.FromPointer(lpVtbl);
            vTableIArchiveExtractCallback.SetTotal = &ManagedSetTotal;
            vTableIArchiveExtractCallback.SetCompleted = &ManagedSetCompleted;
            vTableIArchiveExtractCallback.GetStream = &ManagedGetStream;
            vTableIArchiveExtractCallback.PrepareOperation = &ManagedPrepareOperation;
            vTableIArchiveExtractCallback.SetOperationResult = &ManagedSetOperationResult;
        }

        RegisterInterface(implementation => new ArchiveExtractCallbackProxy(implementation));
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
    private static HRESULT ManagedSetTotal(ComObject* that, ulong size)
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
            return proxy.PersistAndExtractException(e);
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSetCompleted(ComObject* that, ulong* size)
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
            return proxy.PersistAndExtractException(e);
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedGetStream(ComObject* that, uint index, ComObject** outStream, NAskMode askExtractMode)
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
                fixed (ComObject* comPtr = &proxy._currentStreamProxy.ComObject)
                {
                    *outStream = comPtr;
                }
            }

            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return proxy.PersistAndExtractException(e);
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedPrepareOperation(ComObject* that, NAskMode askExtractMode)
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
            return proxy.PersistAndExtractException(e);
        }
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSetOperationResult(ComObject* that, NOperationResult opRes)
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
            return proxy.PersistAndExtractException(e);
        }
    }

    private SequentialOutStreamProxy? _currentStreamProxy;
    private bool _disposedValue;

    public ArchiveExtractCallbackProxy(IArchiveExtractCallback implementation) : base(implementation)
    {
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
