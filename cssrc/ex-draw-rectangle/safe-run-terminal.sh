#!/bin/bash

# Safe wrapper to run terminal demo and ensure echo is restored

echo "=== Safe Terminal Demo Runner ==="
echo "This wrapper ensures terminal echo is restored after the demo."
echo

# Save current terminal state
SAVED_STTY=$(stty -g 2>/dev/null)
echo "Saved terminal state: OK"

# Function to restore terminal
restore_terminal() {
    echo
    echo "Restoring terminal settings..."
    
    # Try multiple methods to restore
    if [ -n "$SAVED_STTY" ]; then
        stty "$SAVED_STTY" 2>/dev/null || true
    fi
    
    stty echo icanon 2>/dev/null || true
    stty sane 2>/dev/null || true
    
    # Clear any pending input
    while read -t 0 -n 1; do :; done 2>/dev/null || true
    
    echo "Terminal restored."
}

# Set up trap to restore on exit
trap restore_terminal EXIT INT TERM

# Run the terminal demo
echo "Starting terminal demo..."
echo "Press Enter to continue..."
read -r

dotnet run --terminal

# Explicitly restore (trap will also do it)
restore_terminal

# Test echo
echo
echo "Testing echo functionality..."
echo -n "Type something to verify echo works: "
read -r test_input

if [ -n "$test_input" ]; then
    echo "Success! You typed: $test_input"
else
    echo "Warning: Couldn't read input. You may need to run ./reset-terminal.sh"
fi