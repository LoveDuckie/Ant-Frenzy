using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Entity
{
    public class Water : Resource
    {
        #region Constructors
        public Water(Vector2 pPosition, float pScale, float pRotation, int pAmount) 
            : base(pAmount, new Point(4,4))
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
           
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
