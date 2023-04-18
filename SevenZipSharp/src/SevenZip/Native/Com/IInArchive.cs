
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SevenZip.Native.Com;

[Guid("23170F69-40C1-278A-0000-000600600000")]
public unsafe struct IInArchive
{
    public void** lpVtbl;

    public uint AddRef()
    {
        return VTableIUnknown.FromPointer(lpVtbl).AddRef(Unsafe.AsPointer(ref this));
    }

    public uint Release()
    {
        return VTableIUnknown.FromPointer(lpVtbl).Release(Unsafe.AsPointer(ref this));
    }

    public void Open(in IInStream stream)
    {
        ulong maxCheckStartPosition = 1 << 15;
        VTableIInArchive.FromPointer(lpVtbl).Open(in this, in stream, in maxCheckStartPosition, null).EnsureSuccess();
    }

    public uint GetNumberOfItem()
    {
        uint num;
        VTableIInArchive.FromPointer(lpVtbl).GetNumberOfItem(in this, out num).EnsureSuccess();
        return num;
    }

    public void GetProperty(uint index, PROPID propID, out PROPVARIANT prop)
    {
        VTableIInArchive.FromPointer(lpVtbl).GetProperty(in this, index, propID, out prop).EnsureSuccess();
    }

    public void GetArchiveProperty(PROPID propID, out PROPVARIANT prop)
    {
        VTableIInArchive.FromPointer(lpVtbl).GetArchiveProperty(in this, propID, out prop).EnsureSuccess();
    }

    public uint GetNumberOfProperties()
    {
        uint num;
        VTableIInArchive.FromPointer(lpVtbl).GetNumberOfProperties(in this, out num).EnsureSuccess();
        return num;
    }

    public uint GetNumberOfArchiveProperties()
    {
        uint num;
        VTableIInArchive.FromPointer(lpVtbl).GetNumberOfArchiveProperties(in this, out num).EnsureSuccess();
        return num;
    }

    public void GetArchivePropertyInfo(uint index, out string? name, out PROPID propID, out VARENUM type)
    {
        IntPtr nameBstr;
        VTableIInArchive.FromPointer(lpVtbl).GetArchivePropertyInfo(in this, index, out nameBstr, out propID, out type).EnsureSuccess();
        name = nameBstr == 0 ? null : nameBstr.ReadBSTR();
    }
}

public unsafe struct VTableIInArchive
{
    public static ref VTableIInArchive FromPointer(void** lpVtbl) => ref *(VTableIInArchive*)(lpVtbl + VTableIUnknown.vTableOffset);

    public delegate* unmanaged[Thiscall]<in IInArchive, in IInStream, in ulong, void*, HRESULT> Open;
    public delegate* unmanaged[Thiscall]<in IInArchive, HRESULT> Close;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfItem;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, PROPID, out PROPVARIANT, HRESULT> GetProperty;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint*, uint, int, void*, HRESULT> Extract;
    public delegate* unmanaged[Thiscall]<in IInArchive, PROPID, out PROPVARIANT, HRESULT> GetArchiveProperty;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfProperties;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, void*, PROPID*, VARENUM*, HRESULT> GetPropertyInfo;
    public delegate* unmanaged[Thiscall]<in IInArchive, out uint, HRESULT> GetNumberOfArchiveProperties;
    public delegate* unmanaged[Thiscall]<in IInArchive, uint, out IntPtr, out PROPID, out VARENUM, HRESULT> GetArchivePropertyInfo;
}
