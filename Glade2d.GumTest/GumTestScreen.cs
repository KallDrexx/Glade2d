using Glade2d.Screens;
using Glade2d.Services;
using GladeSampleShared.Entities;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using RenderingLibrary;
using RenderingLibrary.Graphics;

namespace Glade2d.GumTest;

public class GumTestScreen : Screen
{
    public GumTestScreen()
    {
        GraphicalUiElement.CanvasHeight = GameService.Instance.GameInstance.Renderer.Height;
        GraphicalUiElement.CanvasWidth = GameService.Instance.GameInstance.Renderer.Width;

        var container = new GraphicalUiElement();
        container.SetContainedObject(new InvisibleRenderable {Visible = true});
        container.HeightUnits = DimensionUnitType.RelativeToContainer;
        container.ChildrenLayout = ChildrenLayout.TopToBottomStack;
        container.X = 50;

        for (var x = 0; x < 5; x++)
        {
            var cloud = new Cloud();
            var ui = new GraphicalUiElement();
            var invisibleRenderable = new InvisibleRenderable
            {
                Visible = true,
            };
            
            ui.SetContainedObject(invisibleRenderable);
            ui.Parent = container;
            ui.Width = cloud.Width;
            ui.Height = cloud.Height;
            ui.UpdateLayout();
            
            cloud.X = invisibleRenderable.GetAbsoluteX();
            cloud.Y = invisibleRenderable.GetAbsoluteY();
            AddSprite(cloud);
        }
    }
}