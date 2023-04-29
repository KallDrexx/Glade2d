using Glade2d.Screens;
using Glade2d.Utility;
using System;

namespace Glade2d.Services
{
    public class GameService
    {
        private Screen _currentScreen;
        
        static GameService instance;

        public static GameService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameService();
                }
                return instance;
            }
        }

        public GameTime Time { get; private set; }

        public Screen CurrentScreen
        {
            get => _currentScreen;
            set
            {
                _currentScreen = value;
                ScreenChanged = true;
            }
        }

        public Random Random { get; set; } = new Random();

        public Game GameInstance { get; set; }

        internal bool ScreenChanged { get; set; } = true;

        private GameService() { }

        public void Initialize()
        {
            LogService.Log.Trace("Initializing GameService");
            Time = new GameTime();
        }
    }
}
