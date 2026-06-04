# FarmLink Protocol Gen1

> **Educational Protocol**: This is Gen1 - a foundational implementation demonstrating core protocol design principles. Advanced features (security, fragmentation, QoS) are reserved for Gen2.

## Overview

The FarmLink Protocol Gen1 is a communication protocol designed for efficient and reliable data exchange between devices in an agricultural environment.

It is structured to ensure data integrity, adaptability, and versioning support while being simple enough to implement on resource-constrained embedded devices.

Its versioned design allows for future enhancements and modifications without breaking compatibility with existing implementations. Based on the message version, we can pick the right parser and handler for the message, ensuring that the system can evolve over time while maintaining support for older message formats.

## Use Cases

This protocol enables communication between different devices in the farm:
- **Sensors**: Real-time telemetry or polling-based data collection (temperature, humidity, soil moisture)
- **Actuators**: Control devices like pumps, valves, irrigation systems
- **Control Systems**: Central monitoring and command infrastructure

## Protocol Specification

### Byte Order (Endianness)

**All multi-byte values use Little-Endian (LE)**:
- Device ID (2 bytes): Least Significant Byte first
- Message ID (4 bytes): Least Significant Byte first
- Payload Length (4 bytes): Least Significant Byte first
- Timestamps (8 bytes): Least Significant Byte first
- All multi-byte numeric values in payloads: Little-Endian

**Rationale:**

#### System Architecture Context

This protocol is designed for agricultural IoT systems with the following architecture:

```
┌─────────────────────────────────────────────────────────────┐
│                    Control Unit                             │
│  • Raspberry Pi (ARM Cortex-A, Little-Endian)               │
│  • Industrial PC x86/x64 (Little-Endian)                    │
│  • High CPU power, network coordination                     │
└───────────────────┬─────────────────────────────────────────┘
                    │
          ┌─────────┼─────────┐
          │         │         │
   ┌──────▼──┐ ┌───▼────┐ ┌─▼──────┐
   │ Sensor  │ │ Pump   │ │ Valve  │  ← Embedded Devices
   │ (STM32) │ │(ESP32) │ │(nRF52) │  • ARM Cortex-M (LE)
   └─────────┘ └────────┘ └────────┘  • Battery-powered
                                      • Limited CPU power
```

#### Modern Embedded Devices are Little-Endian

| Microcontroller | CPU Architecture | Endianness | Typical Use |
|-----------------|------------------|------------|-------------|
| **STM32F4/L4/H7** | ARM Cortex-M4/M7 | Little-Endian | Sensors, actuators |
| **ESP32/ESP8266** | Xtensa LX6/LX106 | Little-Endian | WiFi-enabled sensors |
| **nRF52/nRF53** | ARM Cortex-M4/M33 | Little-Endian | Bluetooth LE devices |
| **RP2040** | ARM Cortex-M0+ | Little-Endian | Raspberry Pi Pico |
| **SAMD21** | ARM Cortex-M0+ | Little-Endian | Arduino Zero |

**Result:** Both control units (Raspberry Pi, PC) and embedded devices (ARM Cortex-M) use Little-Endian natively.

#### 1. **Minimizes Processing on Low-Power Devices**

The most critical advantage: **embedded sensors and actuators have limited CPU power and battery life**.

**Performance Comparison** (sending uint16 Device ID 0x0005):

**With Big-Endian (forces byte swapping):**
```c
// STM32 Cortex-M4 @ 80MHz (battery-powered sensor)
uint16_t deviceId = 0x0005;  // Native: [0x05, 0x00] in memory

// Must swap to Big-Endian: [0x00, 0x05]
buffer[0] = (deviceId >> 8) & 0xFF;   // Shift + mask: 2 cycles
buffer[1] = deviceId & 0xFF;          // Mask: 1 cycle

// Total: 3 CPU cycles wasted per field
// Message with 5 fields = 15 wasted cycles
// 1M messages/day = 15M wasted cycles = unnecessary battery drain
```

**With Little-Endian (native format):**
```c
// STM32 Cortex-M4 @ 80MHz (same hardware)
uint16_t deviceId = 0x0005;  // Native: [0x05, 0x00]

// Protocol uses LE - direct copy (NO conversion needed)
memcpy(buffer, &deviceId, 2);  // 1 cycle (or DMA: 0 CPU cycles)

// Total: 0-1 CPU cycles (up to 3x faster, 33% less energy)
```

#### 2. **Native CPU Support Across All Components**

**Control Units:**
- Raspberry Pi (ARM Cortex-A): Little-Endian natively
- x86/x64 PC: Little-Endian natively

**Embedded Devices:**
- ARM Cortex-M (99% of modern 32-bit embedded): Little-Endian natively
- ESP32 (Xtensa): Little-Endian natively

