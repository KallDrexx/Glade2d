using Glade2d.Services;

namespace Glade2d.Graphics
{
    public class Sprite
    {
        internal readonly record struct RegionChanges(RenderRegion? Old, RenderRegion? New);

        private Frame _lastFrame;
        private RenderRegion? _lastRenderRegion;

        /// <summary>
        /// The X position of this object
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// The Y position of this object
        /// </summary>
        public float Y { get; set; }

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
        public Frame CurrentFrame { get; set; }

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
            RenderRegion? newRegion = new RenderRegion(
                (int)X, 
                (int)Y, 
                CurrentFrame.Width,
                CurrentFrame.Height);
            
            // If the frame hasn't changed, and the new position is the same as
            // the old, then we can consider this unchanged. This can occur
            // if the sprite moves under a pixel.
            if (_lastFrame == CurrentFrame && newRegion.Value == _lastRenderRegion)
            {
                newRegion = null;
            }
            else
            {
                _lastRenderRegion = newRegion;
            }

            _lastFrame = CurrentFrame;
            return new RegionChanges(_lastRenderRegion, newRegion);
        }
    }
}
