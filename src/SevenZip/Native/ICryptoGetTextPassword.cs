using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native;

public unsafe struct VTableICryptoGetTextPassword
{
    public static ref VTableICryptoGetTextPassword FromPointer(void** lpVtbl) => ref *(VTableICryptoGetTextPassword*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged<ComObject*, void**, HRESULT> CryptoGetTextPassword;
}

public unsafe class CryptoGetTextPasswordProxy : ComProxy<CryptoGetTextPasswordProxy, ICryptoGetTextPassword>
{
    private struct ManagedVTable
    {
        private void* _m1, _m2, _m3, _m4, _m5, _m6;
    }

    [FixedAddressValueType]
    private static readonly ManagedVTable _lpVtbl;

    static CryptoGetTextPasswordProxy()
    {
        fixed (void* ptr = &_lpVtbl)
        {
            void** lpVtbl = (void**)ptr;

            ref VTableIUnknown vTableIUnknown = ref VTableIUnknown.FromPointer(lpVtbl);
            vTableIUnknown.QueryInterface = &ManagedQueryInterface;
            vTableIUnknown.AddRef = &VTableIUnknown.NoopAddRef;
            vTableIUnknown.Release = &VTableIUnknown.NoopRelease;

            ref VTableICryptoGetTextPassword vTableICryptoGetTextPassword = ref VTableICryptoGetTextPassword.FromPointer(lpVtbl);
            vTableICryptoGetTextPassword.CryptoGetTextPassword = &ManagedCryptoGetTextPassword;
        }

        RegisterInterface(implementation => new CryptoGetTextPasswordProxy(implementation));
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
    private static HRESULT ManagedCryptoGetTextPassword(ComObject* that, void** password)
    {
        if (!TryGetProxy(that->id, out var proxy))
        {
            return HRESULT.E_INVALIDARG;
        }

        try
        {
            var input = proxy._implementation.CryptoGetTextPassword();
            if (input == null)
            {
                *password = null;
            }
            else
            {
                *password = (void*)input.AllocateBSTR();
            }
            return HRESULT.S_OK;
        }
        catch (Exception e)
        {
            return proxy.PersistAndExtractException(e);
        }
    }

    public CryptoGetTextPasswordProxy(ICryptoGetTextPassword implementation) : base(implementation)
    {
        fixed (void* lpVtbl = &_lpVtbl)
        {
            ComObject.lpVtbl = (void**)lpVtbl;
        }
        ComObject.id = _id;
    }
}
