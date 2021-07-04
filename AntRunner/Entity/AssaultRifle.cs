using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;

// Required using states to be implemented.
using AntRunner.States;
using AntRunner.Cameras;
using AntRunner.Entity;

namespace AntRunner.Entity
{
    public class AssaultRifle : Weapon
    {
        public AssaultRifle()
        {

        }

        public override void Fire(Vector2 pDirection, float pRotation)
        {
            base.Fire(pDirection, pRotation);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            base.Draw(pSpriteBatch);
        }

    }
}
