using Glade2d.Services;

namespace Glade2d.Graphics
{
    public class Sprite
    {
        internal readonly record struct RegionChanges(SpriteRenderRegion? Old, SpriteRenderRegion? New);
        
        // True if the sprite had any changes which require a re-rendering
        private bool _changeInRendering = true;
        private bool _frameChanged = false;
        private float _xPos, _yPos;
        private Frame _currentFrame;
        private SpriteRenderRegion? _lastRenderRegion;

        /// <summary>
        /// The X position of this object
        /// </summary>
        public float X
        {
            get => _xPos;
            set
            {
                _xPos = value;
                _changeInRendering = true;
            }
        }

        /// <summary>
        /// The Y position of this object
        /// </summary>
        public float Y
        {
            get => _yPos;
            set
            {
                _yPos = value;
                _changeInRendering = true;
            }
        }

        /// <summary>
        /// The X velocity of this object in pixels per second
        /// </summary>
        public float VelocityX { get; set; }

        /// <summary>
        /// The Y velocity of this object in pixels per second
        /// </summary>
        public float VelocityY { get; set; }

        /// <summary>
        /// The layer (rendering order) of this object...
        /// Larger numbers render on top
        /// </summary>
        public float Layer { get; set; }

        /// <summary>
        /// Tracks whether this object has been destroyed and should
        /// be removed from the scene graph
        /// </summary>
        public bool Destroyed { get; set; }

        /// <summary>
        /// The texture portion to use for this sprite
        /// </summary>
        public Frame CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = value;
                _changeInRendering = true;
                _frameChanged = true;
            }
        }

        public Sprite() { }
        public Sprite(Frame frame)
        {
            this.CurrentFrame = frame;
        }

        /// <summary>
        /// Called by the engine to update this object
        /// </summary>
        public void Update()
        {
            var delta = GameService.Instance.Time.FrameDelta;
            this.X += (float)(VelocityX * delta);
            this.Y += (float)(VelocityY * delta);
            Activity();
        }

        /// <summary>
        /// Frame time method - intended to be overridden
        /// by child classes
        /// </summary>
        public virtual void Activity() { }

        /// <summary>
        /// Intended to be overridden by child classes to perform
        /// any destroy effects before this object is marked as
        /// destroyed.
        /// </summary>
        public virtual void Die()
        {
            this.Destroy();
        }

        /// <summary>
        /// Marks this object as destroyed
        /// </summary>
        public void Destroy()
        {
            Destroyed = true;
        }

        /// <summary>
        /// Returns the sprite render regions where this sprite existed last
        /// frame and this frame. If the sprite requires no render changes
        /// then no new region is returned.
        /// </summary>
        internal RegionChanges GetRenderRegionIfChanged()
        {
            if (!_changeInRendering)
            {
                // No change
                return new RegionChanges(_lastRenderRegion, null);
            }

            SpriteRenderRegion? newRegion = new SpriteRenderRegion(
                (int)_xPos, 
                (int)_yPos, 
                _currentFrame.Width, 
                _currentFrame.Width);
            
            // If the frame hasn't changed, and the new position is the same as
            // the old, then we can consider this unchanged. This can occur
            // if the sprite moves under a pixel.
            if (!_frameChanged && newRegion.Value == _lastRenderRegion)
            {
                newRegion = null;
            }

            var result = new RegionChanges(_lastRenderRegion, newRegion);
            _changeInRendering = false;
            _frameChanged = false;
            _lastRenderRegion = newRegion;

            return result;
        }
    }
}
