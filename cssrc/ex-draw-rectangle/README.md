# Ex-Draw-Rectangle - Lanterna C# Terminal Examples

This project demonstrates the Lanterna C# port's terminal and screen layer functionality.

## Running the Examples

```bash
# Basic demos
dotnet run                    # Shows basic data types and color system
dotnet run --terminal         # Interactive terminal layer demo  
dotnet run --screen          # Advanced screen layer demo with animations
dotnet run --test-cleanup    # Tests terminal cleanup functionality
```

## Known Issue: Terminal Echo

After running the terminal or screen demos, your VT100-compatible terminal may stop echoing typed characters. This happens because escape sequence responses remain in the input buffer.

### Why This Happens

1. Some ANSI escape sequences (like cursor position queries ESC[6n) cause the terminal to send response data
2. If these responses aren't fully consumed before the program exits, they remain in the input buffer
3. The shell interprets these leftover escape sequences as input, disrupting echo and other settings
4. Background threads may continue sending queries during program shutdown

### Solutions

We've implemented several fixes:

1. **Improved cleanup in UnixTerminal.cs**:
   - Overrides GetCursorPosition() to avoid sending ESC[6n queries
   - Stops background threads immediately during disposal
   - Aggressively drains input buffer using multiple techniques
   - Adds delays between cleanup steps to ensure all responses are received
   - Uses multiple methods to clear input (bash read loops, dd, Python tcflush)

2. **Helper scripts**:
   - `./reset-terminal.sh` - Comprehensive terminal reset
   - `./diagnose-terminal.sh` - Diagnose terminal state

### If Your Terminal Stops Echoing

1. **Quick fix**: Type `reset` and press Enter (even if you can't see it)
2. **Script fix**: Run `./reset-terminal.sh`
3. **Manual fix**: 
   ```bash
   stty sane
   stty echo
   ```

### Prevention

The latest version includes:
- Multiple input draining passes during cleanup
- Proper thread synchronization
- Avoidance of problematic escape sequences
- Extended delays for response handling

## Technical Details

The issue primarily affects:
- VT100/xterm compatible terminals
- SSH sessions
- Terminal emulators that strictly follow ANSI standards

The fix ensures all escape sequence responses are consumed before returning control to the shell.