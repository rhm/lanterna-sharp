# Lanterna-Sharp Test Fixes TODO List

## Current Status
14 failing tests out of 61 total tests (previously 16 failing)

## Completed Fixes ✅
1. **TerminalSize.ToString()** - Fixed format to return "TerminalSize{columns=X, rows=Y}"
2. **TextCharacter.ToString()** - Fixed to return only the character
3. **TerminalPosition.ToString()** - Fixed format to return "TerminalPosition{column=X, row=Y}"
4. **Cursor Wrapping** - Fixed cursor wrapping at line edges (changed > to >= comparison)

## Remaining Issues

### 1. Virtual Terminal Character Writing Issues (High Priority)
- **CanWriteString** - Multiple characters written in sequence not being stored properly
- **CanWriteTwoCharactersInSequence** - New test showing the issue more clearly
- **DebugCharacterWriting** - Debug test also failing
- **Issue**: When writing multiple characters in sequence, only the last character appears to be stored

### 2. Private Mode Cursor Position (Medium Priority)
- **CanEnterAndExitPrivateMode** - Cursor position incorrect after exiting private mode
- **Issue**: Test expects cursor at [5:5] but it's at [6:5] (cursor advances after writing)
- **Note**: This might be a test expectation issue rather than implementation bug

### 3. Screen Rendering Issues (High Priority)
- **CanRefreshScreenToTerminal** - Screen refresh not writing to terminal
- **CanTestScreenGraphics** - Graphics not rendering
- **CanTestMultipleScreenOperations** - Complex operations not working
- **CanTestPartialRefresh** - Partial refresh not tracking dirty cells
- **Issue**: Screen.Refresh() appears to not be writing characters to the underlying terminal

### 4. Integration & UI Rendering (Medium Priority)
- **CanTestBoxDrawing** - Box drawing characters not rendering
- **CanTestProgressBarRender** - Progress bar not rendering
- **CanValidateComplexTerminalOutput** - Complex output validation failing
- **CanTestCompleteDialogInterface** - Dialog not rendering
- **CanTestMenuSystem** - Menu system not working
- **Issue**: All dependent on screen rendering working correctly

### 5. Input Handling (Low Priority)
- **CanSimulateModifierKeys** - Modifier key simulation not working
- **Issue**: KeyStroke implementation may not handle modifiers correctly

## Root Cause Analysis

The main issues appear to be:

1. **Character Writing Bug**: The DefaultVirtualTerminal has a critical bug where sequential character writes overwrite previous characters or fail to store them properly. This is affecting multiple tests.

2. **Screen-Terminal Disconnect**: The TerminalScreen refresh mechanism isn't properly writing to the underlying virtual terminal. This could be due to:
   - Buffer comparison issues
   - Incorrect cursor positioning during refresh
   - Missing state synchronization

3. **Test Expectation Issues**: Some tests (like private mode) may have incorrect expectations about cursor behavior.

## Recommended Fix Order

1. **Fix Character Writing Bug** - This is critical and affects multiple tests
2. **Fix Screen Refresh** - This will likely fix most integration tests
3. **Review Test Expectations** - Some tests may need their assertions updated
4. **Fix Input Handling** - Lower priority, isolated issue

## Next Steps

1. Debug the character writing issue in DefaultVirtualTerminal
2. Add logging to understand why characters aren't being stored
3. Verify the TextBuffer implementation is working correctly
4. Check if there's a state management issue with cursor position