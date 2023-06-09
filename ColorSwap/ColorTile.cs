using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSwap
{
    internal class ColorTile
    {
        public SwapColor BaseColor;
        public Rectangle Position;

        public ColorTile(SwapColor BaseColor, Rectangle Position) 
        {
            this.BaseColor = BaseColor;
            this.Position = Position;
        }
    }
}
