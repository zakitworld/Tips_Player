# Tips Player

A modern, cross-platform audio/video player built with .NET MAUI featuring a sleek dark theme and intuitive controls.

## Features

- **Unified Media Player** - Play both audio and video files in a single application
- **Modern Dark Theme** - Minimalist design with indigo accent colors
- **Fullscreen Mode** - Expand video playback to fill the entire screen
- **Media Library** - Organize and browse your media files
- **Playback Controls** - Play, pause, skip, shuffle, and repeat functionality
- **Seek Bar** - Precise navigation through media with drag-to-seek
- **Volume Control** - Adjustable volume with mute toggle
- **Mini Player** - Persistent playback controls in library view
- **Cross-Platform** - Runs on Windows, Android, iOS, and macOS

## Screenshots

| Player View | Library View | Fullscreen |
|-------------|--------------|------------|
| Dark themed player with album art visualization | Media library with swipe-to-delete | Full window video playback |

## Requirements

- .NET 10.0 SDK or later
- Visual Studio 2022 (17.8+) or VS Code with C# Dev Kit
- Platform-specific requirements:
  - **Windows**: Windows 10 version 1809 or higher
  - **Android**: Android 8.0 (API 26) or higher
  - **iOS**: iOS 15.0 or higher
  - **macOS**: macOS 15.0 or higher

## Getting Started

### Clone the Repository

```bash
git clone https://github.com/yourusername/tips-player.git
cd tips-player
```

### Restore Dependencies

```bash
cd "Tips Player"
dotnet restore
```

### Build and Run

**Windows:**
```bash
dotnet build --framework net10.0-windows10.0.19041.0
dotnet run --framework net10.0-windows10.0.19041.0
```

Release builds produce an MSIX bundle. A distribution build can be signed without storing a certificate in source control:

```powershell
dotnet publish "Tips Player/Tips Player.csproj" -f net10.0-windows10.0.19041.0 -c Release `
  -p:PackageCertificateKeyFile="C:\secure\tips-player.pfx" `
  -p:PackageCertificatePassword="$env:TIPS_PLAYER_CERT_PASSWORD"
```

Omit the certificate properties for an unsigned local validation package.

**Android:**
```bash
dotnet build --framework net10.0-android
# Deploy to connected device or emulator
dotnet build -t:Run --framework net10.0-android
```

**iOS (macOS only):**
```bash
dotnet build --framework net10.0-ios
```

**macOS:**
```bash
dotnet build --framework net10.0-maccatalyst
```

## Project Structure

```
Tips Player/
├── Models/
│   ├── MediaType.cs          # Audio/Video enum
│   ├── MediaItem.cs          # Media file model
│   └── Playlist.cs           # Playlist collection
├── ViewModels/
│   ├── BaseViewModel.cs      # MVVM base class
│   ├── PlayerViewModel.cs    # Player state & commands
│   └── LibraryViewModel.cs   # Library management
├── Views/
│   ├── PlayerPage.xaml       # Main player UI
│   ├── LibraryPage.xaml      # Media library UI
│   └── Controls/
│       ├── PlaybackControls.xaml
│       └── MiniPlayerBar.xaml
├── Services/
│   ├── Interfaces/
│   │   ├── IMediaPlayerService.cs
│   │   └── IFilePickerService.cs
│   ├── MediaPlayerService.cs # MediaElement wrapper
│   └── FilePickerService.cs  # File picking
├── Converters/
│   └── BoolToPlayPauseIconConverter.cs
├── Resources/
│   └── Styles/
│       ├── Colors.xaml       # Dark theme colors
│       ├── Styles.xaml       # Base styles
│       └── PlayerStyles.xaml # Player-specific styles
├── Platforms/
│   ├── Android/
│   ├── iOS/
│   ├── MacCatalyst/
│   └── Windows/
├── App.xaml                  # Application resources
├── AppShell.xaml             # Navigation shell
└── MauiProgram.cs            # DI configuration
```

## Technologies Used

- **.NET MAUI** - Cross-platform UI framework
- **CommunityToolkit.Maui** - MAUI extensions and behaviors
- **CommunityToolkit.Maui.MediaElement** - Media playback component
- **CommunityToolkit.Mvvm** - MVVM toolkit with source generators

## NuGet Packages

```xml
<PackageReference Include="CommunityToolkit.Maui" Version="9.0.0" />
<PackageReference Include="CommunityToolkit.Maui.MediaElement" Version="4.0.0" />
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.3.2" />
```

## Supported Media Formats

### Audio
- MP3, WAV, AAC, M4A, FLAC, OGG, WMA

### Video
- MP4, AVI, MKV, MOV, WMV, WebM, M4V

## Usage

1. **Add Media Files** - Click the "+" button or "Add Files" to select audio/video files
2. **Play Media** - Tap on any item in the library to start playback
3. **Control Playback** - Use the play/pause, next, previous buttons
4. **Seek** - Drag the progress slider to jump to any position
5. **Fullscreen** - Click the expand button to enter fullscreen mode
6. **Shuffle/Repeat** - Toggle shuffle and repeat modes for continuous playback

## Architecture

The app follows the **MVVM (Model-View-ViewModel)** pattern:

- **Tips Player.Core** - Platform-neutral models, validation, results, and bounded atomic JSON persistence
- **Models** - Data structures for media items and playlists
- **Views** - XAML pages and controls for the UI
- **ViewModels** - Business logic and state management
- **Services** - Abstracted platform services (media playback, file picking)
- **Persistence adapters** - Transactional SQLite storage for the media library and listening sessions

Dependency Injection is configured in `MauiProgram.cs` with services registered as singletons for shared state across the application.
Legacy JSON library and statistics files are imported into SQLite once and retained with a `.migrated` suffix for recovery.

## Color Scheme

| Color | Hex | Usage |
|-------|-----|-------|
| Accent | `#6366F1` | Primary actions, highlights |
| Page Background | `#0A0A0B` | Main background |
| Card Background | `#1A1A1D` | Cards, containers |
| Text Primary | `#FAFAFA` | Main text |
| Text Secondary | `#A1A1AA` | Subtitles, hints |

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui)
- [MVVM Toolkit](https://github.com/CommunityToolkit/dotnet)
- Icons from Unicode Emoji

---

**Tips Player** - A modern media experience for everyone.
