using System.IO.Ports;
using Mythosia.Integrity.CRC;

namespace serialcontroller;

public static class SerialPortExtensions
{
    public async static Task ReadAsync(this SerialPort serialPort, byte[] buffer, int offset, int count)
    {
        var bytesToRead = count;
        var temp = new byte[count];

        while (bytesToRead > 0)
        {
            var readBytes = await serialPort.BaseStream.ReadAsync(temp, 0, bytesToRead);
            Array.Copy(temp, 0, buffer, offset + count - bytesToRead, readBytes);
            bytesToRead -= readBytes;
        }
    }

    public async static Task<byte[]> ReadAsync(this SerialPort serialPort, int count)
    {
        var buffer = new byte[count];
        await serialPort.ReadAsync(buffer, 0, count);
        return buffer;
    }
}

public static class SemaphoreSlimExtensions {
    public async static Task<IDisposable> Raii(this SemaphoreSlim semaphore) {
        await semaphore.WaitAsync();
        return new Lock(semaphore);

    }
    private class Lock(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose()
        {
            semaphore.Release();
        }
    }
}

interface IDriveTrain: IDisposable {
    public Task Drive(DriveMode mode, byte[] data, byte sensor);
    public Task<(double, double)> Position();
}

enum DriveMode: byte {
    Straight=0x01,
    TurnOnPoint=0x02,
}

class UartDriveTrain : IDriveTrain
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly SerialPort _serialPort = new("/dev/ttyAMA0", 115200);

    public UartDriveTrain() {
        _serialPort.Open();
    }

    public async Task Drive(DriveMode mode, byte[] data, byte sensor) {
        if(data.Length > 3) {
            throw new Exception();
        }
        using var raii = await _semaphore.Raii();

        var frame = new byte[] {0x01, 0x01, (byte) mode, 0x00, 0x00, 0x00, sensor};
        data.CopyTo(frame, 3);
        frame = frame.WithCRC8(CRC8Type.Maxim).ToArray();

        _serialPort.Write(frame, 0, frame.Length);
        // todo: check
        var _ = await _serialPort.ReadAsync(8);
    }

    public async Task<(double, double)> Position() {
        using var raii = await _semaphore.Raii();
        var frame = new byte[] {0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00}
            .WithCRC8(CRC8Type.Maxim)
            .ToArray();
        _serialPort.Write(frame, 0, frame.Length);

        // var response = await serialPort.ReadAsync(8);
        // var absByte = response[2];
        // var relByte = response[3];
        // var abs = 10.0 / 0xFF * absByte;
        // var rel = 10.0 / 0xFF * relByte;
        return (0.0, 0.0);
    }

    public void Dispose()
    {
        _serialPort.Dispose();
    }
}

class NoopDriveTrain : IDriveTrain
{
    public void Dispose()
    {
    }

    public Task Drive(DriveMode mode, byte[] data, byte sensor)
    {
        return Task.CompletedTask;
    }

    public Task<(double, double)> Position()
    {
        return Task.FromResult((0.0, 0.0));
    }
}