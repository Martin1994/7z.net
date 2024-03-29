namespace SevenZip;

public enum NMethodPropID : uint
{
    kID,
    kName,
    kDecoder,
    kEncoder,
    kPackStreams,
    kUnpackStreams,
    kDescription,
    kDecoderIsAssigned,
    kEncoderIsAssigned,
    kDigestSize,
    kIsFilter
}

public enum NHandlerPropID : uint
{
    kName = 0,        // VT_BSTR
    kClassID,         // binary GUID in VT_BSTR
    kExtension,       // VT_BSTR
    kAddExtension,    // VT_BSTR
    kUpdate,          // VT_BOOL
    kKeepName,        // VT_BOOL
    kSignature,       // binary in VT_BSTR
    kMultiSignature,  // binary in VT_BSTR
    kSignatureOffset, // VT_UI4
    kAltStreams,      // VT_BOOL
    kNtSecure,        // VT_BOOL
    kFlags,           // VT_UI4
    kTimeFlags        // VT_UI4
}

public enum PROPID : uint
{
    kpidNoProperty = 0,
    kpidMainSubfile,
    kpidHandlerItemIndex,
    kpidPath,
    kpidName,
    kpidExtension,
    kpidIsDir,
    kpidSize,
    kpidPackSize,
    kpidAttrib,
    kpidCTime,
    kpidATime,
    kpidMTime,
    kpidSolid,
    kpidCommented,
    kpidEncrypted,
    kpidSplitBefore,
    kpidSplitAfter,
    kpidDictionarySize,
    kpidCRC,
    kpidType,
    kpidIsAnti,
    kpidMethod,
    kpidHostOS,
    kpidFileSystem,
    kpidUser,
    kpidGroup,
    kpidBlock,
    kpidComment,
    kpidPosition,
    kpidPrefix,
    kpidNumSubDirs,
    kpidNumSubFiles,
    kpidUnpackVer,
    kpidVolume,
    kpidIsVolume,
    kpidOffset,
    kpidLinks,
    kpidNumBlocks,
    kpidNumVolumes,
    kpidTimeType,
    kpidBit64,
    kpidBigEndian,
    kpidCpu,
    kpidPhySize,
    kpidHeadersSize,
    kpidChecksum,
    kpidCharacts,
    kpidVa,
    kpidId,
    kpidShortName,
    kpidCreatorApp,
    kpidSectorSize,
    kpidPosixAttrib,
    kpidSymLink,
    kpidError,
    kpidTotalSize,
    kpidFreeSpace,
    kpidClusterSize,
    kpidVolumeName,
    kpidLocalName,
    kpidProvider,
    kpidNtSecure,
    kpidIsAltStream,
    kpidIsAux,
    kpidIsDeleted,
    kpidIsTree,
    kpidSha1,
    kpidSha256,
    kpidErrorType,
    kpidNumErrors,
    kpidErrorFlags,
    kpidWarningFlags,
    kpidWarning,
    kpidNumStreams,
    kpidNumAltStreams,
    kpidAltStreamsSize,
    kpidVirtualSize,
    kpidUnpackSize,
    kpidTotalPhySize,
    kpidVolumeIndex,
    kpidSubType,
    kpidShortComment,
    kpidCodePage,
    kpidIsNotArcType,
    kpidPhySizeCantBeDetected,
    kpidZerosTailIsAllowed,
    kpidTailSize,
    kpidEmbeddedStubSize,
    kpidNtReparse,
    kpidHardLink,
    kpidINode,
    kpidStreamId,
    kpidReadOnly,
    kpidOutName,
    kpidCopyLink,
    kpidArcFileName,
    kpidIsHash,
    kpidChangeTime,
    kpidUserId,
    kpidGroupId,
    kpidDeviceMajor,
    kpidDeviceMinor,

    kpid_NUM_DEFINED,

    kpidUserDefined = 0x10000
}

public enum NAskMode : int
{
    kExtract = 0,
    kTest,
    kSkip,
    kReadExternal
}

public enum NOperationResult : int
{
    kOK = 0,
    kUnsupportedMethod,
    kDataError,
    kCRCError,
    kUnavailable,
    kUnexpectedEnd,
    kDataAfterEnd,
    kIsNotArc,
    kHeadersError,
    kWrongPassword
}
