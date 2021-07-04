using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Menu;
using AntRunner.Particles;
using AntRunner.Tower;
using AntRunner.Utility;

namespace AntRunner.Entity
{
    /// <summary>
    /// This ant will employ the ThetaStarPath method for finding the shortest path.
    /// </summary>
    public class RedAnt : Ant
    {
        #region Members
        private ThetaStarPath m_Pathfinding = null;
        private int m_ReplanTimeBegin = 0;
        private int m_ReplanTimeEnd = 0;
        private int m_TotalMillisecondsTaken = 0;

        // Nodes that are going to be covered.
        private Point[] m_RaytracedNodes = new Point[] {};

        // Ant dies when it arrives at the end of its path
        private bool m_DeadOnArrival = false;

        private MovementDirection m_LeftRight;
        private MovementDirection m_UpDown;

        public BoundingBox m_CheckAheadBox = new BoundingBox();

        private bool m_FollowingPath = false;
        #endregion

        #region Constructors
        public RedAnt() : base()
        {
            
        }

        public RedAnt(Vector2 pPosition)
        {
            this.Position = pPosition;
            this.m_SpawnFrom = null;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            this.m_CheckRadius = 10f;
            this.m_ChaseRadius = 20f;
        }

        public RedAnt(Vector2 pPosition, SpawnPoint pSpawnPoint)
            : base(pPosition, pSpawnPoint)
        {
            OnPathReplan = Method_PathReplan;
            this.m_FocusCake = null;

            this.m_CurrentState = "MoveToPath";

            #region State Initialization
            this.m_States = new Dictionary<string, FiniteState>();
            m_States.Add("Attacking", new FiniteState()
            {
                m_Action = Attacking,
                m_OnBegin = OnBegin_Attacking,
                m_OnSuspend = OnSuspend_Attacking,
                m_StateName = "Attacking"
            });
            m_States.Add("MoveToPath", new FiniteState()
            {
                m_StateName = "MoveToPath",
                m_Action = MoveToPath,
                m_OnBegin = OnBegin_MoveToPath,
                m_OnSuspend = OnSuspend_MoveToPath
            });
            m_States.Add("Wandering", new FiniteState()
            {
                m_Action = Wandering,
                m_StateName = "Wandering",
                m_OnBegin = OnBegin_Wandering,
                m_OnSuspend = OnSuspend_Wandering
            });
            m_States.Add("Returning", new FiniteState()
            {
                m_Action = Returning,
                m_OnSuspend = OnSuspend_Returning,
                m_OnBegin = OnBegin_Returning,
                m_StateName = "Returning"
            });
            #endregion

            this.m_FrameSize = new Point(56, 30);
            this.Size = new Point(56, 30);
            this.Origin = new Vector2(Size.X / 2, Size.Y / 2);

            this.m_MovementDirection = MovementDirection.None;

            this.m_UpDown = MovementDirection.None;
            this.m_LeftRight = MovementDirection.None;

            this.m_MovementSpeed = 5.0f;

            // Set the sprite sheet
            m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_red"];
            m_FrameIndex = new Point(0, 0);
            this.Position = pPosition;
            this.m_SpawnFrom = pSpawnPoint;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            // Values used in checking how adjacent the player is to this entity.
            this.m_CheckRadius = 1.5f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;


            m_DisplayPhysicsDebug = false;
            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;

            this.Mass = 15f;
   
            m_CanAttackCounter = 0f;
            m_CanAttack = true;

            // This box will be used for determining whether or not there is something in the way of our traversal.
            this.m_CheckAheadBox = new BoundingBox(new Vector3(this.Position.X + 25, Position.Y, 0),
                                                   new Vector3(this.Position.X + 50, Position.Y + Size.Y, 0));


            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
        }
        #endregion

        public void Method_PathBegin(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void Method_PathReplan(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        #region OnBegin Methods
        public void OnBegin_Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Called when
        }

        public void OnBegin_Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Called when the wandering state is done for the first time
        }

        public void OnBegin_MoveToPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void OnBegin_Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }
        #endregion

