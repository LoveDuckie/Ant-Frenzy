using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework;

using PriorityQueueLib;

namespace AntRunner.Utility
{
    public class DStarLitePathOpt : Pathfinding
    {
        public DStarLitePathOpt()
        {

        }

        public void Initialize(Point pFrom, Point pTo)
        {

        }

        public double CalculateKey(DStarLitePathNode pOther)
        {
            return double.PositiveInfinity;
        }
    }
}
