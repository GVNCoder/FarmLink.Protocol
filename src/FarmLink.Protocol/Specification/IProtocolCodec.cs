using FarmLink.Protocol.Specification.Types;

namespace FarmLink.Protocol.Specification;

/// <summary>
/// Defines the interface for encoding and decoding messages in the FarmLink Protocol.
/// </summary>
public interface IProtocolCodec
{
    /// <summary>
    /// Gets the protocol version that this codec supports.
    /// </summary>
    public ProtocolVersion ProtocolVersion { get; }

    /// <summary>
    /// Encodes a FarmLink Protocol Message into a byte span.
    /// </summary>
    /// <param name="message">The message to encode.</param>
    /// <param name="buffer">The buffer to write the encoded message into.</param>
    void Encode(Message message, ref Span<byte> buffer);

    /// <summary>
    /// Decodes a byte span into a FarmLink Protocol Message.
    /// </summary>
    /// <param name="frame">The byte span to decode.</param>
    /// <returns>The result of the decoding operation.</returns>
    ProtocolDecodeResult Decode(ref ReadOnlySpan<byte> frame);
}
