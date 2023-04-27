﻿using System;
using System.Numerics;
using Glade2d.Services;
using Meadow.Foundation;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.Buffers;

namespace Glade2d.Graphics.Layers;

/// <summary>
/// A layer represents an isolated pixel buffer that can be drawn to, cached, and manipulated individually. 
/// </summary>
public class Layer
{
    private readonly record struct DrawableRectangle(Point Start, Dimensions Dimensions);
    
    private readonly BufferRgb565 _layerBuffer;
    private readonly TextureManager _textureManager;
    private Vector2 _internalOrigin;
    private Point _cameraOffset;
    private bool _hasRenderChanges;

    /// <summary>
    /// How far the layer's origin (0,0) is offset from the camera's origin. 
    /// </summary>
    public Point CameraOffset
    {
        get => _cameraOffset;
        set
        {
            _cameraOffset = value;
            _hasRenderChanges = true;
        }
    }
   
    /// <summary>
    /// The default background for this layer
    /// </summary>
    public Color BackgroundColor { get; set; } = Color.Black;
    
    /// <summary>
    /// What RGB value we consider to be transparent. When textures are drawn to the buffer, pixels of this color
    /// will be skipped.
    /// </summary>
    public Color TransparentColor { get; set; } = Color.Magenta;
   
    /// <summary>
    /// If true, then the layer will be drawn taking transparency into account. Any pixels that match the
    /// set transparent color will not be drawn to the target, and thus layers below will be visible. Enabling
    /// this slows down layer draw times and should only be enabled where a benefit to transparency will
    /// be given.
    /// </summary>
    public bool DrawLayerWithTransparency { get; set; }
    
    /// <summary>
    /// The width of the layer's canvas
    /// </summary>
    public int Width => _layerBuffer.Width;

    /// <summary>
    /// The height of the layer's canvas
    /// </summary>
    public int Height => _layerBuffer.Height;

    private Layer(BufferRgb565 layerBuffer, TextureManager textureManager)
    {
        _layerBuffer = layerBuffer;
        _textureManager = textureManager;
    }

    /// <summary>
    /// Creates a new Layer with the specified dimensions
    /// </summary>
    public static Layer Create(Dimensions dimensions)
    {
        var pixelBuffer = new BufferRgb565(dimensions.Width, dimensions.Height);
        return new Layer(pixelBuffer, GameService.Instance.GameInstance.TextureManager);
    }
   
    /// <summary>
    /// Creates a new Layer from an existing pixel buffer. Only really used for the internal sprite layer, so that
    /// we can immediately draw sprite textures to the MicroGraphics display buffer instead of adding the overhead of
    /// an intermediary buffer. Having sprites rendered via a layer allows consolidation of drawing code.
    /// </summary>
    internal static Layer FromExistingBuffer(BufferRgb565 pixelBuffer)
    {
        return new Layer(pixelBuffer, GameService.Instance.GameInstance.TextureManager);
    }

    /// <summary>
    /// Fills the whole pixel buffer with the specified color
    /// </summary>
    public void Clear()
    {
        _layerBuffer.Clear(BackgroundColor.Color16bppRgb565);
        _hasRenderChanges = true;
    }

    /// <summary>
    /// Draws the texture defined by the passed in frame
    /// </summary>
    /// <param name="frame">The texture frame to draw</param>
    /// <param name="topLeftOnLayer">
    /// The X and Y coordinates on the layer that we will start drawing onto.
    /// </param>
    public void DrawTexture(Frame frame, Point topLeftOnLayer)
    {
        var buffer = _textureManager.GetTexture(frame.TextureName);
        DrawTexture(buffer,
            new Point(frame.X, frame.Y),
            topLeftOnLayer,
            new Dimensions(frame.Width, frame.Height));
    }

