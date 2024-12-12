using System.IO.Ports;
using Mythosia.Integrity.CRC;

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
    private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);
    private readonly SerialPort serialPort = new SerialPort("/dev/ttyS0", 115200);

    public async Task Drive(DriveMode mode, byte[] data, byte sensor) {
        if(data.Length > 3) {
            throw new Exception();
        }
        using var raii = await semaphore.Raii();
        var frame = (new byte[] {0x01, 0x01, (byte) mode, 0x00, 0x00, 0x00, sensor}).WithCRC8().ToArray();
        serialPort.Write(frame, 0, frame.Length);
        // todo: check
        var _ = await serialPort.ReadAsync(8);
    }

    public async Task<(double, double)> Position() {
        using var raii = await semaphore.Raii();
        var frame = (new byte[] {0x01, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00}).WithCRC8().ToArray();
        serialPort.Write(frame, 0, frame.Length);

        var response = await serialPort.ReadAsync(8);
        var absByte = response[2];
        var relByte = response[3];
        var abs = 10.0 / 0xFF * absByte;
        var rel = 10.0 / 0xFF * relByte;
        return (abs, rel);
    }

    public void Dispose()
    {
        serialPort.Dispose();
    }
}