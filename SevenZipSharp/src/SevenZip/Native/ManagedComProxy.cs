using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public abstract unsafe class ManagedComProxy<T> : IDisposable where T : unmanaged
{
    private static readonly ConcurrentDictionary<long, ManagedComProxy<T>> _idRef = new();
    private static long _nextId = 0;

    protected static bool TryGetProxy(long id, [MaybeNullWhen(false)] out ManagedComProxy<T> wrapper)
    {
        return _idRef.TryGetValue(id, out wrapper);
    }

    protected readonly long _id;
    private readonly T* _com;
    public ref T ComObject => ref *_com;
    private bool _disposedValue;

    public ManagedComProxy()
    {
        _id = Interlocked.Add(ref _nextId, 1);
        _idRef.TryAdd(_id, this);
        this._com = (T*)Marshal.AllocHGlobal(sizeof(T));
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

    ~ManagedComProxy()
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