    /// <summary>
    /// Draws the specified texture to the layer.
    /// </summary>
    /// <param name="texture">The full image texture we will draw from</param>
    /// <param name="topLeftOnTexture">
    /// The X and Y coordinates of the top left layer on the texture that we will read pixels from
    /// </param>
    /// <param name="topLeftOnLayer">
    /// The X and Y coordinates on the layer that we will start drawing onto.
    /// </param>
    /// <param name="drawSize">The height and width of the amount of pixel data to draw</param>
    /// <param name="ignoreTransparency">
    /// If true, all pixels from the buffer will be drawn, even if it matches the
    /// transparency color
    /// </param>
    /// <param name="resetRenderFlag">
    /// If false, drawing this texture will not consider this as modified. Should always
    /// be true except for the sprite layer.
    /// </param>
    public void DrawTexture(BufferRgb565 texture,
        Point topLeftOnTexture,
        Point topLeftOnLayer,
        Dimensions drawSize,
        bool ignoreTransparency = false,
        bool resetRenderFlag = true)
    {
        // The draw size and top left on layer needs to be adjusted to handle parts
        // of the texture that would end up off the layer.
        if (topLeftOnLayer.X < 0)
        {
            var moveCount = -topLeftOnLayer.X;
            drawSize = new Dimensions(drawSize.Width - moveCount, drawSize.Height);
            topLeftOnLayer = new Point(0, topLeftOnLayer.Y);
            topLeftOnTexture = new Point(topLeftOnTexture.X + moveCount, topLeftOnTexture.Y);
        }

        if (topLeftOnLayer.X + drawSize.Width >= _layerBuffer.Width)
        {
            var moveCount = topLeftOnLayer.X + drawSize.Width - _layerBuffer.Width;
            drawSize = new Dimensions(drawSize.Width - moveCount, drawSize.Height);
        }

        if (topLeftOnLayer.Y < 0)
        {
            var moveCount = -topLeftOnLayer.Y;
            drawSize = new Dimensions(drawSize.Width, drawSize.Height - moveCount);
            topLeftOnLayer = new Point(topLeftOnLayer.X);
            topLeftOnTexture = new Point(topLeftOnTexture.X, topLeftOnTexture.Y + moveCount);
        }

        if (topLeftOnLayer.Y + drawSize.Height >= _layerBuffer.Height)
        {
            var moveCount = topLeftOnLayer.Y + drawSize.Height - _layerBuffer.Height;
            drawSize = new Dimensions(drawSize.Width, drawSize.Height - moveCount);
        }

        if (drawSize.Width <= 0 || drawSize.Height <= 0)
        {
            // nothing to draw
            return;
        }
        
        var transparency = ignoreTransparency
            ? (Color?)null
            : TransparentColor;
        
        // Layer shifting means we can't just apply the texture as is, but we need to 
        // adjust it based on the internal offset. Split the drawable portions into 
        // quadrants and render each individually.
        var adjustedTopLeftOnLayer = topLeftOnLayer + new Point((int)_internalOrigin.X, (int)_internalOrigin.Y);
        
        // How far off the layer does this texture end up starting. E.g. if the internal origin is
        // in the center of the layer, and the texture is being drawn on the (unadjusted) right side
        // of the layer, then the adjusted position will cause the texture's starting points to be 
        // wrapped around and not painted on the actual right side of the layer. 
        //
        // If the adjusted start position does not go off the layer boundaries, then it has an
        // under-draw of 0.
        var horizontalUnderDraw = Math.Max(adjustedTopLeftOnLayer.X - _layerBuffer.Width, 0);
        var verticalUnderDraw = Math.Max(adjustedTopLeftOnLayer.Y - _layerBuffer.Height, 0);
        
        // How far would this texture be drawn past the bounds of the layer after being adjusted
        // by the internal origin.
        var horizontalOverdraw = Math.Max(adjustedTopLeftOnLayer.X + drawSize.Width - _layerBuffer.Width - horizontalUnderDraw, 0);
        var verticalOverdraw = Math.Max(adjustedTopLeftOnLayer.Y + drawSize.Height - +_layerBuffer.Height - verticalUnderDraw, 0);
        
        // Where on the texture we start pulling pixels from
        var horizontalOverdrawTextureX = topLeftOnTexture.X + drawSize.Width - horizontalOverdraw;
        var verticalOverdrawTextureY = topLeftOnTexture.Y + drawSize.Height - verticalOverdraw;

        // Console.WriteLine($"Internal origin: {_internalOrigin}");
        // Console.WriteLine($"Top left on layer: ({topLeftOnLayer}) ({adjustedTopLeftOnLayer})");
        // Console.WriteLine($"Draw size: {drawSize}");
        // Console.WriteLine($"Horizontal: {horizontalOverdraw} {horizontalUnderDraw} {horizontalOverdrawTextureX}");
        // Console.WriteLine($"Vertical: {verticalOverdraw} {verticalUnderDraw} {verticalOverdrawTextureY}");
     
        // Paint part of the texture to the bottom right of the internal origin
        if (adjustedTopLeftOnLayer.X < _layerBuffer.Width && adjustedTopLeftOnLayer.Y < _layerBuffer.Height)
        {
            var bottomRight = new DrawableRectangle(
                adjustedTopLeftOnLayer,
                new Dimensions(
                    drawSize.Width - horizontalOverdraw,
                    drawSize.Height - verticalOverdraw));
            
            // Console.WriteLine($"BR: {bottomRight}");
            Drawing.ExecuteOperation(new Drawing.Operation(
                texture,
                _layerBuffer,
                topLeftOnTexture,
                bottomRight.Start,
                bottomRight.Dimensions,
                transparency));
        }
       
        // Draw part of the texture to the bottom left of the internal origin
        if (horizontalOverdraw > 0 && adjustedTopLeftOnLayer.Y < _layerBuffer.Height)
        {
            var bottomLeft = new DrawableRectangle(
                adjustedTopLeftOnLayer with { X = horizontalUnderDraw },
                new Dimensions(
                    horizontalOverdraw,
                    drawSize.Height - verticalOverdraw));
            
            // Console.WriteLine($"BL: {bottomLeft}");
            Drawing.ExecuteOperation(new Drawing.Operation(
                texture,
                _layerBuffer,
                topLeftOnTexture with { X = horizontalOverdrawTextureX },
                bottomLeft.Start,
                bottomLeft.Dimensions,
                transparency
            ));
        }
        
        // Draw part of the texture to the top right of the internal origin
        if (verticalOverdraw > 0)
        {
            var topRight = new DrawableRectangle(
                adjustedTopLeftOnLayer with { Y = verticalUnderDraw },
                new Dimensions(
                    drawSize.Width - horizontalOverdraw,
                    verticalOverdraw));
            
            Drawing.ExecuteOperation(new Drawing.Operation(
                texture,
                _layerBuffer,
                topLeftOnTexture with { Y = verticalOverdrawTextureY },
                topRight.Start,
                topRight.Dimensions,
                transparency));
        }
        
        // Draw part of the texture to the top left of the internal origin
        if (horizontalOverdraw > 0 && verticalOverdraw > 0)
        {
            var topLeft = new DrawableRectangle(
                new Point(horizontalUnderDraw, verticalUnderDraw),
                new Dimensions(horizontalOverdraw, verticalOverdraw));
            
            Drawing.ExecuteOperation(new Drawing.Operation(
                texture,
                _layerBuffer,
                new Point(horizontalOverdrawTextureX, verticalOverdrawTextureY),
                topLeft.Start,
                topLeft.Dimensions,
                transparency));
        }

        if (resetRenderFlag)
        {
            _hasRenderChanges = true;
        }
    }

