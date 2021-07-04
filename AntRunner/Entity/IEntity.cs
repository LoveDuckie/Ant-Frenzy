using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Entity
{
    interface IEntity
    {
        void Initialize();
        void Update(GameTime pGameTime, InputHandler pInputHandler);
        void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel);
        void Draw(SpriteBatch pSpriteBatch);
        
    }
}
