; MyTime installer script for Inno Setup 6
; Build command (if ISCC is on PATH):
; iscc installer\MyTime.iss

#define MyAppName "MyTime"
#ifndef MyAppVersion
	#define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "MyTime"
#define MyAppExeName "MyTime.exe"
#define PublishDir "..\src\MyTime\bin\Release\net10.0-windows\win-x64\publish"

[Setup]
AppId={{4E94E16A-149A-4B8E-9B7B-0A2A4F8CF8AA}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\dist
OutputBaseFilename=MyTime-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
UninstallDisplayIcon={app}\Assets\Icons\mytime.ico

#ifexist "..\src\MyTime\Assets\Icons\mytime.ico"
SetupIconFile=..\src\MyTime\Assets\Icons\mytime.ico
#endif

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\Assets\Icons\mytime.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; IconFilename: "{app}\Assets\Icons\mytime.ico"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent
