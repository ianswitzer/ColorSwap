using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace ColorSwap
{
    public class LevelSelect : GameScreen
    {
        private new Game1 Game => (Game1)base.Game;
        private SpriteFont _font;
        private int IconWidth = 80;
        private int IconHeight = 120;
        private int IconSpacing = 40;
        private int IconMargin = 10;
        private int SelectedIcon = 1;

        private bool LeftKey = false;
        private bool RightKey = false;
        private bool UpKey = false;
        private bool DownKey = false;

        private Texture2D BlankTexture;

        private string Instructions = "(Press Enter)";

        public LevelSelect(Game1 game) : base(game) { }

        public override void LoadContent()
        {
            base.LoadContent();
            _font = Game.Content.Load<SpriteFont>("LevelSelect");

            if (BlankTexture == null)
            {
                BlankTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
                BlankTexture.SetData<Color>(new Color[] { Color.White });
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Enter))
            {
                Game.LoadGameplayScreen(SelectedIcon);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                if (!LeftKey)
                {
                    LeftKey = true;
                    if (SelectedIcon > 1)
                    {
                        SelectedIcon -= 1;
                    }
                }
            }
            else LeftKey = false;

            if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                if (!RightKey)
                {
                    RightKey = true;
                    if (SelectedIcon < Game.TotalLevels && SelectedIcon < Game.CompletedLevel + 1)
                    {
                        SelectedIcon += 1;
                    }
                }
            }
            else RightKey = false;

            if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                if (!UpKey)
                {
                    UpKey = true;
                    if (SelectedIcon > Game.LevelSelectColumns)
                    {
                        SelectedIcon -= Game.LevelSelectColumns;
                    }
                }
            }
            else UpKey = false;

            if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                if (!DownKey)
                {
                    DownKey = true;
                    if (SelectedIcon <= Game.TotalLevels - Game.LevelSelectColumns && SelectedIcon + Game.LevelSelectColumns <= Game.CompletedLevel + 1)
                    {
                        SelectedIcon += Game.LevelSelectColumns;
                    }
                }
            }
            else DownKey = false;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(Color.White);
            Game._spriteBatch.Begin();

            Vector2 InstructionSize = _font.MeasureString(Instructions);
            Vector2 InstructionPosition = new Vector2(Game.GameBounds.X / 2 - InstructionSize.X / 2, Game.GameBounds.Y - 100 - InstructionSize.Y / 2);
            Game._spriteBatch.DrawString(_font, Instructions, InstructionPosition, Color.MonoGameOrange);

            int Left = Game.GameBounds.X / 2 - (IconSpacing * (Game.LevelSelectColumns - 1) / 2) - (IconWidth * Game.LevelSelectColumns / 2);
            int Top = Game.GameBounds.Y / 2 - (IconSpacing * (Game.LevelSelectRows - 1) / 2) - (IconHeight * Game.LevelSelectRows / 2);
            for (int r = 0; r < Game.LevelSelectRows; r++)
            {
                for (int c = 0; c < Game.LevelSelectColumns; c++)
                {
                    if (c + 1 + Game.LevelSelectColumns * r == SelectedIcon)
                    {
                        Game._spriteBatch.Draw(BlankTexture, new Rectangle(Left + c * (IconWidth + IconSpacing) - IconMargin, Top + r * (IconHeight + IconSpacing) - IconMargin, IconWidth + IconMargin * 3, IconHeight + IconMargin * 3), Color.LightYellow);
                    }
                    Color ShadowColor = Color.LightSteelBlue;
                    Color TextColor = Color.MediumPurple;
                    if (c + (r * Game.LevelSelectColumns) > Game.CompletedLevel) { ShadowColor = Color.LightGray; TextColor = Color.IndianRed; }
                    Game._spriteBatch.Draw(BlankTexture, new Rectangle(Left + c * (IconWidth + IconSpacing), Top + r * (IconHeight + IconSpacing), IconWidth, IconHeight), ShadowColor);
                    Game._spriteBatch.Draw(BlankTexture, new Rectangle(Left + c * (IconWidth + IconSpacing) + IconMargin, Top + r * (IconHeight + IconSpacing) + IconMargin, IconWidth, IconHeight), Color.WhiteSmoke);

                    string Label = "" + (c + 1 + Game.LevelSelectColumns * r);
                    Vector2 StringSize = _font.MeasureString(Label);

                    Game._spriteBatch.DrawString(_font, Label, new Vector2(Left + (c * (IconWidth + IconSpacing)) + (IconWidth - StringSize.X) / 2 + 12, Top + r * (IconHeight + IconSpacing) + (IconHeight - StringSize.Y) / 2 + 15), TextColor);
                }
            }

            Game._spriteBatch.End();
        }
    }
}
