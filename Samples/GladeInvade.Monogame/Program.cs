﻿using Glade2d;
using GladeInvade.Shared;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 240);
var engine = new Game();

var inputs = new GameInputs
{
    LeftButton = environment.CreatePortForKey(Keys.Left),
    RightButton = environment.CreatePortForKey(Keys.Right),
    ActionButton = environment.CreatePortForKey(Keys.Up),
};

engine.Initialize(environment.Display, inputs, 2, contentRoot: Environment.CurrentDirectory);
GladeInvadeGame.Run(engine, new NullScoreBoard());
