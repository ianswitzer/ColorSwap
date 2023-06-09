using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;

namespace ColorSwap

{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        public Point GameBounds = new Point(1200, 720);
        public SpriteBatch _spriteBatch;
        private readonly ScreenManager _screenManager;
        private int CurrentScreen = 0;
        // 0 - TitleScreen
        // 1 - LevelSelect
        // 2 - In-game
        public int CompletedLevel = 0;
        public int LevelSelectRows = 2;
        public int LevelSelectColumns = 5;
        public int TotalLevels;

        public Game1()
        {
            TotalLevels = LevelSelectRows * LevelSelectColumns;
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = GameBounds.X;
            _graphics.PreferredBackBufferHeight = GameBounds.Y;
            _screenManager = new ScreenManager();
            Components.Add(_screenManager);
        }

        protected override void Initialize()
        {
            base.Initialize();

            LoadTitleScreen();
        }

        protected override void LoadContent()
        {   
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        public void UpdateGame(GameTime gameTime)
        {
            Update(gameTime);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                if (CurrentScreen == 2)
                {
                    LoadLevelSelect();
                } 
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Space) && CurrentScreen == 0)
            {
                LoadLevelSelect();
            }

            base.Update(gameTime);
        }

        private void LoadTitleScreen()
        {
            CurrentScreen = 0;
            _screenManager.LoadScreen(new TitleScreen(this), new FadeTransition(GraphicsDevice, Color.Black));
        }

        private void LoadLevelSelect()
        {
            CurrentScreen = 1;
            _screenManager.LoadScreen(new LevelSelect(this), new FadeTransition(GraphicsDevice, Color.Black));
        }

        public void LoadGameplayScreen(int Level)
        {
            CurrentScreen = 2;
            _screenManager.LoadScreen(new GameplayScreen(this, Level), new FadeTransition(GraphicsDevice, Color.Black));
        }

        public void CompleteLevel(int Level)
        {
            CompletedLevel = Math.Max(Level, CompletedLevel);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}