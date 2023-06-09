using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGame.Extended;
using Microsoft.Xna.Framework.Input;

namespace ColorSwap
{
    public class TitleScreen : GameScreen
    {
        private new Game1 Game => (Game1)base.Game;

        private Texture2D _logo;
        private SpriteFont _font;
        private Rectangle LogoRectangle = new Rectangle(1, 0, 14, 16);
        private Rectangle LogoPosition;
        private Vector2 LogoSize = new Vector2(200, 200);
        private Vector2 TitlePosition;
        private Vector2 InstructionPosition;
        private string TitleString = "ColorSwap";
        private string InstructionString = "(Press Space)";
        private int C = 0;

        public TitleScreen(Game1 game) : base(game) { }

        public override void LoadContent()
        {
            base.LoadContent();
            _font = Game.Content.Load<SpriteFont>("TitleScreen");
            _logo = Game.Content.Load<Texture2D>("Meow-Knight_Idle");
            LogoPosition = new Rectangle(Game.GameBounds.X / 2 - (int)LogoSize.X / 2, Game.GameBounds.Y / 2 - (int)LogoSize.Y / 2, (int)LogoSize.X, (int)LogoSize.Y);
            
            Vector2 TitleSize = _font.MeasureString(TitleString);
            Vector2 InstructionSize = _font.MeasureString(InstructionString);

            TitlePosition = new Vector2(Game.GameBounds.X / 2 - TitleSize.X / 2, 100);
            InstructionPosition = new Vector2(Game.GameBounds.X / 2 - InstructionSize.X / 2, Game.GameBounds.Y - 100 - InstructionSize.Y);
        }

        public override void Update(GameTime gameTime)
        {
            C += 1;
        }

        public override void Draw(GameTime gameTime)
        {
            Game.GraphicsDevice.Clear(new Color(16, 139, 204));
            Game._spriteBatch.Begin();
            Game._spriteBatch.DrawString(_font, TitleString, TitlePosition, new Color(C % 256, (C + 85) % 256, (C + 170) % 256));
            Game._spriteBatch.DrawString(_font, InstructionString, InstructionPosition, new Color(255, 255, 0, 128));
            Game._spriteBatch.Draw(_logo, LogoPosition, LogoRectangle, Color.White);
            Game._spriteBatch.End();
        }
    }
}
