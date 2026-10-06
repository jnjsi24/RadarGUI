# RadarGUI

RadarGUI is a C# Windows Forms desktop application developed as a college project for a robotics class. It serves as a PC-side graphical interface designed to visualize real-time distance and sweep data from a physical radar/motion-detection system (such as an Arduino paired with an ultrasonic sensor and a rotating servo motor).

---

## System Architecture

The application acts purely as a graphical front-end data visualizer. It does not control or drive the scanning hardware; it simply processes and maps metrics received via serial communication.

```text
Distance Sensor + Rotating Servo
              │
              ▼
       Arduino / Device
              │
       Serial USB @ 9600 Baud
              │
              ▼
  RadarGUI Windows Application (This Repo)
              │
              ▼
  Radar Visualization + Motion Indication
```

---

## Features

* **Real-Time Radar Display:** Renders a circular radar scope complete with 5 concentric distance range circles, 12 radial grid lines, and directional compass headings (North, South, East, West).
* **Phosphor Trail Simulation:** Stores up to 100 historical sweep data points, gradually fading older lines in lime green to mimic an authentic legacy radar screen.
* **Threshold Motion Detection:** Compares consecutive distance data packets. If a delta change greater than 10.0 units is triggered, the scan line changes to bright red, a "Motion Detected" warning flags, and the estimated cardinal direction is calculated and displayed.
* **Start Control:** Provides a simple manual START toggle to initialize the connection loop.

---

## Tech Stack and Specs

* **Language:** C#
* **Framework:** .NET Framework 4.7.2
* **UI Framework:** Windows Forms (WinForms)
* **Project Type:** Windows GUI Executable
* **Target Environment:** Windows 10/11
* **Window Canvas Resolution:** ~683 x 339 pixels

---

## Expected Serial Data Format

The application interfaces across COM3 at 9600 baud. It expects string messages terminated by a newline (\n), consisting of three comma-separated values:

```text
[angle],[prevDistance],[distance]
```

* **angle** — Physical angle of the sensor in degrees (0 to 360).
* **prevDistance** — The baseline distance measurement from the previous step.
* **distance** — The current/live distance measurement.

**Example Input Packet:**
```csv
90,45.2,32.1
```
Because the delta (45.2 - 32.1 = 13.1) is greater than 10.0, this packet triggers a red motion alert line at 90 degrees.

---

## Directory Structure

```text
├── RadarGUI.sln             # Visual Studio Solution File
└── RadarGUI/
    ├── RadarGUI.csproj      # Build and dependency target properties
    ├── App.config           # App configuration mapping (.NET 4.7.2)
    ├── Program.cs           # Main execution entry point
    ├── Form1.cs             # Core application logic, Serial handling & GDI+ Canvas drawing
    ├── Form1.Designer.cs    # Layout UI design parameters
    ├── Form1.resx           # Application resource properties
    └── Properties/          # Build properties and assembly profiles
```

---

## Limitations and Notes

* **No Hardware Firmware Included:** The code running on the microcontroller/Arduino side is not contained in this repository.
* **Hardcoded COM Port:** The connection port is locked entirely to COM3. Operating on any other COM configuration requires altering the Form1.cs constructor file manually.
* **Basic Filter Tracking:** Motion tracking relies on a basic differential threshold check. It does not filter signal background noise, average adjacent metrics, or track distinct target entities.
* **Manual Cleanup Warning:** The program instantiates structural Pen and Font graphic configurations inside drawing loops without explicit resource disposal, which may increase GDI object footprints during long uninterrupted sessions.
