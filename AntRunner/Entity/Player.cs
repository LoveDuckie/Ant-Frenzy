using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage;

// Required includes.
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

using AntRunner.Utility;
using AntRunner.Cameras;
using AntRunner.Entity;

namespace AntRunner.Entity
{
    public class Player : Character
    {
        #region Debug Values
        BasicEffect m_WireframeEffect;
        #endregion

        // Constants that are to be used for moving
        #region Constants
        private const float MOVEMENT_SPEED = 5.0f;
        public const float SUCK_DISTANCE = 10.0f;
        #endregion

        #region Members
        private int m_PlayerIndex;

        // Used for displaying the likes of multi kills and what not and the appropriate
        // combo that has been achieved.
        private int m_KillCounter = 0;
        private float m_TimeSinceLastKill = 0f;
        private const float MULTI_KILL_TIME_THRESHOLD = 1500f;

        private Texture2D m_CharacterImage;

        private List<Weapon> m_Weapons;

        private SoundEffect m_CollectSound;

        // Event handlers for dealing with various events within the game.
        public EventHandler OnKilledAnt = delegate(object sender, EventArgs e) {  };
        public EventHandler OnCollectedItem;
        public EventHandler OnDestroyedBox;

        private int m_WeaponActive;
        #endregion

        #region Properties
        public int WeaponActive
        {
            get { return m_WeaponActive; }
            set { m_WeaponActive = value; }
        }

        public List<Weapon> Weapons
        {
            get { return m_Weapons; }
            set { m_Weapons = value; }
        }
        #endregion

        #region Constructors
        public Player()
        {
            
            Initialize();

            // Determine first that the shotgun fire is valid
            if (MainGame.Instance.Textures["shotgun_fire"] != null)
            {
                m_CharacterImage = MainGame.Instance.Textures["shotgun_fire"];
            }

            this.m_CollectSound = MainGame.Instance.Sounds["pickup2"];
        }

        public string CurrentWeapon
        {
            get { return m_Weapons[m_WeaponActive].m_WeaponName.ToString(); }
        }

        public int Ammo
        {
            get
            {
                if (m_Weapons != null)
                {
                    return m_Weapons[m_WeaponActive].m_Ammo;
                }
                else
                {
                    return 0;
                }
            }
        }

        public int MaxAmmo
        {
            get
            {
                // Make sure that the weapons list is there.
                if (m_Weapons != null)
                {
                    return m_Weapons[m_WeaponActive].m_MaxAmmo;
                }
                else
                {
                    return 0;
                }
            }
        }

        #region Events
        public void Event_OnKilledAnt(object sender, EventArgs e)
        {
            if (Environment.TickCount > (m_TimeSinceLastKill + MULTI_KILL_TIME_THRESHOLD))
            {
                m_KillCounter = 0;    
            }
            else
            {
                m_KillCounter++;
            }

            m_TimeSinceLastKill = Environment.TickCount;
            
            string _message = "";
            Color _textcolor = Color.White;

            // Dependant on the state of the killing spree, we will do something
            switch (m_KillCounter)
            {
                case 0:
                    _textcolor = Color.White;
                    _message = "KILLED ANT!";
                break;

                case 1:
                    _textcolor = Color.Green;
                    _message = "DOUBLE KILL!";
                break;

                case 2:
                    _textcolor = Color.Yellow;
                    _message = "MULTI KILL!";
                break;

                case 3:
                    _textcolor = Color.Red;
                    _message = "ULTRA KILL!";
                break;

                case 4:
                    _textcolor = Color.DarkRed;
                    _message = "MONSTER KILL!";
                break;

                default:
                    _textcolor = Color.DarkRed;
                    _message = "GOD LIKE!";
                break;
            }

            NotificationText.Entities.Add(new NotificationText(true, _message, ((Ant)sender).Position, true, _textcolor, true));
        }

        public void Event_OnDestroyedBox(object sender, EventArgs e)
        {
            NotificationText.Entities.Add(new NotificationText(false,
                                                               "DESTROYED BOX", 
                                                               ((Box)sender).Position,
                                                               true,
                                                               Color.White,
                                                               true));
        }

        public void Event_OnCollectedItem(object sender, EventArgs e)
        {
            // Do something when something is colleted
        }
        #endregion

