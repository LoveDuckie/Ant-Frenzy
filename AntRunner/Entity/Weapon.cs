using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace AntRunner.Entity
{
    public class Weapon : IEntity
    {
        #region Members
        private Player m_Owner;

        public string m_WeaponName;
        public int m_Ammo;
        public int m_MaxAmmo;
        public int m_ClipSize;
        public bool m_Reloading; // Active when the player is currently reloading their weapon
        protected float m_ReloadingTime;
        protected float m_ReloadingCounter;
        protected SoundEffect m_ShootSound;

        protected Random m_Random;

        protected float m_ShootRadius;
        #endregion

        #region Properties
        public Player Owner
        {
            get { return m_Owner; }
            set { m_Owner = value; }
        }
        #endregion

        #region Constructors
        public Weapon()
        {
            m_Random = new Random();
            m_ShootSound = MainGame.Instance.Sounds["shoot_sound"];
        }
        #endregion

        #region Methods

        // Fire in the specific direction
        public virtual void Fire(Vector2 pDirection, float pRotation)
        {
           
        }

        public virtual void Reload()
        {
            
            m_Reloading = true;
        }

        public virtual void Initialize()
        {
        
        }

        /// <summary>
        /// Update function with some additional parameters.
        /// </summary>
        /// <param name="pGameTime"></param>
        /// <param name="pInputHandler"></param>
        /// <param name="pLevel"></param>
        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
        }
        #endregion
    }
}
