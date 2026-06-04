namespace FarmLink.Protocol.Specification;

[Flags]
public enum TelemetryStatusFlags : byte
{
    BATTERY_LOW         = 0,
    SENSOR_ERROR        = 1,
    CALIBRATION_NEEDED  = 2,
    OUT_OF_RANGE        = 4,
}
