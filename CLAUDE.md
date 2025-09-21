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

### ✅ Fully Implemented
- **Directory Scanning**: `DirectoryScanService` recursively scans directories with error handling
- **Snapshot Persistence**: `SnapshotRepository` handles JSON serialization of `DirectorySnapshot`
- **Export/Import**: `ExportService` supports Excel, CSV, and text export with description import
- **UI Framework**: Complete WPF-UI implementation with tabbed interface and tree views
- **MVVM Architecture**: Proper ViewModels with dependency injection setup in `App.xaml.cs`
- **Notifications**: `NotificationService` using Messenger pattern for UI feedback

### ❌ Missing Core Functionality
- **Comparison Logic**: `ComparisonService.CompareAsync()` method is empty - needs implementation
- **ComparisonResult Model**: Empty class that needs properties for storing comparison results
- **Display Conversion**: `ComparisonViewModel.CreateCompareResultForDisplay()` method missing
- **Command Implementations**: Export/Import commands marked as TODO in `ComparisonViewModel`

### 🔄 Partially Implemented
- **ComparisonViewModel**: Has UI bindings but lacks core comparison logic integration

## Key Models and Data Flow

### Core Models
- **FileSystemEntry**: Hierarchical file/directory representation with difference tracking
- **DirectorySnapshot**: Timestamped snapshot with metadata and file naming conventions
- **DifferenceType**: Enum (None, Added, Deleted, Modified, Unchanged)

### Services Architecture
- **DirectoryScanService**: Directory traversal with permission handling
- **ComparisonService**: Core comparison logic (needs implementation)
- **ExportService**: Multi-format export with description import capability
- **SnapshotRepository**: JSON persistence layer
- **NotificationService**: UI messaging via CommunityToolkit.Mvvm.Messaging

### ViewModels Pattern
- **MainWindowViewModel**: Root coordinator with InfoBar notification handling
- **ComparisonViewModel**: Main workspace for snapshot selection and comparison
- **FileItemViewModel**: UI wrapper for FileSystemEntry with tree binding support

## Implementation Priorities

When working on this codebase, focus on these areas in order:

1. **Complete ComparisonService.CompareAsync()**: Implement directory diff algorithm
2. **Populate ComparisonResult model**: Add properties for storing comparison metadata
3. **Implement CreateCompareResultForDisplay()**: Convert results to UI-bindable format
4. **Connect Export/Import commands**: Wire up existing ExportService to ViewModels

## Development Guidelines

- Follow existing MVVM patterns using CommunityToolkit.Mvvm
- Use dependency injection configured in `App.xaml.cs`
- Maintain async/await patterns for file operations
- Handle file system exceptions gracefully (see DirectoryScanService examples)
- Use WPF-UI components for consistent Fluent Design
- Color-code differences using existing `DifferenceTypeToColorConverter`

## Sample Data
The `examples/` directory contains versioned sample directories (v1.0.0, v1.1.0, v2.0.0) for testing comparison functionality.