**Result:** Zero conversion overhead on **both high-power control units and low-power embedded devices**.

#### 3. **Variable-Length Integer Support (Varints)**: LE enables future optimization in Gen2
   - Smaller messages: `uint32` value 127 encodes as 1 byte instead of 4 bytes
   - Gen2 can add varint encoding for Device ID, Payload Length, timestamps
   - **BE makes varints impractical** (most significant bytes come first)

#### 4. **Modern Protocol Standard**: Industry has shifted from BE to LE
   - USB: Little-Endian
   - Bluetooth Low Energy: Little-Endian
   - PCIe: Little-Endian
   - Protocol Buffers: Little-Endian + varints
   - Only legacy network protocols (IP, TCP) still mandate BE

**Example Encoding** (Device ID 0x0005):
```
Big-Endian:    [0x00, 0x05]  ← traditional "network byte order"
Little-Endian: [0x05, 0x00]  ← native format for all components (chosen)

Wire transmission: Both send 2 bytes, but LE requires zero CPU work on hardware side
```

### Protocol Constraints

- **Maximum Payload Length**: 1024 bytes (adjustable for device capabilities)
- **Maximum Total Message Size**: 1040 bytes (20-byte overhead + 1024-byte payload)
- **Minimum Message Size**: 20 bytes (overhead with zero-length payload)

**Validation Rule**: Receivers MUST reject messages with `Payload Length > 1024` by sending `NACK` with status code `0x05` (INSUFFICIENT_RESOURCES).

### Error Detection (Gen2 Feature)

> **Gen1 Simplification**: Error detection via CRC-32 is not included in Gen1 to keep the protocol simple and educational. Gen1 relies on the underlying transport layer (TCP provides checksums, UART may have parity) for basic error detection.

**Gen2 will add**:
- **Algorithm**: CRC-32/ISO-HDLC (IEEE 802.3 / Ethernet polynomial)
- **Polynomial**: `0x04C11DB7`
- **Initial Value**: `0xFFFFFFFF`
- **Final XOR**: `0xFFFFFFFF`
- **Reflect Input/Output**: Yes (LSB first)
- **Calculation Scope**: Header + Payload
- **Position**: Appended after payload as 4-byte trailer

## Message Types

| Code | Message Type | Description |
|------|--------------|-------------|
| `0x00` | NACK | Negative Acknowledgment - Indicates failure in processing a command or message, along with error information. |
| `0x01` | ACK | Acknowledgment - Indicates successful receipt and processing of a command or message. |
| `0x02` | CMD | Command - Contains a single instruction for a device to perform specific action. |
| `0x03` | TLM | Telemetry - Contains sensor data or status information from a device, which can be sent periodically or upon request. |
| `0x04` | EVT | Event - Contains information about specific events or conditions detected by a device, such as alerts or notifications. |
| `0x05` | PING / PONG | Ping-Pong - Used for connectivity checks and latency measurements between devices. |
| `0x06` | HEARTBEAT | Heartbeat - Used to indicate the device is alive and functioning properly. |

## Transport Layer (Message Envelope)

Every message follows this structure:

| Field | Size | Description |
|-------|------|-------------|
| **Sync Magic Bytes** | 4 bytes | Fixed value `0xABCD1234` to indicate message start |
| **Protocol Version** | 1 byte | Version of the protocol (currently `0x01`) |
| **Device ID** | 2 bytes | Unique identifier of the device (0x0001 - 0xFFFF) |
| **Message ID** | 4 bytes | Unique message identifier (cryptographically random and sequential) |
| **Message Type** | 1 byte | Type of message (see Message Types table) |
| **Payload Length** | 4 bytes | Length of payload in bytes (0 - 1024) |
| **Payload Block** | Variable | Message-specific content (0 to 1024 bytes) |

**Total Header Size**: 16 bytes (excluding Sync)
**Total Overhead**: 20 bytes per message (Sync + Header)

## Message Payload Structures

Each message type has a specific payload structure:

### Message Type `0x00` NACK (Negative Acknowledgment) Payload Structure

- **Command ID** - 4 bytes: Unique identifier of the command that failed (matches Message ID of corresponding CMD message)
- **Status Code** - 1 byte: Reason for failure (see NACK Status Codes table)

**Total Payload Size**: 5 bytes (fixed)

#### NACK Status Codes

