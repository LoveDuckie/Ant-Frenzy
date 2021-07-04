using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Required from the antrunner namespace.
using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.States;
using AntRunner.Tower;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    public class NormalAnt : Ant
    {
        #region Constructors
        public NormalAnt()
        {
            // Add the new states.
            this.m_States.Add("Attacking", new FiniteState() { m_OnBegin = Attacking_OnBegin, m_Action = Attacking, m_StateName = "Attacking" });
            this.m_States.Add("Wandering", new FiniteState() { m_OnBegin = Wandering_OnBegin, m_Action = Wandering, m_StateName = "Wandering" });
        
            //this.m_States.Add("MoveToPath", new FiniteState() { m_OnBegin = MoveToPath_OnBegin, m_Action = MoveToPath, m_OnSuspend = MoveToPath_OnBegin
        }
        #endregion

        #region Finite States
        
            
#region MoveToPath
        private void MoveToPath_OnBegin(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        private void MoveToPath_OnSuspend(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }
#endregion

#region Attacking
        private void Attacking_OnSuspend(Level pLevel,GameTime pGameTime, InputHandler pInputHandler)
        {

        }
            
        private void Attacking_OnBegin(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        protected override void Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }
#endregion
        
        // This is the function that is called 
        private void Wandering_OnBegin(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        private void Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        private void Wandering_OnSuspend(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }
        #endregion

        public override void Initialize()
        {
            // Clear out anything htat was inherited.
            this.m_States.Clear();

            base.Initialize();
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            
            base.Draw(pSpriteBatch);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            
            base.Update(pGameTime, pInputHandler);
        }

    }
}