        #region OnSuspend Methods
        public void OnSuspend_Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void OnSuspend_MoveToPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void OnSuspend_Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void OnSuspend_Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }
        #endregion

        #region State Methods
        public void Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        public void MoveToPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        #endregion

        public override void Initialize()
        {
            // Set the default values that are going to be used
            m_DisplayNotificationTimer = 0f;
            m_DisplayNotification = false;
            m_DisplayNotificationDuration = 1500f;

            m_PathIndex = 0;

            this.m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_red"];

            this.m_FrameIndex = ANT_SPRITES[m_Random.Next(0, this.ANT_SPRITES.Length)];
            this.m_FrameSize = new Point(56, 30);
            this.Scale = 1f;
            this.Rotation = 0f;

            this.Forces.Add("Movement", Vector2.Zero);
            this.Forces.Add("Angular", Vector2.Zero);
            this.Forces.Add("Steering", Vector2.Zero);
            this.Forces.Add("Collision", Vector2.Zero);
            this.Forces.Add("Gravity", Vector2.Zero);

            m_CheckRadius = 1.5f;

            this.m_CanAttackCounter = 0f;
            this.m_CanAttack = true;

            this.m_TraversalTimeBegin = Environment.TickCount;
        }

        /// <summary>
        /// For updating the box that will be used for determining whether or not there is a box in-front.
        /// </summary>
        public void UpdateCheckAheadBox()
        {
            // Update the for vectors that make up for the check ahead box.
            Vector2 _v0 = MathUtilities.RotateVector(Position,Rotation,Position + new Vector2(50,0));
            
            // Generate the new checkahead box used for collision detection
            this.m_CheckAheadBox = new BoundingBox(new Vector3(_v0.X, _v0.Y, 0),
                                                   new Vector3(_v0.X + 64, _v0.Y + 64, 0));
            
        }

        public override bool CollideWithEntities(Rectangle pBoundingBox)
        {
            return base.CollideWithEntities(pBoundingBox);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            // Store a local copy of it.
            Vector2 _currentposition = Position;
            Vector2 _nextNode = Vector2.Zero;

            this.WorldOrigin = Position + Origin;

            if (AlignedBox != null)
            {
                this.AlignedBox.Update(pGameTime, pInputHandler);
            }

            if (m_Pathfinding != null)
            {
                if (m_Pathfinding.Path.Count > m_PathIndex)
                {
                    _nextNode = new Vector2(m_Pathfinding.Path[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                            m_Pathfinding.Path[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32);
                }
            }
            this.m_FrameCounter += (int)pGameTime.ElapsedGameTime.TotalMilliseconds;

            if (m_FocusCake == null)
            {
                // Grab the nearest cake in the environment.
                m_FocusCake = pLevel.GetNearestCake(Position);
            }

            #region Collision Box Updating
            this.CollisionBox = new BoundingBox(new Vector3(Position - Origin, 0),
                                                new Vector3(Position.X + Origin.X,
                                                            Position.Y + Origin.Y, 0));

            // Update them both at the same time
            this.BoundingBox = new Rectangle((int)Position.X - (int)Origin.X,
                                             (int)Position.Y - (int)Origin.Y,
                                             m_FrameSize.X - 2,
                                             m_FrameSize.Y - 2);

            // Update the check ahead box so we can tell whether or not there is an object in front of us.
            UpdateCheckAheadBox();
            #endregion

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

            // If the path object hasn't been generated then do something about it
            if (m_Pathfinding != null)
            {
                if (m_Pathfinding.Path.Count > 0)
                {
                    if (m_PathIndex < m_Pathfinding.Path.Count)
                    {
                        this.Rotation = RotateTo(new Vector2(m_Pathfinding.Path[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                                 m_Pathfinding.Path[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32));
                        _currentposition += MoveTo(Rotation, m_MovementSpeed);

                        if (m_LeftRight != MovementDirection.None || m_UpDown != MovementDirection.None)
                        {
                            _currentposition = Position;
                        }

                        if (m_LeftRight == MovementDirection.Left)
                        {
                            _currentposition += new Vector2(-m_MovementSpeed, 0);

                        }
                        else if (m_LeftRight == MovementDirection.Right)
                        {
                            _currentposition += new Vector2(m_MovementSpeed, 0);
                        }

                        if (m_UpDown == MovementDirection.Up)
                        {
                            _currentposition += new Vector2(0, -m_MovementSpeed);
                        }
                        else if (m_UpDown == MovementDirection.Down)
                        {
                            _currentposition += new Vector2(0, m_MovementSpeed);
                        }

                        // Determine if there is a collision on the X axis
                        if (!pLevel.CheckCollision(this.Position, new Rectangle((int)_currentposition.X - (int)Origin.X,
                                                                                (int)Position.Y - (int)Origin.Y,
                                                                                30,
                                                                                30)))
                        {
                            this.Position = new Vector2(_currentposition.X, Position.Y);
                            m_UpDown = MovementDirection.None;
                        }
                        else
                        {
                            // Used for determining which direction to head in
                            Vector2 _output = _nextNode - Position;

                            if (_output.Y > 0)
                            {
                                this.m_UpDown = MovementDirection.Down;
                            }
                            else // Going Up
                            {
                                this.m_UpDown = MovementDirection.Up;
                            }
                        }

                        // Determine if there is a collision on the Y axis
                        if (!pLevel.CheckCollision(this.Position, new Rectangle((int)Position.X - (int)Origin.X,
                                                                                (int)_currentposition.Y - (int)Origin.Y,
                                                                                30,
                                                                                30)))
                        {
                            // Set the new position
                            this.Position = new Vector2(Position.X, _currentposition.Y);
                            this.m_LeftRight = MovementDirection.None;
                        }
                        else
                        {
                            Vector2 _output = _nextNode - Position;

                            // Going right
                            if (_output.X > 0)
                            {
                                this.m_LeftRight = MovementDirection.Right;
                            }
                            else // Going left
                            {
                                this.m_LeftRight = MovementDirection.Left;
                            }
                        }

                        // Check whether or not the path node is within radius
                        if (WithinRange(new Vector2((m_Pathfinding.Path[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth) + 32,
                                                    (m_Pathfinding.Path[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight) + 32), m_CheckRadius))
                        {
                            m_Pathfinding.Path[m_PathIndex].visited = true;
                            m_PathIndex++;
                        }
                    }
                    else
                    {
                        NotificationText.Notifications.Add(new NotificationText(true, "COMPLETED PATH!", Position, true, Color.White, true));

                        MainGame.Instance.GameState.RedAntTraversalTime = Environment.TickCount - m_TraversalTimeBegin;

                        this.Dead = true;
                    }
                }
            }
            else
            {
                // Create the new pathfinding object that we are going to use.
                m_Pathfinding = new ThetaStarPath(pLevel,this);
                m_Pathfinding.Initialize(new Point((int)Position.X / 64, (int)Position.Y / 64),
                                         new Point((int)m_FocusCake.Position.X / 64, (int)m_FocusCake.Position.Y / 64));
                m_Pathfinding.Replan();
            }
        }

        /// <summary>
        /// Render the output for the check-ahead box that is used to determine if there is a crate in the way
        /// </summary>
        /// <param name="pSpriteBatch">SpriteBatch object used for rendering</param>
        public void DrawCheckAhead(SpriteBatch pSpriteBatch)
        {
            if (m_CheckAheadBox != null)
            {
                DebugDraw.DebugDrawBox(false, m_CheckAheadBox, pSpriteBatch, Color.NavajoWhite);
            }
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            if (Global.DEBUG)
            {
                //ShadowText.Draw(m_UpDown.ToString(),pSpriteBatch,new Vector2(Position.X,Position.Y + Size.Y + 75));
                //ShadowText.Draw(m_LeftRight.ToString(), pSpriteBatch, new Vector2(Position.X, Position.Y + Size.Y + 125));

                DrawCheckAhead(pSpriteBatch);
            }

            if (m_Pathfinding != null)
            {
                m_Pathfinding.DrawPathfinding(pSpriteBatch);
            }

            base.Draw(pSpriteBatch);
        }
    }
}
