#!/bin/bash

# Emergency terminal reset script
# Run this if your terminal stops echoing after running the screen demo

echo "Resetting terminal..."

# Clear any pending input first (multiple aggressive methods)
for i in {1..5}; do
    read -t 0.001 -n 10000 discard < /dev/tty 2>/dev/null || true
done

# Consume all available input
while read -t 0 -n 1; do :; done < /dev/tty 2>/dev/null || true

# Use dd to clear more aggressively
dd if=/dev/tty of=/dev/null bs=1 count=10000 iflag=nonblock 2>/dev/null || true

# Try Python tcflush if available (most direct method)
python3 -c 'import termios,sys; termios.tcflush(sys.stdin.fileno(), termios.TCIFLUSH)' 2>/dev/null || true

# Clear line and reset cursor position
printf '\033[2K\r' > /dev/tty 2>/dev/null || true

# Try multiple methods to restore terminal
stty sane 2>/dev/null || true
stty echo 2>/dev/null || true  
stty icanon 2>/dev/null || true
stty intr ^C 2>/dev/null || true
stty icrnl 2>/dev/null || true
stty onlcr 2>/dev/null || true
stty -ixon 2>/dev/null || true  # Disable flow control

# Clear the screen and reset cursor
printf '\033[2J\033[H' 2>/dev/null || true

# Reset all graphics rendition
printf '\033[0m' > /dev/tty 2>/dev/null || true

# Make cursor visible
printf '\033[?25h' > /dev/tty 2>/dev/null || true

# Clear input again after restoring settings
for i in {1..3}; do
    read -t 0.001 -n 10000 discard < /dev/tty 2>/dev/null || true
    sleep 0.05
done

# Reset terminal completely
reset 2>/dev/null || true

# Final aggressive input clear with delays
sleep 0.1
while read -t 0 -n 1; do :; done < /dev/tty 2>/dev/null || true
dd if=/dev/tty of=/dev/null bs=1 count=10000 iflag=nonblock 2>/dev/null || true

echo "Terminal should now be restored."
echo "If this doesn't work, try closing and reopening your terminal."