    /// <summary>
    /// Shifts the pixels in the layer by the specified amount. Pixels that shift off the layer
    /// will be wrapped to the other side.
    /// </summary>
    /// <param name="shiftAmount"></param>
    public void Shift(Vector2 shiftAmount)
    {
        // The naive approach to shifting the layer around would be to rotate the actual bytes
        // in the buffer around. This requires 3 array copy calls per row (1 for the pixels that''
        // will move off layer into a temporary buffer, one to move the remaining pixels over,
        // then a third to move the pixels from the temporary buffer to their wrapped around
        // area. This is particularly costly when most layers will only shift one pixel at a
        // time in any direction.
        // 
        // Instead we can fake it by just changing where the origin/top-left of the layer is
        // considered to be. This adds a tiny bit of extra draw calculations but should still
        // be cheaper than constant byte transferring.
        //
        // We apply the shift amount in the opposite direction as provided, as if the layer
        // is being shifted left then we want to execute that by shifting the origin right.
        _internalOrigin -= shiftAmount;
        
        // Normalize the origin so it's always somewhere within the bounds
        // of the layer.
        while (_internalOrigin.X < 0) _internalOrigin.X += _layerBuffer.Width;
        while (_internalOrigin.X >= _layerBuffer.Width) _internalOrigin.X -= _layerBuffer.Width;
        while (_internalOrigin.Y < 0) _internalOrigin.Y += _layerBuffer.Height;
        while (_internalOrigin.Y >= _layerBuffer.Height) _internalOrigin.Y -= _layerBuffer.Height;

        if (Math.Abs(shiftAmount.X) > float.Epsilon || Math.Abs(shiftAmount.Y) > float.Epsilon)
        {
            _hasRenderChanges = true;
        }
    }

