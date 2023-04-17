
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native.Com;

[Guid("23170F69-40C1-278A-0000-000600600000")]
public unsafe struct IInArchive
{
    public void** lpVtbl;

    public uint AddRef()
    {
        return VTableIUnknown.FromPointer(this.lpVtbl).AddRef(Unsafe.AsPointer(ref this));
    }

    public uint Release()
    {
        return VTableIUnknown.FromPointer(this.lpVtbl).Release(Unsafe.AsPointer(ref this));
    }

    public void Open(in IInStream stream)
    {
        ulong maxCheckStartPosition = 1 << 15;
        VTableIInArchive.FromPointer(this.lpVtbl).Open(in this, in stream, in maxCheckStartPosition, null).EnsureSuccess();
    }

    public uint GetNumberOfItem()
    {
        uint num;
        VTableIInArchive.FromPointer(this.lpVtbl).GetNumberOfItem(in this, out num).EnsureSuccess();
        return num;
    }

    public uint GetNumberOfProperties()
    {
        uint num;
        VTableIInArchive.FromPointer(this.lpVtbl).GetNumberOfProperties(in this, out num).EnsureSuccess();
        return num;
    }
}

public unsafe struct VTableIInArchive
{
    public static ref VTableIInArchive FromPointer(void** lpVtbl) => ref *(VTableIInArchive*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged[Thiscall]<in IInArchive, in IInStream, in ulong, void*, HRESULT> Open;
    public delegate* unmanaged[Thiscall]<in IInArchive, HRESULT> Close;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfItem;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, uint, PROPVARIANT*, HRESULT> GetProperty;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint*, uint, int, void*, HRESULT> Extract;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, PROPVARIANT*, HRESULT> GetArchiveProperty;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfProperties;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, void*, uint*, ushort*, HRESULT> GetPropertyInfo;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfArchiveProperties;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, void*, uint*, ushort*, HRESULT> GetArchivePropertyInfo;
}
