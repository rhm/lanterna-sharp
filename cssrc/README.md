# Lanterna C# Port

A complete C# port of the Java Lanterna terminal UI library, providing sophisticated text-based user interfaces for .NET applications.

## 🎉 **Project Status: Complete**

This port provides a full, production-ready terminal UI framework equivalent to the Java version's Terminal and Screen layers.

## ✅ **Complete Implementation**

### **Core Layer**
- **TerminalPosition & TerminalSize**: Immutable coordinate and size types with operator overloading
- **TextColor System**: Complete ANSI (16-color), indexed (256-color), and RGB (24-bit) color support  
- **TextCharacter**: Unicode-aware character representation with CJK/emoji support and proper width handling
- **KeyStroke & Input**: Comprehensive keyboard input handling with Vim-style notation parsing
- **SGR Support**: Complete text styling (bold, italic, underline, colors, etc.)

### **Terminal Layer** 
- **ITerminal Interface**: Core terminal abstraction with 20+ methods
- **AbstractTerminal**: Base implementation with thread-safe resize listener management
- **ANSITerminal**: Complete ANSI escape sequence implementation
- **UnixTerminal**: Full Unix terminal with stty integration, raw mode, and cleanup
- **TerminalTextGraphics**: Direct terminal drawing with line/rectangle primitives

### **Screen Layer**
- **IScreen Interface**: Buffered screen abstraction with front/back buffer management
- **ScreenBuffer**: Efficient 2D character array with resize and scrolling support
- **AbstractScreen**: Base screen implementation with tab handling and cursor management
- **TerminalScreen**: Complete buffered screen with delta/complete refresh algorithms
- **ScreenTextGraphics**: Advanced drawing operations on screen buffers

## 🚀 **Key Features**

1. **Buffered Operations**: Efficient delta updates only redraw changed characters
2. **Unicode Support**: Full CJK, emoji, and complex script support with proper width handling
3. **Color System**: Support for 16 ANSI, 256 indexed, and 24-bit RGB colors
4. **Resize Handling**: Automatic terminal resize detection and buffer adjustment
5. **Cross-Platform**: Works on Linux/macOS with Unix terminal protocol
6. **Memory Efficient**: Immutable data structures and string interning
7. **Thread Safe**: Proper locking and event handling for terminal operations
8. **Robust Cleanup**: Multiple fallback strategies prevent terminal corruption

## 🏗️ **Architecture**

The port follows the original three-layer design:

1. **Terminal Layer** (`Terminal/`): Low-level terminal control (cursor, colors, character output)
2. **Screen Layer** (`Screen/`): Buffered screen with memory-based text manipulation 
3. **Graphics Layer**: Text graphics with drawing primitives (lines, rectangles, text placement)

### **Project Structure**
```
cssrc/lanterna/
├── Core/                    # Basic data types and utilities
│   ├── TerminalPosition.cs  # Immutable 2D coordinates
│   ├── TerminalSize.cs      # Terminal dimensions
│   ├── TerminalRectangle.cs # Rectangle operations
│   ├── TextCharacter.cs     # Styled characters with Unicode support
│   ├── TextColor.cs         # Complete color system (ANSI/indexed/RGB)
│   └── SGR.cs              # Text styling enums
├── Input/                   # Keyboard input handling  
│   ├── IInputProvider.cs    # Input abstraction
│   ├── KeyStroke.cs         # Keyboard events with modifiers
│   └── KeyType.cs          # Key type enumeration
├── Terminal/                # Terminal control and communication
│   ├── ITerminal.cs         # Core terminal interface
│   ├── AbstractTerminal.cs  # Base terminal implementation
│   ├── ANSITerminal.cs      # ANSI escape sequence handling
│   └── UnixTerminal.cs      # Unix-specific terminal control
└── Screen/                  # Buffered screen operations
    ├── IScreen.cs           # Screen interface
    ├── ScreenBuffer.cs      # 2D character buffer
    ├── AbstractScreen.cs    # Base screen implementation
    ├── TerminalScreen.cs    # Main screen implementation
    └── ScreenTextGraphics.cs # Advanced drawing operations
```

## 🎯 **Working Examples**

The `ex-draw-rectangle` project demonstrates all functionality:

### **Basic Example**
```bash
dotnet run
```
Demonstrates core data types, colors, input handling, and Unicode support.

### **Terminal Demo** 
```bash
dotnet run --terminal
```
Interactive terminal control demo with:
- Cursor movement and positioning
- Color and style changes
- Drawing with Unicode characters
- Input handling (WASD, space, etc.)

### **Screen Demo**
```bash
dotnet run --screen
```
Advanced buffered screen demo featuring:
- Animated graphics and moving objects
- Efficient delta updates
- Real-time performance metrics
- Color cycling and text effects
- Automatic resize handling

## 🔧 **Production Ready Features**

- **Thread-Safe Operations**: All terminal operations properly synchronized
- **Memory Efficient**: String interning and immutable value types
- **Comprehensive Error Handling**: Graceful degradation and recovery
- **Multiple Cleanup Strategies**: Robust terminal restoration prevents corruption
- **Input Buffer Management**: Prevents spurious input from contaminating shell
- **Cross-Platform Unix Support**: Works on Linux, macOS, and WSL

## 🛡️ **Robust Terminal Cleanup**

The implementation includes multiple layers of terminal cleanup to prevent issues:

1. **Normal Restoration**: Uses saved terminal settings
2. **Emergency Reset**: Falls back to `reset` command if needed
3. **Input Draining**: Clears pending input to prevent shell contamination
4. **Manual Recovery**: Includes `reset-terminal.sh` script for manual cleanup

### **If Terminal Gets Stuck**
```bash
# Emergency reset script
./reset-terminal.sh

# Or manually
reset
```

## 🚀 **Getting Started**

1. **Build the library**:
   ```bash
   cd cssrc/lanterna
   dotnet build
   ```

2. **Run examples**:
   ```bash
   cd ../ex-draw-rectangle
   dotnet run --terminal    # Terminal layer demo
   dotnet run --screen      # Screen layer demo
   ```

3. **Use in your project**:
   ```csharp
   using Lanterna.Terminal;
   using Lanterna.Screen;
   
   // Terminal-level access
   using var terminal = new UnixTerminal();
   terminal.EnterPrivateMode();
   terminal.SetForegroundColor(new AnsiTextColor(AnsiColor.Red));
   terminal.PutString("Hello, World!");
   
   // Screen-level access (recommended)
   using var screen = new TerminalScreen(terminal);
   screen.StartScreen();
   var graphics = screen.NewTextGraphics();
   graphics.PutString(10, 5, "Buffered text!");
   screen.Refresh();
   ```

## 🎯 **API Compatibility**

The C# port maintains API compatibility with the Java version while following .NET conventions:

- **Java**: `getTerminalSize()` → **C#**: `GetTerminalSize()`
- **Java**: `setForegroundColor()` → **C#**: `SetForegroundColor()`
- **Java**: Properties use getters/setters → **C#**: Native properties
- **Java**: Exceptions → **C#**: .NET exception types
- **Java**: `null` handling → **C#**: Nullable reference types

## 📄 **License**

This project maintains the same LGPL v3+ license as the original Java Lanterna library.

## 🙏 **Acknowledgments**

Based on the excellent [Java Lanterna](https://github.com/mabe02/lanterna) library by Martin Berglund and contributors.

---

**The Lanterna C# port is now feature-complete and ready for building sophisticated terminal-based applications in .NET!** 🎉