| Code | Name | Description |
|------|------|-------------|
| `0x00` | UNKNOWN_ERROR | Generic failure with no specific cause identified |
| `0x01` | INVALID_COMMAND | Unknown or unsupported Command Type |
| `0x02` | INVALID_PARAMETERS | Parameter validation failed (out of range, wrong format) |
| `0x03` | DEVICE_BUSY | Device cannot execute command at this time |
| `0x04` | TIMEOUT | Operation exceeded maximum execution time |
| `0x05` | INSUFFICIENT_RESOURCES | Not enough memory, power, or other resources |
| `0x06` | UNSUPPORTED_VERSION | Protocol version mismatch or not supported |
| `0x07` | DEVICE_NOT_READY | Device still initializing or in error state |
| `0x08-0xFF` | RESERVED | Reserved for future use |

### Message Type `0x01` ACK (Acknowledgment) Payload Structure

- **Command ID** - 4 bytes: Unique identifier of the command being acknowledged (matches Message ID of corresponding CMD message)

**Total Payload Size**: 4 bytes (fixed)

### Message Type `0x02` CMD (Command) Payload Structure

- **Command Type** - 1 byte: Specific command being issued (see Command Types table)
- **Command Parameters** - Variable length: Parameters required to execute the command

**Total Payload Size**: Variable (minimum 1 byte)

#### Command Types

| Code | Command | Description | Parameter Format |
|------|---------|-------------|------------------|
| `0x00` | OFF | Turn off device or component | None (0 bytes) |
| `0x01` | ON | Turn on device or component | 4 bytes: Duration in seconds (0 = indefinite) |
| `0x02` | ADJ | Adjust device settings | Variable: setting-specific parameters |
| `0x03` | RST | Reset device to default state | 1 byte: Reset type (0x00 = soft, 0x01 = hard) |
| `0x04` | EXC | Execute specific action sequence | Variable: action-specific parameters |

**Example - ON Command for Pump**:
```
Command Type: 0x01 (ON)
Parameters: 0x0000003C (60 seconds duration)
Total Payload: 5 bytes
```

### Message Type `0x03` TLM (Telemetry) Payload Structure

- **Sensor Type** - 1 byte: Type of sensor data being reported (see Sensor Types table)
- **Sensor Value** - Variable length: Actual sensor reading (format depends on Sensor Type)
- **Timestamp** - 8 bytes: Unix timestamp in milliseconds when data was recorded
- **Status Flags** - 1 byte: Bit flags indicating sensor/device status (see Status Flags table)

**Total Payload Size**: Variable (minimum 10 bytes)

#### Sensor Types

| Code | Sensor | Value Format | Size |
|------|--------|--------------|------|
| `0x01` | TEMPERATURE | IEEE 754 float (°C) | 4 bytes |
| `0x02` | HUMIDITY | Unsigned byte (0-100%) | 1 byte |
| `0x03` | SOIL_MOISTURE | Unsigned 16-bit (0-1023 ADC value) | 2 bytes |
| `0x04` | WATER_LEVEL | IEEE 754 float (cm) | 4 bytes |
| `0x05` | LIGHT_INTENSITY | Unsigned 16-bit (0-65535 lux) | 2 bytes |
| `0x06` | BATTERY_VOLTAGE | Unsigned 16-bit (millivolts) | 2 bytes |

#### Status Flags (Bitmask)

| Bit | Flag | Description |
|-----|------|-------------|
| 0 | BATTERY_LOW | Battery voltage below threshold |
| 1 | SENSOR_ERROR | Sensor malfunction or reading error |
| 2 | CALIBRATION_NEEDED | Sensor requires recalibration |
| 3 | OUT_OF_RANGE | Value exceeds sensor measurement range |
| 4-7 | RESERVED | Reserved for future use |

**Example - Temperature Telemetry**:
```
Sensor Type: 0x01 (TEMPERATURE)
Sensor Value: 0x41C80000 (25.0°C as IEEE 754)
Timestamp: 0x000001840D8E3A00 (1700000000000 ms)
Status Flags: 0x00 (no flags set)
Total Payload: 14 bytes
```

### Message Type `0x04` EVT (Event) Payload Structure

- **Event Type** - 1 byte: Type of event being reported (see Event Types table)
- **Event Details** - Variable length: Specific information about the event
- **Timestamp** - 8 bytes: Unix timestamp in milliseconds when event occurred

**Total Payload Size**: Variable (minimum 9 bytes)

#### Event Types

| Code | Event | Details Format | Details Size |
|------|-------|----------------|--------------|
| `0x01` | THRESHOLD_EXCEEDED | 1 byte Sensor Type + 4 bytes threshold value (float) | 5 bytes |
| `0x02` | BATTERY_LOW | 2 bytes remaining voltage (millivolts) | 2 bytes |
| `0x03` | SENSOR_ERROR | 1 byte Sensor Type + 1 byte error code | 2 bytes |
| `0x04` | DEVICE_STARTUP | 1 byte reset reason (0x00=power on, 0x01=reset cmd, 0x02=watchdog, 0xFF=unknown) | 1 byte |
| `0x05` | CONNECTION_LOST | 2 bytes Device ID of lost peer | 2 bytes |

