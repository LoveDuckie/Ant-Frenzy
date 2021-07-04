using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;

namespace AntRunner.Entity
{
    public class DoorBlock : Entity, IToggable
    {
        private bool m_Activated = false;
        private Entity m_LastTouched; // The last entity that touched this door.
        private Point m_FrameSize = Point.Zero;
        private Point m_FrameIndex = Point.Zero;

        #region Constructors
        public DoorBlock()
            : base()
        {

        }

        public DoorBlock(Vector2 pPosition, float pRotation, float pScale)
            : base(pScale, pPosition, MainGame.Instance.Textures["terrain_tiles"], pRotation)
        {
            this.Position = pPosition;
            this.Rotation = pRotation;
            this.Scale = pScale;
            this.Size = new Point(64, 64);
            this.m_FrameIndex = new Point(5, 0);
            this.m_FrameSize = Size;
        }
        #endregion

        public void Toggle(Entity pOther)
        {
            this.m_Activated = m_Activated == true ? false : true;
        }

        public override void Initialize()
        {
            
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            #region Collision Box Updating

            this.CollisionBox = new BoundingBox(new Vector3(Position, 0),
                                                new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0));

            // Create a new bounding box using the rectangle object
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
            #endregion

            
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            #region Collision Box Updating

            this.CollisionBox = new BoundingBox(new Vector3(Position, 0),
                                                new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0));

            // Create a new bounding box using the rectangle object
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);
            #endregion



            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Output the door block that we wish to use.
            pSpriteBatch.Draw(m_SpriteSheet, 
                              Position, 
                              new Rectangle(m_FrameIndex.X * 64, 
                                            m_FrameIndex.Y * 64,
                                            64,
                                            64), 
                              Color.White);

            base.Draw(pSpriteBatch);
        }
        
    }
}
