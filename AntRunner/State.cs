using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;
using AntRunner.States;

namespace AntRunner
{

    // Basic enum used for determining which state to run the code from.
    public enum StateValue
    {
        Menu,
        Game,
        Settings,
        Help
    }

    /// <summary>
    /// State class that is used for offering some kind of state to the gmae.
    /// </summary>
    public class State : IEntity
    {
        public string StateName { get; set; }
        
        public State()
        {

        }

        public virtual void Initialize()
        {

        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {      
            // Do nothing
        }      
               
        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            // Do nothing
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
