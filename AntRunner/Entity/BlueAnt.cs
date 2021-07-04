using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Required libraries for XNA
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

using AntRunner.Utility;
using AntRunner.Entity;
using AntRunner.Particles;
using AntRunner.Tower;
using AntRunner.Cameras;


namespace AntRunner.Entity
{
    /// <summary>
    /// This ant will display the DStarLite pathfinding working within the environment.
    /// </summary>
    public class BlueAnt : Ant
    {
        // Initialize with a null level value for now.
        private DStarLitePath m_Pathfinding = null;

        // Definition for the current task that the ant is going to be paying attention to
        private HTNTask m_CurrentTask = null;

        private int m_ReplanTimeBegin = 0;
        private int m_ReplanTimeEnd = 0;
        private int m_TotalMillisecondsTaken = 0;

        private bool m_FollowingPath = false;

        private List<HTNTask> m_Tasks = new List<HTNTask>();

        private List<Point> m_ImpassableNodes = new List<Point>();


        #region Constructors
        public BlueAnt()
        {

        }

        public BlueAnt(Vector2 pPosition)
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

            m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_blue"];
            m_FrameIndex = new Point(0, 0);

            this.Position = pPosition;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            this.m_CheckRadius = 10f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;
            m_DisplayPhysicsDebug = false;

            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;

            this.m_TraversalTimeBegin = Environment.TickCount;

