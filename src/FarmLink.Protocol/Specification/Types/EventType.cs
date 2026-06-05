namespace FarmLink.Protocol.Specification.Types;

public enum EventType : byte
{
    THRESHOLD_EXCEEDED  = 0x01,
    THRESHOLD_RECOVERED = 0x02,
    BATTERY_LOW         = 0x03,
    SENSOR_ERROR        = 0x04,
    DEVICE_STARTUP      = 0x05,
    CONNECTION_LOST     = 0x06,
}
