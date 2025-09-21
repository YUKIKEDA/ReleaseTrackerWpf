# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**ReleaseTrackerWpf** is a WPF desktop application that tracks and compares directory structures of software installation modules between versions. It helps detect unintended file additions, deletions, or changes to improve release quality.

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

- **Framework**: .NET 8 WPF application using MVVM pattern with dependency injection
- **UI Framework**: WPF-UI for modern Fluent Design System components
- **Architecture Pattern**: Clean Architecture with Repository pattern
- **Key Dependencies**:
  - `CommunityToolkit.Mvvm` - Modern MVVM implementation with ObservableObject, RelayCommand
  - `ClosedXML` - Excel file export/import (.xlsx)
  - `CsvHelper` - CSV file export/import
  - `WPF-UI` - Fluent Design System components (FluentWindow, NavigationView, etc.)
  - `R3` - Reactive extensions for async operations

## Current Implementation Status

### ✅ Fully Implemented (All Core Functionality Complete)
- **Directory Scanning**: `DirectoryScanService` recursively scans directories with error handling
- **Snapshot Persistence**: `SnapshotRepository` handles JSON serialization of `DirectorySnapshot`
- **Comparison Logic**: `ComparisonService.CompareAsync()` - Complete recursive directory comparison with 228 lines of sophisticated logic
- **ComparisonResult Model**: Full model with statistics, tree collections, and comparison metadata
- **Display Conversion**: `ComparisonViewModel.CreateCompareResultForDisplay()` - Complete UI binding conversion
- **Export/Import**: All commands fully implemented including CSV export with multiple format options
- **Description Management**: Import/Export descriptions via CSV with `ImportDescriptionService`
- **Settings Management**: Auto-save settings with `SettingsViewModel` and CSV format options
- **Auto-scan Feature**: Automatic comparison execution when both snapshots are selected
- **UI Framework**: Complete WPF-UI implementation with tabbed interface, tree views, and progress notifications
- **MVVM Architecture**: Proper ViewModels with dependency injection setup in `App.xaml.cs`
- **Notifications**: `NotificationService` using Messenger pattern for UI feedback

### 🎯 Enhanced Components
- **EnumToBooleanConverter**: UI converter for enum radio button binding in settings
- **CSV Export Formats**: Tree and Normal path display options via `ExportedCsvPathFormat`
- **TitleBarView**: Custom title bar component
- **Theme Support**: Custom DesertTheme implementation

## Key Models and Data Flow

### Core Models
- **FileSystemEntry**: Hierarchical file/directory representation with difference tracking
- **DirectorySnapshot**: Timestamped snapshot with metadata and file naming conventions
- **DifferenceType**: Enum (None, Added, Deleted, Modified, Unchanged)

### Services Architecture
- **DirectoryScanService**: Directory traversal with permission handling
- **ComparisonService**: Complete recursive comparison logic with difference detection
- **ExportService**: Multi-format export (Excel, CSV, text) with configurable path formats
- **ImportDescriptionService**: CSV description import/export functionality
- **SnapshotRepository**: JSON persistence layer
- **NotificationService**: UI messaging via CommunityToolkit.Mvvm.Messaging

### ViewModels Pattern
- **MainWindowViewModel**: Root coordinator with InfoBar notification handling
- **ComparisonViewModel**: Main workspace for snapshot selection and comparison with auto-scan
- **SettingsViewModel**: Settings management with auto-save and CSV format options
- **FileItemViewModel**: UI wrapper for FileSystemEntry with tree binding support

## Implementation Status: Feature Complete

All core functionality has been implemented. The application is feature-complete with:
- Complete directory comparison engine with recursive diff algorithms
- CSV export with multiple format options (Tree/Normal path display)
- Description import/export workflow via CSV files
- Auto-save settings management with user preferences
- Auto-scan functionality for immediate comparison when snapshots are selected
- Comprehensive error handling and user notifications

## Potential Future Enhancements
When extending this codebase, consider these areas:
1. **Additional Export Formats**: JSON, XML, or custom report formats
2. **Advanced Filtering**: File type, size, or date-based filtering options
3. **Comparison History**: Track and manage multiple comparison sessions
4. **Performance Optimization**: Large directory handling improvements

## Development Guidelines

- Follow existing MVVM patterns using CommunityToolkit.Mvvm
- Use dependency injection configured in `App.xaml.cs`
- Maintain async/await patterns for file operations
- Handle file system exceptions gracefully (see DirectoryScanService examples)
- Use WPF-UI components for consistent Fluent Design
- Color-code differences using existing `DifferenceTypeToColorConverter`

## Sample Data
The `examples/` directory contains versioned sample directories (v1.0.0 through v4.0.0) for testing comparison functionality across multiple version transitions.