using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public interface IManagedComProxy : IDisposable
{
    unsafe void* ComPointer { get; }
}

public abstract unsafe class ManagedComProxy<TSelf, TCom, TImplementation> : IManagedComProxy where TSelf : ManagedComProxy<TSelf, TCom, TImplementation> where TCom : unmanaged
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
    void* IManagedComProxy.ComPointer => _com;
    protected readonly TImplementation _implementation;
    private bool _disposedValue;
    private List<Exception>? _pendingExceptions = null;
    private Dictionary<Guid, IManagedComProxy?>? _queriedProxies = null; // need to keep the managed references of the queried interfaces to prevent them being garbage collected

    public ManagedComProxy(TImplementation implementation)
    {
        _id = Interlocked.Add(ref _nextId, 1);
        _idRef.TryAdd(_id, (TSelf)this);
        this._com = (TCom*)Marshal.AllocHGlobal(sizeof(TCom));
        _implementation = implementation;
    }

    protected unsafe void QueryInterface(Guid* iid, void** outObject)
    {
        if (iid == null)
        {
            throw new InvalidOperationException("Cannot query interface with null IID.");
        }

        _queriedProxies ??= new();

        if (!_queriedProxies.TryGetValue(*iid, out var proxy))
        {
            proxy = ManagedComProxyRegistry.QueryInterface(*iid, _implementation);
            _queriedProxies[*iid] = proxy;
        }

        if (proxy == null)
        {
            *outObject = null;
            throw new NotImplementedException();
        }
        *outObject = proxy.ComPointer;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_queriedProxies != null)
                {
                    foreach (var proxy in _queriedProxies.Values)
                    {
                        proxy?.Dispose();
                    }
                }
            }
            _idRef.Remove(this._id, out _);
            Marshal.FreeHGlobal((nint)this._com);
            _disposedValue = true;
        }
    }

    protected HRESULT PersistAndExtractException(Exception ex)
    {
        _pendingExceptions ??= new();
        _pendingExceptions.Add(ex);
        return (HRESULT)ex.HResult;
    }

    public void ThrowPendingException()
    {
        var ex = PopPendingException();
        if (ex != null)
        {
            throw ex;
        }
    }

    public Exception? PopPendingException()
    {
        if (_pendingExceptions == null)
        {
            return null;
        }

        Exception ex;
        if (_pendingExceptions.Count == 1)
        {
            ex = _pendingExceptions[0];
        }
        else
        {
            ex = new AggregateException(_pendingExceptions);
        }
        _pendingExceptions = null;
        return ex;
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

public static class ManagedComProxyRegistry
{
    private static readonly Dictionary<Guid, Func<object, IManagedComProxy?>> _registry = new();

    public static void Register<TImplementation>(Guid iid, Func<TImplementation, IManagedComProxy> creator) where TImplementation : class
    {
        if (_registry.ContainsKey(iid))
        {
            throw new ManagedComProxyRegistryException($"Managed COM Proxy for IID ${iid} already exists");
        }
        _registry[iid] = implementation =>
        {
            TImplementation? newInterfaceImpl = implementation as TImplementation;
            if (newInterfaceImpl is null)
            {
                return null;
            }
            return creator(newInterfaceImpl);
        };
    }

    public static IManagedComProxy? QueryInterface(Guid iid, object implementation)
    {
        if (!_registry.TryGetValue(iid, out var creator))
        {
            return null;
        }

        return creator(implementation);
    }
}

public class ManagedComProxyRegistryException : Exception
{
    public ManagedComProxyRegistryException(string message): base(message)
    { }
}
