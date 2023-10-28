using Glade2d;
using Glade2d.Graphics;
using Glade2d.Graphics.SelfRenderer;
using Glade2d.GumTest;
using Glade2d.Profiling;
using GladeSampleShared.Screens;
using MeadowMgTestEnvironment;

var environment = new TestEnvironment(240, 240);
var engine = new Game();

var textureManager = new TextureManager(Environment.CurrentDirectory);
var layerManager = new LayerManager();
var profiler = new Profiler();
var renderer = new GladeSelfRenderer(environment.Display, textureManager, layerManager, profiler, 2);

engine.Initialize(renderer, textureManager, layerManager, profiler); 
engine.Start(() => new GumTestScreen());