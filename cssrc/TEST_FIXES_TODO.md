# Lanterna-Sharp Test Fixes TODO List

## Current Status
12 failing tests out of 74 total tests (reduced from 14 failing out of 61)

## Completed Fixes ✅
1. **TerminalSize.ToString()** - Fixed format to return "TerminalSize{columns=X, rows=Y}"
2. **TextCharacter.ToString()** - Fixed to return only the character
3. **TerminalPosition.ToString()** - Fixed format to return "TerminalPosition{column=X, row=Y}"
4. **Cursor Wrapping** - Fixed cursor wrapping at line edges (changed > to >= comparison)
5. **TextBuffer Double-Width Character Bug** - Fixed double-width character padding detection using reference equality instead of value equality, preventing space characters from being treated as padding

## Remaining Issues

### 1. Screen Rendering Issues (High Priority) - PARTIALLY FIXED ⚠️
- **CanRefreshScreenToTerminal** - Screen refresh not writing to terminal
- **CanTestScreenGraphics** - Graphics not rendering
- **CanTestMultipleScreenOperations** - Complex operations not working
- **CanTestPartialRefresh** - Partial refresh not tracking dirty cells
- **Issue**: While the TextBuffer character writing bug is fixed, there's still an issue with `DrawCharacterToTerminal` not being called or not working correctly during screen refresh. Investigation shows the refresh logic is working correctly at the buffer level but characters aren't appearing in the virtual terminal.

### 2. Private Mode Cursor Position (Medium Priority)
- **CanEnterAndExitPrivateMode** - Cursor position incorrect after exiting private mode
- **Issue**: Test expects cursor at [5:5] but it's at [6:5] (cursor advances after writing)
- **Note**: This might be a test expectation issue rather than implementation bug

### 3. Integration & UI Rendering (Medium Priority)
- **CanTestCompleteDialogInterface** - Dialog not rendering
- **CanTestMenuSystem** - Menu system not working
- **Issue**: All dependent on screen rendering working correctly

### 4. Input Handling (Low Priority)
- **CanSimulateModifierKeys** - Modifier key simulation not working
- **Issue**: KeyStroke implementation may not handle modifiers correctly

## Root Cause Analysis

The main issues identified and their status:

1. **✅ FIXED - Character Writing Bug**: The TextBuffer had a critical bug where space characters were being treated as double-width padding due to value equality check instead of reference equality. This has been fixed.

2. **⚠️ PARTIAL - Screen-Terminal Disconnect**: The TerminalScreen refresh mechanism has issues where `DrawCharacterToTerminal` is being called but characters don't appear in the virtual terminal. Investigation shows:
   - RefreshComplete is being called (not RefreshDelta)
   - Characters are correctly identified in back buffer
   - DrawCharacterToTerminal is being called for each character
   - Manual replication of DrawCharacterToTerminal works
   - But screen refresh doesn't result in characters appearing in terminal

3. **Test Expectation Issues**: Some tests (like private mode) may have incorrect expectations about cursor behavior.

## Recommended Fix Order

1. **✅ COMPLETED - Fix TextBuffer Character Writing Bug**
2. **🔄 IN PROGRESS - Debug DrawCharacterToTerminal issue** - Need to identify why manual calls work but screen refresh calls don't
3. **Review Test Expectations** - Some tests may need their assertions updated
4. **Fix Input Handling** - Lower priority, isolated issue

## Investigation Notes

- The character writing issue was in TextBuffer.cs lines 132 and 171
- Screen refresh uses RefreshComplete (not RefreshDelta) due to _fullRedrawHint=true
- DrawCharacterToTerminal is called but doesn't affect the virtual terminal during refresh
- Manual sequence replication works perfectly
- Issue may be related to terminal state during refresh sequence