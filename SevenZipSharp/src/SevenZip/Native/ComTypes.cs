using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SevenZip.Native;

[StructLayout(LayoutKind.Sequential, Pack=0)]
public struct PROPARRAY
{
    internal UInt32 cElems;
    internal IntPtr pElems;
}

[StructLayout(LayoutKind.Explicit, Pack=1)]
public struct PROPVARIANT
{
    [FieldOffset(0)] internal VARENUM varType;
    [FieldOffset(2)] internal ushort wReserved1;
    [FieldOffset(4)] internal ushort wReserved2;
    [FieldOffset(6)] internal ushort wReserved3;

    [FieldOffset(8)] internal byte bVal;
    [FieldOffset(8)] internal sbyte cVal;
    [FieldOffset(8)] internal ushort uiVal;
    [FieldOffset(8)] internal short iVal;
    [FieldOffset(8)] internal UInt32 uintVal;
    [FieldOffset(8)] internal Int32 intVal;
    [FieldOffset(8)] internal UInt64 ulVal;
    [FieldOffset(8)] internal Int64 lVal;
    [FieldOffset(8)] internal float fltVal;
    [FieldOffset(8)] internal double dblVal;
    [FieldOffset(8)] internal bool boolVal;
    [FieldOffset(8)] internal IntPtr ptrVal;
    [FieldOffset(8)] internal PROPARRAY ca;
    [FieldOffset(8)] internal System.Runtime.InteropServices.ComTypes.FILETIME filetime;
}

public enum VARENUM : ushort
{
    VT_EMPTY	= 0,
    VT_NULL	= 1,
    VT_I2	= 2,
    VT_I4	= 3,
    VT_R4	= 4,
    VT_R8	= 5,
    VT_CY	= 6,
    VT_DATE	= 7,
    VT_BSTR	= 8,
    VT_DISPATCH	= 9,
    VT_ERROR	= 10,
    VT_BOOL	= 11,
    VT_VARIANT	= 12,
    VT_UNKNOWN	= 13,
    VT_DECIMAL	= 14,
    VT_I1	= 16,
    VT_UI1	= 17,
    VT_UI2	= 18,
    VT_UI4	= 19,
    VT_I8	= 20,
    VT_UI8	= 21,
    VT_INT	= 22,
    VT_UINT	= 23,
    VT_VOID	= 24,
    VT_HRESULT	= 25,
    VT_PTR	= 26,
    VT_SAFEARRAY	= 27,
    VT_CARRAY	= 28,
    VT_USERDEFINED	= 29,
    VT_LPSTR	= 30,
    VT_LPWSTR	= 31,
    VT_RECORD	= 36,
    VT_INT_PTR	= 37,
    VT_UINT_PTR	= 38,
    VT_FILETIME	= 64,
    VT_BLOB	= 65,
    VT_STREAM	= 66,
    VT_STORAGE	= 67,
    VT_STREAMED_OBJECT	= 68,
    VT_STORED_OBJECT	= 69,
    VT_BLOB_OBJECT	= 70,
    VT_CF	= 71,
    VT_CLSID	= 72,
    VT_VERSIONED_STREAM	= 73,
    VT_BSTR_BLOB	= 0xfff,
    VT_VECTOR	= 0x1000,
    VT_ARRAY	= 0x2000,
    VT_BYREF	= 0x4000,
    VT_RESERVED	= 0x8000,
    VT_ILLEGAL	= 0xffff,
    VT_ILLEGALMASKED	= 0xfff,
    VT_TYPEMASK	= 0xfff
}

public static unsafe class BSTRExtension
{
    private static UTF32Encoding utf32 = new UTF32Encoding();

    public static string ReadBSTR(this IntPtr ptr)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Marshal.PtrToStringBSTR(ptr)!;
        }
        else
        {
            // The non-Windows port of BSTR uses the native wchar_t, which uses UTF-32 with bits length.
            // See: MyWindows.h
            //     typedef wchar_t WCHAR;
            //     typedef WCHAR OLECHAR;
            return utf32.GetString(new ReadOnlySpan<byte>((byte*)ptr, *(((int*)ptr) - 1)));
        }
    }

}

public static class PROPVARIANTExtension
{
    public static string ReadString(this ref PROPVARIANT prop)
    {
        if (prop.varType != VARENUM.VT_BSTR)
        {
            throw new ArgumentException($"Expect VT_BSTR but got {Enum.GetName(prop.varType)}");
        }

        return prop.ptrVal.ReadBSTR();
    }

    public static string? ReadOptionalString(this ref PROPVARIANT prop)
    {
        if (prop.varType == VARENUM.VT_EMPTY)
        {
            return null;
        }
        if (prop.varType != VARENUM.VT_BSTR)
        {
            throw new ArgumentException($"Expect VT_BSTR but got {Enum.GetName(prop.varType)}");
        }

        return prop.ptrVal.ReadBSTR();
    }

