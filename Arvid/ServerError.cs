namespace Arvid
{
    public enum ServerError
    {
        ArvidErrorNotInitialized = -1,
        ArvidErrorPruInitFailed = -2,
        ArvidErrorPruLoadFailed = -3,
        ArvidErrorPruMemoryMappingFailed = -4,
        ArvidErrorPruPermissionRequired = -5,
        ArvidErrorPruCloseFailed = -6,
        ArvidErrorIllegalVideoMode = -7,
        ArvidErrorThreadFailed = -8,
        ArvidErrorDmaInitFailed = -9,
    }
}