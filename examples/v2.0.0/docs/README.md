# Sample Application v2.0.0

This is a major release with significant architectural changes.

## New Features
- Complete architectural redesign with dependency injection
- Service-based architecture (ILoggerService, IConfigService)
- Enhanced configuration management
- Database connection support
- API endpoint configuration
- Improved logging with timestamps

## Breaking Changes from v1.x
- Removed Utils class (replaced with services)
- Removed Logger class (replaced with LoggerService)
- Changed main program structure
- New configuration format

## New Architecture
- Core: Application logic
- Services: Business logic services
- Interfaces: Service contracts

## Installation
1. Compile the source code
2. Configure database connection
3. Run the executable

## Usage
Run the main executable to start the application.
The new architecture provides better separation of concerns and testability.
