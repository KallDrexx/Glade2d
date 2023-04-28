using System;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.BufferTransferring;

internal class NoRotationBufferTransferrer : IBufferTransferrer
{
    public void Transfer(BufferRgb565 source, BufferRgb565 target, int scale, RenderRegion? region)
    {
        if (source.Width * scale != target.Width || source.Height * scale != target.Height)
        {
            var message = $"Source dimensions at scale {scale} is not compatible with " +
                          "the target dimensions: " +
                          $"{source.Width}x{source.Height} vs {target.Width}x{target.Height}";

            throw new InvalidOperationException(message);
        }
        
        TransferBuffer(source, target, scale, region);
    }

    private static unsafe void TransferBuffer(BufferRgb565 source, BufferRgb565 target, int scale, RenderRegion? region)
    {
        var sourceWidth = region?.Width ?? source.Width;
        var sourceHeight = region?.Height ?? source.Height;
        var sourceRowByteLength = source.Width * Renderer.BytesPerPixel;
        var targetRowByteLength = target.Width * Renderer.BytesPerPixel;
        var targetRegionByteLength = sourceWidth * scale * Renderer.BytesPerPixel;
        var sourceRowStartIndex = region?.Y ?? 0;
        var startCol = region?.X ?? 0;
        
        fixed (byte* sourceBufferPtr = source.Buffer)
        fixed (byte* targetBufferPtr = target.Buffer)
        {
            for (var sourceRow = sourceRowStartIndex; sourceRow < sourceHeight; sourceRow++)
            {
                var sourceByte1 = sourceBufferPtr + 
                                  sourceRow * sourceRowByteLength +
                                  startCol * Renderer.BytesPerPixel;

                var targetByte1 = targetBufferPtr +
                                  sourceRow * scale * targetRowByteLength +
                                  startCol * scale * Renderer.BytesPerPixel;

                var arrayCopyStartIndex = (int)(targetByte1 - targetBufferPtr);
                
                for (var sourceCol = startCol; sourceCol < sourceWidth; sourceCol++)
                {
                    for (var scaleX = 0; scaleX < scale; scaleX++)
                    {
                        *targetByte1 = *sourceByte1;
                        *(targetByte1 + 1) = *(sourceByte1 + 1);

                        targetByte1 += Renderer.BytesPerPixel;
                    }

                    sourceByte1 += Renderer.BytesPerPixel;
                }

                // Now copy the previously pre-scale row
                for (var scaleY = 1; scaleY < scale; scaleY++)
                {
                    Array.Copy(
                        target.Buffer,
                        arrayCopyStartIndex,
                        target.Buffer,
                        arrayCopyStartIndex + targetRegionByteLength,
                        targetRegionByteLength);
                }
            }
        }
    }
}