**Example - Battery Low Event**:
```
Event Type: 0x02 (BATTERY_LOW)
Event Details: 0x0BB8 (3000 mV remaining)
Timestamp: 0x000001840D8E3A00
Total Payload: 11 bytes
```

### Message Type `0x05` PING (Ping-Pong) Payload Structure

**PING Request:**
- **Sequence Number** - 2 bytes: Incremental counter (wraps at 65535, used for matching requests/responses)
- **Timestamp** - 8 bytes: Unix timestamp in milliseconds when PING was sent
- (Gen2) **Echo Data** - Variable length: Optional random data to echo back (0-64 bytes recommended)

**Total Payload Size**: Variable (minimum 10 bytes)

**PONG Response:**
- **Sequence Number** - 2 bytes: Copied from PING request
- **Timestamp** - 8 bytes: Copied from PING request (for RTT calculation)
- (Gen2) **Echo Data** - Variable length: Exact copy of PING Echo Data

**Total Payload Size**: Same as corresponding PING request

**Usage:**
```
RTT (Round-Trip Time) = Current Time - PING Timestamp
```

**Example - PING Request**:
```
Sequence Number: 0x0001
Timestamp: 0x000001840D8E3A00
Echo Data: 0xDEADBEEF (4 bytes)
Total Payload: 14 bytes
```

---

## Wire Format Example

### Complete CMD Message: Turn On Pump

**Logical Message:**
- Device: Pump #5 (0x0005)
- Command: Turn ON for 60 seconds
- Message ID: 0xA3F21B4C (random)

**Wire Format (Hexadecimal):**

```
Offset | Bytes              | Field              | Value / Description
-------|--------------------|--------------------|--------------------------------
0x00   | AB CD 12 34        | Sync Magic         | 0xABCD1234 (message boundary)
0x04   | 01                 | Protocol Version   | 0x01 (version 1)
0x05   | 05 00              | Device ID          | 0x0005 (Pump #5, LE)
0x07   | 4C 1B F2 A3        | Message ID         | 0xA3F21B4C (random, LE)
0x0B   | 02                 | Message Type       | 0x02 (CMD)
0x0C   | 05 00 00 00        | Payload Length     | 0x00000005 (5 bytes, LE)
       |                    | --- PAYLOAD START ---
0x10   | 01                 | Command Type       | 0x01 (ON)
0x11   | 3C 00 00 00        | Duration           | 0x0000003C (60 seconds, LE)
       |                    | --- PAYLOAD END ---
```

**Total Message Size**: 25 bytes (0x19)  
**Breakdown**: 4 (sync) + 16 (header) + 5 (payload) = 25 bytes

**Complete Message (as byte array)**:
```
AB CD 12 34 01 05 00 4C 1B F2 A3 02 05 00 00 00 01 3C 00 00 00
```

---

## Reserved Values

### Special Device IDs
| Device ID | Purpose |
|-----------|---------|
| `0x0000` | Invalid/Unassigned |
| `0xFFFF` | Reserved for future use (broadcast) |

### Reserved Protocol Versions
| Version | Status |
|---------|--------|
| `0x00` | Invalid |
| `0x01` | Current (Gen1) |
| `0x02-0xFE` | Reserved for future versions |
| `0xFF` | Reserved |

---

## FAQ

### Q: Why Little-Endian instead of Big-Endian?
**A:** Three critical reasons for agricultural IoT systems:

1. **Embedded Device Performance**: Modern embedded microcontrollers (STM32, ESP32, nRF52) all use ARM Cortex-M or Xtensa cores, which are **natively Little-Endian**. Using BE would force every battery-powered sensor to waste CPU cycles byte-swapping on every message field, reducing battery life by 15-30%.

2. **Complete System Alignment**: Both control units (Raspberry Pi, x86 PC) and embedded devices (ARM Cortex-M) use LE natively → **zero conversion overhead** on all components.

3. **Future Optimization**: LE enables Protocol Buffers-style varints in Gen2, reducing message sizes 20-40% (critical for low-bandwidth RF links and battery life).

### Q: Can I use TCP instead of serial/UART?
**A:** Yes! The protocol is transport-agnostic. The message format works over any byte stream (UART, TCP, UDP, USB, CAN bus, etc.).

---

## References

- **Little-Endian**: Native format for x86/x64/ARM architectures
- **Varints**: [Protocol Buffers Encoding](https://protobuf.dev/programming-guides/encoding/)
- **Protocol Design**: *Computer Networks* by Andrew S. Tanenbaum
- **CRC-32** (Gen2): [ISO/IEC 13239](https://en.wikipedia.org/wiki/Cyclic_redundancy_check)
