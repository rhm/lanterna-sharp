# Lanterna Testing Framework

This directory contains a comprehensive testing framework for the Lanterna console UI library that uses the virtual terminal approach to validate output without requiring actual terminal hardware.

## Overview

The testing framework leverages the `DefaultVirtualTerminal` class to create isolated, controlled testing environments where:

- **Terminal output can be captured and validated** character by character
- **Input can be simulated** programmatically 
- **Complex UI scenarios can be tested** end-to-end
- **Tests run fast** with no real I/O delays
- **Tests are deterministic** and don't depend on actual terminal hardware

## Architecture

### Virtual Terminal Testing
- **`DefaultVirtualTerminal`**: Core testing infrastructure that simulates a terminal in memory
- **Buffer inspection**: Exact validation of character placement, colors, and styling
- **Input simulation**: Programmatic injection of keystroke events
- **State verification**: Checking cursor position, dirty cells, and terminal state

### Test Structure
```
Lanterna.Tests/
├── Core/                          # Core data type tests
│   ├── TextCharacterTest.cs       # TextCharacter validation
│   ├── TerminalSizeTest.cs        # TerminalSize operations
│   └── TerminalPositionTest.cs    # TerminalPosition operations
├── Terminal/Virtual/              # Virtual terminal tests
│   └── DefaultVirtualTerminalTest.cs # Basic terminal operations
├── Input/                         # Input handling tests  
│   └── InputSimulationTest.cs     # Keyboard input simulation
├── Screen/                        # Screen layer tests
│   └── TerminalScreenTest.cs      # Buffered screen operations
├── Integration/                   # Complete UI tests
│   ├── TerminalOutputValidationTest.cs # Output validation examples
│   └── CompleteUITest.cs          # Full dialog and menu testing
└── SimpleTest.cs                  # Basic framework validation
```

## Key Testing Patterns

### 1. Terminal Output Validation
```csharp
var virtualTerminal = new DefaultVirtualTerminal();
virtualTerminal.SetCursorPosition(5, 3);
virtualTerminal.PutCharacter('X');

// Validate exact output
var character = virtualTerminal.GetCharacter(5, 3);
Assert.Equal('X', character.Character);
Assert.Equal(new TerminalPosition(6, 3), virtualTerminal.GetCursorPosition());
```

### 2. Input Simulation
```csharp
var virtualTerminal = new DefaultVirtualTerminal();

// Queue input events
virtualTerminal.AddInput(new KeyStroke('A', false, false));
virtualTerminal.AddInput(new KeyStroke(KeyType.Enter));

// Process input
var keyStroke = virtualTerminal.PollInput();
Assert.Equal('A', keyStroke.Character);
```

### 3. Screen Layer Testing
```csharp
var virtualTerminal = new DefaultVirtualTerminal();
var screen = new TerminalScreen(virtualTerminal);
screen.StartScreen();

// Modify screen buffer
screen.SetCharacter(5, 3, new TextCharacter('Y'));

// Validate buffer (not yet on terminal)
Assert.Equal(new TextCharacter('Y'), screen.GetBackCharacter(5, 3));
Assert.Equal(TextCharacter.DefaultCharacter, virtualTerminal.GetCharacter(5, 3));

// Refresh to terminal and validate
screen.Refresh();
Assert.Equal(new TextCharacter('Y'), virtualTerminal.GetCharacter(5, 3));
```

### 4. Complex UI Testing
The framework supports testing complete UI components like dialogs, menus, and forms by:
- Rendering UI elements to the virtual terminal
- Validating exact visual output
- Simulating user input sequences
- Verifying interaction handling

## Benefits

### For Console UI Library Testing
1. **Complete Output Control**: Validate exact character placement, colors, and styling
2. **No Hardware Dependencies**: Tests run entirely in memory
3. **Fast Execution**: No real terminal I/O delays
4. **Deterministic Results**: Same input always produces same output
5. **Comprehensive Coverage**: Test all library layers from terminal to GUI

### For Development
1. **Early Bug Detection**: Catch rendering issues before manual testing
2. **Regression Prevention**: Automated tests prevent breaking existing functionality  
3. **Documentation**: Tests serve as examples of correct library usage
4. **Refactoring Safety**: Comprehensive test coverage enables safe code changes

## Running Tests

```bash
# Run all tests
dotnet test Lanterna.Tests/

# Run specific test class
dotnet test Lanterna.Tests/ --filter "SimpleTest"

# Run with detailed output
dotnet test Lanterna.Tests/ --verbosity normal
```

## Current Status

✅ **Working**: Basic framework, core tests, virtual terminal tests, simple validation
🚧 **In Progress**: Color system integration, complex UI component tests
📋 **TODO**: GUI layer tests, comprehensive integration tests

The testing framework successfully demonstrates that the virtual terminal approach provides an excellent foundation for testing console UI libraries without requiring actual terminal hardware.