        public Player(Texture2D pSpriteImage, Vector2 pPosition, float pScale, float pRotation)
        {
            this.m_SpriteSheet = pSpriteImage;
            this.Rotation = pRotation;
            this.Scale = pScale;
            this.m_Size = new Point(m_CharacterImage.Width, m_CharacterImage.Height);
            Initialize();
        }
        
        public Player(int pPlayerIndex) : base()
        {
            m_PlayerIndex = pPlayerIndex;
            this.m_CharacterImage = MainGame.Instance.Textures["shotgun_fire"];
            this.m_Position = new Vector2(250, 250);
            this.Scale = 1.0f;
            this.m_Origin = new Vector2(this.m_CharacterImage.Width / 2, this.m_CharacterImage.Height / 2);
            this.m_Size = new Point(m_CharacterImage.Width, m_CharacterImage.Height);
            
            Initialize();
        }
        #endregion

        public override void Initialize()
        {
            // Initialize the shader effect that we are going to use to the wireframe around the bounding box.
            m_WireframeEffect = new BasicEffect(MainGame.Instance.GraphicsDevice);
            m_WireframeEffect.DiffuseColor = Color.White.ToVector3();
            m_Mass = 1.0f;
            m_Weapons = new List<Weapon>();

            // Add the weapons to the list
            m_Weapons.Add(new Handgun(150,this));
            m_Weapons.Add(new AssaultRifle());

            this.Health = 50;
            this.MaxHealth = 100;
            //this.Origin = new Vector2(m_CharacterImage.Width / 2, m_CharacterImage.Height / 2);

            base.Initialize();
        }

        public void KilledAnt()
        {

        }

        /// <summary>
        /// Replenish the ammo of the player by a certain amount
        /// </summary>
        /// <param name="pAmount">The ammount of oammo that we ant to add</param>
        public void AddAmmo(int pAmount)
        {
            // Add ammo onto the weapon that is active at the moment
            if ((this.m_Weapons[m_WeaponActive].m_Ammo + pAmount) > this.m_Weapons[m_WeaponActive].m_MaxAmmo)
            {
                this.m_Weapons[m_WeaponActive].m_Ammo = m_Weapons[m_WeaponActive].m_MaxAmmo;
            }
            else
            {
                this.m_Weapons[m_WeaponActive].m_Ammo += pAmount;
            }
        }

        /// <summary>
        /// Add health onto the player or the amount that is necessary only.
        /// </summary>
        /// <param name="pAmount">The amount that is to be added.</param>
        public void AddHealth(int pAmount)
        {
            if ((this.Health + pAmount) > MaxHealth)
            {
                this.Health = MaxHealth;
            }
            else
            {
                this.Health += pAmount;
            }
        }

