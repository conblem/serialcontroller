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

controller.ButtonPressed += async (_, button) => {
    try
    {
        if (button != Button.A)
        {
            return;
        }

        if (lastDirectionEvent == null)
        {
            return;
        }

        if (lastDirectionEvent.Direction == Direction.Up)
        {
            var distance = Convert.ToByte(lastDirectionEvent.Value * 255 / 32767);
            Console.WriteLine("Straight" + distance);
            await driveTrain.Drive(DriveMode.Straight, [distance, distance], 0x00);
        }

        if (lastDirectionEvent.Direction == Direction.Left)
        {
            var left = Convert.ToByte(128 + (lastDirectionEvent.Value * 127 / 32767));
            Console.WriteLine("Left" + left);
            await driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, left], 0x00);
        }

        if (lastDirectionEvent.Direction == Direction.Right)
        {
            var right = Convert.ToByte(lastDirectionEvent.Value * 128 / 32767);
            Console.WriteLine("Right" + right);
            await driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, right], 0x00);
        }

        Console.WriteLine("Command run successfully");
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
        throw;
    }
};

Console.ReadKey();

// while (true)
// {
//     var key = Console.ReadKey();
//     if (key.Key == ConsoleKey.Q)
//     {
//         break;
//     }
//     
//     if (key.Key == ConsoleKey.W)
//     {
//         Console.WriteLine("Straight");
//         await driveTrain.Drive(DriveMode.Straight, [0xFF, 0xFF], 0x00);
//     }
//     if (key.Key == ConsoleKey.S)
//     {
//         Console.WriteLine("Stop");
//         await driveTrain.Drive(DriveMode.Straight, [0x00, 0x00], 0x00);
//     }
//
//     if (key.Key == ConsoleKey.A)
//     {
//         Console.WriteLine("Left");
//         await driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, 0xFF], 0x00);
//     }
//
//     if (key.Key == ConsoleKey.D)
//     {
//         Console.WriteLine("Right");
//         await driveTrain.Drive(DriveMode.TurnOnPoint, [0x00, 0x00], 0x00);
//     }
// }