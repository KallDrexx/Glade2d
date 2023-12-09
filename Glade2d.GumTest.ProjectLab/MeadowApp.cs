using Glade2d.Graphics;
using Glade2d.Graphics.SelfRenderer;
using Glade2d.Profiling;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Graphics;

namespace Glade2d.GumTest.ProjectLab;

public class MeadowApp : App<F7CoreComputeV2>
{
    private IGraphicsDisplay _display = default!;
    private IProjectLabHardware _projectLab = default!;
    
    public override Task Initialize()
    {
        _projectLab = Meadow.Devices.ProjectLab.Create();
        _display = _projectLab.Display!;
        
        return base.Initialize();
    }

    public override Task Run()
    {
        var textureManager = new TextureManager(MeadowOS.FileSystem.UserFileSystemRoot);
        var layerManager = new LayerManager();
        var profiler = new Profiler { IsActive = true};
        var renderer = new GladeSelfRenderer(_display, textureManager, layerManager, profiler, 2);
        
        var glade = new Game();
        glade.Initialize(renderer, textureManager, layerManager, profiler);
        glade.InputManager.RegisterPushButton(_projectLab.LeftButton, nameof(GumTestScreen.GameInputs.LeftButton));
        glade.InputManager.RegisterPushButton(_projectLab.RightButton, nameof(GumTestScreen.GameInputs.RightButton));
        glade.InputManager.RegisterPushButton(_projectLab.UpButton, nameof(GumTestScreen.GameInputs.UpButton));
        glade.InputManager.RegisterPushButton(_projectLab.DownButton, nameof(GumTestScreen.GameInputs.DownButton));
        
        glade.Start(() => new GumTestScreen());
        
        return base.Run();
    }
}