using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.Storage.Search;
using Windows.Storage;
using Windows.Storage.Streams;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.States;
using AntRunner.Tower;

namespace AntRunner.Utility
{
    public class Key
    {

    }

    public class LPAStarPathNode : 
        PathNode, 
        IComparable<LPAStarPathNode>,
        IEquatable<LPAStarPathNode>
    {
        #region Members
        private int _rhs = 0;
        private int iteration = 0; /** iteration of the last change **/
        #endregion

        #region Properties
        public int RHS
        {
            get { return _rhs; }
            set { _rhs = value; }
        }
        #endregion

        #region Constructors
        public LPAStarPathNode(Point pPosition)
        {
            parent = null;
            this.position = pPosition;
            this.h = 0;
            this.g = RHS = int.MaxValue;
            this.iteration = 0;
        }

        #endregion

        #region Methods
        public bool Equals(LPAStarPathNode other)
        {
            return false;
        }

        public int CompareTo(LPAStarPathNode other)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    // Lifelong Planning AStarPath
    public class LPAStarPath
    {
        #region Members
        private Level m_Level;
        public SortedSet<LPAStarPathNode> m_Nodes = new SortedSet<LPAStarPathNode>();

        private List<LPAStarPathNode> m_Path = new List<LPAStarPathNode>();

        /// <summary>
        /// The base members used for defining the start, end and current node that we are looking at
        /// </summary>
        private LPAStarPathNode m_Start;
        private LPAStarPathNode m_Goal;
        private LPAStarPathNode m_Current;
        #endregion

        #region Properties
        public List<LPAStarPathNode> Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }
        #endregion

        #region Constructors
        public LPAStarPath(Level pLevel)
        {
            m_Level = pLevel;
        }
        #endregion

        #region Methods
        public void Initialize()
        {
            
        }

        public void UpdateVertex(LPAStarPathNode pNode)
        {

        }


        public static List<LPAStarPathNode> ComputePath(Point pFrom, Point pTo, Level pLevel)
        {
            return null;
        }
        #endregion
    }
}
