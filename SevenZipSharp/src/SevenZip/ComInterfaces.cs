using System.Runtime.InteropServices;

namespace SevenZip;

[Guid("23170F69-40C1-278A-0000-000600200000")]
public interface IArchiveExtractCallback
{
    void SetTotal(ulong size);
    void SetCompleted(in ulong size);
    Stream? GetStream(uint index, NAskMode askExtractMode);
    void PrepareOperation(NAskMode askExtractMode);
    void SetOperationResult(NOperationResult opRes);
}

[Guid("23170f69-40c1-278a-0000-000400040000")]
public interface ICompressProgressInfo
{
    ///
    /// Throw AbortedException to abort the current operation.
    ///
    void SetRatioInfo(in ulong inSize, in ulong outSize);
}

[Guid("23170f69-40c1-278a-0000-000500100000")]
public interface ICryptoGetTextPassword
{
    string? CryptoGetTextPassword();
}
