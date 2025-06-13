#!/bin/bash

# Emergency terminal reset script
# Run this if your terminal stops echoing after running the screen demo

echo "Resetting terminal..."

# Clear any pending input first
read -t 0.1 -n 10000 discard 2>/dev/null || true

# Try multiple methods to restore terminal
stty sane 2>/dev/null || true
stty echo 2>/dev/null || true  
stty icanon 2>/dev/null || true
stty intr ^C 2>/dev/null || true

# Clear input again after restoring settings
read -t 0.1 -n 10000 discard 2>/dev/null || true

# Reset terminal completely
reset 2>/dev/null || true

# Final input clear
read -t 0.1 -n 10000 discard 2>/dev/null || true

echo "Terminal should now be restored."
echo "If this doesn't work, try closing and reopening your terminal."