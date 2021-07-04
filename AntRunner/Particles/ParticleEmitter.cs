using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Windows.Storage;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;

namespace AntRunner.Particles
{
    public class ParticleItem : IEntity
    {
        #region Members
        protected float m_LifeTime;
        protected Color m_BlendColor;

        public const int AlphaBlendDrawOrder = 100;
        public const int AdditiveDrawOrder = 200;

        // The center of the sprite that will be drawn to the screen
        protected Vector2 m_Origin;
        protected Vector2 m_Position;
        protected Vector2 m_Velocity;
        protected Vector2 m_Acceleration;
        protected float m_Rotation;
        protected float m_RotationSpeed;
        protected float m_Scale;
        protected bool m_Active;
        protected float m_TimeSinceStart;
        protected Texture2D m_Sprite;
        #endregion

        #region Properties
        public Vector2 Origin
        {
            get { return m_Origin; }
            set { m_Origin = value; }
        }

        public Texture2D Sprite
        {
            get { return m_Sprite; }
            set { m_Sprite = value; }
        }

        public float TimeSinceStart
        {
            get { return m_TimeSinceStart; }
            set { m_TimeSinceStart = value; }
        }

        public float RotationSpeed
        {
            get { return m_RotationSpeed; }
            set { m_RotationSpeed = value; }
        }

        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public float Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }

        public Vector2 Acceleration
        {
            get { return m_Acceleration; }
            set { m_Acceleration = value; }
        }

        public Vector2 Velocity
        {
            get { return m_Velocity; }
            set { m_Velocity = value; }
        }

        public Color BlendColor
        {
            get { return m_BlendColor; }
            set { m_BlendColor = value; }
        }

        public float Scale
        {
            get { return m_Scale; }
            set { m_Scale = value; }
        }

