using serialcontroller;

using IDriveTrain driveTrain = new UartDriveTrain();
using IController controller = new Controller();

DirectionEvent? lastDirectionEvent = null;
controller.DirectionChanged += (_, e) => {
    if(lastDirectionEvent?.Direction != e.Direction) {
        Console.WriteLine("New Direction " + e.Direction);
    }
    lastDirectionEvent = e;
};

controller.ButtonPressed += (_, button) => {
    if(button != Button.A) {
        return;
    }
    if(lastDirectionEvent == null) {
        return;
    }
    if(lastDirectionEvent.Direction == Direction.Up) {
        var distance = Convert.ToByte(lastDirectionEvent.Value * 255 / 32767);
        Console.WriteLine("Straight" + distance);
        driveTrain.Drive(DriveMode.Straight, [distance, distance], 0x00);
    }
    if(lastDirectionEvent.Direction == Direction.Left) {
        var left = Convert.ToByte(128 + (lastDirectionEvent.Value * 127 / 32767));
        Console.WriteLine("Left" + left);
        driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, left], 0x00);
    }
    if(lastDirectionEvent.Direction == Direction.Right) {
        var right = Convert.ToByte(lastDirectionEvent.Value * 128 / 32767);
        Console.WriteLine("Right" + right);
        driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, right], 0x00);
    }
};

Console.ReadKey();