
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native.Com;

[Guid("23170F69-40C1-278A-0000-000300030000")]
public unsafe struct IInStream
{
    public void** lpVtbl;
    // Used by managed implementation
    public long id;
}

public unsafe struct VTableIInStream
{
    public static ref VTableIInStream FromPointer(void** lpVtbl) => ref *(VTableIInStream*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<IInStream*, byte*, uint, uint*, HRESULT> Read;
    public delegate* unmanaged<IInStream*, long, uint, ulong*, HRESULT> Seek;
}

public unsafe struct VTableManagedInStream
{
    private fixed long _methods[6];
}

public unsafe class ManagedInStream : IDisposable
{
    private static readonly ConcurrentDictionary<long, Stream> _idRef = new();
    [FixedAddressValueType]
    private static readonly VTableManagedInStream _lpVtbl;
    private static long _nextId = 0;

    [UnmanagedCallersOnly]
    private static HRESULT ManagedRead(IInStream* that, byte* data, uint size, uint* processedSize)
    {
        Stream? implementation;
        if (!_idRef.TryGetValue(that->id, out implementation))
        {
            return HRESULT.E_INVALIDARG;
        }
        if (size >= 0x80000000) {
            return HRESULT.E_INVALIDARG;
        }
        *processedSize = (uint)implementation.Read(new Span<byte>(data, (int)size));
        return HRESULT.S_OK;
    }

    [UnmanagedCallersOnly]
    private static HRESULT ManagedSeek(IInStream* that, long offset, uint seekOrigin, ulong* newPosition)
    {
        Stream? implementation;
        if (!_idRef.TryGetValue(that->id, out implementation))
        {
            return HRESULT.E_INVALIDARG;
        }
        ulong newPos = (ulong)implementation.Seek(offset, (SeekOrigin)seekOrigin);
        if (newPosition != null) {
            *newPosition = newPos;
        }
        return HRESULT.S_OK;
    }

    static ManagedInStream()
    {
        fixed (VTableManagedInStream* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableIInStream vTableIInStream = ref VTableIInStream.FromPointer(lpVtbl);
            vTableIInStream.Read = &ManagedRead;
            vTableIInStream.Seek = &ManagedSeek;
        }
    }

    private readonly Stream _implementation;
    private readonly long _id;
    private readonly IInStream* _com;
    public ref IInStream ComObject => ref *_com;
    private bool _disposedValue;

    public ManagedInStream(Stream implementation)
    {
        this._implementation = implementation;
        _id = Interlocked.Add(ref _nextId, 1);
        _idRef.TryAdd(_id, implementation);
        this._com = (IInStream*)Marshal.AllocHGlobal(sizeof(IInStream));
        fixed (VTableManagedInStream* lpVtbl = &_lpVtbl)
        {
            this._com->lpVtbl = (void**)lpVtbl;
        }
        this._com->id = _id;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _idRef.Remove(this._id, out _);
            Marshal.FreeHGlobal((nint)this._com);
            _disposedValue = true;
        }
    }

    ~ManagedInStream()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
