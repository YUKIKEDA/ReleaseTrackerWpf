# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**Installer Tracker** (ReleaseTrackerWpf) is a WPF desktop application that tracks and compares directory structures of software installation modules between versions. It helps detect unintended file additions, deletions, or changes to improve release quality.

## Build and Development Commands

```bash
# Build the solution
dotnet build ReleaseTrackerWpf.sln

# Run the application
dotnet run --project ReleaseTrackerWpf/ReleaseTrackerWpf.csproj

# Build for release
dotnet build ReleaseTrackerWpf.sln --configuration Release

# Clean build artifacts
dotnet clean ReleaseTrackerWpf.sln
```

## Project Architecture

- **Framework**: .NET 8 WPF application using MVVM pattern
- **UI Framework**: WPF-UI for modern Fluent Design System components
- **Dependencies**:
  - `CommunityToolkit.Mvvm` - MVVM pattern implementation
  - `ClosedXML` - Excel file export/import (.xlsx)
  - `CsvHelper` - CSV file export/import
  - `WPF-UI` - Modern UI components

## Directory Structure

```
ReleaseTrackerWpf/
├── Models/          # Data models and entities (placeholder)
├── ViewModels/      # MVVM ViewModels (placeholder)
├── Views/           # XAML views and windows (placeholder)
├── Services/        # Business logic and data services (placeholder)
├── Converters/      # Value converters for data binding (placeholder)
├── MainWindow.xaml  # Main application window
└── App.xaml         # Application entry point
```

## Key Features to Implement

1. **Directory Structure Scanning**: Recursively scan folders and save snapshots as JSON
2. **Snapshot Comparison**: Compare old vs new directory structures
3. **Difference Detection**: Categorize changes as Added, Deleted, or Modified files
4. **Visual Tree Display**: Show comparison results in tree view with color coding
5. **Export Functionality**: Export reports to Excel, CSV, and text formats
6. **Import Descriptions**: Import Excel/CSV files with user-added descriptions

## Development Notes

- The project currently contains only basic WPF scaffolding
- All major functionality folders (Models, ViewModels, Views, Services) are placeholders
- The application specification is documented in `.dev/requirement.md` (Japanese)
- Uses modern MVVM patterns with CommunityToolkit.Mvvm
- UI should follow Fluent Design System principles via WPF-UI