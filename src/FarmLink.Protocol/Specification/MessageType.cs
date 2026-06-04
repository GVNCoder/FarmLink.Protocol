namespace FarmLink.Protocol.Specification;

public enum MessageType : byte
{
    NACK        = 0x00,
    ACK         = 0x01,
    CMD         = 0x02,
    TLM         = 0x03,
    EVT         = 0x04,
    PINGPONG    = 0x05,
    HBT         = 0x06,
}
