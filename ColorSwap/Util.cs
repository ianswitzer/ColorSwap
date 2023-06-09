using Microsoft.Xna.Framework;
using SharpDX.Direct2D1.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ColorSwap
{
    internal class Util
    {
        // Return the current color of a tile, offset by the current color index
        public static SwapColor GetCurrentColor(SwapColor BaseColor, int ColorIndex, SwapColor[] ColorList)
        {
            return ColorList[(Array.IndexOf(ColorList, BaseColor) + ColorIndex + ColorList.Length) % ColorList.Length];
        }

        // Return true if source will collide with target given source's motion
        public static bool WillCollide(Rectangle source, Rectangle target, Vector2 motion)
        {
            if (motion.Y > 0) // Downward collision
            {// bottom source is above target
                if (source.Y + source.Height <= target.Y)
                {// bottom of source is no longer above target after motion
                    if (source.Y + source.Height + motion.Y >= target.Y)
                    {// source is between left/right ends of target
                        if (source.X + source.Width > target.X && source.X < target.X + target.Width)
                            return true;
                        else if (source.X >= target.X + target.Width && source.X + motion.X < target.X + target.Width)
                            return true;
                        else if (source.X + source.Width <= target.X && source.X + source.Width + motion.X > target.X)
                            return true;
                    }
                }
            }
            else if (motion.Y < 0) // Upward collision
            {// top source is below target
                if (source.Y >= target.Y + target.Height)
                {// top of source is no longer below target after motion
                    if (source.Y + motion.Y <= target.Y + target.Height)
                    {// source is between left/right ends of target
                        if (source.X + source.Width > target.X && source.X < target.X + target.Width)
                            return true;
                        else if (source.X >= target.X + target.Width && source.X + motion.X < target.X + target.Width)
                            return true;
                        else if (source.X + source.Width <= target.X && source.X + source.Width + motion.X > target.X)
                            return true;
                    }
                }
            }
            if (motion.X < 0) // Left collision
            {// left of source is right of target
                if (source.X >= target.X + target.Width)
                {// source is between top/bottom ends of target
                    if (source.Y + source.Height > target.Y && source.Y < target.Y + target.Height)
                    {// left of source is no longer right of target
                        if (source.X + motion.X < target.X + target.Width)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (motion.X > 0) // Right collision
            {// right of source is left of target
                if (source.X + source.Width <= target.X)
                {// source is between top/bottom ends of target
                    if (source.Y + source.Height > target.Y && source.Y < target.Y + target.Height)
                    {// right of source is no longer left of target
                        if (source.X + source.Width + motion.X > target.X)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Return X/Y coordinates and horizontal/vertical velocity for source after calculating whether it will collide with target
        public static Rectangle CalculateCollision(Rectangle source, Rectangle target, Vector2 motion)
        {
            Rectangle outcome = new Rectangle(source.X, source.Y, (int)motion.X, (int)motion.Y);
            bool hasCollided = false;
            if (WillCollide(source, target, new Vector2(motion.X, 0)))
            {
                hasCollided = true;
                if (motion.X > 0)
                {
                    outcome.X = target.X - source.Width;
                } else
                {
                    outcome.X = target.X + target.Width;
                }
                outcome.Width = 0;
            }
            if (WillCollide(source, target, new Vector2(motion.X, motion.Y)) && !hasCollided)
            {
                if (motion.Y > 0)
                {
                    outcome.Y = target.Y - source.Height;
                }
                else
                {
                    outcome.Y = target.Y + target.Height;
                }
                outcome.Height = 0;
            }
            return outcome;
        }
    }
}
