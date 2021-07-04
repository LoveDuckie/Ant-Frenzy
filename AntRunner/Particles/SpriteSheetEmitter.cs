using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.Particles
{
    /// <summary>
    /// Responsible for extracting only part of a spritesheet that is required
    /// to give off that effect that the item in question is breaking
    /// </summary>
    public class SpriteSheetParticleItem : ParticleItem
    {
        #region Members
        private Point m_FrameIndex = Point.Zero;
        private Point m_FrameSize = Point.Zero;
        #endregion

        #region Properties
        public Point FrameIndex
        {
            get { return m_FrameIndex; }
            set { m_FrameIndex = value; }
        }

        public Point FrameSize
        {
            get { return m_FrameSize; }
            set { m_FrameSize = value; }
        }
        #endregion

        public SpriteSheetParticleItem()
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Initialize(Vector2 pPosition, Vector2 pVelocity, Vector2 pAcceleration, float pLifetime, float pScale, float pRotationSpeed)
        {
            base.Initialize(pPosition, pVelocity, pAcceleration, pLifetime, pScale, pRotationSpeed);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

    }

    public class SpriteSheetEmitter : ParticleEmitter
    {
        #region Constructors
        private Point m_SpriteSheetPosition;
        private Point m_Size;
        
        /// <summary>
        /// The primary constructor that is used for this
        /// </summary>
        /// <param name="pHowManyEffects">How many particles do we want to appear exactly?</param>
        /// <param name="pSpriteSheet">The sprite sheet that we're going to be extracting from</param>
        /// <param name="pParticleSize">By how much are we going to split up the particle area for particles?</param>
        /// <param name="pTextureArea">The part of the spritesheet that we're actually concerned about.</param>
        public SpriteSheetEmitter(int pHowManyEffects, 
                                  Texture2D pSpriteSheet, 
                                  Point pParticleSize,
                                  Point pTextureArea) :
               base(pHowManyEffects,pSpriteSheet)
        {
            this.m_TemplateSprite = pSpriteSheet;
            m_SpriteSheetPosition = pTextureArea;
            m_Size = pParticleSize;
        }
        #endregion

        /// <summary>
        /// Called when we are getting  rid of the object
        /// </summary>
        ~SpriteSheetEmitter()
        {

        }

        /// <summary>
        /// Create a new particle item
        /// </summary>
        /// <param name="pItem">The particle object that we are going to position</param>
        /// <param name="pPosition">The point in the game world that we are going to place</param>
        public override void CreateParticle(ParticleItem pItem, Vector2 pPosition)
        {
            Vector2 _direction = GenerateRandomDirection();

            // The values that are required for generated the particle item that is 
            // in question
            float _velocity = PickRandom(m_MinInitialSpeed, m_MaxInitialSpeed);
            float _acceleration = PickRandom(m_MinAcceleration, m_MaxAcceleration);
            float _lifetime = PickRandom(m_MinLifeTime, m_MaxLifeTime);
            float _scale = PickRandom(m_MinScale, m_MaxScale);
            float _inertia = PickRandom(m_MinRotationSpeed, m_MaxRotationSpeed);

            pItem.Initialize(pPosition, 
                            _velocity * _direction, 
                            _acceleration * _direction, 
                            _lifetime, 
                            _scale, 
                            _inertia);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void InitializeComponent()
        {
            
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            // Loop through the particles that are in state
            foreach (var item in m_Particles)
            {
                if (!item.Active)
                    continue;

                // Put the life time between the values of 0 and 1
                float _normalizedLifetime;

                // Render the particle sprite to the screen
                //pSpriteBatch.Draw(m_TemplateSprite,
                //    new Rectangle(((SpriteSheetParticleItem)item).FrameIndex.X * 

            }

            //base.Draw(pSpriteBatch);

            pSpriteBatch.End();
        }

        /// <summary>
        /// Add particles to the queue again presuming that there are 
        /// some free
        /// </summary>
        /// <param name="pPosition">The point within the environment htat we are going
        /// to add the particles.</param>
        public override void AddParticles(Vector2 pPosition)
        {
            base.AddParticles(pPosition);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }
    }
}
