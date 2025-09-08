# System Sounds Volume Controller (SystemSoundsVolumeTray)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A simple and lightweight Windows utility that allows you to control the volume of **only** system sounds (notifications, clicks, etc.) without affecting the master volume or the volume of other applications (like browsers, games, or media players).

## Key Features

- **Granular Volume Control:** Manage the "System Sounds" audio session volume separately from everything else.
- **System Tray Integration:** The application runs in the background and is accessible via a tray icon.
- **Smart Popup UI:**
    - A **left-click** on the icon shows a convenient volume slider.
    - The window appears **top-most** (above all other windows, including the taskbar) directly above the tray icon.
    - The window automatically hides when you click anywhere else.
- **Settings Persistence:** The selected volume level is saved and restored on application restart.
- **Windows Theme Support:** The UI automatically adapts to your OS's light or dark theme.
- **No Dependencies:** Uses only native Windows Core Audio APIs without any third-party libraries.

## Installation

1.  Go to the [**Releases**](https://github.com/i-s-rusakov/SystemSoundsVolumeTray/releases) page of this project.
2.  Download the latest version.

Two options are available:

#### Framework-Dependent (Recommended)
- **Small download size.**
- **Requires .NET 8 Desktop Runtime.** If you don't have it, Windows will automatically prompt you to install it on the first run.
- Download the archive, extract it to any folder, and run `SystemSoundsVolumeTray.exe`.

#### Self-Contained
- **Large file size** (>100 MB).
- **Requires no installation.** Works "out of the box" on any 64-bit Windows 10/11.
- Download `SystemSoundsVolumeTray.exe`, place it in any folder, and run it.

## Building from Source

If you want to compile the project yourself:

1.  Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2.  Clone the repository: `git clone https://github.com/i-s-rusakov/SystemSoundsVolumeTray.git`
3.  Navigate to the project directory.
4.  To build the **Framework-Dependent** version, run:
    ```
    dotnet publish -c Release
    ```
    The output files will be in the `bin\Release\net8.0-windows...\publish` folder.

## License

This project is licensed under the **MIT License**. This means you are free to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the software.

See the [LICENSE](LICENSE) file for details.

## Acknowledgements

The application icon is based on icons from the **Material Design Icons** library by Pictogrammers.
- **Website:** [pictogrammers.com/library/mdi/](https://pictogrammers.com/library/mdi/)
- **License:** Apache License 2.0
