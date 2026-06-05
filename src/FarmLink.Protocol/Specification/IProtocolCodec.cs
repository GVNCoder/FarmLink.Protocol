using FarmLink.Protocol.Specification.Types;

namespace FarmLink.Protocol.Specification;

/// <summary>
/// Defines the interface for encoding and decoding messages in the FarmLink Protocol.
/// </summary>
public interface IProtocolCodec
{
    /// <summary>
    /// Encodes a FarmLink Protocol Message into a byte span.
    /// </summary>
    /// <param name="message">The message to encode.</param>
    /// <returns>A byte span representing the encoded message.</returns>
    ReadOnlySpan<byte> Encode(Message message);

    /// <summary>
    /// Decodes a byte span into a FarmLink Protocol Message.
    /// </summary>
    /// <param name="frame">The byte span to decode.</param>
    /// <returns>The result of the decoding operation.</returns>
    ProtocolDecodeResult Decode(ReadOnlySpan<byte> frame);
}
