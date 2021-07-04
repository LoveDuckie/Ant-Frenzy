using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    public class Diamond : Resource
    {
        #region Constructors
        public Diamond() : base()
        {

        }

        public Diamond(Vector2 pPosition, float pRotation, float pScale, int pAmount)
            : base(pAmount,new Point(2,3))
        {
            this.Position = pPosition;
            this.Rotation = pRotation;
            this.Scale = pScale;
            this.Size = new Point(64, 64);
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Render the resource to the screen
            pSpriteBatch.Draw(m_SpriteSheet, m_Position, 
                              new Rectangle(m_FrameIndex.X * 64, m_FrameIndex.Y * 64, Size.X,Size.Y), 
                              Color.White);

            base.Draw(pSpriteBatch);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            #region Collision Box Updating
           
            this.CollisionBox = new BoundingBox(new Vector3(Position, 0), 
                                                new Vector3(Position.X + Size.X, Position.Y + Size.Y,0));

            // Create a new bounding box using the rectangle object
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
           #endregion

            base.Update(pGameTime, pInputHandler);
        }
        #endregion

    }
}
