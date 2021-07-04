using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Utility;
using AntRunner.Tower;
using AntRunner.States;
using AntRunner.Menu;
using AntRunner.Cameras;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.Entity
{
    public enum BlackHoleType
    {
        Absorb = 1,
        Eject
    }

    public class BlackHole : Entity
    {
        // Used for determining how close that the ants have to be before the
        // gravitational effect takes place.
        private float m_PullRadius = 14f;

        private BlackHoleType m_BlackHoleType;

        // For the pulsating effect that we want
        private bool m_Pulsate = false;
        private float m_PulsateScale = 2f;

        #region Properties
        public BlackHoleType BlackHoleType
        {
            get { return m_BlackHoleType; }
            set { m_BlackHoleType = value; }
        }

        public float PullRadius
        {
            get { return m_PullRadius; }
            set { m_PullRadius = value; }
        }
        #endregion

        #region Constructors
        // Set the position of the black hole
        public BlackHole(Vector2 pPosition)
            : base()
        {
            this.BlackHoleType = AntRunner.Entity.BlackHoleType.Absorb;
            this.Position = pPosition;
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
            this.Size = new Point(64, 64);
            this.Origin = new Vector2(Size.X / 2, Size.Y / 2);
            this.WorldOrigin = Position + Origin;
            this.Mass = 1f;
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            this.WorldOrigin = Position + Origin;

            // Update the collision bounding boxes that we are going to be requiring.
            this.CollisionBox = new BoundingBox(new Vector3(Position, 0), 
                                                new Vector3(Position.X + Size.X, Position.Y + Size.Y, 0));

            this.BoundingBox = new Rectangle((int)Position.X, 
                                             (int)Position.Y,
                                             Size.X, 
                                             Size.Y);

            // Loop through the entities and do something
            foreach (var item in Entity.Entities)
            {
                // Kill the ant if it falls into the trap!
                if (item is Ant && 
                    this.CollisionBox.Contains(item.CollisionBox) != ContainmentType.Disjoint)
                {
                    item.Dead = true;
                }

                if (item is Ant &&
                    this.BoundingBox.Intersects(item.BoundingBox))
                {
                    item.Dead = true;
                }

                // Check that the item is within distance
                if (item is Ant && WithinRange(item, PullRadius))
                {
                    m_Pulsate = true;
                    break;
                }
                else
                {
                    m_Pulsate = false;
                }
            }

            // Apply the pulsating effect that we want
            if (!m_Pulsate)
            {
                m_PulsateScale = 2f;
            }
            else
            {
                // If the scale has gotten too low, then set it back to normal again
                if (m_PulsateScale < 1f)
                {
                    m_PulsateScale = 2f;
                }
                else
                {
                    m_PulsateScale -= 0.025f;
                }
            }

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {



            // Draw the "black hole" to the screen
            pSpriteBatch.Draw(m_SpriteSheet,
                                Position,
                                new Rectangle(
                                    5*64,
                                    2*64,
                                    Size.X,
                                    Size.Y),Color.White
                                    );

            // Only draw this is the pulsating effect has been enabled.
            if (m_Pulsate)
            {
                // Draw the pulse from the black hole if we're pulsating;
                pSpriteBatch.Draw(MainGame.Instance.Textures["pulse"],
                    Position + Origin,
                    null,
                    Color.White,
                    Rotation,
                    Origin,
                    m_PulsateScale,
                    SpriteEffects.None,
                    0f);
            }

            base.Draw(pSpriteBatch);
        }
        #endregion
    }
}
