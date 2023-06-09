using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using TiledCS;
using System.Collections.Generic;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Screens.Transitions;
using MonoGame.Extended;
using System.Security.Cryptography.X509Certificates;

namespace ColorSwap
{
    public class GameplayScreen : GameScreen
    {
        private new Game1 Game => (Game1)base.Game;

        private TiledMap map;
        private Dictionary<int, TiledTileset> tilesets;
        private Texture2D tilesetTexture;

        [Flags]
        enum Trans
        {
            None = 0,
            Flip_H = 1 << 0,
            Flip_V = 1 << 1,
            Flip_D = 1 << 2,

            Rotate_90 = Flip_D | Flip_H,
            Rotate_180 = Flip_H | Flip_V,
            Rotate_270 = Flip_V | Flip_D,

            Rotate_90AndFlip_H = Flip_H | Flip_V | Flip_D,
        }

        private Texture2D FireSetGrid;
        private Texture2D IceSetGrid;
        private Texture2D OutsideHouseSetGrid;
        private Texture2D TileSet;
        private Texture2D SmallSlime;
        private Texture2D CharacterIdle;
        private Texture2D CharacterWalk;
        private Texture2D CharacterJump;
        private Texture2D CharacterDie;

        // animation variables
        private int threshold;
        private bool IsWalking;
        private int currAnimationIndexLimit;
        private int idleAnimationIndexLimit = 5;
        private int jumpAnimationIndexLimit = 11;
        private int walkAnimationIndexLimit = 7;
        private int dieAnimationIndexLimit = 5;
        private SpriteEffects flipEffect = SpriteEffects.None;
        private int currentAnimationIndex;
        private float timer;
        private int ScaleFactor = 3;

        // Tiled variables
        private TiledLayer bluePlatLayer;
        private TiledLayer redPlatLayer;
        private TiledLayer greenPlatLayer;
        private TiledLayer purplePlatLayer;
        private TiledLayer starLayer;
        private TiledLayer playerSpawnLayer;

        // sprite variables
        private Rectangle[] idleRectangles;
        private Rectangle[] walkRectangles;
        private Rectangle[] jumpRectangles;
        private Rectangle[] dieRectangles;

        private Rectangle RedTexture = new Rectangle(52, 52, 16, 16); // from FireSetGrid
        private Rectangle BlueTexture = new Rectangle(18, 52, 16, 16); // from IceSetGrid
        private Rectangle GreenTexture = new Rectangle(0, 0, 100, 100); // from small_slime
        private Rectangle PurpleTexture = new Rectangle(304, 288, 15, 15); // from tileset
        private Rectangle WallTexture = new Rectangle(17, 35, 17, 17); // from Pirate thing

        private bool IsLeftKeyDown = true;
        private bool IsRightKeyDown = true;

        private SwapColor[] CurrentSwapColors;
        private Rectangle[] SwapColorRectangles = new Rectangle[4];
        private int ColorIndex = 0;

        private ArrayList ColorTiles;
        public ArrayList Walls;
        private Rectangle Star;

        private Rectangle Player;
        private Vector2 PlayerVelocity = new Vector2(0, 0);
        private int PlayerMovementSpeed = 5;
        private int PlayerJumpHeight = 12;
        private int Gravity = 1;
        private bool IsOnGround = false;
        private bool IsDead = false;
        private int BounceSpeed = 24;

        private Texture2D BlankTexture;
        private SpriteFont _font;

        // Levels
        private int CurrentLevel;
        private int LevelCount;
        private string LevelPrefix;
        private string LevelSuffix = ".tmx";
        private Point PlayerSpawn;

        public GameplayScreen(Game1 game) : base(game)
        {
            CurrentLevel = 1;
        }

        public GameplayScreen(Game1 game, int Level) : base(game)
        {
            CurrentLevel = Level;
        }

