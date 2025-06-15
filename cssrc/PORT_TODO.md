# Lanterna-Sharp Java to C# Port TODO List

This document tracks the comprehensive list of Java classes that need to be ported to C#. 

## Summary Statistics

- **Total Java Classes**: 186
- **Already Ported**: 141 (75.8%)
- **Remaining to Port**: 45 (24.2%)

## Priority Levels
- **HIGH**: Core functionality required for basic operation
- **MEDIUM**: Important features that enhance usability  
- **LOW**: Advanced/specialized features

---

## HIGH PRIORITY CLASSES (0 Missing)

### Core GUI Infrastructure
- [x] `gui2/BasicWindow.java` - Basic window implementation (implemented in AbstractWindow.cs)
- [x] `gui2/WindowBasedTextGUI.java` - Essential GUI infrastructure (IWindowBasedTextGUI.cs + MultiWindowTextGUI.cs)
- [x] `gui2/AsynchronousTextGUIThread.java` - Async GUI threading (IAsynchronousTextGUIThread.cs + SeparateTextGUIThread.cs)
- [x] `gui2/WindowListener.java` - Window event handling interface (defined in IWindow.cs)
- [x] `gui2/WindowPostRenderer.java` - Post-rendering hooks (defined in IWindow.cs)

### Essential Components
- [x] `gui2/ComboBox.java` - Dropdown combo box component
- [x] `gui2/SplitPanel.java` - Resizable split panel

### Graphics Core
- [x] `graphics/TextImage.java` - Core graphics interface (interface already existed in ITextGraphics.cs)
- [x] `screen/VirtualScreen.java` - Virtual screen implementation

### Terminal Infrastructure  
- [x] `terminal/DefaultTerminalFactory.java` - Auto-detection terminal factory
- [x] `terminal/TerminalTextGraphics.java` - Terminal-based text graphics
- [x] `terminal/ExtendedTerminal.java` - Extended terminal interface
- [x] `terminal/IOSafeTerminal.java` - IO-safe terminal interface
- [x] `terminal/IOSafeExtendedTerminal.java` - IO-safe extended terminal
- [x] `terminal/IOSafeTerminalAdapter.java` - IO-safe adapter

### Platform Support
- [x] `terminal/ansi/UnixLikeTTYTerminal.java` - Unix TTY terminal
- [x] `terminal/ansi/StreamBasedTerminal.java` - Stream-based terminal base
- [x] `terminal/virtual/DefaultVirtualTerminal.java` - Virtual terminal implementation
- [x] `terminal/virtual/VirtualTerminal.java` - Virtual terminal interface
- [x] `terminal/virtual/VirtualTerminalListener.java` - Virtual terminal events

### Input System Core
- [x] `input/InputDecoder.java` - Input sequence decoder
- [x] `input/KeyDecodingProfile.java` - Key decoding configuration
- [x] `input/DefaultKeyDecodingProfile.java` - Default key decoding
- [x] `input/CharacterPattern.java` - Input pattern matching interface

### Essential Enums
- [x] `screen/WrapBehaviour.java` - Text wrapping behavior enum
- [x] `terminal/MouseCaptureMode.java` - Mouse capture configuration
- [x] `input/MouseActionType.java` - Mouse action types

---

## MEDIUM PRIORITY CLASSES (11 Missing)

### Graphics Utilities
- [x] `graphics/AbstractTheme.java` - Base theme implementation (complete hierarchical theme system)
- [x] `graphics/DefaultMutableThemeStyle.java` - Mutable theme style
- [x] `graphics/DefaultShapeRenderer.java` - Shape drawing algorithms
- [x] `graphics/ShapeRenderer.java` - Shape rendering interface
- [x] `graphics/SimpleTheme.java` - Simple theme implementation (complete with component overrides)
- [x] `graphics/SubTextGraphics.java` - Sub-region graphics wrapper
- [x] `graphics/ThemedTextGraphics.java` - Theme-aware graphics (interface already existed in Theme.cs)
- [ ] `graphics/TextGraphicsWriter.java` - Writer pattern for graphics (COMPLEX - interface compatibility issues)
- [x] `graphics/Scrollable.java` - Scrolling behavior interface (interface already existed in ITextGraphics.cs)
- [x] `graphics/StyleSet.java` - Style management utilities
- [x] `graphics/ThemeStyle.java` - Theme style interface (interface already existed in Theme.cs)
- [x] `graphics/DoublePrintingTextGraphics.java` - Double-width rendering
- [x] `graphics/NullTextGraphics.java` - Null object pattern

### Theme System
- [x] `bundle/BundleLocator.java` - Resource bundle localization
- [x] `bundle/DefaultTheme.java` - Default theme implementation  
- [x] `bundle/LanternaThemes.java` - Theme management utilities
- [x] `bundle/LocalizedUIBundle.java` - Localized UI strings

