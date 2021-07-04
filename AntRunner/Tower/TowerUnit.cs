using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using AntRunner.Entity;

namespace AntRunner.Tower
{
    // Basic tower class that is going to be deployable by the player.
    class TowerUnit : Entity.Entity
    {
        // Distance in which the tower can target the enemy.
        protected float m_TargetRadius;
        protected float m_ShootInterval;
        protected Entity.Entity m_CurrentTarget;

        #region Constructors
        public TowerUnit() : base()
        {
            // Default target radius is this
        }

        public TowerUnit(Vector2 pPosition, Texture2D pSpriteBatch)
        {

        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            base.Initialize();
        }

        protected virtual void Shoot()
        {
            // Determine first that the target in question is not null.
            if (m_CurrentTarget != null)
            {

            }
        }

        public bool CanTarget(Entity.Entity pEntityOther)
        {
            float _squaredistance = (float)Math.Sqrt(Math.Pow((this.Origin.X - pEntityOther.Position.X), 2) +
                                                      Math.Pow((this.Origin.Y - pEntityOther.Position.Y), 2));

            return _squaredistance <= Math.Pow(this.m_TargetRadius, 2);
        }
                

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Loop through entities and determine if it's within reach.

            if (m_CurrentTarget == null)
            {
                foreach (var item in Entities)
                {
                    if (CanTarget(item) && m_CurrentTarget == null)
                    {
                        m_CurrentTarget = item;

                    }
                }
            }
            else
            {

            }

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            base.Draw(pSpriteBatch);
        }
        #endregion

    }
}
