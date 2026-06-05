using FarmLink.Protocol.Specification.Types;

namespace FarmLink.Protocol.Specification;

/// <summary>
/// Represents the result of decoding a byte span into a FarmLink Protocol Message.
/// </summary>
/// <param name="Message">The decoded message, if successful; otherwise, null.</param>
/// <param name="Success">Indicates whether the decoding was successful.</param>
public record struct ProtocolDecodeResult(
    Message? Message,
    bool Success,
    int ConsumedBytes)
{
    /// <summary>
    /// Creates a successful decode result with the given message.
    /// </summary>
    /// <param name="message">The decoded message.</param>
    /// <param name="consumedBytes">The number of bytes consumed during decoding.</param>
    /// <returns>A successful ProtocolDecodeResult containing the message.</returns>
    public static ProtocolDecodeResult Successful(Message message, int consumedBytes) => new (message, true, consumedBytes);

    /// <summary>
    /// Creates a failed decode result.
    /// </summary>
    /// <returns>A failed ProtocolDecodeResult with no message.</returns>
    public static ProtocolDecodeResult Failed() => new (null, false, 0);
}