        public float LifeTime
        {
            get { return m_LifeTime; }
            set { m_LifeTime = value; }
        }

        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; }
        }
        #endregion

        #region Constructors
        public ParticleItem()
        {

        }
        #endregion

        #region Methods
        public virtual void Initialize(Vector2 pPosition, Vector2 pVelocity, Vector2 pAcceleration,
            float pLifetime, float pScale, float pRotationSpeed)
        {
            this.m_Position = pPosition;
            this.m_Scale = pScale;
            this.m_Rotation = pRotationSpeed;
            this.m_Acceleration = pAcceleration;
            this.m_LifeTime = pLifetime;
            this.m_Velocity = pVelocity;
        }

        public virtual void Initialize()
        {

        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Update the particle effects.
            Update((float)pGameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public void Update(float pDeltaTime)
        {
            Velocity += Acceleration * pDeltaTime;
            Position += Velocity * pDeltaTime;
            Rotation += RotationSpeed * pDeltaTime;
            TimeSinceStart += pDeltaTime;
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {

        }
        #endregion

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
        
        }
    }

    public abstract class ParticleEmitter : IEntity
    {
        // List of emitters that will go off in the game.
        private static List<ParticleEmitter> m_Emitters = new List<ParticleEmitter>();

        #region Properties
        public static List<ParticleEmitter> Emitters
        {
            get { return ParticleEmitter.m_Emitters; }
            set { ParticleEmitter.m_Emitters = value; }
        }

        public bool IsActive
        {
            get { return m_IsActive; }
            set { m_IsActive = value; }
        }
        #endregion

        #region Members
        /** Members **/
        protected ParticleItem[]      m_Particles;
        protected Queue<ParticleItem> m_FreeParticles;
        protected Random              m_Random;

        protected BlendState m_BlendState;

        protected float m_MinLifeTime;
        protected float m_MaxLifeTime;

        // The min and max angle for the direction in which the particles will generate.
        protected float m_MinAngleDirection;
        protected float m_MaxAngleDirection;

        // The size of which it'll grow towards
        protected float m_MinScale;
        protected float m_MaxScale;

        // The rate at which it'll increase its speed from creation
        protected float m_MinAcceleration;
        protected float m_MaxAcceleration;

        protected int m_EffectsCount;

        // The speed at which it will rotate at.
        protected float m_MinRotationSpeed;
        protected float m_MaxRotationSpeed;

        // The speed at which the particle starts out at within it's life.
        protected float m_MinInitialSpeed;
        protected float m_MaxInitialSpeed;

        // The minimum and maximum amount of particles that are to be on the screen
        protected int m_MinNumberParticles;
        protected int m_MaxNumberParticles;

        // Apply some kind of gravitational force?
        protected bool m_ApplyGravity;
        protected bool m_Upwards;
        protected bool m_IsActive;

        // The sprite that is going to be used with the particles emitted.
        protected Texture2D m_TemplateSprite;

        // The point at which the emitter starts at.
        protected Vector2 m_Origin;        
        #endregion
        
        /// <summary>
        /// Create the particle emitter that is going to be used for drawing the particles
        /// </summary>
        /// <param name="pHowManyEffects">How many particle effects do we ultimately want in the scene</param>
        /// <param name="pSprite">The sprite that is going to be used to demonstrate the particle</param>
        public ParticleEmitter(int pHowManyEffects, Texture2D pSprite)
        {
            m_EffectsCount = pHowManyEffects;
            m_TemplateSprite = pSprite;
            m_ApplyGravity = false;
            m_Upwards = false;
            m_Random = new Random();
        }

        /// <summary>
        /// Do initialisations of objects and other required things for emitter here.
        /// </summary>
        public virtual void Initialize()
        {
            // Set all the required values.
            InitializeComponent();
            
            // Generate the array of particles that are going to be displayed at any one time
            m_Particles = new ParticleItem[m_EffectsCount * m_MaxNumberParticles];
            m_FreeParticles = new Queue<ParticleItem>(m_EffectsCount * m_MaxNumberParticles);
            
            for (int i = 0; i < m_Particles.Length; i++)
            {
                m_Particles[i] = new ParticleItem();
                m_FreeParticles.Enqueue(m_Particles[i]);
            }
        }

        /// <summary>
        /// Called when the emitter is made for the first time.
        /// </summary>
        public abstract void InitializeComponent();

        /// <summary>
        /// The method that is used for creating a new particle item that is used for added
        /// another particle to the visuals
        /// </summary>
        /// <param name="pItem">The particle item that is to be used</param>
        /// <param name="pPosition">The position in which it'll start</param>
        public virtual void CreateParticle(ParticleItem pItem, Vector2 pPosition)
        {
            Vector2 _direction = GenerateRandomDirection();

            float _velocity = PickRandom(m_MinInitialSpeed, m_MaxInitialSpeed);
            float _rotation = PickRandom(m_MinRotationSpeed, m_MaxRotationSpeed);
            float _lifetime = PickRandom(m_MinLifeTime, m_MaxLifeTime);
            float _scale = PickRandom(m_MinScale, m_MaxScale);
            float _acceleration = PickRandom(m_MinAcceleration,m_MaxAcceleration);
            
            // TODO: Make it so that it limits the axes that it generates on
            // Horizontal and vertical.

            // Load in the particle effect.
            pItem.Initialize(pPosition, _velocity * _direction, _acceleration * _direction,
                _lifetime, _scale, _rotation);
        }

        /// <summary>
        /// Generate a random vector2 that will be used for accelerating the particle in a 
        /// certain direction
        /// </summary>
        /// <param name="pMin">The minimum of the range in which the random will generate</param>
        /// <param name="pMax">The max in the range that the random number will generate</param>
        /// <returns></returns>
        protected virtual Vector2 GenerateRandomDirection()
        {
            float _angle = PickRandom(0, MathHelper.TwoPi);
            return new Vector2((float)Math.Cos(_angle), (float)Math.Sin(_angle));
        }

        protected float PickRandom(float pMin, float pMax)
        {
            // Return a random number that is amplified by the max number
            return pMin + (float)m_Random.NextDouble() * (pMax - pMin);
        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            float _deltatime = (float)pGameTime.ElapsedGameTime.TotalSeconds;

            // Loop through the particles and update them so that they continue behaving as they should
            foreach (var item in m_Particles)
            {
                if (item.Active)
                {
                    item.Update(_deltatime);

                    if (!item.Active)
                    {
                        //m_Particles.Enqueue(item);
                    }
                }
            }
        }

        public virtual void AddParticles(Vector2 pPosition)
        {
            int _numberOfParticles = m_Random.Next(m_MinNumberParticles, m_MaxNumberParticles);
            
            // Loop through and generate the appropriate amount of particles
            for (int i = 0; i < _numberOfParticles; i++)
            {
                ParticleItem p = m_FreeParticles.Dequeue();
            }
        }

        public virtual void Draw(SpriteBatch pSpriteBatch)
        {
            // Loop through the particles and draw them to the screen
            foreach (var item in m_Particles)
            {
                // If the particle is no longer active, then don't bother rendering it
                if (!item.Active)
                    continue;

                // This is a value anywhere between 0 and 1 and represents how far it is in its life.
                float _normalizedLifetime = item.TimeSinceStart / item.LifeTime;
                float _alpha = 4 * _normalizedLifetime * (1 - _normalizedLifetime);
                Color _color = Color.White * _alpha;
                
                float _scale = item.Scale * (0.75f + (0.25f * _normalizedLifetime));


                pSpriteBatch.Draw(item.Sprite, 
                                  item.Position, 
                                  null, _color, 
                                  item.Rotation, 
                                  item.Origin ,_scale, SpriteEffects.None, 0.0f);
            }
        }


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
