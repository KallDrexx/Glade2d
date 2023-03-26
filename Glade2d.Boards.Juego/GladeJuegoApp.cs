using Glade2d.Services;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;
using Meadow.Units;

namespace Glade2d.Boards.Juego
{
    public abstract class GladeJuegoApp : GladeMeadowApp<F7CoreComputeV2>
    {
        protected PushButton LeftSideLeftButton = default!, 
            LeftSideUpButton = default!, 
            LeftSideDownButton = default!, 
            LeftSideRightButton = default!,
            RightSideLeftButton = default!, 
            RightSideUpButton = default!, 
            RightSideDownButton = default!, 
            RightSideRightButton = default!,
            StartButton = default!, 
            SelectButton = default!;

        protected override void InitializeBoard()
        {
            LogService.Log.Trace("Creating I2C Bus");
            var i2CBus = Device.CreateI2cBus();

            LogService.Log.Trace("Creating MCP1");
            var mcpReset = Device.CreateDigitalOutputPort(Device.Pins.D11, true);
            var mcp1Interrupt = Device.CreateDigitalInputPort(Device.Pins.D09, InterruptMode.EdgeRising);
            var mcp1 = new Mcp23008(i2CBus, 0x20, mcp1Interrupt, mcpReset);

            LogService.Log.Trace("Creating MCP2");
            var mcp2Interrupt = Device.CreateDigitalInputPort(Device.Pins.D10, InterruptMode.EdgeRising);
            var mcp2 = new Mcp23008(i2CBus, 0x21, mcp2Interrupt);

            LogService.Log.Trace("Initializing SPI bus...");
            var config = new SpiClockConfiguration(
                new Frequency(48000, Frequency.UnitType.Kilohertz),
                SpiClockConfiguration.Mode.Mode0);

            var spi = Device.CreateSpiBus(Device.Pins.SPI5_SCK, Device.Pins.SPI5_COPI, Device.Pins.SPI5_CIPO, config);

            LogService.Log.Trace("Initializing ILI9341 display...");
            var chipSelectPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP5);
            var dcPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP6);
            var resetPort = mcp1.CreateDigitalOutputPort(mcp1.Pins.GP7);

            // Turn on the display's backlight
            Device.CreateDigitalOutputPort(Device.Pins.D05, true);

            var ili9341 = new Ili9341(
                spi,
                chipSelectPort,
                dcPort,
                resetPort,
                240,
                320,
                ColorMode.Format16bppRgb565);

            Display = ili9341;

            LogService.Log.Trace("Initializing buttons...");
            LeftSideLeftButton = new PushButton(
                mcp1.CreateDigitalInputPort(mcp1.Pins.GP4, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            LeftSideUpButton = new PushButton(
                mcp1.CreateDigitalInputPort(mcp1.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            LeftSideDownButton = new PushButton(
                mcp1.CreateDigitalInputPort(mcp1.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            LeftSideRightButton = new PushButton(
                mcp1.CreateDigitalInputPort(mcp1.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            
            RightSideLeftButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            RightSideUpButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP5, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            RightSideDownButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            RightSideRightButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP4, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));

            StartButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
            
            SelectButton = new PushButton(
                mcp2.CreateDigitalInputPort(mcp2.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.ExternalPullUp));
        }
    }
}