# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

This is Lanterna-Sharp, a C# port of the Java Lanterna terminal UI library. The project contains both the original Java codebase and a partial C# port located in the `cssrc/` directory.

## Build System

### Java (Original Codebase)
- **Build Tool**: Maven (though `mvn` command may not be available in all environments)
- **Project File**: `pom.xml` (Java 8 target, JUnit 4 tests)
- **Standard Maven Commands**:
  - Compile: `java -cp target/classes:target/test-classes com.googlecode.lanterna.examples.*` (after manual compilation)
  - Test: Use JUnit runner or IDE integration
  - Package: Standard Maven lifecycle

### C# (Port in cssrc/)
- **Build Tool**: .NET CLI (`dotnet` command available)
- **No project files**: The C# code doesn't have `.csproj` files yet
- **Manual compilation**: Use `dotnet` commands or `csc` compiler
- **Example program**: `cssrc/Lanterna/example/ExampleProgram.cs`

## Architecture

### Three-Layer Design
1. **Terminal Layer** (`terminal/`): Low-level terminal control (cursor, colors, character output)
2. **Screen Layer** (`screen/`): Buffered screen with memory-based text manipulation 
3. **GUI Layer** (`gui2/`): Full GUI toolkit with windows, buttons, dialogs, and layout managers

### Key Interfaces
- **Terminal**: Core terminal abstraction (`com.googlecode.lanterna.terminal.Terminal`)
- **Screen**: Buffered screen interface (`com.googlecode.lanterna.screen.Screen`)
- **TextGUI**: GUI framework entry point (`com.googlecode.lanterna.gui2.TextGUI`)

### C# Port Status
- **Completed**: Terminal layer basics, input handling, core data types
- **In Progress**: Screen layer partially ported
- **Not Started**: GUI layer (gui2 equivalent)

### Platform Abstraction
- **Java**: Supports ANSI terminals, Swing terminal emulator, Windows console
- **C#**: Supports Windows console and Unix terminals via different implementations

## Testing
- **Java Tests**: Located in `src/test/java/` using JUnit 4
- **Test Categories**: Unit tests, integration tests, issue-specific regression tests
- **C# Tests**: No test structure exists yet in the C# port

## Key Files
- **Java Main**: `src/main/java/com/googlecode/lanterna/`
- **C# Port**: `cssrc/Lanterna/`
- **Examples**: `src/examples/java/` (Java), `cssrc/Lanterna/example/` (C#)
- **Documentation**: `docs/` with tutorials and API guides

## Development Notes
- The C# port mirrors Java package structure as C# namespaces
- Both codebases target cross-platform terminal applications
- Focus on text-based UIs that work in both GUI and headless environments