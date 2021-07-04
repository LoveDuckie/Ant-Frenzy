using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;

using AntRunner.Utility;

namespace AntRunner.Entity
{
    public class GreenAnt : Ant
    {
        #region Members
        private AStarPathOpt m_Pathfinding = new AStarPathOpt(MainGame.Instance.GameState.Level,null);
        #endregion

        #region Constructors
        public GreenAnt()
            : base()
        {
            this.m_ApplyPhysics = false;
            this.Mass = 15f;
            this.m_CanAttackCounter = 0f;
            this.m_CanAttack = true;
            this.m_PathIndex = 0;

            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
        }

        public GreenAnt(Vector2 pPosition, Level pLevel) 
            : base(pPosition)
        {
            this.m_FocusCake = null;

            this.m_FrameSize = new Point(56, 30);
            this.Size = new Point(56, 30);
            this.m_FrameSpeed = 250;

            this.Origin = new Vector2(Size.X / 2, Size.Y / 2);

            m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_green"];
            m_FrameIndex = new Point(0, 0);

            this.Position = pPosition;
            this.Health = 50;

            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;


            this.m_CheckRadius = 2f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;

            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;

            this.m_CheckRadius = 10f;
            this.m_ChaseRadius = 20f;

            this.Mass = 15f;

            m_CanAttackCounter = 0f;
            m_CanAttack = true;

            this.m_Pathfinding = new AStarPathOpt(pLevel, this);
            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
        }

        public GreenAnt(Vector2 pPosition, SpawnPoint pSpawnPoint)
            : base(pPosition, pSpawnPoint)
        {
            this.m_FocusCake = null;

            this.m_FrameSize = new Point(56, 30);
            this.Size = new Point(56, 30);

            m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_green"];
            m_FrameIndex = new Point(0, 0);

            this.Origin = new Vector2(Size.X / 2, Size.Y / 2);

            this.Position = pPosition;
            this.m_SpawnFrom = pSpawnPoint;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            this.m_CheckRadius = 5f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;

            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;

            this.Mass = 15f;

            m_CanAttackCounter = 0f;
            m_CanAttack = true;
            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
            
        }

        #endregion

        #region Methods
        public override void Initialize()
        {
            // Set the default values that are going to be used
            m_DisplayNotificationTimer = 0f;
            m_DisplayNotification = false;
            m_DisplayNotificationDuration = 1500f;

            m_PathIndex = 0;

            this.m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_green"];

            this.m_FrameIndex = ANT_SPRITES[m_Random.Next(0, this.ANT_SPRITES.Length)];
            this.m_FrameSize = new Point(56, 30);
            
            // Used for calculating the total physics in the environment.
            this.Forces.Add("Movement", Vector2.Zero);
            this.Forces.Add("Angular", Vector2.Zero);
            this.Forces.Add("Steering", Vector2.Zero);
            this.Forces.Add("Collision", Vector2.Zero);
            this.Forces.Add("Gravity", Vector2.Zero);
            this.m_CanAttackCounter = 0f;
            this.m_CanAttack = true;
           // base.Initialize();

            this.m_TraversalTimeBegin = Environment.TickCount;
        }


        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Render the output for the Green Ant as well.
            //pSpriteBatch.Draw(m_SpriteSheet, Position, new Rectangle(m_FrameIndex.X * 64,
            //                                                         m_FrameIndex.Y * 64,
            //                                                         Size.X,
            //                                                         Size.Y), Color.White);


           m_Pathfinding.DrawPathfinding(pSpriteBatch);

            base.Draw(pSpriteBatch);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            this.m_FrameCounter += (float)pGameTime.ElapsedGameTime.TotalMilliseconds;
            Vector2 _currentposition = Position;

            #region Collision Box Updating
            this.CollisionBox = new BoundingBox(new Vector3(Position - Origin, 0),
                                    new Vector3(Position.X + Origin.X,
                                                Position.Y + Origin.Y, 0));

            // Update them both at the same time
            this.BoundingBox = new Rectangle((int)Position.X - (int)Origin.X,
                                             (int)Position.Y - (int)Origin.Y,
                                             m_FrameSize.X - 2,
                                             m_FrameSize.Y - 2);
            #endregion

            // Keep the aligned box updated on each tick
            if (AlignedBox != null)
            {
                AlignedBox.Update(pGameTime, pInputHandler, pLevel);
            }

            // Make sure that there is a cake tha we are looking at first.
            if (m_FocusCake == null)
            {
                this.m_FocusCake = pLevel.GetNearestCake(Position);
            }

