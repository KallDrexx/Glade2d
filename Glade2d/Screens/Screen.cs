using Glade2d.Graphics;
using Glade2d.Services;
using System.Collections.Generic;
using System.Linq;

namespace Glade2d.Screens
{
    public class Screen
    {
        List<Sprite> sprites = new List<Sprite>();
        bool listSortNeeded = false;

        internal SortedSet<RenderRegion> ModifiedRegions { get; } = new(new RenderRegionComparer());

        public Screen() { }

        /// <summary>
        /// Update method called by the engine, keeps children
        /// updated
        /// </summary>
        public void Update()
        {
            ModifiedRegions.Clear();
            if(listSortNeeded)
            {
                LogService.Log.Trace("Resorting sprite list.");
                sprites = sprites.OrderBy(s => s.Layer).ToList();
                listSortNeeded = false;
            }

            for(var i = sprites.Count - 1; i > -1; i--)
            {
                var currentSprite = sprites[i];
                if (currentSprite.Destroyed)
                {
                    RemoveSprite(i);
                    var region = currentSprite.GetRenderRegionIfChanged();
                    if (region.Old != null)
                    {
                        ModifiedRegions.Add(region.Old.Value);
                    }
                }
                else
                {
                    sprites[i].Update();
                }
            }

            Activity();
            
            // Now that all sprites have been updated, if any of them have 
            // changed we need to consider their old and new regions
            // as modified
            for (var x = 0; x < sprites.Count; x++)
            {
                var regions = sprites[x].GetRenderRegionIfChanged();
                if (regions.New == null)
                {
                    // If there is no new region, then the sprite requires
                    // no new rendering
                    continue;
                }

                if (regions.Old != null)
                {
                    ModifiedRegions.Add(regions.Old.Value);
                }
                
                ModifiedRegions.Add(regions.New.Value);
            }
        }

        /// <summary>
        /// Frame-time method intended to be overridden by deriving
        /// screens
        /// </summary>
        public virtual void Activity()
        {

        }

        /// <summary>
        /// Adds a sprite to the list and marks the list as
        /// needing sorted so layer order is correct.
        /// 
        /// If the sprite is already in the list, this call is
        /// ignored.
        /// </summary>
        /// <param name="sprite">The sprite to add</param>
        public Sprite AddSprite(Sprite sprite)
        {
            LogService.Log.Trace("Adding sprite to scene graph");
            if (!sprites.Contains(sprite))
            {
                sprites.Add(sprite);

                // Can't reorder the list right now because we could
                // be iterating over it. Flag that the list needs sorting
                listSortNeeded = true;
            }
            return sprite;
        }

        /// <summary>
        /// Removes a sprite from the list if it exists in the list
        /// </summary>
        /// <param name="sprite"></param>
        public void RemoveSprite(Sprite sprite)
        {
            LogService.Log.Trace("Removing sprite to scene graph");
            if (sprites.Contains(sprite))
            {
                sprites.Remove(sprite);
            }
        }
        protected void RemoveSprite(int index)
        {
            LogService.Log.Trace("Removing sprite from scene graph");
            sprites.RemoveAt(index);
        }

        /// <summary>
        /// Get the sprite list. This should only be used for rendering
        /// 
        /// TODO: find a better way to protect the list so it is always sorted
        /// while still making it available to the renderer and not duplicating
        /// the collection every frame
        /// </summary>
        /// <returns></returns>
        internal List<Sprite> AccessSpritesForRenderingOnly()
        {
            return sprites;
        }
        
    }
}