    /// <summary>
    /// Renders the layer from its own buffer to the specified buffer.
    /// The passed in buffer is assumed to be the "camera" and thus the
    /// first byte of the target buffer is assumed to be the camera's
    /// 0,0/origin.
    /// </summary>
    /// <returns></returns>
    internal RenderRegion? RenderToBuffer(BufferRgb565 target)
    {
        // Don't render if our buffer is the same as the target. This is
        // essentially a "don't do anything with the sprite layer" 
        // condition.
        if (target == _layerBuffer)
        {
            return null;
        }
        
        // Figure out where the source buffer overlaps the camera. All this code
        // assumes the engine does not support zooming, thus 1 unit is 1 pixel. This 
        // works with game scaling because the target buffer should be the 
        // renderer's pixel buffer, *not* the display buffer in that case.
        if (_layerBuffer.Height + CameraOffset.Y < 0 || // Layer is fully above the camera
            _layerBuffer.Width + CameraOffset.X < 0 || // Layer is fully left of the camera
            CameraOffset.Y >= target.Height || // Layer is fully below the camera
            CameraOffset.X >= target.Width) // Layer is fully right of the camera
        {
            return null;
        }
        
        // We have four quadrants to draw from depending on how the internal origin
        // has shifted around. Calculate them out.
        var topLeft = new DrawableRectangle(
            new Point(0), 
            new Dimensions((int)_internalOrigin.X, (int)_internalOrigin.Y));

        var topRight = new DrawableRectangle(
            new Point((int)_internalOrigin.X),
            new Dimensions(_layerBuffer.Width - (int)_internalOrigin.X, (int)_internalOrigin.Y));

        var bottomLeft = new DrawableRectangle(
            new Point(0, (int)_internalOrigin.Y),
            new Dimensions((int)_internalOrigin.X, _layerBuffer.Height - (int)_internalOrigin.Y));

        var bottomRight = new DrawableRectangle(
            new Point((int)_internalOrigin.X, (int)_internalOrigin.Y),
            new Dimensions(
                _layerBuffer.Width - (int)_internalOrigin.X,
                _layerBuffer.Height - (int)_internalOrigin.Y));
        
        // Since we are drawing considering the internal origin as the top left/start
        // of the layer, we need to flip the quadrants. So instead of
        // top left -> top right -> bottom left -> bottom right we need to draw them
        // bottom right -> bottom left -> top right -> top left. This allows us to
        // pretend like we are wrapping the layer around.
        void PerformDraw(DrawableRectangle quadrant, BufferRgb565 innerTarget, Point targetOrigin)
        {
            var operation = CalculateDrawOperation(quadrant, innerTarget, targetOrigin.X, targetOrigin.Y);
            if (operation != null)
            {
                Drawing.ExecuteOperation(operation.Value);
            }
        }

        PerformDraw(bottomRight, target, new Point(0));
        PerformDraw(bottomLeft, target, new Point(bottomRight.Dimensions.Width));
        PerformDraw(topRight, target, new Point(0, bottomRight.Dimensions.Height));
        PerformDraw(topLeft, target, new Point(bottomRight.Dimensions.Width, bottomRight.Dimensions.Height));

        if (!_hasRenderChanges)
        {
            // Since we haven't changed since last frame, no need to provide a region
            // that needs to be re-rendered. We still needed to draw to the target
            // buffer though in case a sprite or something else with transparency
            // does need to be redrawn on top of this area.
            return null;
        }

        var startX = Math.Max(CameraOffset.X, 0);
        var startY = Math.Max(CameraOffset.Y, 0);
        var endX = Math.Min(CameraOffset.X + Width, target.Width);
        var endY = Math.Min(CameraOffset.Y + Height, target.Height);
        var width = endX - startX;
        var height = endY - startY;

        _hasRenderChanges = false; 
        return new RenderRegion(startX, startY, width, height);
    }

