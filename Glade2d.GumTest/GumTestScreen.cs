using Glade2d.Graphics;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeSampleShared.Entities;
using Gum.Converters;
using Gum.DataTypes;
using Gum.Managers;
using Gum.Wireframe;
using Meadow.Hardware;
using Microsoft.Xna.Framework.Input;
using RenderingLibrary;
using RenderingLibrary.Graphics;
using ButtonState = Glade2d.Input.ButtonState;

namespace Glade2d.GumTest;

public class GumTestScreen : Screen
{
    private readonly GraphicalUiElement _sunsContainer = new();
    private readonly GraphicalUiElement _cloudsContainer = new();

    public GumTestScreen()
    {
        GraphicalUiElement.CanvasHeight = GameService.Instance.GameInstance.Renderer.Height;
        GraphicalUiElement.CanvasWidth = GameService.Instance.GameInstance.Renderer.Width;

        GraphicalUiElement.IsAllLayoutSuspended = true;
        CreateSuns();
        CreateClouds();
        GraphicalUiElement.IsAllLayoutSuspended = false;
        
        _sunsContainer.UpdateLayout();
        _cloudsContainer.UpdateLayout();
    }

    public override void Activity()
    {
        
        var inputs = GameService.Instance.GameInstance.InputManager;
        var leftButtonState = inputs.GetButtonState(nameof(GameInputs.LeftButton));
        var rightButtonState = inputs.GetButtonState(nameof(GameInputs.RightButton));
        
        if (leftButtonState == ButtonState.Pressed && _cloudsContainer.Children.Any())
        {
            var lastChild = (CustomSpriteRenderable)_cloudsContainer.Children.Last();
            _cloudsContainer.Children.Remove(lastChild);
            
            RemoveSprite(lastChild.Sprite);
        }
        else if (rightButtonState == ButtonState.Pressed)
        {
            var cloud = new CustomSpriteRenderable(new Cloud());
            var element = new GraphicalUiElement();
            element.SetContainedObject(cloud);
            _cloudsContainer.Children.Add(element);
            
            AddSprite(cloud.Sprite);
        }
    }

    private void CreateSuns()
    {
        _sunsContainer.SetContainedObject(new InvisibleRenderable { Visible = true });
        _sunsContainer.HeightUnits = DimensionUnitType.RelativeToContainer;
        _sunsContainer.WidthUnits = DimensionUnitType.RelativeToContainer;
        _sunsContainer.Width = 0;
        _sunsContainer.Height = 0;

        var suns =
            new (Sun sprite, HorizontalAlignment xOrigin, GeneralUnitType xUnits, VerticalAlignment yOrigin,
                GeneralUnitType yUnits)[]
                {
                    (new Sun(), xOrigin: HorizontalAlignment.Left, xUnits: GeneralUnitType.PixelsFromSmall,
                        yOrigin: VerticalAlignment.Top, yUnits: GeneralUnitType.PixelsFromSmall),
                    (new Sun(), xOrigin: HorizontalAlignment.Right, xUnits: GeneralUnitType.PixelsFromLarge,
                        yOrigin: VerticalAlignment.Top, yUnits: GeneralUnitType.PixelsFromSmall),
                    (new Sun(), xOrigin: HorizontalAlignment.Left, xUnits: GeneralUnitType.PixelsFromSmall,
                        yOrigin: VerticalAlignment.Bottom, yUnits: GeneralUnitType.PixelsFromLarge),
                    (new Sun(), xOrigin: HorizontalAlignment.Right, xUnits: GeneralUnitType.PixelsFromLarge,
                        yOrigin: VerticalAlignment.Bottom, yUnits: GeneralUnitType.PixelsFromLarge),
                };

        foreach (var sun in suns)
        {
            var element = new GraphicalUiElement();
            element.SetContainedObject(new CustomSpriteRenderable(sun.sprite) { Visible = true });
            element.XOrigin = sun.xOrigin;
            element.XUnits = sun.xUnits;
            element.YOrigin = sun.yOrigin;
            element.YUnits = sun.yUnits;
            element.Width = sun.sprite.Width;
            element.Height = sun.sprite.Height;
            _sunsContainer.Children.Add(element);

            AddSprite(sun.sprite);
        }
    }

    private void CreateClouds()
    {
        var clouds = new[]
        {
            new CustomSpriteRenderable(new Cloud()),
            new CustomSpriteRenderable(new Cloud()),
            new CustomSpriteRenderable(new Cloud()),
            new CustomSpriteRenderable(new Cloud()),
            new CustomSpriteRenderable(new Cloud()),
        };

        _cloudsContainer.SetContainedObject(new InvisibleRenderable { Visible = true });
        _cloudsContainer.HeightUnits = DimensionUnitType.RelativeToContainer;
        _cloudsContainer.WidthUnits = DimensionUnitType.RelativeToContainer;
        _cloudsContainer.XOrigin = HorizontalAlignment.Center;
        _cloudsContainer.YOrigin = VerticalAlignment.Center;
        _cloudsContainer.WrapsChildren = true;
        _cloudsContainer.Height = clouds[0].Sprite.Height * -1.5f;
        _cloudsContainer.Width = clouds[0].Sprite.Width * -1.5f;
        _cloudsContainer.XUnits = GeneralUnitType.PixelsFromMiddle;
        _cloudsContainer.YUnits = GeneralUnitType.PixelsFromMiddle;
        _cloudsContainer.StackSpacing = 8f;
        _cloudsContainer.ChildrenLayout = ChildrenLayout.LeftToRightStack;

        foreach (var cloud in clouds)
        {
            var element = new GraphicalUiElement();
            element.SetContainedObject(cloud);

            _cloudsContainer.Children.Add(element);

            AddSprite(cloud.Sprite);
        }
    }

    public class GameInputs : GameInputSetBase
    {
        public IDigitalInputPort LeftButton { get; set; } = null!;
        public IDigitalInputPort RightButton { get; set; } = null!;
    }
}