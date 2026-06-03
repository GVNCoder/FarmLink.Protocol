# FarmLink Protocol Gen1

So the FarmLink Protocol Gen1 is a communication protocol designed for efficient and reliable data exchange
between devices in an agricultural environment.

It is structured to ensure data integrity, security, and scalability while being adaptable to various types of messages and payloads.

Its versioned design allows for future enhancements and modifications without braking compatibility with existing implementations.
So based on the message version, we can pick the right parser and handler for the message, ensuring that the system can evolve over time while maintaining support for older message formats.

Basically, we want to have a protocol that is robust, and can provide us an ability to communicate between different devices in the farm,
such as sensors (real-time telemetry or polling-based), actuators (e.g., pumps), and control systems.

Telemetry messages can be used to report sensor data from device, or to request data from a device.
Command messages can be used to send instructions to devices, such as turning on a pump or adjusting settings.

Each command message must have a corresponding response message that indicates the success or failure of the command execution, along with any relevant data or error information.
For example:
	Send self-contained* command message to turn on a pump =>
	Pump responds with ACK message if the command was successful.

*(may have a full set of parameters, for example, pump ID, duration, flow rate, how many liters and other relevant settings)

The goal is to implement the following features in the protocol:
- Transport Layer (message envelope)
- Auto-synchronization and alignment using magic bytes to indicate the start of a message
- Partial message handling to allow processing of messages that may be received in fragments
- Length validation to protect against allocation attacks
- Versioning to ensure compatibility with future protocol updates
- Error detection using Cyclic Redundancy Check (CRC) to ensure data integrity

## Message Types

| Message Type | Description |
|--------------|-------------|
| `0x01` ACK (Acknowledgment) | Indicates successful receipt and processing of a command or message. |
| `0x00` NACK (Negative Acknowledgment) | Indicates failure in processing a command or message, along with error information. |
| `0x02` CMD (Command) | Contains a single instruction for a device to perform specific action. |
| `0x03` TLM (Telemetry) | Contains sensor data or status information from a device, which can be sent periodically or upon request. |
| `0x04` EVT (Event) | Contains information about specific events or conditions detected by a device, such as alerts or notifications. |
| `0x05` SETUP (Setup) | Contains configuration parameters or settings for a device, which can be used to initialize or update device settings. |
| `0x06` PING-PONG (Ping-Pong) | Used for connectivity checks and latency measurements between devices. |

## Message Structures

Each message consists of a transport layer (message envelope) and a payload block.
The transport layer contains metadata about the message, while the payload block contains the actual content of the message.
Each message payload block is unique to the message type and can be structured differently.

### Message Type `0x01` ACK (Acknowledgment) Payload Structure
- **Command ID** - 4 bytes indicating the unique identifier of the command being acknowledged (matches the Message ID of the corresponding CMD message)

### Message Type `0x00` NACK (Negative Acknowledgment) Payload Structure
- **Command ID** - 4 bytes indicating the unique identifier of the command that failed (matches the Message ID of the corresponding CMD message)
- **Status Code** - 1 byte indicating the reason for the failure (e.g., invalid parameters, device busy, etc.)

### Message Type `0x02` CMD (Command) Payload Structure
- **Command Type** - 1 byte indicating the specific command being issued (e.g., turn on, turn off, adjust settings, etc.)
- **Command Parameters** - Variable length based on the Command Type and Device ID, containing the parameters required to execute the command (e.g., duration, flow rate, etc.)

#### Command Message Types
| Command Type | Description |
|--------------|-------------|
| `0x01` ON (Turn On) | Command to turn on a device or component. |
| `0x00` OFF (Turn Off) | Command to turn off a device or component. |
| `0x02` ADJ (Adjust Settings) | Command to adjust settings of a device, such as flow rate or duration. |
| `0x03` RST (Reset) | Command to reset a device to its default state. |
| `0x04` EXC (Execute) | Command to execute a specific action or sequence of actions on a device. |

### Message Type `0x03` TLM (Telemetry) Payload Structure
- **Sensor Type** - 1 byte indicating the type of sensor data being reported (e.g., temperature, humidity, water level, etc.)
- **Sensor Value** - Variable length based on the Sensor Type, containing the actual sensor data value (e.g., temperature in Celsius, humidity percentage, etc.)
- **Timestamp** - 8 bytes indicating the time the telemetry data was recorded, represented as a Unix timestamp in milliseconds
- **Status Flags** - 1 byte containing bit flags that indicate the status of the sensor or device (e.g., battery low, sensor error, etc.)

### Message Type `0x04` EVT (Event) Payload Structure
- **Event Type** - 1 byte indicating the type of event being reported (e.g., alert, notification, etc.)
- **Event Details** - Variable length based on the Event Type, containing specific information about the event (e.g., water level low, temperature threshold exceeded, etc.)
- **Timestamp** - 8 bytes indicating the time the event was recorded, represented as a Unix timestamp in milliseconds

### Message Type `0x05` SETUP (Setup) Payload Structure
- **Setup Type** - 1 byte indicating the type of setup being performed (e.g., device configuration, sensor calibration, etc.)
- **Configuration Parameters** - Variable length based on the specific setup being performed, containing the parameters required to configure a device (e.g., sensor calibration settings, device operating modes, etc.)

## Transport Layer (message envelope)
- **Sync / Alignment Magic Bytes** - 4 bytes (e.g., `0xABCD1234`) to indicate the start of a message
- **Header: Protocol Version** - 1 byte indicating the version of the protocol
- **Header: Device ID** - 2 bytes indicating the unique identifier of the device (there must be a device type registered in the system that is associated with the device ID)
- **Header: Message ID** - 4 bytes unique identifier for the message, cryptographically random to avoid collisions
- **Header: Message Type** - 1 byte indicating the type of message (e.g., command, response, event)
- **Header: Payload Length** - 4 bytes indicating the length of the payload in bytes
- **Payload Block** - Variable length based on the Payload Length field, containing the actual message content
- **Cyclic Redundancy Check (CRC)** - 4 bytes for error detection and integrity verification

