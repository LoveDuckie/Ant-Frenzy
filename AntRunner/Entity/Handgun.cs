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
using AntRunner.Tower;
using AntRunner.Utility;

namespace AntRunner.Entity
{
    /// <summary>
    /// The base class that is used for weapons
    /// </summary>
    public class Handgun : Weapon
    {
        // For the semi-auto effect.
        private bool m_HasFired;

        #region Constructors
        public Handgun(int pAmmo, Player pOwner)
        {
            m_MaxAmmo = 150;
            m_Ammo = pAmmo;
            m_ShootRadius = 0.5f;
            Owner = pOwner;
            this.m_WeaponName = "HANDGUN";
        }

        #endregion

        public override void Fire(Vector2 pDirection, float pRotation)
        {
            // Only fire if there is currently enough ammo
            if (!m_HasFired)
            {
                if ((m_Ammo - 1) > 0)
                {
                    m_ShootSound.Play();
                    // Add a new entity and fire it in a give direction
                    Entity.Entities.Add(new Bullet(5.0f, new Vector2((float)Math.Cos(pRotation), (float)Math.Sin(pRotation)) * 20.0f, pRotation, MainGame.Instance.Textures["bullet_single"], Owner.Position, this));

                    // Deduct the count from the ammo.
                    m_Ammo--;
                }
            }
            base.Fire(pDirection,pRotation);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            if (pInputHandler.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                m_HasFired = true;
            }
            else if (pInputHandler.CurrentMouseState.LeftButton == ButtonState.Released)
            {
                m_HasFired = false;
            }

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            base.Draw(pSpriteBatch);
        }
        

    }
}
