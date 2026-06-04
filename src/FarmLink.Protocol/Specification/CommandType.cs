namespace FarmLink.Protocol.Specification;

public enum CommandType : byte
{
    OFF = 0x00,
    ON  = 0x01,
    TGL = 0x02,
    ADJ = 0x03,
    RST = 0x04,
    EXC = 0x05,
}
