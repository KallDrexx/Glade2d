﻿using Glade2d;
using Glade2d.Input;
using Glade2d.Services;
using GladeInvade.Shared;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;

namespace GladeInvade.ProjectLab;

public class MeadowApp : App<F7FeatherV2>
{
    private IGraphicsDisplay _display = default!;
    private IProjectLabHardware _projectLab = default!;
    
    public override Task Initialize()
    {
        _projectLab = Meadow.Devices.ProjectLab.Create();
        _display = _projectLab.Display!;

        return Task.CompletedTask;
    }
    
    public override Task Run()
    {
        Console.WriteLine("Testing output");
        var ht16k33 = new Ht16k33(_projectLab.I2cBus, 0x70);
        var scoreboard = new Ht16k33LedScoreboard(ht16k33);
        scoreboard.SetDisplay("1234");

        LogService.Log.Trace("Initializing Glade game engine...");
        var glade = new Game();
        glade.Initialize(_display, 2, EngineMode.GameLoop);
        InitializeInput(glade.InputManager);
        
        GladeInvadeGame.Run(glade);
        
        return base.Run();
    }

    private void InitializeInput(InputManager inputManager)
    {
        // can't use input ports directly with the PL abstraction
        inputManager.RegisterPushButton(_projectLab.UpButton!, nameof(GameInputs.ActionButton));
        inputManager.RegisterPushButton(_projectLab.LeftButton!, nameof(GameInputs.LeftButton));
        inputManager.RegisterPushButton(_projectLab.RightButton!, nameof(GameInputs.RightButton));
    }
}