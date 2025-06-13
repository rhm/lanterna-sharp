#!/bin/bash

echo "Testing terminal cleanup..."

# Run the screen demo briefly and then exit
timeout 2s dotnet run --screen &
DEMO_PID=$!

# Let it run for a moment
sleep 1

# Kill it abruptly to simulate unexpected exit
kill $DEMO_PID 2>/dev/null

# Wait a moment for cleanup
sleep 1

# Test if there's any leftover input that would be interpreted by shell
echo "Type 'test' and press Enter to verify shell is working normally:"
read response

if [ "$response" = "test" ]; then
    echo "✓ Terminal cleanup appears to be working correctly!"
else
    echo "✗ Detected leftover input: '$response'"
    echo "Running reset script..."
    ./reset-terminal.sh
fi