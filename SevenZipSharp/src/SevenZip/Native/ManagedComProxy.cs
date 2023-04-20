using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public abstract unsafe class ManagedComProxy<TSelf, TCom> : IDisposable where TSelf : ManagedComProxy<TSelf, TCom> where TCom : unmanaged
{
    private static readonly ConcurrentDictionary<long, TSelf> _idRef = new();
    private static long _nextId = 0;

    protected static bool TryGetProxy(long id, [MaybeNullWhen(false)] out TSelf proxy)
    {
        return _idRef.TryGetValue(id, out proxy);
    }

    protected readonly long _id;
    private readonly TCom* _com;
    public ref TCom ComObject => ref *_com;
    private bool _disposedValue;

    public ManagedComProxy()
    {
        _id = Interlocked.Add(ref _nextId, 1);
        _idRef.TryAdd(_id, (TSelf)this);
        this._com = (TCom*)Marshal.AllocHGlobal(sizeof(TCom));
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