    public static unsafe T* ReadPointer<T>(this ref PROPVARIANT prop) where T : unmanaged
    {
        if (prop.varType == VARENUM.VT_EMPTY)
        {
            return null;
        }
        if (prop.varType != VARENUM.VT_BSTR)
        {
            throw new ArgumentException($"Expect VT_BSTR but got {Enum.GetName(prop.varType)}");
        }
        return (T*)prop.ptrVal;
    }

    public static bool ReadBool(this ref PROPVARIANT prop)
    {
        if (prop.varType != VARENUM.VT_BOOL)
        {
            throw new ArgumentException($"Expect VT_BOOL but got {Enum.GetName(prop.varType)}");
        }
        return prop.boolVal;
    }

    public static bool ReadBool(this ref PROPVARIANT prop, bool defaultValue)
    {
        if (prop.varType == VARENUM.VT_EMPTY)
        {
            return defaultValue;
        }
        if (prop.varType != VARENUM.VT_BOOL)
        {
            throw new ArgumentException($"Expect VT_BOOL but got {Enum.GetName(prop.varType)}");
        }
        return prop.boolVal;
    }

    public static uint ReadUInt32(this ref PROPVARIANT prop)
    {
        if (prop.varType != VARENUM.VT_UI4)
        {
            throw new ArgumentException($"Expect VT_UI4 but got {Enum.GetName(prop.varType)}");
        }
        return prop.uintVal;
    }

    public static bool TryReadUInt32(this ref PROPVARIANT prop, out uint value)
    {
        if (prop.varType == VARENUM.VT_EMPTY)
        {
            value = default;
            return false;
        }
        if (prop.varType != VARENUM.VT_UI4)
        {
            throw new ArgumentException($"Expect VT_UI4 but got {Enum.GetName(prop.varType)}");
        }
        value = prop.uintVal;
        return true;
    }

    public static ulong ReadUInt64(this ref PROPVARIANT prop)
    {
        if (prop.varType != VARENUM.VT_UI8)
        {
            throw new ArgumentException($"Expect VT_UI8 but got {Enum.GetName(prop.varType)}");
        }
        return prop.ulVal;
    }

    public static DateTime ReadFileTime(this ref PROPVARIANT prop)
    {
        if (prop.varType != VARENUM.VT_FILETIME)
        {
            throw new ArgumentException($"Expect VT_FILETIME but got {Enum.GetName(prop.varType)}");
        }
        return DateTime.FromFileTime((long)prop.filetime.dwHighDateTime << 32 | (uint)prop.filetime.dwLowDateTime);
    }

    public static DateTime ReadOptionalFileTime(this ref PROPVARIANT prop)
    {
        if (prop.varType == VARENUM.VT_EMPTY)
        {
            return default;
        }
        if (prop.varType != VARENUM.VT_FILETIME)
        {
            throw new ArgumentException($"Expect VT_FILETIME but got {Enum.GetName(prop.varType)}");
        }
        return DateTime.FromFileTime((long)prop.filetime.dwHighDateTime << 32 | (uint)prop.filetime.dwLowDateTime);
    }

    public static unsafe string Format(this ref PROPVARIANT prop) {
        switch (prop.varType)
        {
            case VARENUM.VT_EMPTY:
                return "";

            case VARENUM.VT_BOOL:
                return prop.boolVal.ToString();

            case VARENUM.VT_UI4:
                return (prop.bVal & 0xF).ToString();

            case VARENUM.VT_UI8:
                return prop.bVal.ToString();

            case VARENUM.VT_BSTR:
                return prop.ptrVal.ReadBSTR();

            default:
                throw new Exception($"Unexpected property type {Enum.GetName(prop.varType)}");
        }
    }
}

public enum HRESULT : uint
{
    S_OK = 0x00000000,
    S_FALSE = 0x00000001,
    E_NOTIMPL = 0x80004001,
    E_NOINTERFACE = 0x80004002,
    E_ABORT = 0x80004004,
    E_FAIL = 0x80004005,
    STG_E_INVALIDFUNCTION = 0x80030001,
    CLASS_E_CLASSNOTAVAILABLE = 0x80040111,
    E_OUTOFMEMORY = 0x8007000E,
    E_INVALIDARG = 0x80070057,
    MY__E_ERROR_NEGATIVE_SEEK = 0x80070083
}

public static class HRESULTExtension
{
    public static void EnsureSuccess(this HRESULT result, [CallerMemberName] string methodName = "")
    {
        if (result == HRESULT.S_OK) {
            return;
        }
        throw new SevenZipComException(result, methodName);
    }
}

public class SevenZipComException : Exception
{
    private static string GetExceptionMessage(HRESULT code, string methodName)
    {
        return $"7-zip COM invocation {methodName} returned unsuccessful result: 0x{(uint)code:X} {Enum.GetName(code)}";
    }

    public HRESULT Code { get; }

    public SevenZipComException(HRESULT code, string methodName)
        : base(GetExceptionMessage(code, methodName))
    {
        Code = code;
    }

    public SevenZipComException(HRESULT code, string methodName, Exception innerException)
        : base(GetExceptionMessage(code, methodName), innerException)
    {
        Code = code;
    }
}
