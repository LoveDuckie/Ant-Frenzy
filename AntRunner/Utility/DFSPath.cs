using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Utility
{
    public class DFSPath : Pathfinding
    {
        private List<PathNode> m_OpenList = new List<PathNode>();
        private List<PathNode> m_ClosedList = new List<PathNode>();

        public DFSPath()
        {

        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            base.DrawPathfinding(pSpriteBatch);
        }

        public override int DistanceBetween(PathNode pA, PathNode pB)
        {
            return base.DistanceBetween(pA, pB);
        }

        protected override double EuclideanDistance(PathNode pA, PathNode pB)
        {
            return base.EuclideanDistance(pA, pB);
        }

        protected override double Diagonal(PathNode pA, PathNode pB)
        {
            return base.Diagonal(pA, pB);
        }
    }
}
