﻿using Glade2d;
using Glade2d.Services;
using GladeInvade.Shared.Screens;
using Meadow.Foundation;

namespace GladeInvade.Shared;

public static class GladeInvadeGame
{
    public static void Run(Game engine, IScoreBoard scoreBoard)
    {
        engine.Renderer.BackgroundColor = GameConstants.BackgroundColor;

        LogService.Log.Trace("Running game...");
		engine.Start(new TitleScreen(scoreBoard));
    }
}