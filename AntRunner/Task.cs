using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;

using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.States;
using AntRunner.Utility;

namespace AntRunner
{

    // This will determine how the condition will behave.
    public enum ConditionType
    {
        AtLocation = 1,
        GreaterThan,
        LessThan,
        GreaterThanEqual,
        LessThanEqual,
        WithinRadius
    }

    // This is something that would have to be completed before hand.
    public class HTNPrecondition
    {
        
       // public bool ConditionMet(string pValue
    }

    /// <summary>
    /// Used for developing a hierarchical task planning system
    /// </summary>
    public abstract class HTNTask : IComparable<HTNTask>
    {
        #region Members
        protected List<HTNPrecondition> m_Preconditions = new List<HTNPrecondition>();
        protected List<HTNTask> m_Tasks = new List<HTNTask>();
        protected int m_CurrentTask = 0;
        private bool m_TaskCompleted = false;
        private float m_Priority = 0f;
        private BlueAnt m_Owner;
        #endregion

        /// <summary>
        /// Assign ownership of the task to the Ant
        /// </summary>
        /// <param name="pAnt">Ant object that is to be used.</param>
        public HTNTask(BlueAnt pAnt)
        {
            m_Owner = pAnt;
        }
        
        public abstract bool Preconditions();

        #region Properties
        public float Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }
        
        public bool TaskCompleted
        {
            get { return m_TaskCompleted; }
        }
        #endregion


        /// <summary>
        /// If the preconditions haven't been met, then execute the task.
        /// </summary>
        public abstract void ExecuteTask(GameTime pGameTime, InputHandler pInputHandler, Level pLevel);


        public static HTNTask InterpretTask(string pFileName)
        {
            // Load in the file in question
            XDocument _document = XDocument.Load("Assets//" + pFileName);

            return null;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            // Loop through the tasks and only render information about the ones that are not completed.
            for (int i = 0; i < m_Tasks.Count; i++)
            {
                
            }
        }

        /// <summary>
        /// Return whether or not this one is better than the other.
        /// </summary>
        /// <param name="other">The other object that we are comparing against.</param>
        /// <returns>Returns the value based on a comparison of either objects.</returns>
        public int CompareTo(HTNTask other)
        {
            if (this.Priority > other.Priority)
                return 1;
            else if (this.Priority < other.Priority)
                return -1;
            else
                return 0; // At this point, they are bound to be equal.
        }
    }

    /// <summary>
    /// Move towards a given direction that is provided
    /// </summary>
    public class HTNMove : HTNTask
    {
        private Vector2 m_Location;

        public HTNMove(BlueAnt pAnt, Vector2 pPosition) : base(pAnt)
        {

        }

        public override void ExecuteTask(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }

        /// <summary>
        /// Leave as is for now.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Return whether or not we have met the pre-conditions!
        /// </summary>
        /// <returns>Returns true or false based on whether we have met the condition</returns>
        public override bool Preconditions()
        {
            return false;
        }

    }

    public class HTNCollectCake : HTNTask
    {
        public HTNCollectCake(BlueAnt pAnt) : base(pAnt)
        {

        }

        public override void ExecuteTask(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }

        public override bool Preconditions()
        {
            return false;
        }
    }

    public class HTNDrinkWater : HTNTask
    {
        #region Constructors
        public HTNDrinkWater(BlueAnt pAnt)
            : base(pAnt)
        {

        }
        #endregion

        // Determine whether or not the precondition is met.
        public override bool Preconditions()
        {
            return false;
        }

        public override void ExecuteTask(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        } 
    }
}
