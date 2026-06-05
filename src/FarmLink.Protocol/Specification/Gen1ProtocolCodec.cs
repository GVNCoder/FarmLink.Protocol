using FarmLink.Protocol.Specification.Types;
using System.Buffers.Binary;

namespace FarmLink.Protocol.Specification;

public sealed class Gen1ProtocolCodec : IProtocolCodec
{
    public ProtocolVersion ProtocolVersion => ProtocolVersion.Gen1;

    public ProtocolDecodeResult Decode(ReadOnlySpan<byte> frame)
    {
        // Nothing to decode if the frame is empty
        if (frame.Length is 0)
            return ProtocolDecodeResult.Failed();

        /* Protocol Constraints
         * - Maximum Payload Length: 1024 bytes (adjustable for device capabilities)
         * - Maximum Total Message Size: 1040 bytes (20-byte overhead + 1024-byte payload)
         * - Minimum Message Size: 20 bytes (overhead with zero-length payload)
         */

        /* Protocol Message Structure
         * - Device ID: 2 bytes => Unique identifier of the device (0x0001 - 0xFFFF)
         * - Message ID: 4 bytes => Unique message identifier (cryptographically random and sequential)
         * - Message Type: 1 byte => Type of message (see Message Types table)
         * - Payload Length: 4 bytes => Length of payload in bytes (0 - 1024)
         * - Payload Block: Variable => Message-specific content (0 to 1024 bytes)
         */

        // To validate the frame against the protocol constraints
        // we need to decode its components first and then check if they meet the specified limits.
        var position = 0;

        // Read Device ID (2 bytes LE)
        var deviceIDSlice = frame.Slice(position, ProtocolSpecs.DeviceIDSize);
        var deviceID = BinaryPrimitives.ReadInt16LittleEndian(deviceIDSlice);

        // Move position forward by Device ID size
        position += ProtocolSpecs.DeviceIDSize;

        // Validate Device ID
        // Device IDs must be in the range 0x0001 to 0xFFFF
        // 0 means Invalid/Unassigned
        if (deviceID is 0)
            return ProtocolDecodeResult.Failed();

        // Read Message ID (4 bytes LE)
        var messageIDSlice = frame.Slice(position, ProtocolSpecs.MessageIDSize);
        var messageID = BinaryPrimitives.ReadInt32LittleEndian(messageIDSlice);

        // Validate Message ID
        // Message IDs must be positive integers (1 to 0x7FFFFFFF)
        if (messageID <= 0)
            return ProtocolDecodeResult.Failed();

        // Move position forward by Message ID size
        position += ProtocolSpecs.MessageIDSize;

        // Read Message Type (1 byte)
        var messageTypeSlice = frame.Slice(position, ProtocolSpecs.MessageTypeSize);
        var messageType = (MessageType)messageTypeSlice[0];

        // Validate Message Type
        // Message Type must be a defined value in the MessageType enum
        // TODO: can be validated much faster by checking if the byte value is within the valid range of enum values
        if (!Enum.IsDefined<MessageType>(messageType))
            return ProtocolDecodeResult.Failed();

        // Move position forward by Message Type size
        position += ProtocolSpecs.MessageTypeSize;

        // Read Payload Length (4 bytes LE)
        var payloadLengthSlice = frame.Slice(position, ProtocolSpecs.PayloadLengthSize);
        var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(payloadLengthSlice);

        // Validate Payload Length value
        if (payloadLength < 0)
            return ProtocolDecodeResult.Failed();

        // Validate Payload Length against protocol constraints
        if (payloadLength > ProtocolSpecs.MaxMessagePayloadSize)
            return ProtocolDecodeResult.Failed();

        // Validate Total Message Size against protocol constraints
        if (position + payloadLength > ProtocolSpecs.MaxMessageSize)
            return ProtocolDecodeResult.Failed();

        // Check if the frame has enough bytes for the payload
        if (frame.Length - position < payloadLength)
            return ProtocolDecodeResult.Failed();

        // Move position forward by Payload Length size
        position += ProtocolSpecs.PayloadLengthSize;

        // Read Payload Block (variable length)
        var payloadSlice = frame.Slice(position, payloadLength);
        position += payloadLength;

        var message = new Message
        {
            ProtocolVersion = ProtocolVersion.Gen1,
            DeviceID = deviceID,
            MessageID = messageID,
            MessageType = messageType,
            Payload = payloadSlice.ToArray()
        };

        // Create a message object with the decoded values
        return ProtocolDecodeResult.Successful(message, consumedBytes: position);
    }

    public void Encode(Message message, Span<byte> buffer)
    {
        // Compute the total message size
        var messageSize = ProtocolSpecs.MinMessageSize + message.Payload.Length;

        // Validate the message size and buffer size compliance
        ValidateMessageAndBufferSizeCompliance(messageSize, buffer.Length);

        // Allocate a buffer for the encoded message
        // Span<byte> buffer = stackalloc byte[messageSize];
        var position = 0;

        // Encode Protocol Version
        buffer[position] = (byte)ProtocolVersion;

        // Move position forward by Protocol Version size
        position += ProtocolSpecs.ProtocolVersionSize;

        // Encode Device ID (2 bytes LE)
        BinaryPrimitives.WriteInt16LittleEndian(buffer.Slice(position, ProtocolSpecs.DeviceIDSize), message.DeviceID);
        position += ProtocolSpecs.DeviceIDSize;

        // Encode Message ID (4 bytes LE)
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(position, ProtocolSpecs.MessageIDSize), message.MessageID);
        position += ProtocolSpecs.MessageIDSize;

        // Encode Message Type (1 byte)
        buffer[position] = (byte)message.MessageType;
        position += ProtocolSpecs.MessageTypeSize;

        // Encode Payload Length (4 bytes LE)
        BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(position, ProtocolSpecs.PayloadLengthSize), message.Payload.Length);
        position += ProtocolSpecs.PayloadLengthSize;

        // Encode Payload Block (variable length)
        message.Payload.CopyTo(buffer.Slice(position, message.Payload.Length));
    }

    private static void ValidateMessageAndBufferSizeCompliance(int messageSize, int allocatedBufferSize)
    {
        if (messageSize > ProtocolSpecs.MaxMessageSize)
            throw new ArgumentException($"Message size exceeds protocol limits. Maximum allowed size: {ProtocolSpecs.MaxMessageSize} bytes.", nameof(messageSize));
        if (allocatedBufferSize < messageSize)
            throw new ArgumentException($"Provided buffer is too small. Required size: {messageSize} bytes.", nameof(allocatedBufferSize));
        if (allocatedBufferSize > messageSize)
            throw new ArgumentException($"Provided buffer is larger than necessary. Required size: {messageSize} bytes.", nameof(allocatedBufferSize));
    }
}
