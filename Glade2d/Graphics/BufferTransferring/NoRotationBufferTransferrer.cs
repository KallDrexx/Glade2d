﻿using System;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.BufferTransferring;

internal class NoRotationBufferTransferrer : IBufferTransferrer
{
    public void Transfer(BufferRgb565 source, BufferRgb565 target, int scale)
    {
        var sourceWidth = source.Width;
        var sourceHeight = source.Height;
        var targetWidth = target.Width;
        var targetHeight = target.Height;

        if (sourceWidth * scale != targetWidth || sourceHeight * scale != targetHeight)
        {
            var message = $"Source dimensions at scale {scale} is not compatible with " +
                          "the target dimensions: " +
                          $"{sourceWidth}x{sourceHeight} vs {targetWidth}x{targetHeight}";

            throw new InvalidOperationException(message);
        }

        unsafe
        {
            fixed (byte* sourceBufferPtr = source.Buffer)
            fixed (byte* targetBufferPtr = target.Buffer)
            {
                var targetRowStartIndex = 0;
                var sourceByte1 = sourceBufferPtr;
                var targetByte1 = targetBufferPtr;

                for (var sourceRow = 0; sourceRow < sourceHeight; sourceRow++)
                {
                    // First copy the source row scaled horizontally
                    for (var sourceCol = 0; sourceCol < sourceWidth; sourceCol++)
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
                    // for (var scaleY = 1; scaleY < scale; scaleY++)
                    // {
                    //     Array.Copy(
                    //         target.Buffer,
                    //         targetRowStartIndex,
                    //         target.Buffer,
                    //         targetRowStartIndex + targetWidth,
                    //         targetWidth);
                    //
                    //     targetRowStartIndex += targetWidth;
                    //     targetByte1 += targetWidth;
                    // }

                    targetRowStartIndex += targetWidth;
                }
            }
        }
    }
}