### Advanced Input Patterns
- [x] `input/AltAndCharacterPattern.java` - Alt+key combinations
- [x] `input/BasicCharacterPattern.java` - Basic character patterns (already existed)
- [x] `input/CtrlAltAndCharacterPattern.java` - Ctrl+Alt+key patterns
- [x] `input/CtrlAndCharacterPattern.java` - Ctrl+key patterns
- [x] `input/EscapeSequenceCharacterPattern.java` - Escape sequences
- [x] `input/MouseCharacterPattern.java` - Mouse input patterns
- [x] `input/NormalCharacterPattern.java` - Normal character patterns (already existed)
- [x] `input/ScreenInfoAction.java` - Screen information actions
- [x] `input/ScreenInfoCharacterPattern.java` - Screen info patterns

### Terminal Implementations
- [-] `terminal/ansi/CygwinTerminal.java` - Cygwin terminal support - DON'T DO THIS
- [x] `terminal/ansi/TelnetProtocol.java` - Telnet protocol handling
- [x] `terminal/ansi/TelnetTerminal.java` - Telnet terminal
- [x] `terminal/ansi/TelnetTerminalServer.java` - Telnet server
- [x] `terminal/ansi/UnixTerminalSizeQuerier.java` - Unix size query
- [x] `terminal/virtual/TextBuffer.java` - Text buffer utilities
- [x] `terminal/virtual/VirtualTerminalTextGraphics.java` - Virtual graphics

### Advanced GUI Features
- [ ] `gui2/TextGUIThreadFactory.java` - GUI thread factory
- [ ] `gui2/FatWindowDecorationRenderer.java` - Thick window borders

### Screen Utilities
- [x] `screen/ScreenTranslator.java` - Screen coordinate translation (implemented as IScreenTranslator.cs)

### Core Utilities
- [x] `Symbols.java` - Complete Unicode symbols (current C# version now complete)

### Platform Specific
- [ ] `terminal/ansi/FixedTerminalSizeProvider.java` - Fixed size provider
- [ ] `terminal/SimpleTerminalResizeListener.java` - Simple resize handler
- [ ] `terminal/TerminalFactory.java` - Terminal factory interface

### Theme Infrastructure
- [ ] `graphics/DelegatingTheme.java` - Theme delegation pattern
- [ ] `graphics/DelegatingThemeDefinition.java` - Theme definition delegation
- [ ] `graphics/PropertyTheme.java` - Property file-based themes
- [ ] `graphics/ThemeDefinition.java` - Theme definition interface

---

## LOW PRIORITY CLASSES (25 Missing)

### Swing/AWT Support (Desktop GUI)
- [ ] `terminal/swing/AWTTerminalFontConfiguration.java`
- [ ] `terminal/swing/AWTTerminalFrame.java`
- [ ] `terminal/swing/GraphicsTerminalRenderer.java`
- [ ] `terminal/swing/ScrollingSwingTerminal.java`
- [ ] `terminal/swing/SwingTerminal.java`
- [ ] `terminal/swing/SwingTerminalColorConfiguration.java`
- [ ] `terminal/swing/SwingTerminalDeviceConfiguration.java`
- [ ] `terminal/swing/SwingTerminalFontConfiguration.java`
- [ ] `terminal/swing/SwingTerminalFrame.java`
- [ ] `terminal/swing/TerminalAppearance.java`
- [ ] `terminal/swing/TerminalEmulatorAutoCloseTrigger.java`
- [ ] `terminal/swing/TerminalEmulatorColorConfiguration.java`
- [ ] `terminal/swing/TerminalEmulatorDeviceConfiguration.java`
- [ ] `terminal/swing/TerminalEmulatorPalette.java`
- [ ] `terminal/swing/TerminalPalette.java`

### Windows Console Support
- [ ] `terminal/win/WindowsConsoleInputStream.java`
- [ ] `terminal/win/WindowsConsoleOutputStream.java` 
- [ ] `terminal/win/WindowsTerminal.java`

### Test/Debug Utilities
- [ ] `TestTerminalFactory.java`
- [ ] `input/ScreenInfoActionType.java`

### Advanced Graphics
- [ ] `graphics/DelegatingTextImage.java`
- [ ] `graphics/BasicTextImage.java`

### Platform Detection
- [ ] `terminal/DetachedTerminal.java`
- [ ] `terminal/TerminalResizeListener.java`
- [ ] `terminal/ansi/ANSIArgumentType.java`

---

## Package Completion Status

| Package | Ported | Total | Completion |
|---------|--------|-------|------------|
| Core | 9 | 9 | 100% |
| Bundle | 4 | 4 | 100% |
| Graphics | 15 | 17 | 88% |
| Input | 12 | 15 | 80% |
| Screen | 9 | 9 | 100% |
| Terminal | 18 | 46 | 39% |
| GUI Core | 65 | 65 | 100% |
| GUI Dialogs | 12 | 12 | 100% |
| GUI Menu | 3 | 3 | 100% |
| GUI Table | 9 | 9 | 100% |

## Next Steps Recommendation

Focus on **HIGH PRIORITY** classes first, particularly:
1. ✅ ~~Complete GUI infrastructure (BasicWindow, WindowBasedTextGUI)~~ **COMPLETED**
2. ✅ ~~Essential components (ComboBox, SplitPanel)~~ **COMPLETED**
3. Terminal factory and platform abstractions
4. Core input decoding system
5. Graphics fundamentals (TextImage, VirtualScreen)

The GUI Core package is now 100% complete! The terminal layer needs the most work (only 26% complete) but is critical for cross-platform support.