        public override void Initialize()
        {
            LevelCount = Game.TotalLevels;
            LevelPrefix = Content.RootDirectory + "\\Level";

            ColorTiles = new ArrayList();
            Walls = new ArrayList();
            Player = new Rectangle(0, 0, 42, 48);
            SwapColorRectangles[0] = BlueTexture;
            SwapColorRectangles[1] = RedTexture;
            SwapColorRectangles[2] = GreenTexture;
            SwapColorRectangles[3] = PurpleTexture;

            if (BlankTexture == null)
            {
                BlankTexture = new Texture2D(Game.GraphicsDevice, 1, 1);
                BlankTexture.SetData<Color>(new Color[] { Color.White });
            }

            // base.Initialize();
        }

        public override void LoadContent()
        {
            SpriteBatch _spriteBatch = Game._spriteBatch;
            Point GameBounds = Game.GameBounds;

            _font = Game.Content.Load<SpriteFont>("LevelSelect");
            tilesetTexture = Content.Load<Texture2D>("tileTextures2");

            FireSetGrid = Content.Load<Texture2D>("FireSetGrid");
            IceSetGrid = Content.Load<Texture2D>("IceSetGrid");
            OutsideHouseSetGrid = Content.Load<Texture2D>("OutsideHouseSetGrid");
            TileSet = Content.Load<Texture2D>("tileset");
            SmallSlime = Content.Load<Texture2D>("small_slime");
            CharacterIdle = Content.Load<Texture2D>("Meow-Knight_Idle");
            CharacterWalk = Content.Load<Texture2D>("Meow-Knight_Run");
            CharacterJump = Content.Load<Texture2D>("Meow-Knight_Jump");
            CharacterDie = Content.Load<Texture2D>("Meow-Knight_Death");

            idleRectangles = new Rectangle[6];
            idleRectangles[0] = new Rectangle(1, 0, 14, 16);
            idleRectangles[1] = new Rectangle(1, 26, 14, 16);
            idleRectangles[2] = new Rectangle(1, 52, 14, 16);
            idleRectangles[3] = new Rectangle(1, 78, 14, 16);
            idleRectangles[4] = new Rectangle(1, 104, 14, 16);
            idleRectangles[5] = new Rectangle(1, 130, 14, 16);

            jumpRectangles = new Rectangle[12];
            jumpRectangles[0] = new Rectangle(1, 0, 14, 16);
            jumpRectangles[1] = new Rectangle(1, 26, 14, 16);
            jumpRectangles[2] = new Rectangle(1, 52, 14, 16);
            jumpRectangles[3] = new Rectangle(1, 78, 14, 18);
            jumpRectangles[4] = new Rectangle(1, 120, 14, 20);
            jumpRectangles[5] = new Rectangle(1, 162, 14, 18);
            jumpRectangles[6] = new Rectangle(1, 205, 14, 16);
            jumpRectangles[7] = new Rectangle(1, 246, 14, 16);
            jumpRectangles[8] = new Rectangle(1, 272, 14, 16);
            jumpRectangles[9] = new Rectangle(1, 299, 14, 16);
            jumpRectangles[10] = new Rectangle(1, 326, 14, 16);
            jumpRectangles[11] = new Rectangle(1, 350, 14, 16);

            walkRectangles = new Rectangle[8];
            walkRectangles[0] = new Rectangle(1, 0, 14, 16);
            walkRectangles[1] = new Rectangle(1, 25, 14, 16);
            walkRectangles[2] = new Rectangle(1, 67, 14, 16);
            walkRectangles[3] = new Rectangle(1, 94, 14, 16);
            walkRectangles[4] = new Rectangle(1, 122, 14, 16);
            walkRectangles[5] = new Rectangle(1, 146, 14, 16);
            walkRectangles[6] = new Rectangle(1, 187, 14, 16);
            walkRectangles[7] = new Rectangle(1, 214, 14, 16);

            dieRectangles = new Rectangle[6];
            dieRectangles[0] = new Rectangle(1, 0, 14, 16);
            dieRectangles[1] = new Rectangle(1, 26, 15, 16);
            dieRectangles[2] = new Rectangle(1, 52, 17, 16);
            dieRectangles[3] = new Rectangle(1, 78, 23, 16);
            dieRectangles[4] = new Rectangle(1, 104, 22, 16);
            dieRectangles[5] = new Rectangle(1, 130, 22, 16);

            currentAnimationIndex = 0;

            // Walls for every level are the same
            Walls.Add(new Wall(new Rectangle(0, 0, GameBounds.X - 48, 48)));
            Walls.Add(new Wall(new Rectangle(0, 48, 48, GameBounds.Y)));
            Walls.Add(new Wall(new Rectangle(48, GameBounds.Y - 48, GameBounds.X, GameBounds.Y)));
            Walls.Add(new Wall(new Rectangle(GameBounds.X - 48, 0, GameBounds.X, GameBounds.Y - 48)));

            LoadMapObjects();
        }

