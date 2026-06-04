namespace FarmLink.Protocol.Specification;

public enum SensorType : byte
{
    TEMPERATURE     = 0x00,
    HUMIDITY        = 0x01,
    SOIL_MOISTURE   = 0x02,
    WATER_LEVEL     = 0x03,
    LIGHT_INTENSITY = 0x04,
    BATTERY_VOLTAGE = 0x05,
}
