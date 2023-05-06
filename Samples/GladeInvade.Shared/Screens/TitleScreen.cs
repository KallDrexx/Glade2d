using Glade2d;
using Glade2d.Graphics.Layers;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using GladeInvade.Shared.Services;
using GladeInvade.Shared.Sprites;
using Meadow.Foundation.Graphics;

namespace GladeInvade.Shared.Screens;

public class TitleScreen : Screen
{
    const int ScreenEdgePadding = 4;

    private readonly int _screenHeight, _screenWidth;
    private readonly Game _engine = GameService.Instance.GameInstance;
    private readonly GameTitleDisplay _gameTitle;
    private readonly IScoreBoard _scoreBoard;
    private Layer _inputPromptLayer;
    
    public TitleScreen(IScoreBoard scoreBoard)
    {
        _screenHeight = GameService.Instance.GameInstance.Renderer.Height;
        _screenWidth = GameService.Instance.GameInstance.Renderer.Width;
        _scoreBoard = scoreBoard;

        _gameTitle = new GameTitleDisplay
        {
            X = 5,
            Y = 5,
            VelocityX = 12,
            VelocityY = 10,
        };
            
        AddSprite(_gameTitle);

        CreateTextLayers();
        _scoreBoard.SetDisplay("----");

        LogService.Log.Info("Started title screen.");
    }

    public override void Activity()
    {
        if (_engine.InputManager.GetButtonState(nameof(GameInputs.ActionButton)) == ButtonState.Pressed)
        {
            // restart progression at level 1
            ProgressionService.Instance.Restart();

            // launch the game screen
            GameService.Instance.GameInstance.TransitionToScreen(() => new GameScreen(_scoreBoard));
        }

        if (_gameTitle.X < 5 || _gameTitle.X + _gameTitle.CurrentFrame.Width > _screenWidth - 5)
        {
            _gameTitle.VelocityX *= -1;
        }

        if (_gameTitle.Y < 5 || _gameTitle.Y + _gameTitle.CurrentFrame.Height > _screenHeight - 5)
        {
            _gameTitle.VelocityY *= -1;
        }
        
        base.Activity();
    }


    void CreateTextLayers()
    {
        _inputPromptLayer = Layer.Create(new Dimensions(_screenWidth, _screenHeight));
        _inputPromptLayer.BackgroundColor = GameConstants.BackgroundColor;
        _inputPromptLayer.DrawLayerWithTransparency = false;
        _inputPromptLayer.Clear();

        var font = _inputPromptLayer.DefaultFont;
        var prompt = "Press [Action] to Play!";
        var position = new Point(
            (_screenWidth / 2) - (prompt.Length * font.Width / 2),
            _screenHeight - font.Height - ScreenEdgePadding);
        _inputPromptLayer.DrawText(
            position: position,
            text: prompt,
            color: GameConstants.RedTextColor);

        _engine.LayerManager.AddLayer(_inputPromptLayer, -1);
    }
}