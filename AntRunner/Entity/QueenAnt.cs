using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    public class QueenAnt : Ant
    {
        #region Constructors
        public QueenAnt()
        {

        }
        #endregion

        public override void Initialize()
        {
            this.m_States.Clear();
            //this.m_States.Add("Attacking", new FiniteState() { m_Action = Attacking, });
            this.m_States.Add("MoveToPath", new FiniteState() { });
            this.m_States.Add("MoveToPlayer", new FiniteState() { m_Action = MoveToPlayer, m_StateName = "MoveToPlayer"});
            base.Initialize();
        }

        public void MoveToPath_OnBegin(GameTime pGameTime, InputHandler pInputHanderl, Level pLevel)
        {

        }

        public void MoveToPath(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }

        #region Attacking
        public void Attacking_OnBegin(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }

        public void Attacking_OnSuspend(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }

        public void Attacking(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
        #endregion

        #region Wandering
        public void Wandering(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {


        }

        #endregion



        public override void Update(GameTime pGameTime,InputHandler pInputHandler)
        {


            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {


            base.Draw(pSpriteBatch);
        }
    }
}
