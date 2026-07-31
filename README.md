# .NET Music Player

A feature-rich **WPF Desktop Music Player** built with **C#** and **.NET 10**, developed for the **C# Language and .NET** course at **Tel-Hai University**.

This application provides local audio playback, automatic library management, online iTunes metadata fetching, custom artwork galleries with slideshow capabilities, and folder scanning persistence.

---

## 🎵 Features

### 1. 🎧 Audio Playback & Control
- High-fidelity local MP3 playback using WPF's `MediaPlayer`.
- Interactive playback controls: **Play**, **Pause**, and **Stop**.
- Dynamic seek bar (`Slider`) displaying current elapsed time vs. total duration (`mm:ss`).
- Smooth volume adjustment slider.

### 2. 📂 Media Library & Persistence
- **Track Management**: Add individual `.mp3` files or remove existing tracks from your library.
- **Auto-Persistence**: Library data (titles, paths, artist info, custom artwork links) is automatically saved to `library.json` and restored on application startup.
- **Folder Scanning**: Define watched music directories in the Settings menu (`settings.json`). Recursively scans subdirectories for new MP3 files and syncs them directly to the main library.

### 3. 🌐 Online iTunes Metadata Integration
- Integrates with the **Apple iTunes Search API** (`ItunesService`) via non-blocking `HttpClient` and `System.Text.Json`.
- Automatically fetches song details:
  - **Artist Name**
  - **Album Name**
  - **High-Quality Album Artwork**
- Asynchronous API calls with `CancellationTokenSource` support to ensure responsive UI when switching tracks rapidly.
- Caches fetched metadata in the local JSON library to eliminate redundant API requests.

### 4. 🖼️ Custom Track Editing & Image Slideshow
- **Edit Window (`EditTrackWindow`)**: Attach custom local image files (`.jpg`, `.png`) to individual tracks using an MVVM architecture (`EditTrackViewModel` & `RelayCommand`).
- **Interactive Slideshow**: When multiple user images or album artworks are associated with a track, a built-in `DispatcherTimer` automatically cycles through images in a smooth slideshow during playback.

### 5. 🧪 Built-in API & Playback Tester
- Includes a dedicated `Tester.xaml` window for testing media playback and live iTunes API responses independently.

---

## 🏗️ Project Architecture & Technologies

- **Target Framework**: `.NET 10.0` (`net10.0-windows`)
- **UI Framework**: Windows Presentation Foundation (WPF)
- **Language**: C# 13
- **Architectural Patterns**:
  - **MVVM Pattern**: View Models, `RelayCommand`, `ObservableCollection` bindings for modal editing.
  - **Event-Driven Communications**: Event handlers and delegates (`OnScanCompleted`) for inter-window communication.
  - **Async/Await**: Non-blocking async API requests with cancellation tokens.
  - **JSON Serialization**: Built-in `System.Text.Json` for data persistence (`library.json`, `settings.json`).

---

## 📁 File & Directory Structure

```text
Telhai.DotNet.PlayerProject/
├── App.xaml / App.xaml.cs           # Application entry point
├── AppSettings.cs                  # Folder settings manager & JSON persistence
├── MusicPlayer.xaml / .cs           # Main Player UI window & playback logic
├── SettingsWindow.xaml / .cs        # Folder configuration & recursive scanning window
├── EditTrackWindow.xaml / .cs       # Track metadata & gallery edit dialog
├── EditTrackViewModel.cs            # MVVM ViewModel for editing track images
├── RelayCommand.cs                  # Generic ICommand implementation for MVVM
├── Models/
│   ├── MusicTrack.cs                # Core data model for audio tracks
│   ├── ItunesTrackInfo.cs           # Normalized iTunes track info model
│   ├── ItunesResultItem.cs          # iTunes API JSON response item model
│   └── ItunesSearchResponse.cs      # iTunes API root search response container
├── Services/
│   └── ItunesService.cs             # Async HTTP client for iTunes Search API
├── Tester.xaml / .cs                # Test interface for API & media verification
├── Pictures/                        # Default fallback artwork and application assets
└── Telhai.DotNet.PlayerProject.csproj # Project file (.NET 10 WPF configuration)
```

---

## 🚀 Getting Started

### Prerequisites
- **Operating System**: Windows 10 / Windows 11
- **SDK**: [.NET 10.0 SDK](https://dotnet.microsoft.com/download) (or compatible .NET SDK with Windows Desktop workload)
- **IDE**: Visual Studio 2022 / Rider / VS Code (with C# Extension)

### Building & Running

1. **Clone or Open the Project**:
   Open the solution or repository directory in Visual Studio or your preferred terminal.

2. **Build the Project**:
   ```bash
   dotnet build
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```

---

## 🛠️ How to Use

1. **Adding Music**:
   - Click **Add** to pick individual `.mp3` files.
   - Click **Settings** to add music folders and click **Scan** to automatically discover all tracks across your subdirectories.
2. **Playing Music**:
   - Double-click any track in the list (or select a track and press **Play**) to begin listening.
   - Use the slider bar to seek through the track or adjust volume.
3. **Editing Tracks & Gallery**:
   - Select a track and click **Edit** to open the track manager.
   - Add custom images to form a personalized slideshow for that track.