            // Determine first that a path was generated appropriately.
            if (m_Pathfinding != null)
            {
                if (m_Pathfinding.Pathlist.Count == 0)
                {
                    // Generate the path between the two points
                    m_Pathfinding.Initialize(new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                       (int)this.Position.Y / pLevel.TMXLevel.TileHeight),
                                             new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth, 
                                                       (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight));
                    m_Pathfinding.Replan();
                }

                // Keep following the generated paths until we get towards the end.
                if (m_PathIndex < m_Pathfinding.Pathlist.Count)
                {
                    if (pLevel.IsObjectAt(m_Pathfinding.Pathlist[m_PathIndex].position.X,
                                          m_Pathfinding.Pathlist[m_PathIndex].position.Y,typeof(NewBox)))
                    {
                        m_Pathfinding.UpdateStart(new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                            (int)this.Position.Y / pLevel.TMXLevel.TileHeight));
                        m_Pathfinding.UpdateGoal(new Point((int)this.m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                                                           (int)this.m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight));
                        m_PathIndex = 0;
                        m_Pathfinding.Replan();
                    }

                    if (m_PathIndex < m_Pathfinding.Pathlist.Count)
                    {
                        Rotation = RotateTo(new Vector2(m_Pathfinding.Pathlist[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                                        m_Pathfinding.Pathlist[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32));
                        _currentposition += MoveTo(Rotation, m_MovementSpeed);

                        // Determine if it's within range.
                        if (WithinRange(new Vector2(m_Pathfinding.Pathlist[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                                    m_Pathfinding.Pathlist[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32), 2))
                        {
                            m_Pathfinding.Pathlist[m_PathIndex].visited = true;
                            m_PathIndex++;
                        }
                    }
                }
                else
                {
                    // Calculate the time that it took
                    MainGame.Instance.GameState.GreenAntTraversalTime = Environment.TickCount - m_TraversalTimeBegin;
                    NotificationText.Notifications.Add(new NotificationText(true, "PATH COMPLETE!", Position, true, Color.White, true));
                    this.Dead = true;
                }
            }
            else
            {
                this.m_Pathfinding = new AStarPathOpt(pLevel, this);

                // Initialize the pathfinding with the start and end.
                m_Pathfinding.Initialize(new Point((int)this.Position.X / 64,
                                                    (int)this.Position.Y / 64),
                                         new Point((int)m_FocusCake.Position.X / 64,
                                                   (int)m_FocusCake.Position.Y / 64));
                m_Pathfinding.Replan();
            }

            #region Animation Handling
            // If the counter has met the speed of the animation frame, then increase the frame index!
            if (m_FrameCounter > m_FrameSpeed)
            {
                // Determine first that we are still within the sprite sheet.
                if (m_FrameIndex.X < 1)
                {
                    m_FrameIndex.X++;
                }
                else
                {
                    m_FrameIndex.X = 0;
                }

                m_FrameCounter = 0;
            }
            #endregion

            #region Collision Checking
            if (!pLevel.CheckCollision(_currentposition,new Rectangle((int)_currentposition.X - (int)Origin.X,
                                                                      (int)Position.Y - (int)Origin.Y,
                                                                      Size.X,
                                                                      Size.Y)))
            {
                this.Position = new Vector2(_currentposition.X,Position.Y);
            }

            if (!pLevel.CheckCollision(_currentposition, new Rectangle((int)Position.X - (int)Origin.X,
                                                                      (int)_currentposition.Y - (int)Origin.Y,
                                                                      Size.X,
                                                                      Size.Y)))
            {
                this.Position = new Vector2(Position.X, _currentposition.Y);
            }
            #endregion

            //base.Update(pGameTime, pInputHandler, pLevel);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        #endregion

        #region State Suspend and Begin
        public override void OnBeginState(string pStateName)
        {
            base.OnBeginState(pStateName);
        }

        public override void OnSuspend(string pStateName)
        {
            switch (pStateName)
            {
                case "Wandering":

                break;
                
                case "Attacking":

                break;
                
                case "MoveToPath":

                break;

                case "Returning":

                break;
            }

            base.OnSuspend(pStateName);
        }
        #endregion

        #region Action States
        protected override void Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Attacking(pLevel, pGameTime, pInputHandler);
        }

        protected override void Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)        {
            base.Wandering(pLevel, pGameTime, pInputHandler);
        }

        protected override void Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Returning(pLevel, pGameTime, pInputHandler);
        }
        #endregion


    }
}
