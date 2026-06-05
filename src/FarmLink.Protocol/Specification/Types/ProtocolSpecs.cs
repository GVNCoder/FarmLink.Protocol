namespace FarmLink.Protocol.Specification.Types;

public static class ProtocolSpecs
{
    /// <summary>
    /// The magic bytes used to identify the start of a synchronization message in the FarmLink protocol.
    /// The magic bytes are "FLK\0" in ASCII.
    /// </summary>
    public static readonly byte[] SyncMagicBytes = [0x46, 0x4C, 0x4B, 0x00];

    /// <summary>
    /// The length of the synchronization magic bytes in the FarmLink protocol.
    /// </summary>
    public const int SyncMagicBytesLength = 4;

    /// <summary>
    /// The maximum allowed size of a message payload in the FarmLink protocol, in bytes.
    /// </summary>
    public const int MaxMessagePayloadSize = 1024;

    /// <summary>
    /// The minimum size of a message in the FarmLink protocol, including the overhead with zero-length payload.
    /// </summary>
    public const int MinMessageSize = 20;
    
    /// <summary>
    /// The maximum size of a message in the FarmLink protocol, including the overhead and the maximum payload.
    /// </summary>
    public const int MaxMessageSize = MinMessageSize + MaxMessagePayloadSize;

    /// <summary>
    /// The size of the Device ID field in the FarmLink protocol, in bytes.
    /// </summary>
    public const int DeviceIDSize = sizeof(short);

    /// <summary>
    /// The size of the Message ID field in the FarmLink protocol, in bytes.
    /// </summary>
    public const int MessageIDSize = sizeof(int);

    /// <summary>
    /// The size of the Message Type field in the FarmLink protocol, in bytes.
    /// </summary>
    public const int MessageTypeSize = sizeof(byte);

    /// <summary>
    /// The size of the Payload Length field in the FarmLink protocol, in bytes.
    /// </summary>
    public const int PayloadLengthSize = sizeof(int);
}
