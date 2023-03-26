using System;
using System.Threading.Tasks;
using Glade2d.Input;
using Glade2d.Screens;
using Glade2d.Services;
using Meadow;
using Meadow.Foundation.Graphics;

namespace Glade2d;

public abstract class GladeMeadowApp<T> : App<T> where T : class, IMeadowDevice
{
    protected IGraphicsDisplay Display;

    public override Task Initialize()
    {
        Console.WriteLine("Test");
        LogService.Log.Trace("Initializing board");
        InitializeBoard();
        LogService.Log.Trace("Board initialized");

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        if (Display == null)
        {
            var message = "No display created as part of initialization. A display is required";
            throw new InvalidOperationException(message);
        }
        
        LogService.Log.Trace("Starting Glade engine");
        var glade = new Game();
        var startingScreen = ConfigureGladeAndGetStartScreen(glade);
        
        glade.Start(startingScreen);

        return Task.CompletedTask;
    }

    protected abstract void InitializeBoard();

    protected abstract void HookUpInput(InputManager inputManager);
    
    protected abstract Screen ConfigureGladeAndGetStartScreen(Game engine);
}