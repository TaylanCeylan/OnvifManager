<div align="center">

# ONVIF Camera Manager

**A Windows desktop application for discovering, configuring, and viewing ONVIF cameras.**

Built with C#, .NET 10, Windows Forms, ONVIF, and LibVLCSharp.

</div>

---

## Overview

ONVIF Camera Manager helps you find cameras on a local network, save their connection details, and open live RTSP video streams. Camera discovery and device operations run in a companion process so the main interface stays responsive during network requests.

The application interface is currently in Turkish.

## Features

- Discover ONVIF cameras on the local network.
- Add and edit camera names, addresses, ONVIF ports, and credentials.
- Retrieve a camera's ONVIF media profiles and choose a stream profile.
- Open a live RTSP stream in a separate LibVLC-based viewer.
- Use on-screen pan, tilt, and zoom controls when the camera supports PTZ.
- Check camera connectivity and capture individual or batch snapshots.
- Save camera settings between sessions, with passwords protected for the current Windows account.

## Requirements

- Windows 10 or later.
- .NET 10 SDK to build from source, or the .NET 10 Windows Desktop Runtime to run a framework-dependent build.
- Network access to the cameras. ONVIF discovery uses local-network discovery; camera ONVIF and RTSP services must be enabled and reachable.

## Build and run

Open a terminal in the repository directory and run:

```powershell
dotnet restore OnvifManager.slnx
dotnet build OnvifManager.slnx --configuration Release
```

Run the application from the build output:

```powershell
.\OnvifManager\bin\Release\net10.0-windows\OnvifManager.exe
```

The build places the `SPCConnector` companion application and its runtime dependencies beside the main application in an `SPCConnector` subdirectory. Keep that directory with `OnvifManager.exe`; launching the main executable on its own will prevent discovery, profile lookup, snapshots, and live view from working.

You can also start the main project directly with:

```powershell
dotnet run --project OnvifManager\OnvifManager.csproj --configuration Release
```

## First-time setup

1. Start **ONVIF Camera Manager** and open **Kamera yönetimi** (Camera Management).
2. Choose **Ağda ONVIF kamera ara** (Search for ONVIF cameras) to scan the local network, or add a camera manually.
3. Select a camera and enter its ONVIF username and password. Discovered cameras are added without default credentials.
4. Return to **Canlı izleme** (Live View), select a camera, and retrieve its profiles with **Profilleri getir**.
5. Choose a profile and open the live view. PTZ controls appear when a stream connection is established.

Some cameras use a dedicated ONVIF account or a port that differs from their web interface. Check the camera's configuration if discovery or profile lookup fails.

## Data and privacy

- Camera settings are stored in `%LOCALAPPDATA%\OnvifManager\cameras.json`.
- Passwords are protected with Windows DPAPI and can be decrypted only by the Windows account that saved them. If the settings file is moved to another account or computer, enter the passwords again.
- Batch snapshots are written under `SPCConnector\Snapshots\<batch-id>` beside the connector executable. Single-camera snapshots are saved to the location selected in the save dialog.
- Camera streams and credentials are sent to the camera and passed locally between the two application processes. The application does not upload camera data to a hosted service.

## Project structure

| Project | Purpose |
| --- | --- |
| `OnvifManager` | Main Windows Forms application, camera management, persistence, and process launcher. |
| `SPCConnector` | ONVIF discovery, profile lookup, connection checks, snapshots, RTSP playback, and PTZ operations. |

The main application and connector exchange request and response data through Windows memory-mapped files. Both projects target `net10.0-windows`.

```mermaid
flowchart LR
    User[Operator] --> UI[OnvifManager]
    UI -->|Launches and exchanges requests| Connector[SPCConnector]
    Connector -->|ONVIF discovery, profiles, snapshots, and PTZ| Camera[(IP camera)]
    Camera -->|RTSP stream| VLC[LibVLC player in SPCConnector]
```

## Dependencies

- [LibVLCSharp](https://www.nuget.org/packages/LibVLCSharp) and [LibVLCSharp.WinForms](https://www.nuget.org/packages/LibVLCSharp.WinForms) for live video playback.
- [OnvifLib](https://www.nuget.org/packages/OnvifLib) for ONVIF device, media, and discovery operations.
- [VideoLAN.LibVLC.Windows](https://www.nuget.org/packages/VideoLAN.LibVLC.Windows) for the LibVLC native runtime.

## Troubleshooting

### No cameras are discovered

- Make sure the computer and cameras are on the same reachable local network.
- Enable ONVIF on each camera and allow local discovery traffic through the network.
- If discovery is unavailable on your network, add the camera manually using its address and ONVIF port.

### Profiles or live video cannot be opened

- Confirm the ONVIF address, port, username, and password in Camera Management.
- Verify that the account is allowed to use ONVIF and that the selected profile supports streaming.
- Check that RTSP traffic is reachable from the computer running the application.
- Run the main application from its build output so the `SPCConnector` directory and LibVLC runtime files are available.

### PTZ controls do not move the camera

The camera and selected ONVIF profile must support PTZ, and the configured account must have PTZ permissions.

---

<div align="center">

Made for local ONVIF camera workflows.

</div>
