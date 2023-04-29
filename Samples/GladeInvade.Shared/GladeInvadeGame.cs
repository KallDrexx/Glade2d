﻿using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared.Screens;
using Meadow.Foundation;

namespace GladeInvade.Shared;

public static class GladeInvadeGame
{
    public static void Run(Game engine)
    {
        engine.Renderer.BackgroundColor = new Color(0, 0, 0);
        engine.Renderer.ShowPerf = false;

        LogService.Log.Trace("Running game...");
        engine.Profiler.IsActive = true;
        engine.Start(new TitleScreen());
    }
}