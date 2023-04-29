using Glade2d.Graphics;
using Glade2d.Services;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;

namespace GladeInvade.ProjectLab;

public class GladeSt7789 : St7789, IGladeDisplay
{
    private const int BytesPerPixel = 2;
    private readonly byte[] _sectionBuffer;

    public GladeSt7789(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, int width, int height,
        ColorMode colorMode)
        : base(spiBus, chipSelectPin, dcPin, resetPin, width, height, colorMode)
    {
        if (colorMode != ColorMode.Format16bppRgb565)
        {
            throw new InvalidOperationException("Invalid color mode");
        }

        _sectionBuffer = new byte[width * height * BytesPerPixel];
    }

    public GladeSt7789(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort dataCommandPort,
        IDigitalOutputPort resetPort, int width, int height, ColorMode colorMode = ColorMode.Format12bppRgb444)
        : base(spiBus, chipSelectPort, dataCommandPort, resetPort, width, height, colorMode)
    {
        if (colorMode != ColorMode.Format16bppRgb565)
        {
            throw new InvalidOperationException("Invalid color mode");
        }

        _sectionBuffer = new byte[width * height * BytesPerPixel];
    }

    public void WriteSection(int left, int top, int right, int bottom)
    {
        var profiler = GameService.Instance.GameInstance.Profiler;
        left = Math.Max(left, 0);
        right = Math.Min(right, Width - 1);
        top = Math.Max(top, 0);
        bottom = Math.Min(bottom, Height - 1);
        var sectionWidth = right - left;
        var sectionHeight = bottom - top;

        if (sectionWidth <= 0 || sectionHeight <= 0)
        {
            // Section is off the pixel buffer
            return;
        }
       
        profiler.StartTiming("GladeST7789.Copy");
        var bytesPerFullRow = Width * BytesPerPixel;
        var bytesPerSectionRow = sectionWidth * BytesPerPixel;
        for (var row = 0; row < sectionHeight; row++)
        {
            var sourceStart = top * bytesPerFullRow +
                              row * bytesPerFullRow +
                              left * BytesPerPixel;

            var targetStart = row * BytesPerPixel;

            Array.Copy(PixelBuffer.Buffer,
                sourceStart,
                _sectionBuffer,
                targetStart,
                bytesPerSectionRow);
        }
        var bufferToSend = _sectionBuffer.AsSpan(0, sectionWidth * sectionHeight * BytesPerPixel);
        profiler.StopTiming("GladeST7789.Copy");

        profiler.StartTiming("GladeST7789.SPI");
        profiler.StartTiming("GladeST7789.AddressWindow");
        SetAddressWindow(left, top, right, bottom);
        profiler.StopTiming("GladeST7789.AddressWindow");
        dataCommandPort.State = true;
        profiler.StartTiming($"GladeST7789.Write {bufferToSend.Length}");
        spiDisplay.Bus.Write(chipSelectPort, bufferToSend);
        profiler.StopTiming($"GladeST7789.Write {bufferToSend.Length}");
        profiler.StopTiming("GladeST7789.SPI");
    }

    protected override void SetAddressWindow(int x0, int y0, int x1, int y1)
    {
        // x0 += xOffset;
        // y0 += yOffset;
        //
        // x1 += xOffset;
        // y1 += yOffset;

        SendCommand(LcdCommand.CASET);  // column addr set
        dataCommandPort.State = Data;
        var dataPart1 = new byte[]
        {
            (byte)(x0 >> 8),
            (byte)(x0 & 0xff),
            (byte)(x1 >> 8),
            (byte)(x1 & 0xff),
        };
        
        Write(dataPart1);
        // Write((byte)(x0 >> 8));
        // Write((byte)(x0 & 0xff));   // XSTART 
        // Write((byte)(x1 >> 8));
        // Write((byte)(x1 & 0xff));   // XEND

        SendCommand(LcdCommand.RASET);  // row addr set
        dataCommandPort.State = Data;
        // Write((byte)(y0 >> 8));
        // Write((byte)(y0 & 0xff));    // YSTART
        // Write((byte)(y1 >> 8));
        // Write((byte)(y1 & 0xff));    // YEND
        var dataPart2 = new byte[]
        {
            (byte)(y0 >> 8),
            (byte)(y0 & 0xff),
            (byte)(y1 >> 8),
            (byte)(y1 & 0xff),
        };
        Write(dataPart2);

        SendCommand(LcdCommand.RAMWR);  // write to RAM
    }
}