    /// <summary>
    /// Calculate how to draw the drawableRectangle onto its correct area on the target
    /// </summary>
    private Drawing.Operation? CalculateDrawOperation(DrawableRectangle drawableRectangle, 
        BufferRgb565 target,
        int targetOriginX,
        int targetOriginY)
    {
        if (drawableRectangle.Dimensions.Width <= 0 || drawableRectangle.Dimensions.Height <= 0)
        {
            return null;
        }
        
        // Function to consolidate the code for both vertical and horizontal axis values.
        // This prevents us from copy/pasting code just to swap X and Y variable names
        (int layerStart, int targetStart, int dimension)? CalculateAxis(int layerStart, 
            int offset, 
            int quadrantDimension,
            int targetDimension,
            int targetOrigin)
        {
            var targetStart = targetOrigin + offset;
            var targetEnd = targetStart + quadrantDimension;
            if (targetStart >= targetDimension || targetEnd <= 0)
            {
                // It starts past the target or ends before the target
                return null;
            }

            if (targetStart < 0)
            {
                // Adjust the target layerStart back to 0 (to be within bounds) and adjust
                // the layer's layerStart position by the same amount
                layerStart += -targetStart;
                targetStart = 0;
            }
            
            var dimension = targetEnd - targetStart;
            
            // Make sure this doesn't go past the target's dimensions. Horizontally,
            // that will cause overdraw to the next row, while vertically that will
            // cause crashes.
            var overdraw = (targetStart + dimension) - targetDimension;
            if (overdraw > 0)
            {
                dimension -= overdraw;
            }

            return (layerStart, targetStart, dimension);
        }

        var horizontal = CalculateAxis(
            drawableRectangle.Start.X, 
            CameraOffset.X, 
            drawableRectangle.Dimensions.Width, 
            target.Width, 
            targetOriginX);
        
        var vertical = CalculateAxis(
            drawableRectangle.Start.Y, 
            CameraOffset.Y, 
            drawableRectangle.Dimensions.Height, 
            target.Height, 
            targetOriginY);
        
        if (horizontal == null || vertical == null)
        {
            // One of the axis was not on the target, so nothing to draw
            return null;
        }

        var transparency = DrawLayerWithTransparency
            ? TransparentColor
            : (Color?)null;

        return new Drawing.Operation(
            _layerBuffer,
            target,
            new Point(horizontal.Value.layerStart, vertical.Value.layerStart),
            new Point(horizontal.Value.targetStart, vertical.Value.targetStart),
            new Dimensions(horizontal.Value.dimension, vertical.Value.dimension),
            transparency
        );
    }
}