            this.Mass = 15f;
            this.m_Water = 0;
            m_CanAttackCounter = 0f;
            m_CanAttack = true;
            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y), this);
        }

        /// <summary>
        /// Deploy the ant to a certain position
        /// </summary>
        /// <param name="pPosition">The place that we want to spawn the blue ant at</param>
        /// <param name="pSpawnPoint">The spawn point that the ant is coming from</param>
        public BlueAnt(
            Vector2 pPosition, 
            SpawnPoint pSpawnPoint)
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

            m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_blue"];
            m_FrameIndex = new Point(0, 0);
            this.Position = pPosition;
            this.m_SpawnFrom = pSpawnPoint;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            this.m_CheckRadius = 10f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;

            m_DisplayPhysicsDebug = false;

            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;

            this.Mass = 15f;
            this.m_Water = 0;
            m_CanAttackCounter = 0f;
            m_CanAttack = true;
            this.AlignedBox = new AABB(Position, new Vector2(Position.X + Size.X, Position.Y + Size.Y),this);
        }
        #endregion

        /// <summary>
        /// Called when there is a replan that is done
        /// </summary>
        /// <param name="pLevel">The level that we are responding to</param>
        /// <param name="pGameTime">The delta time object.</param>
        /// <param name="pInputHandler"></param>
        public void Method_PathReplan(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Time how long it takes for the path regeneration to take.
            m_ReplanTimeBegin = Environment.TickCount;
            //m_Pathfinding.Replan();
            m_ReplanTimeEnd = Environment.TickCount;

            // Determine the total milliseconds taken
            m_TotalMillisecondsTaken = (m_ReplanTimeEnd - m_ReplanTimeBegin);
        }

        /// <summary>
        /// For when the path begins for the first time
        /// </summary>
        /// <param name="pLevel">The level that we are interacting with</param>
        /// <param name="pGameTime">The delta time object</param>
        /// <param name="pInputHandler"></param>
        public void Method_PathBegin(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
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
            // Called when the movement a long the path has begun for the first time
            OnPathBegin(pLevel, pGameTime, pInputHandler);
        }

        public void OnBegin_Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            OnPathBegin(pLevel, pGameTime, pInputHandler);
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
        protected override void Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            
        }

        protected override void Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {

        }

        protected override void Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
        
            
            this.Rotation = MathHelper.ToRadians(90f);
            this.Forces["Movement"] += MoveTo(Rotation, m_MovementSpeed); 
        }

        public void MoveToPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Check to see if a new path has to be generated.
            //if (m_Pathfinding.Path.Count == 0)
            //{
            //    // Initialize the pathfinding
            //    m_Pathfinding.Initalize(WorldGridPosition(Position, pLevel.TileSize),
            //                            WorldGridPosition(pLevel.GetRandomCake().Position, pLevel.TileSize),
            //                            pLevel);
            //}

        }

        #endregion

        public override void Initialize()
        {
            // Set the default values that are going to be used
            m_DisplayNotificationTimer = 0f;
            m_DisplayNotification = false;
            m_DisplayNotificationDuration = 1500f;

            m_PathIndex = 0;

            this.m_SpriteSheet = MainGame.Instance.Textures["ant_spritesheet_blue"];

            this.m_FrameIndex = ANT_SPRITES[m_Random.Next(0, this.ANT_SPRITES.Length)];
            this.m_FrameSize = new Point(56, 30);

            this.Forces.Add("Movement", Vector2.Zero);
            this.Forces.Add("Angular", Vector2.Zero);
            this.Forces.Add("Steering", Vector2.Zero);
            this.Forces.Add("Collision", Vector2.Zero);
            this.Forces.Add("Gravity", Vector2.Zero);
            this.m_CanAttackCounter = 0f;
            this.m_CanAttack = true;
            this.m_CurrentState = "MoveToPath";
          
            //base.Initialize();
        }

        public override void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            if (m_Pathfinding != null)
            {
                m_Pathfinding.Draw(pSpriteBatch);
            }
        }

        public void CheckSurrounding()
        {

        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Display the debugging information for the pathfinding.
            if (Global.DEBUG)
            {
                if (m_TotalMillisecondsTaken != 0)
                {
                    ShadowText.Draw(string.Format("{0} ms", m_TotalMillisecondsTaken.ToString()), pSpriteBatch, new Vector2(Position.X, Position.Y - 25));
                }
                DrawPathfinding(pSpriteBatch);
            }

            m_Pathfinding.Draw(pSpriteBatch);
            base.Draw(pSpriteBatch);
        }


        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            Vector2 _currentposition = Position;
            this.m_FrameCounter += (float)pGameTime.ElapsedGameTime.TotalMilliseconds;
            this.WorldOrigin = Position + Origin;

            // Determine first if we need to focus on a cake.
            if (m_FocusCake == null)
            {
                m_FocusCake = pLevel.GetRandomCake();
            }

            this.CollisionBox = new BoundingBox(new Vector3(Position - Origin,0), 
                                                new Vector3(Position.X + Origin.X, 
                                                            Position.Y + Origin.Y,0));

            // Update them both at the same time
            this.BoundingBox = new Rectangle((int)Position.X - (int)Origin.X,
                                             (int)Position.Y - (int)Origin.Y, 
                                             m_FrameSize.X - 2, 
                                             m_FrameSize.Y - 2);

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

            // Update the hurt opacity if the ant has been shot or something or other.
            if (m_HurtRedOpacity < 254)
            {
                m_HurtRedOpacity += 10;
            }

            // Make sure that a task has been assigned first.
            if (m_CurrentTask != null)
            {
                // If the task hasn't been completed, then check it out
                if (!m_CurrentTask.TaskCompleted)
                {
                    m_CurrentTask.ExecuteTask(pGameTime,pInputHandler,pLevel);
                }
            }

            // Determine first that the pathfinding has been made.
            if (m_Pathfinding != null)
            {
                if (m_Pathfinding.Path.Count > 0)
                {
                    if (m_PathIndex < m_Pathfinding.Path.Count)
                    {
                        //// Determine whether there is an object at the given position or whether
                        //// or not the next node is within the bounds of the level
                        //if (pLevel.IsObjectAt(m_Pathfinding.Path[m_PathIndex].position.X,
                        //    m_Pathfinding.Path[m_PathIndex].position.Y, typeof(NewBox)) ||
                        //    !pLevel.WithinBounds(m_Pathfinding.Path[m_PathIndex].position.X,
                        //                         m_Pathfinding.Path[m_PathIndex].position.Y) ||
                        //    !pLevel.IsClear(m_Pathfinding.Path[m_PathIndex].position.X,
                        //                    m_Pathfinding.Path[m_PathIndex].position.Y))
                        //{
                        //    // Inform D* lite that there is a blockage there
                        //    m_Pathfinding.UpdateCell(m_Pathfinding.Path[m_PathIndex].position.X,
                        //                             m_Pathfinding.Path[m_PathIndex].position.Y, -1);

                        //    // Inform D* lite that we want to change the start and end point to our new path
                        //    m_Pathfinding.UpdateStart((int)Position.X / pLevel.TMXLevel.TileWidth,
                        //                              (int)Position.Y / pLevel.TMXLevel.TileHeight);
                        //    m_Pathfinding.UpdateGoal((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                        //                             (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight);
                        //    m_Pathfinding.Replan();
                        //    m_PathIndex = 0;

                        //}

                        // Grab the current position that the ant is on within the map.
                        Point _currentgridposition = new Point((int)Position.X / pLevel.TMXLevel.TileWidth, 
                                                               (int)Position.Y / pLevel.TMXLevel.TileHeight);
                        Point _cakeposition = new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                                                        (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight);

                        // Loop through the 8 neighbouring nodes.
                        for (int x = _currentgridposition.X - 1; x < _currentgridposition.X + 2; x++)
                        {
                            for (int y = _currentgridposition.Y - 1; y < _currentgridposition.Y + 2; y++)
                            {
                                // Determine if we are observing the node that the agent is on
                                if (x == _currentgridposition.X &&
                                    y == _currentgridposition.Y)
                                    continue;

                                // Determine whether the given node is clear, if not then inform the pathfinding
                                if (!pLevel.IsClear(x, y) ||
                                    !WithinLevelBounds(pLevel,new Vector2(x * pLevel.TMXLevel.TileWidth,
                                                                          y * pLevel.TMXLevel.TileHeight)) ||
                                    pLevel.IsObjectOccupying(x,y,typeof(NewBox)))
                                {

                                    // Should really only be calling UpdateStart() rather than update goal but I get some strange
                                    // bug otherwise.
                                    m_Pathfinding.UpdateStart(_currentgridposition.X, _currentgridposition.Y);
                                  
                                    m_Pathfinding.UpdateCell(x, y, -1);

                                 //  m_Pathfinding.UpdateGoal(_cakeposition.X, _cakeposition.Y);
                                    m_Pathfinding.Replan();

                                    m_PathIndex = 1;

                                }
                            }
                        }


                        // this could be better off but considering that 
                        // we're doing on the fly replanning before hand it just seems silly.
                        if (m_Pathfinding.Path.Count > 0)
                        {
                            this.Rotation = RotateTo(new Vector2((m_Pathfinding.Path[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth) + 32,
                                                                    (m_Pathfinding.Path[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight) + 32));
                            _currentposition += MoveTo(Rotation, m_MovementSpeed);

                            // Have we found the next path node?
                            if (WithinRange(new Vector2((m_Pathfinding.Path[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth) + 32,
                                                        (m_Pathfinding.Path[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight) + 32), 2))
                            {
                                m_Pathfinding.Path[m_PathIndex].visited = true;
                                m_PathIndex++;
                            }
                        }

                    }
                    else
                    {
                        // Return the cake that is the furthest away from the ant.
                        //m_FocusCake = pLevel.GetFarthestCake(this.Position);
                        //m_Pathfinding.UpdateStart((int)Position.X / pLevel.TMXLevel.TileWidth, (int)Position.Y / pLevel.TMXLevel.TileHeight);
                        //m_Pathfinding.UpdateGoal((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                        //                         (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight);
                        
                        
                        //m_Pathfinding.Replan();
                        //m_PathIndex = 0;

                        MainGame.Instance.GameState.BlueAntTraversalTime = Environment.TickCount - m_TraversalTimeBegin;
                        NotificationText.Notifications.Add(new NotificationText(true, "COMPLETED PATH!", Position, true, Color.White, true));
                        this.Dead = true;
                    }
                }
                else
                {

                }
            }
            else
            {
                // Generate the pathfinding object.
                m_Pathfinding = new DStarLitePath(pLevel,this);

                m_ReplanTimeBegin = Environment.TickCount;
                // Generate the path from the DStarLitePath object.
                m_Pathfinding.Initialize(new Point((int)Position.X / pLevel.TMXLevel.TileWidth, (int)Position.Y / pLevel.TMXLevel.TileHeight),
                                         new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth, (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight),
                                        pLevel);
                m_ReplanTimeEnd = Environment.TickCount;
                m_TotalMillisecondsTaken = m_ReplanTimeEnd - m_ReplanTimeBegin;

                // Replan now that we have set the new values.
                m_Pathfinding.Replan();
            }

            // Check on the Y axis
            if (!pLevel.CheckCollision(_currentposition, new Rectangle((int)Position.X - (int)Origin.X,
                                                                      (int)_currentposition.Y - (int)Origin.Y,
                                                                      Size.X,
                                                                      Size.Y)))
            {
                this.Position = new Vector2(Position.X, _currentposition.Y);
            }
            
            // Check on the X axis
            if (!pLevel.CheckCollision(_currentposition, new Rectangle((int)_currentposition.X - (int)Origin.X,
                                                                      (int)Position.Y - (int)Origin.Y,
                                                                      Size.X,
                                                                      Size.Y)))
            {
                this.Position = new Vector2(_currentposition.X,Position.Y);
            }

            if (AlignedBox != null)
            {
                AlignedBox.Update(pGameTime, pInputHandler);
            }

            #region No Longer Necessary
            //this.Forces["Movement"] *= m_IsOnIce == true ? 0.98f : 0.93f;
            //this.Forces["Gravity"] *= m_IsOnIce == true ? 0.98f : 0.93f;
            //this.Forces["Collision"] *= m_IsOnIce == true ? 0.98f : 0.93f;

            //// Generate the new velocity that is going to be applied.
            //this.Velocity = CalculateForces();
            //_currentposition += Velocity;
            #endregion

        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {

            base.Update(pGameTime, pInputHandler);
        }
    }
}
