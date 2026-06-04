namespace FarmLink.Protocol.Specification;

public enum NackStatusCode : byte
{
    UNKNOWN_ERROR           = 0x00,
    INVALID_COMMAND         = 0x01,
    INVALID_PARAMETERS      = 0x02,
    DEVICE_BUSY             = 0x03,
    TIMEOUT                 = 0x04,
    INSUFFICIENT_RESOURCES  = 0x05,
    UNSUPPORTED_VERSION     = 0x06,
    DEVICE_NOT_READY        = 0x07,
}
