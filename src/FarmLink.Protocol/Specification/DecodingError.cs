namespace FarmLink.Protocol.Specification;

public enum DecodingError
{
    /// <summary>
    /// The frame is empty, which is invalid according to the protocol specifications.
    /// </summary>
    FrameEmpty,

    /// <summary>
    /// The protocol version specified in the frame is not recognized or supported by the decoder.
    /// </summary>
    UnrecognizedProtocolVersion,

    /// <summary>
    /// The device ID is invalid, such as being zero (0) which is reserved for unassigned or invalid IDs. Valid device IDs must be in the range 0x0001 to 0xFFFF.
    /// </summary>
    InvalidDeviceID,

    /// <summary>
    /// The message ID is invalid, such as being negative or not following the expected format. Valid message IDs must in the range of 1 to 0x7FFFFFFF (for a 4-byte signed integer).
    /// </summary>
    InvalidMessageID,

    /// <summary>
    /// The message type is not recognized or supported according to the protocol specifications. Message types must be defined and valid for the protocol version being decoded.
    /// </summary>
    InvalidMessageType,

    /// <summary>
    /// The payload length specified in the frame is invalid, such as being negative or exceeding the maximum allowed length. The payload length must be within the defined limits for the protocol version being decoded.
    /// </summary>
    InvalidPayloadLength,
}
