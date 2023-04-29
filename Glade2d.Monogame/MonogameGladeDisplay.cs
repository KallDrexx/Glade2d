using System;
using Glade2d.Graphics;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;
using MeadowMgTestEnvironment;

namespace Glade2d.Monogame;

public class MonogameGladeDisplay : IGladeDisplay, IGraphicsDisplay
{
    private const int BytesPerPixel = 2;
    private readonly byte[] _sectionBuffer;
    
    private readonly MonogameDisplay _innerDisplay;

    public ColorMode ColorMode => _innerDisplay.ColorMode;

    public ColorMode SupportedColorModes => _innerDisplay.SupportedColorModes;

    public int Width => _innerDisplay.Width;

    public int Height => _innerDisplay.Height;

    public IPixelBuffer PixelBuffer => _innerDisplay.PixelBuffer;

    public MonogameGladeDisplay(MonogameDisplay innerDisplay)
    {
        _innerDisplay = innerDisplay;
        _sectionBuffer = new byte[Width * Height * BytesPerPixel];
    }

    public void WriteSection(int left, int top, int right, int bottom)
    {
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
       
        var bytesPerFullRow = Width * BytesPerPixel;
        var bytesPerSectionRow = sectionWidth * BytesPerPixel;
        for (var row = 0; row < sectionHeight; row++)
        {
            var sourceStart = top * bytesPerFullRow +
                              row * bytesPerFullRow +
                              left * BytesPerPixel;

            var targetStart = row * bytesPerSectionRow;

            Array.Copy(PixelBuffer.Buffer,
                sourceStart,
                _sectionBuffer,
                targetStart,
                bytesPerSectionRow);
        }

        var bufferToSend = _sectionBuffer
            .AsSpan(0, sectionWidth * sectionHeight * BytesPerPixel)
            .ToArray();
        
        _innerDisplay.WriteBuffer(left, top, new BufferRgb565(sectionWidth, sectionHeight, bufferToSend));
        // _innerDisplay.Show(left, top, right, bottom);
        _innerDisplay.Show();
    }

    public void Show()
    {
        _innerDisplay.Show();
    }

    public void Show(int left, int top, int right, int bottom)
    {
        _innerDisplay.Show(left, top, right, bottom);
    }

    public void Clear(bool updateDisplay = false)
    {
        _innerDisplay.Clear(updateDisplay);
    }

    public void Fill(Color fillColor, bool updateDisplay = false)
    {
        _innerDisplay.Fill(fillColor, updateDisplay);
    }

    public void Fill(int x, int y, int width, int height, Color fillColor)
    {
        _innerDisplay.Fill(x, y, width, height, fillColor);
    }

    public void DrawPixel(int x, int y, Color color)
    {
        _innerDisplay.DrawPixel(x, y, color);
    }

    public void DrawPixel(int x, int y, bool enabled)
    {
        _innerDisplay.DrawPixel(x, y, enabled);
    }

    public void InvertPixel(int x, int y)
    {
        _innerDisplay.InvertPixel(x, y);
    }

    public void WriteBuffer(int x, int y, IPixelBuffer displayBuffer)
    {
        _innerDisplay.WriteBuffer(x, y, displayBuffer);
    }
}