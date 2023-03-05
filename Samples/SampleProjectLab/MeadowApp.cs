using System;
using System.Diagnostics;
using System.Reflection;
using Glade2d;
using Glade2d.Services;
using GladeSampleShared.Screens;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using Meadow.Units;
using System.Threading.Tasks;

namespace SampleProjectLab
{
    public class MeadowApp : App<F7FeatherV2>
    {
        IGraphicsDisplay display;
        Game glade;

        public override Task Run()
        {
            LogService.Log.Trace("Initializing Glade game engine...");
            glade = new Game();
            glade.Initialize(display, 1, EngineMode.GameLoop);
            glade.Profiler.IsActive = true;

            LogService.Log.Trace("Running game...");
            glade.Start(new GladeDemoScreen());

            return base.Run();
        }

        public override Task Initialize()
        {
            InitializeDisplay();
            return base.Initialize();
        }

        private void InitializeDisplay()
        {
            LogService.Log.Trace("Initializing SPI bus...");
            var config = new SpiClockConfiguration(
                new Frequency(48000, Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode3);
            var spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.COPI, Device.Pins.CIPO, config);

            LogService.Log.Trace("Initializing MCP...");
            var mcp_in = Device.CreateDigitalInputPort(
                Device.Pins.D09,
                InterruptMode.EdgeRising,
                ResistorMode.InternalPullDown);
            var mcp_out = Device.CreateDigitalOutputPort(Device.Pins.D14);
            var mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, mcp_in, mcp_out);

            LogService.Log.Trace("Initializing ST7789 display...");
            var chipSelectPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
            var dcPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
            var resetPort = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);

            var st7789 = new St7789(
                spiBus: spi,
                chipSelectPort: chipSelectPort,
                dataCommandPort: dcPort,
                resetPort: resetPort,
                width: 240, height: 240,
                colorMode: ColorMode.Format16bppRgb565);

            var busNumberProperty = typeof(SpiBus).GetProperty("BusNumber",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (busNumberProperty == null)
            {
                throw new InvalidOperationException("No bus number");
            }

            var busNumber = (int)busNumberProperty.GetValue(spi);
            Console.WriteLine($"Bus number: {busNumber}");

            // st7789.SetRotation(TftSpiBase.Rotation.Rotate_90);
            Console.WriteLine($"Drive handle: {LocalInterop.DriverHandle}");

            var buffer = new byte[240 * 240 * 2];
            for (var x = 0; x < buffer.Length; x += 2)
            {
                buffer[x] = 0b11111000;
            }

            var span = buffer.AsSpan();
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 1 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 1 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 60 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 120 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 180 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 240 * 2), busNumber);
            MeasureWrite(chipSelectPort, span.Slice(0, 240 * 240 * 2), busNumber);
            

            display = st7789;
        }

        private static void MeasureWrite(IDigitalOutputPort chipSelectPort, Span<byte> span, int busNumber)
        {
            var stopwatch = Stopwatch.StartNew();
            var ioctlTIming = Write(chipSelectPort, span, busNumber);
            stopwatch.Stop();
            Console.WriteLine($"Buffer with {span.Length} bytes written in {stopwatch.ElapsedMilliseconds} ms ({ioctlTIming})");
        }


        private static unsafe long Write(
            IDigitalOutputPort? chipSelect,
            Span<byte> writeBuffer,
            int busNumber,
            ChipSelectMode csMode = ChipSelectMode.ActiveLow)
        {
            if (chipSelect != null)
                chipSelect.State = csMode != ChipSelectMode.ActiveLow;

            fixed (byte* numPtr = &writeBuffer.GetPinnableReference())
            {
                var spiCommand = new UpdSPIDataCommand()
                {
                    BufferLength = writeBuffer.Length,
                    TxBuffer = (IntPtr)(void*)numPtr,
                    RxBuffer = IntPtr.Zero,
                    BusNumber = busNumber
                };

                var timer = Stopwatch.StartNew();
                var (result, timing) = LocalInterop.Ioctl(UpdIoctlFn.SPIData, ref spiCommand);
                timer.Stop();
                Console.WriteLine($"Outer ioctl: {timer.ElapsedMilliseconds}");
                if (result != 0)
                {
                    Console.WriteLine($"Error code: {LocalInterop.GetLastError()}");
                }

                if (chipSelect == null)
                {
                    return timing;
                }

                chipSelect.State = csMode == ChipSelectMode.ActiveLow;

                return timing;
            }
        }
    }
}