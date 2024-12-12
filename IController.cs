using Gamepad;

namespace serialcontroller;

enum Direction {
    Left,
    Right,
    Up,
    Down
}

class DirectionEvent {
    public required Direction Direction { init; get;}
    public required short Value { init; get;}
}

// enum Plane: byte {
//     HORIZONTAL=0x00,
//     VERTICAL=0x01
// }

enum Button {
    A,
    Left,
    Right,
    Up,
    Down,
}

interface IController: IDisposable {
    event EventHandler<DirectionEvent>? DirectionChanged;
    event EventHandler<Button>? ButtonPressed;
}

// Axis 0 negative is left
// Axis 0 positive is right
// Axis 1 negative is up
// Axis 1 positive is down
// Axis 6 -32767 is arrow left
// Axis 6 32767 is arrow right
// Axis 7 -32767 is arrow up
// Axis 7 32767 is arrow down
// A is Button 0
class Controller : IController
{
    private readonly GamepadController gamepad = new ("/dev/input/js0");
    public event EventHandler<DirectionEvent>? DirectionChanged;
    public event EventHandler<Button>? ButtonPressed;
    public short HorizontalValue = 0;
    public short VerticalValue = 0;

    public Controller() {
        gamepad.AxisChanged += (_, e) =>
        {
            switch (e.Axis)
            {
                case 6 when e.Value == -32767:
                    ButtonPressed?.Invoke(this, Button.Left);
                    return;
                case 6 when e.Value == 32767:
                    ButtonPressed?.Invoke(this, Button.Right);
                    return;
                case 7 when e.Value == -32767:
                    ButtonPressed?.Invoke(this, Button.Up);
                    return;
                case 7 when e.Value == 32767:
                    ButtonPressed?.Invoke(this, Button.Down);
                    return;
                case 0:
                    HorizontalValue = Math.Abs(e.Value);
                    break;
                case 1:
                    VerticalValue = Math.Abs(e.Value);
                    break;
            }

            if(e.Axis == 0 && HorizontalValue > VerticalValue) {
                if(e.Value < 0) {
                    DirectionChanged?.Invoke(this, new DirectionEvent {
                        Direction = Direction.Left,
                        Value = HorizontalValue
                    });
                    return;
                }
                if(e.Value > 0) {
                    DirectionChanged?.Invoke(this, new DirectionEvent {
                        Direction = Direction.Right,
                        Value = HorizontalValue
                    });
                    return;
                }
            }
            if(e.Axis == 1 && VerticalValue > HorizontalValue) {
                if(e.Value < 0) {
                    DirectionChanged?.Invoke(this, new DirectionEvent {
                        Direction = Direction.Up,
                        Value = VerticalValue 
                    });
                    return;
                }
                if(e.Value > 0) {
                    DirectionChanged?.Invoke(this, new DirectionEvent {
                        Direction = Direction.Down,
                        Value = VerticalValue 
                    });
                    return;
                }
            }
        };

        gamepad.ButtonChanged += (_, e) =>
        {
            if(e.Pressed && e.Button == 0) {
                ButtonPressed?.Invoke(this, Button.A);
            }
        };
    }

    
    public void Dispose()
    {
        gamepad.Dispose();
    }
}