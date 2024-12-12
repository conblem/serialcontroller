using Gamepad;

// You should provide the gamepad file you want to connect to. /dev/input/js0 is the default
using var gamepad = new GamepadController("/dev/input/js0");
// Configure this if you want to get events when the state of a button changes
gamepad.ButtonChanged += (object? sender, ButtonEventArgs e) =>
{
    Console.WriteLine($"Button {e.Button} Pressed: {e.Pressed}");
};

// Configure this if you want to get events when the state of an axis changes
gamepad.AxisChanged += (object? sender, AxisEventArgs e) =>
{
    Console.WriteLine($"Axis {e.Axis} Pressed: {e.Value}");
};

using IDriveTrain driveTrain = new UartDriveTrain();
await driveTrain.Drive(DriveMode.Straight, [0x10], 0xFF);



Console.WriteLine("Hello, World!");
