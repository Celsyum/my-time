# Tray/App Icon

Place your custom icon file here:

- File name: mytime.ico
- Path: src/MyTime/Assets/Icons/mytime.ico

At runtime, the app checks this output path first:

- Assets/Icons/mytime.ico

If it is not found, the app falls back to the default system icon.

Tips:
- Use a real Windows .ico file (not .png renamed to .ico).
- Include these sizes inside the .ico for best Windows coverage: 16x16, 24x24, 32x32, 48x48, 64x64, 128x128, 256x256.
- For Start menu and Installed Apps, 48x48 and 256x256 are especially important.
