using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct ComObject
{
    public void** lpVtbl;
    public long id;
}

public interface IComProxy : IDisposable
{
    unsafe void* ComPointer { get; }
    IEnumerable<Exception> PendingExceptions { get; }
    void ClearPendingExceptions();
}

// A ComProxy is needed when a COM object needs to be constructed in .NET code and then passed
// to unmanaged code. It converts a managed implementation to a COM object pointer and manages
// their life cycle.
public abstract unsafe class ComProxy<TSelf, TImplementation> : IComProxy
    where TSelf : ComProxy<TSelf, TImplementation>
    where TImplementation : class
{
    private static readonly ConcurrentDictionary<long, TSelf> _idRef = new();
    private static long _nextId = 0;

    protected static bool TryGetProxy(long id, [MaybeNullWhen(false)] out TSelf proxy)
    {
        return _idRef.TryGetValue(id, out proxy);
    }

    protected static void RegisterInterface(Func<TImplementation, IComProxy> creator) => RegisterInterface(typeof(TImplementation).GUID, creator);

    protected static void RegisterInterface(Guid guid, Func<TImplementation, IComProxy> creator)
    {
        ManagedComProxyRegistry.Register(guid, creator);
    }

    protected readonly long _id;

    private readonly ComObject* _com;
    public ref ComObject ComObject => ref *_com;
    void* IComProxy.ComPointer => _com;

    protected readonly TImplementation _implementation;
    private bool _disposedValue;
    private List<Exception>? _pendingExceptions = null;
    private Dictionary<Guid, IComProxy?>? _queriedProxies = null; // need to keep the managed references of the queried interfaces to prevent them being garbage collected

    public ComProxy(TImplementation implementation)
    {
        _id = Interlocked.Add(ref _nextId, 1);
        _idRef.TryAdd(_id, (TSelf)this);
        this._com = (ComObject*)Marshal.AllocHGlobal(sizeof(ComObject));
        _implementation = implementation;
    }

    protected unsafe HRESULT QueryInterface(Guid* iid, void** outObject)
    {
        try
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
                #if DEBUG
                Console.WriteLine("Interface {0} is not implemented for {1}.", *iid, _implementation.GetType().Name);
                #endif
                *outObject = null;
                throw new NotImplementedException();
            }
            *outObject = proxy.ComPointer;
            return HRESULT.S_OK;
        }
        catch (NotImplementedException e)
        {
            return (HRESULT)e.HResult;
        }
        catch (Exception e)
        {
            return PersistAndExtractException(e);
        }
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

    IEnumerable<Exception> IComProxy.PendingExceptions
    {
        get
        {
            IEnumerable<Exception> selfExceptions;
            if (_pendingExceptions == null)
            {
                selfExceptions = Enumerable.Empty<Exception>();
            }
            else
            {
                selfExceptions = _pendingExceptions;
            }

            var queriedProxies = _queriedProxies?.Values ?? Enumerable.Empty<IComProxy?>();
            return selfExceptions.Concat(queriedProxies.SelectMany(proxy => proxy?.PendingExceptions ?? Enumerable.Empty<Exception>()));
        }
    }

    void IComProxy.ClearPendingExceptions()
    {
        _pendingExceptions = null;
        foreach (var proxy in _queriedProxies?.Values ?? Enumerable.Empty<IComProxy?>())
        {
            proxy?.ClearPendingExceptions();
        }
    }

    public Exception? PopPendingException()
    {
        Exception[] pendingExceptions = ((IComProxy)this).PendingExceptions.ToArray();
        if (pendingExceptions.Length == 0)
        {
            return null;
        }

        Exception ex;
        if (pendingExceptions.Length == 1)
        {
            ex = pendingExceptions[0];
        }
        else
        {
            ex = new AggregateException(pendingExceptions);
        }
        ((IComProxy)this).ClearPendingExceptions();
        return ex;
    }

    ~ComProxy()
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

file static class ManagedComProxyRegistry
{
    private static readonly Dictionary<Guid, Func<object, IComProxy?>> _registry = new();

    public static void Register<TImplementation>(Guid iid, Func<TImplementation, IComProxy> creator) where TImplementation : class
    {
        if (_registry.ContainsKey(iid))
        {
            throw new ComProxyRegistryException($"Managed COM Proxy for IID ${iid} already exists");
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

    public static IComProxy? QueryInterface(Guid iid, object implementation)
    {
        if (!_registry.TryGetValue(iid, out var creator))
        {
            return null;
        }

        return creator(implementation);
    }
}

public class ComProxyRegistryException : Exception
{
    public ComProxyRegistryException(string message): base(message)
    { }
}
