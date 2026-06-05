namespace FarmLink.Protocol.Specification.Types;

/// <summary>
/// Represents a FarmLink Protocol Message.
/// </summary>
/// <param name="ProtocolVersion">The version of the protocol used for this message.</param>
/// <param name="DeviceID">The unique identifier of the device sending or receiving the message.</param>
/// <param name="MessageID">The unique identifier of the message, used for tracking and correlation.</param>
/// <param name="MessageType">The type of the message, indicating its purpose and how it should be processed.</param>
/// <param name="Payload">The content of the message, which can vary in structure and meaning based on the MessageType.</param>
public record struct Message(
    ProtocolVersion ProtocolVersion,
    short DeviceID,
    int MessageID,
    MessageType MessageType,
    byte[] Payload);
