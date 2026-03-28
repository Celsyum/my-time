# MyTime

MyTime is a small Windows desktop time tracker built with C# and WPF.

It is designed for a simple single-task workflow:
- press Start to begin tracking time
- press Stop to finish the timer
- enter a required description
- save the activity to history

This is also a vibe-coded project: it was built through fast AI-assisted iteration, then tightened into a working Windows app with installer support, tray behavior, persistence, and automated builds.

## Features

- Start and stop a single running activity
- Live elapsed-time display while the timer is active
- Required description before saving a stopped activity
- History list of all recorded activities
- Group history by day, week, or month
- Show total tracked time per selected period
- Save each activity as a separate JSON file
- Store app data in LocalAppData instead of Program Files
- Settings page with retention period in months
- Delete expired activity files only on app startup
- Minimize to system tray when window is closed
- Full app exit support through tray exit or Quit App button
- Automatic exit handling for active timers on full application exit
- 15-minute tray reminders while work is in progress
- Custom tray/application icon support
- Windows installer generation with Inno Setup

## How It Works

### Timer flow

1. Click Start.
2. The elapsed timer updates in real time.
3. Click Stop.
4. A description input appears.
5. Empty descriptions are rejected.
6. Click Save to persist the activity.

### Close vs exit behavior

- Closing the window does not quit the app. It minimizes to tray and keeps the running timer active.
- Full application exit is separate.
- If the app exits completely while an activity is running, MyTime auto-stops that activity and saves it with exit timestamp text in the description.

### Notifications

- While an activity is running, the app shows a tray notification every 15 minutes with this text:
	- Work Activity in Progress...

## Data Storage

MyTime stores user data under LocalAppData:

- Base path: `%LOCALAPPDATA%\\MyTime`
- Activity history: `%LOCALAPPDATA%\\MyTime\\history`
- Settings file: `%LOCALAPPDATA%\\MyTime\\settings.json`

Each recorded activity is saved as its own JSON file.

This avoids Windows permission issues that happen when installed apps try to write into Program Files.

## Tech Stack

- C#
- .NET 10
- WPF
- WinForms NotifyIcon for tray integration
- Inno Setup for installer packaging

## Project Structure

```text
.
├── installer/
│   └── MyTime.iss
├── src/
│   └── MyTime/
│       ├── Assets/
│       ├── Models/
│       ├── Services/
│       ├── App.xaml
│       ├── App.xaml.cs
│       ├── MainWindow.xaml
│       ├── MainWindow.xaml.cs
│       └── MyTime.csproj
├── .github/
│   └── workflows/
├── MyTime.sln
└── README.md
```

## Local Development

### Prerequisites

- Windows
- .NET SDK 10
- Inno Setup 6 if you want to build the installer locally

### Build

```powershell
dotnet build .\MyTime.sln
```

### Run from source

```powershell
dotnet run --project .\src\MyTime\MyTime.csproj
```

### Publish release build

```powershell
dotnet publish .\src\MyTime\MyTime.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false
```

Publish output:

- `src/MyTime/bin/Release/net10.0-windows/win-x64/publish`

### Build installer with Inno Setup

```powershell
".\Inno Setup 6\ISCC.exe" ".\installer\MyTime.iss"
```

Installer output:

- `dist/MyTime-Setup.exe`

## Custom Icon

Place the icon file here:

- `src/MyTime/Assets/Icons/mytime.ico`

Recommended sizes inside the `.ico` file:

- 16x16
- 24x24
- 32x32
- 48x48
- 64x64
- 128x128
- 256x256

For Start menu and Installed Apps visibility, 48x48 and 256x256 matter most.

## GitHub Actions

This repository includes GitHub Actions workflows for:

- continuous build validation on pushes and pull requests
- Windows publish artifact generation
- tagged GitHub release creation with packaged artifacts

## Release Flow

Recommended GitHub release flow:

1. Commit changes.
2. Push to your default branch.
3. Create and push a version tag such as `v1.0.0`.
4. GitHub Actions builds the app, builds the installer, and attaches artifacts to the GitHub release.

Example:

```powershell
git tag v1.0.0
git push origin v1.0.0
```

## Notes

- This project is Windows-only by design.
- Runtime user data is intentionally stored outside the install directory.
- `bin/`, `obj/`, installer output, and local editor state should not be committed to Git.