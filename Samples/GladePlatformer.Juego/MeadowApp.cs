using Glade2d;
using Glade2d.Boards.Juego;
using Glade2d.Input;
using Glade2d.Screens;
using GladePlatformer.Shared;
using GladePlatformer.Shared.Screens;

namespace GladePlatformer.Juego;

public class MeadowApp : GladeJuegoApp
{
    protected override void HookUpInput(InputManager inputManager)
    {
        inputManager.RegisterPushButton(LeftSideLeftButton, GameConstants.InputNames.Left);
        inputManager.RegisterPushButton(LeftSideRightButton, GameConstants.InputNames.Right);
        inputManager.RegisterPushButton(RightSideDownButton, GameConstants.InputNames.Jump);
    }

    protected override Screen ConfigureGladeAndGetStartScreen(Game engine)
    {
        engine.Profiler.IsActive = true;
        engine.Renderer.ShowPerf = true;

        return new LevelScreen();
    }
}