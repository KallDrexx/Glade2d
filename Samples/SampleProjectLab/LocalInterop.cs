using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SampleProjectLab
{
    public struct UpdSPIDataCommand
    {
        public IntPtr TxBuffer;
        public IntPtr RxBuffer;

        /// <summary>SPI requires both buffers be equal length</summary>
        public int BufferLength;

        public int BusNumber;
    }

    public enum UpdIoctlFn
    {
        SetRegister = 1,
        GetRegister = 2,
        UpdateRegister = 3,
        RegisterGpioIrq = 5,
        PwmSetup = 10, // 0x0000000A
        PwmShutdown = 11, // 0x0000000B
        PwmStart = 12, // 0x0000000C
        PwmStop = 13, // 0x0000000D
        I2CShutdown = 20, // 0x00000014
        I2CData = 21, // 0x00000015
        SPIData = 31, // 0x0000001F
        SPISpeed = 32, // 0x00000020
        SPIMode = 33, // 0x00000021
        SPIBits = 34, // 0x00000022
        DirEnum = 41, // 0x00000029
        GetLastError = 51, // 0x00000033
        Esp32Command = 61, // 0x0000003D
        UpdEsp32EventDataPayload = 62, // 0x0000003E
        PowerReset = 71, // 0x00000047
        PowerSleep = 72, // 0x00000048
        PowerWDSet = 74, // 0x0000004A
        PowerWDPet = 75, // 0x0000004B
        GetDeviceInfo = 81, // 0x00000051
        GetSetConfigurationValue = 82, // 0x00000052
    }

    public enum ErrorCode
    {
        OperationNotPermitted = 1,
        NoSuchFileOrDirectory = 2,
        NoSuchProcess = 3,
        InterruptedSystemCall = 4,
        IOError = 5,
        NoSuchDeviceOrAddress = 6,
        ArgListTooLong = 7,
        ExecFormatError = 8,
        BadFileNumber = 9,
        NoChildProcess = 10, // 0x0000000A
        TryAgain = 11, // 0x0000000B
        OutOfMemory = 12, // 0x0000000C
        PermissionDenied = 13, // 0x0000000D
        BadAddress = 14, // 0x0000000E
        BlockDevieRequired = 15, // 0x0000000F
        DeviceOrResourceBusy = 16, // 0x00000010
        FileExists = 17, // 0x00000011
        CrossDeviceLink = 18, // 0x00000012
        NoSuchDevice = 19, // 0x00000013
        NotADirectory = 20, // 0x00000014
        IsADirectory = 21, // 0x00000015
        InvalidArgument = 22, // 0x00000016
    }

    [Flags]
    public enum DriverFlags
    {
        DontCare = 0,
        ReadOnly = 1,
        WriteOnly = 2,
        ReadWrite = WriteOnly | ReadOnly, // 0x00000003
        Create = 4,
        Exclusive = 8,
        Append = 16, // 0x00000010
        Truncate = 32, // 0x00000020
        NonBlocking = 64, // 0x00000040
        SynchronizeOutput = 128, // 0x00000080
        Binary = 256, // 0x00000100
        Direct = 512, // 0x00000200
    }

    public static class LocalInterop
    {
        public static (int, long) Ioctl(UpdIoctlFn request, ref UpdSPIDataCommand spiCommand)
        {
            var driverHandle = DriverHandle;
            var timer = Stopwatch.StartNew();
            int num = ioctl(driverHandle, request, ref spiCommand);
            timer.Stop();

            return (num != 0 ? (int)GetLastError() : num, timer.ElapsedMilliseconds);
        }

        public static IntPtr DriverHandle { get; } = open("/dev/upd", DriverFlags.DontCare);

        [DllImport("nuttx", SetLastError = true)]
        public static extern IntPtr open(string pathname, DriverFlags flags);

        [DllImport("nuttx", SetLastError = true)]
        public static extern int ioctl(
            IntPtr fd,
            UpdIoctlFn request,
            ref UpdSPIDataCommand spiCommand);

        [DllImport("nuttx", SetLastError = true)]
        public static extern int ioctl(IntPtr fd, UpdIoctlFn request, ref int dwData);

        public static ErrorCode GetLastError()
        {
            int dwData = 0;
            ioctl(DriverHandle, UpdIoctlFn.GetLastError, ref dwData);
            return (ErrorCode)dwData;
        }
    }
}