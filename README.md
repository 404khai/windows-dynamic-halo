# Windows Dynamic Halo

A Dynamic Island clone for Windows, built with WPF and .NET 8. This application creates a floating "pill" that expands to show media controls and visualizations when music is playing (Spotify, Chrome, etc.).

## Prerequisites

*   **OS**: Windows 10 (Build 19041+) or Windows 11.
*   **Runtime**: .NET 8.0 Desktop Runtime.
*   **Media**: A compatible media player (Spotify, Chrome, Edge, etc.) that supports Windows System Media Transport Controls (SMTC).

## Setup & Running

1.  **Clone or Download** the repository.
2.  Open a terminal in the project folder.
3.  **Build** the project:
    ```powershell
    dotnet build
    ```
4.  **Run** the application:
    ```powershell
    dotnet run
    ```
    *Note: If you encounter errors, ensure no other instance is running.*

## Troubleshooting Media Detection

If the island remains in the "Idle" (small pill) state while music is playing:

1.  **Browser Settings**:
    *   **Chrome/Edge**: Ensure "Hardware Media Key Handling" is ENABLED. Go to `chrome://flags/#hardware-media-key-handling` and set it to Enabled.
2.  **Windows Permissions**:
    *   Ensure Windows is not in "Do Not Disturb" or "Focus Assist" mode that might suppress notifications (though SMTC usually bypasses this).
3.  **Restart**:
    *   Sometimes the Windows internal media service gets stuck. Restarting your PC often resolves detection issues.
4.  **Active Tab**:
    *   Ensure the tab playing music is active or has recently been active. Some browsers put background tabs to sleep.

## Controls

*   **Idle**: Small pill at the top center.
*   **Active (Compact)**: Shows Album Art, Title, Artist, Controls, and Visualizer.
*   **Expanded**: Click the active pill to expand. Shows large art, seek slider (pink), and full controls.
    *   **Seek**: Drag the pink slider to change position.
    *   **Controls**: Previous, Play/Pause, Next.

## Development

Built using:
*   **WPF** (Windows Presentation Foundation)
*   **MVVM** Pattern
*   **WinRT APIs** (`Windows.Media.Control`) via `net8.0-windows10.0.19041.0`