        private void LoadMapObjects()
        {
            map = new TiledMap(LevelPrefix + CurrentLevel + LevelSuffix);
            tilesets = map.GetTiledTilesets(Content.RootDirectory + "/");

            bluePlatLayer = map.Layers.FirstOrDefault(l => l.name == "BluePlat");
            redPlatLayer = map.Layers.FirstOrDefault(l => l.name == "RedPlat");
            greenPlatLayer = map.Layers.FirstOrDefault(l => l.name == "GreenPlat");
            purplePlatLayer = map.Layers.FirstOrDefault(l => l.name == "PurplePlat");
            starLayer = map.Layers.FirstOrDefault(l => l.name == "Star");
            playerSpawnLayer = map.Layers.FirstOrDefault(l => l.name == "PlayerSpawn");

            ColorTiles.Clear();

            HashSet<SwapColor> colors = new HashSet<SwapColor>();

            foreach (var obj in bluePlatLayer.objects)
            {
                colors.Add(SwapColor.Blue);
                ColorTiles.Add(new ColorTile(SwapColor.Blue, new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height)));
            }
            foreach (var obj in redPlatLayer.objects)
            {
                colors.Add(SwapColor.Red);
                ColorTiles.Add(new ColorTile(SwapColor.Red, new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height)));
            }
            foreach (var obj in greenPlatLayer.objects)
            {
                colors.Add(SwapColor.Green);
                ColorTiles.Add(new ColorTile(SwapColor.Green, new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height)));
            }
            foreach (var obj in purplePlatLayer.objects)
            {
                colors.Add(SwapColor.Purple);
                ColorTiles.Add(new ColorTile(SwapColor.Purple, new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height)));
            }
            CurrentSwapColors = colors.ToArray<SwapColor>();
            foreach (var obj in starLayer.objects)
            {
                if (obj != null)
                {
                    Star = new Rectangle((int)obj.x, (int)obj.y, (int)obj.width, (int)obj.height);
                }
            }
            foreach (var obj in playerSpawnLayer.objects)
            {
                if (obj != null)
                {
                    PlayerSpawn = new Point((int)obj.x, (int)obj.y);
                    Player.X = (int)obj.x;
                    Player.Y = (int)obj.y;
                }
            }
        }

        class ColorTileComparer : Comparer<ColorTile>
        {
            private int Index;
            private SwapColor[] CurrentColors;
            public ColorTileComparer(int index, SwapColor[] currentColors)
            {
                Index = index;
                CurrentColors = currentColors;
            }

            public override int Compare(ColorTile x, ColorTile y)
            {
                return (int)Util.GetCurrentColor(x.BaseColor, Index, CurrentColors) - (int)Util.GetCurrentColor(y.BaseColor, Index, CurrentColors);
            }
        }

        private void SortColorTiles()
        {
            ColorTiles.Sort(new ColorTileComparer(ColorIndex, CurrentSwapColors));
        }

        public override void Update(GameTime gameTime)
        {
            threshold = 250;
            IsWalking = false;
            currAnimationIndexLimit = idleAnimationIndexLimit;
            SpriteEffects prevSpriteEffect = flipEffect;

            // Swap colors when left/right arrows pressed
            if (Keyboard.GetState().IsKeyDown(Keys.Left) && !IsDead)
            {
                if (!IsLeftKeyDown)
                {
                    // check if player is inside purple before swapping
                    if (IsInsidePurplePlatform())
                    {
                        IsLeftKeyDown = true;
                        ColorIndex = (ColorIndex - 1 + CurrentSwapColors.Length) % CurrentSwapColors.Length;
                        Die();
                    }
                    else
                    {
                        IsLeftKeyDown = true;
                        ColorIndex = (ColorIndex - 1 + CurrentSwapColors.Length) % CurrentSwapColors.Length;
                        SortColorTiles();
                    }
                }
            }
            else IsLeftKeyDown = false;
            if (Keyboard.GetState().IsKeyDown(Keys.Right) && !IsDead)
            {
                if (!IsRightKeyDown)
                {
                    // check if player is inside purple before swapping
                    if (IsInsidePurplePlatform())
                    {
                        IsRightKeyDown = true;
                        ColorIndex = (ColorIndex + 1) % CurrentSwapColors.Length;
                        Die();
                    }
                    else
                    {
                        IsRightKeyDown = true;
                        ColorIndex = (ColorIndex + 1) % CurrentSwapColors.Length;
                        SortColorTiles();
                    }
                }
            }
            else IsRightKeyDown = false;

            // Check keyboard controls
            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.D) && !IsDead)
            {
                IsWalking = false;
                flipEffect = prevSpriteEffect;
                threshold = 250;
                currAnimationIndexLimit = idleAnimationIndexLimit;
                PlayerVelocity.X = 0;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.A) && !IsDead)
            {
                if (PlayerVelocity.X > -PlayerMovementSpeed)
                {
                    PlayerVelocity.X -= PlayerMovementSpeed;
                    if (PlayerVelocity.X < -PlayerMovementSpeed)
                    {
                        PlayerVelocity.X = PlayerMovementSpeed;
                    }
                }
                flipEffect = SpriteEffects.FlipHorizontally;
                IsWalking = true;
                currAnimationIndexLimit = walkAnimationIndexLimit;
                threshold = 100;
            }
            else if (Keyboard.GetState().IsKeyDown(Keys.D) && !IsDead)
            {
                if (PlayerVelocity.X < PlayerMovementSpeed)
                {
                    PlayerVelocity.X += PlayerMovementSpeed;
                    if (PlayerVelocity.X > PlayerMovementSpeed)
                    {
                        PlayerVelocity.X = PlayerMovementSpeed;
                    }
                }
                flipEffect = SpriteEffects.None;
                IsWalking = true;
                currAnimationIndexLimit = walkAnimationIndexLimit;
                threshold = 100;
            }
            if (!Keyboard.GetState().IsKeyDown(Keys.A) && !Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (PlayerVelocity.X > 0)
                {
                    PlayerVelocity.X -= Math.Min(PlayerMovementSpeed / 2, PlayerVelocity.X);
                }
                else
                {
                    PlayerVelocity.X += Math.Min(PlayerMovementSpeed / 2, Math.Abs(PlayerVelocity.X));
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.W) && IsOnGround && !IsDead)
            {
                PlayerVelocity.Y = -PlayerJumpHeight;
                IsOnGround = false;
            }

            // Handle collisions 
            foreach (ColorTile Tile in ColorTiles)
            {
                if (Util.GetCurrentColor(Tile.BaseColor, ColorIndex, CurrentSwapColors).Equals(SwapColor.Blue))
                {
                    Rectangle result = Util.CalculateCollision(Player, Tile.Position, PlayerVelocity);
                    Player.X = result.X; Player.Y = result.Y;
                    if (result.Height == 0 && PlayerVelocity.Y > 0 && Util.WillCollide(Player, Tile.Position, new Vector2(0, PlayerVelocity.Y)))
                    {
                        IsOnGround = true;
                    }
                    PlayerVelocity = new Vector2(result.Width, result.Height);

                }
                else if (Util.GetCurrentColor(Tile.BaseColor, ColorIndex, CurrentSwapColors).Equals(SwapColor.Red))
                {
                    if (Util.WillCollide(Player, Tile.Position, PlayerVelocity))
                    {
                        // apply velocity one more time before player death animation occurs
                        Rectangle result = Util.CalculateCollision(Player, Tile.Position, PlayerVelocity);
                        Player.X = result.X; Player.Y = result.Y;
                        Die();
                    }
                }
                else if (Util.GetCurrentColor(Tile.BaseColor, ColorIndex, CurrentSwapColors).Equals(SwapColor.Green))
                {
                    bool hasCollided = false;
                    if (Util.WillCollide(Player, Tile.Position, new Vector2(PlayerVelocity.X, 0)))
                    {
                        hasCollided = true;
                        if (PlayerVelocity.X > 0)
                        {
                            PlayerVelocity.X = -BounceSpeed;
                        }
                        else
                        {
                            PlayerVelocity.X = BounceSpeed;
                        }
                    }
                    if (Util.WillCollide(Player, Tile.Position, new Vector2(PlayerVelocity.X, PlayerVelocity.Y)) && !hasCollided)
                    {
                        if (PlayerVelocity.Y > 0)
                        {
                            PlayerVelocity.Y = -BounceSpeed;
                        }
                        else
                        {
                            PlayerVelocity.Y = BounceSpeed;
                        }
                    }
                }
                if (PlayerVelocity.X == 0 && PlayerVelocity.Y == 0)
                {
                    // No need to continue checking collisions if we are not moving
                    break;
                }
            }

            // Check walls
            foreach (Wall Wall in Walls)
            {
                Rectangle result = Util.CalculateCollision(Player, Wall.Position, PlayerVelocity);
                Player.X = result.X; Player.Y = result.Y;
                if (result.Height == 0 && PlayerVelocity.Y > 0 && Util.WillCollide(Player, Wall.Position, new Vector2(0, PlayerVelocity.Y)))
                {
                    IsOnGround = true;
                }
                PlayerVelocity = new Vector2(result.Width, result.Height);
            }

            // Hit goal
            if (Util.WillCollide(Player, Star, PlayerVelocity))
            {
                NextLevel();
            }

            if (!IsOnGround)
            {
                currAnimationIndexLimit = jumpAnimationIndexLimit;
                threshold = 50;
            }

            if (IsDead)
            {
                currAnimationIndexLimit = dieAnimationIndexLimit;
                threshold = 125;
                flipEffect = prevSpriteEffect;
            }
            else
            {
                // Apply velocity
                Player.X += (int)PlayerVelocity.X;
                Player.Y += (int)PlayerVelocity.Y;
                PlayerVelocity.Y += Gravity;
            }

            if (currentAnimationIndex > currAnimationIndexLimit)
            {
                currentAnimationIndex = 0;
            }

            if (timer > threshold)
            {
                timer = 0;

                if (currentAnimationIndex >= currAnimationIndexLimit && IsDead)
                    Reset();
                else if (currentAnimationIndex >= currAnimationIndexLimit && !IsOnGround)
                    currentAnimationIndex = currAnimationIndexLimit;
                else if (currentAnimationIndex >= currAnimationIndexLimit)
                    currentAnimationIndex = 0;
                else
                    currentAnimationIndex += 1;
            }
            // If the timer has not reached the threshold, then add the milliseconds that have past since the last Update() to the timer.
            else
            {
                timer += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            //base.Update(gameTime);
        }

        private bool IsInsidePurplePlatform()
        {
            foreach (ColorTile Tile in ColorTiles)
            {
                if (Util.GetCurrentColor(Tile.BaseColor, ColorIndex, CurrentSwapColors).Equals(SwapColor.Purple))
                {
                    if (Player.X + Player.Width > Tile.Position.X && Player.X < Tile.Position.X + Tile.Position.Width)
                        if (Player.Y + Player.Height > Tile.Position.Y && Player.Y < Tile.Position.Y + Tile.Position.Height)
                            return true;
                }
            }
            return false;
        }

        private void NextLevel()
        {
            Reset();
            Game.CompleteLevel(CurrentLevel);
            CurrentLevel += 1;
            CurrentLevel %= LevelCount + 1;
            if (CurrentLevel == 0) CurrentLevel = 1;
            LoadMapObjects();
        }

        private void Die()
        {
            currentAnimationIndex = 0;
            IsDead = true;
            PlayerVelocity.X = 0; PlayerVelocity.Y = 0;
            SortColorTiles();
        }

        private void Reset()
        {
            Player.X = PlayerSpawn.X;
            Player.Y = PlayerSpawn.Y;
            ColorIndex = 0;
            PlayerVelocity = new Vector2(0, 0);
            IsDead = false;
            SortColorTiles();
            IsOnGround = false;
        }

        public void DrawGame(GameTime gameTime)
        {
            Draw(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch _spriteBatch = Game._spriteBatch;
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            foreach (Wall Wall in Walls)
            {
                _spriteBatch.Draw(OutsideHouseSetGrid, Wall.Position, WallTexture, Color.White);
            }

            // Draw tiles
            var tileLayers = map.Layers.Where(x => x.type == TiledLayerType.TileLayer);

            Rectangle[] sourceRectangles = new Rectangle[4];
            foreach (var layer in tileLayers)
            {
                int idx = 0;
                if (layer.name == "BlueTileLayer")
                    idx = 0;
                else if (layer.name == "RedTileLayer")
                    idx = 1;
                else if (layer.name == "GreenTileLayer")
                    idx = 2;
                else if (layer.name == "PurpleTileLayer")
                    idx = 3;
                else
                    continue;

                for (var y = 0; y < layer.height; y++)
                {
                    for (var x = 0; x < layer.width; x++)
                    {
                        // Assuming the default render order is used which is from right to bottom
                        var index = (y * layer.width) + x;
                        var gid = layer.data[index]; // The tileset tile index

                        // Gid 0 is used to tell there is no tile set
                        if (gid != 0)
                        {
                            var mapTileset = map.GetTiledMapTileset(gid);
                            var tileset = tilesets[mapTileset.firstgid];
                            var rect = map.GetSourceRect(mapTileset, tileset, gid);
                            var source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                            sourceRectangles[idx] = source;
                        }
                    }
                }
            }

            foreach (var layer in tileLayers)
            {
                for (var y = 0; y < layer.height; y++)
                {
                    for (var x = 0; x < layer.width; x++)
                    {
                        // Wall layer
                        bool NonPlatform = layer.name == "WallTileLayer" || layer.name == "StarTileLayer";
                        // Assuming the default render order is used which is from right to bottom
                        var index = (y * layer.width) + x;
                        var gid = layer.data[index]; // The tileset tile index

                        int idx = 0;
                        if (layer.name == "BlueTileLayer")
                            idx = 0;
                        else if (layer.name == "RedTileLayer")
                            idx = 1;
                        else if (layer.name == "GreenTileLayer")
                            idx = 2;
                        else if (layer.name == "PurpleTileLayer")
                            idx = 3;
                        idx += ColorIndex;
                        idx %= CurrentSwapColors.Length;

                        var tileX = x * map.TileWidth;
                        var tileY = y * map.TileHeight;

                        // Gid 0 is used to tell there is no tile set
                        if (gid == 0)
                        {
                            continue;
                        }

                        // Helper method to fetch the right TieldMapTileset instance
                        // This is a connection object Tiled uses for linking the correct tileset to the 
                        // gid value using the firstgid property
                        var mapTileset = map.GetTiledMapTileset(gid);

                        // Retrieve the actual tileset based on the firstgid property of the connection object 
                        // we retrieved just now
                        var tileset = tilesets[mapTileset.firstgid];

                        // Use the connection object as well as the tileset to figure out the source rectangle
                        var rect = map.GetSourceRect(mapTileset, tileset, gid);

                        // Create destination and source rectangles
                        Rectangle source;
                        if (NonPlatform) source = new Rectangle(rect.x, rect.y, rect.width, rect.height);
                        else source = sourceRectangles[idx];
                        var destination = new Rectangle(tileX, tileY, map.TileWidth, map.TileHeight);

                        // You can use the helper methods to get information to handle flips and rotations
                        Trans tileTrans = Trans.None;
                        if (map.IsTileFlippedHorizontal(layer, x, y)) tileTrans |= Trans.Flip_H;
                        if (map.IsTileFlippedVertical(layer, x, y)) tileTrans |= Trans.Flip_V;
                        if (map.IsTileFlippedDiagonal(layer, x, y)) tileTrans |= Trans.Flip_D;

                        SpriteEffects effects = SpriteEffects.None;
                        double rotation = 0f;
                        switch (tileTrans)
                        {
                            case Trans.Flip_H: effects = SpriteEffects.FlipHorizontally; break;
                            case Trans.Flip_V: effects = SpriteEffects.FlipVertically; break;

                            case Trans.Rotate_90:
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            case Trans.Rotate_180:
                                rotation = Math.PI;
                                destination.X += map.TileWidth;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_270:
                                rotation = Math.PI * 3 / 2;
                                destination.Y += map.TileHeight;
                                break;

                            case Trans.Rotate_90AndFlip_H:
                                effects = SpriteEffects.FlipHorizontally;
                                rotation = Math.PI * .5f;
                                destination.X += map.TileWidth;
                                break;

                            default:
                                break;
                        }

                        // Render sprite at position tileX, tileY using the rect
                        _spriteBatch.Draw(tilesetTexture, destination, source, Color.White,
                            (float)rotation, Vector2.Zero, effects, 0);
                    }
                }
            }

            // Draw player
            if (IsDead)
                _spriteBatch.Draw(CharacterDie, new Vector2(Player.X, Player.Y), dieRectangles[currentAnimationIndex], Color.White, 0, new Vector2(0, 0), ScaleFactor, flipEffect, 0);
            else if (!IsOnGround)
                _spriteBatch.Draw(CharacterJump, new Vector2(Player.X, Player.Y), jumpRectangles[currentAnimationIndex], Color.White, 0, new Vector2(0, 0), ScaleFactor, flipEffect, 0);
            else if (IsWalking)
                _spriteBatch.Draw(CharacterWalk, new Vector2(Player.X, Player.Y), walkRectangles[currentAnimationIndex], Color.White, 0, new Vector2(0, 0), ScaleFactor, flipEffect, 0);
            else
                _spriteBatch.Draw(CharacterIdle, new Vector2(Player.X, Player.Y), idleRectangles[currentAnimationIndex], Color.White, 0, new Vector2(0, 0), ScaleFactor, flipEffect, 0);

            if (CurrentLevel == 1)
                _spriteBatch.DrawString(_font, "Use WAD to move/jump", new Vector2(100, 100), Color.AntiqueWhite);

            if (CurrentLevel == 2)
                _spriteBatch.DrawString(_font, "Use left/right arrow keys to swap colors", new Vector2(100, 100), Color.AntiqueWhite);

            // Draw color sequence
            _spriteBatch.Draw(BlankTexture, new Rectangle(48 * ColorIndex + 48 + 2, Game.GameBounds.Y - 48 + 2, 44, 44), Color.Yellow);
            _spriteBatch.Draw(BlankTexture, new Rectangle(48 + 4, Game.GameBounds.Y - 48 + 4, 40, 40), Color.CornflowerBlue);
            if (CurrentSwapColors.Contains(SwapColor.Red))
                _spriteBatch.Draw(BlankTexture, new Rectangle(48 * 2 + 4, Game.GameBounds.Y - 48 + 4, 40, 40), Color.Red);
            if (CurrentSwapColors.Contains(SwapColor.Green))
                _spriteBatch.Draw(BlankTexture, new Rectangle(48 * 3 + 4, Game.GameBounds.Y - 48 + 4, 40, 40), Color.LimeGreen);
            if (CurrentSwapColors.Contains(SwapColor.Purple))
                _spriteBatch.Draw(BlankTexture, new Rectangle(48 * 4 + 4, Game.GameBounds.Y - 48 + 4, 40, 40), Color.MediumPurple);

            _spriteBatch.End();
        }
    }
}