        // Additional extensions to the update method.
        public void Update(GameTime pGameTime, Level pLevel, InputHandler pInputHandler, CameraManager pCamera)
        {
            Vector2 currentPosition = m_Position;
            // For some reason I have to invert the view matrix before transforming
            // the position of the mouse
            Matrix inverseMatrix = Matrix.Invert(pCamera.ActiveCamera().GetMatrix());
            Vector2 mousePosition = Vector2.Transform(pInputHandler.GetMouse(), inverseMatrix);

            var _currentdirection = mousePosition - m_Position;
            _currentdirection.Normalize();

            // Retreive the angle in which the bullet is going to be firing.
            m_Rotation = (float)Math.Atan2((double)_currentdirection.Y, (double)_currentdirection.X);
            
            // Determine if the mouse button has been pressed.
            if (pInputHandler.CurrentMouseState.LeftButton == ButtonState.Pressed)
            {
                m_Weapons[m_WeaponActive].Fire(new Vector2(Mouse.GetState().X, Mouse.GetState().Y), m_Rotation);
            }

            #region Input Handling
            /** For sending the player in the appropriate direction **/
            if (pInputHandler.IsKeyDown(Keys.W))
            {
                //Position -= new Vector2(0, MOVEMENT_SPEED);
                currentPosition -= new Vector2(0, MOVEMENT_SPEED);
            }

            if (pInputHandler.IsKeyDown(Keys.S))
            {
               // Position += new Vector2(0, MOVEMENT_SPEED);
                currentPosition += new Vector2(0, MOVEMENT_SPEED);
            }

            if (pInputHandler.IsKeyDown(Keys.A))
            {
                //Position -= new Vector2(MOVEMENT_SPEED, 0);
                currentPosition -= new Vector2(MOVEMENT_SPEED, 0);
            }

            if (pInputHandler.IsKeyDown(Keys.D))
            {
                //Position += new Vector2(MOVEMENT_SPEED, 0);
                currentPosition += new Vector2(MOVEMENT_SPEED, 0);
            }
            #endregion

            // Grab all the transformed vertices to be used.
            List<Vector2> _getaxes = GetAxes(false,false);

            // Loop through the axes and check for a collision
            for (int i = 0; i < _getaxes.Count; i++)
            {
                // TODO: Perform SAT collision here
            }

            // Determine if the new co-ordinates are causing problems or not.
            if (currentPosition != Position &&
               !pLevel.CheckCollision(currentPosition, new Rectangle((int)currentPosition.X - m_CharacterImage.Width / 2,
                                                                     (int)Position.Y - m_CharacterImage.Height / 2,
                                                                     m_CharacterImage.Width, 
                                                                     m_CharacterImage.Height)) &&
                !CollideWithEntities(new Rectangle((int)currentPosition.X - m_CharacterImage.Width / 2,
                                                  (int)Position.Y - m_CharacterImage.Height / 2,
                                                  m_CharacterImage.Width,
                                                  m_CharacterImage.Height)))
            {
                // Determine the position that we are going to be using.
                Position = new Vector2(currentPosition.X, Position.Y);
            }

            // Make sure that the Y co-ordinates are OK to be used.
            if (currentPosition != Position &&
                !pLevel.CheckCollision(currentPosition, new Rectangle((int) Position.X       - m_CharacterImage.Width / 2,
                                                                      (int)currentPosition.Y - m_CharacterImage.Height/ 2,
                                                                      m_CharacterImage.Width,
                                                                      m_CharacterImage.Height)) &&
                !CollideWithEntities(new Rectangle((int) Position.X - m_CharacterImage.Width / 2,
                                                  (int) currentPosition.Y - m_CharacterImage.Height / 2,
                                                  m_CharacterImage.Width,
                                                  m_CharacterImage.Height)))
            {
                // Load the position in once we check that it's not colliding
                Position = new Vector2(Position.X, currentPosition.Y);
            }

            // Create an update of the bounding box.
            m_BoundingBox = new Rectangle((int)m_Position.X, 
                                          (int)m_Position.Y, 
                                          m_CharacterImage.Width, 
                                          m_CharacterImage.Height);

            CollisionBox = new BoundingBox(new Vector3(new Vector2(Position.X - (Size.X / 2), 
                                                                   Position.Y - (Size.Y / 2)), 0),
                                                       new Vector3(m_Position.X + Origin.X,
                                                                   m_Position.Y + Origin.Y, 0)); // Update the collision box that is going to be used
            

            // Loop through the weapons and then update them (for semi-automatic functionality).
            foreach (var item in m_Weapons)
            {
                item.Update(pGameTime, pInputHandler);
            }

            Update(pGameTime, pInputHandler, pLevel);
        }
        
        public void Update(GameTime pGameTime, Level pLevel, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {


            if (m_CharacterImage != null)
            {
                // Output the image to the screen

               // pSpriteBatch.Draw(m_CharacterImage, m_Position, Color.White);
                pSpriteBatch.Draw(m_CharacterImage, m_Position, new Rectangle(0,0,m_CharacterImage.Width,m_CharacterImage.Height), Color.White, m_Rotation, new Vector2(m_CharacterImage.Width / 2, m_CharacterImage.Height / 2), 1.0f, SpriteEffects.None, 0);
            }


            // Output the character to the screen
            //pSpriteBatch.Draw(m_SpriteSheet, m_Position, new Rectangle(m_FrameIndex.X * m_FrameSize.X,
            //                                                            m_FrameIndex.Y * m_FrameSize.Y,
            //                                                            m_FrameSize.X,
            //                                                            m_FrameSize.Y), Color.White,this.m_Rotation,this.m_Origin, this.m_Scale,
            //                                                            this.m_Direction == Direction.Left ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);

            base.Draw(pSpriteBatch);
        }


    }
}
