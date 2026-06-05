namespace FarmLink.Protocol.Specification.Types;

public ref struct Message
{
    public ProtocolVersion ProtocolVersion;
    public short DeviceID;
    public int MessageID;
    public MessageType MessageType;
    public ReadOnlySpan<byte> Payload;
}
