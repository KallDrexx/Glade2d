﻿using Glade2d;
using GladePlatformer.Shared;
using MeadowMgTestEnvironment;
using Microsoft.Xna.Framework.Input;

var environment = new TestEnvironment(240, 240);
var engine = new Game();
engine.Initialize(environment.Display, 2, contentRoot: Environment.CurrentDirectory);

var inputManager = engine.InputManager;
environment.BindKey(Keys.Right, 
    () => inputManager.ButtonPushed(GameConstants.InputNames.Right),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Right));

environment.BindKey(Keys.Left,
    () => inputManager.ButtonPushed(GameConstants.InputNames.Left),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Left));

environment.BindKey(Keys.Up,
    () => inputManager.ButtonPushed(GameConstants.InputNames.Jump),
    () => inputManager.ButtonReleased(GameConstants.InputNames.Jump));

GladePlatformerGame.Run(engine);
