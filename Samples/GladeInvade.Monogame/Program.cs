﻿using Glade2d;
using Glade2d.Monogame;
using GladeInvade.Shared;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 240);
var engine = new Game();

var inputs = new GameInputs
{
    Left = environment.CreatePortForKey(Keys.Left),
    Right = environment.CreatePortForKey(Keys.Right),
    Action = environment.CreatePortForKey(Keys.Up),
};

var display = new MonogameGladeDisplay(environment.Display);

engine.Initialize(display, inputs, 2, contentRoot: Environment.CurrentDirectory);
GladeInvadeGame.Run(engine);
