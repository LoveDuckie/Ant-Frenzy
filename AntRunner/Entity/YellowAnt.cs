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
    public class YellowAnt : Ant
    {
        #region Members
        private JPSAStarPath m_Pathfinding;
        #endregion

        #region Constructors
        public YellowAnt()
            : base()
        {

        }

        public YellowAnt(Vector2 pPosition) : base(pPosition)
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["ant_sprite"];
        }

        public YellowAnt(Vector2 pPosition, SpawnPoint pSpawnPoint)
            : base (pPosition, pSpawnPoint)
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["ant_sprite"];

            this.m_FocusCake = null;

            // Create the pathfinding object.
            m_Pathfinding = new JPSAStarPath(MainGame.Instance.GameState.Level, this);

            this.m_CurrentState = "MoveToPath";

            m_TraversalTimeBegin = Environment.TickCount;
            this.m_FrameSize = new Point(56, 30);
            this.Size = new Point(56, 30);
            this.Origin = new Vector2(Size.X / 2, Size.Y / 2);

            this.m_MovementDirection = MovementDirection.None;

            this.m_MovementSpeed = 5.0f;

            m_FrameIndex = new Point(0, 0);
            this.Position = pPosition;
            this.m_SpawnFrom = pSpawnPoint;
            this.Health = 50;
            this.m_FrameSpeed = 250;
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            // Values used in checking how adjacent the player is to this entity.
            this.m_CheckRadius = 2.5f;
            this.m_ChaseRadius = 20f;

            this.m_PathIndex = 0;


            m_DisplayPhysicsDebug = false;
            // Don't want physics to be applied to this ant :<
            this.m_ApplyPhysics = false;
            

            this.Mass = 15f;

            m_CanAttackCounter = 0f;
            m_CanAttack = true;
        }
        #endregion

        #region Method
        public override void Initialize()
        {
            // Set the default values that are going to be used
            m_DisplayNotificationTimer = 0f;
            m_DisplayNotification = false;
            m_DisplayNotificationDuration = 1500f;
            m_PathIndex = 0;

            //base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {

            base.Update(pGameTime, pInputHandler);
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            Vector2 _currentposition = Position;
            this.WorldOrigin = Position + Origin;
            this.m_FrameCounter += (float)pGameTime.ElapsedGameTime.TotalMilliseconds;

            if (m_FocusCake == null)
            {
                m_FocusCake = pLevel.GetNearestCake(Position);
            }

            #region Animation Handling
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

            #region Collision Box Updating
            this.BoundingBox = new Rectangle((int)Position.X - (int)Origin.X,
                                             (int)Position.Y - (int)Origin.Y,
                                             Size.X,
                                             Size.Y);
            this.CollisionBox = new BoundingBox(new Vector3(Position - Origin, 0),
                                                new Vector3(Position + Origin, 0));
            #endregion


            if (m_Pathfinding != null)
            {
                if (m_Pathfinding.Pathlist.Count > 0)
                {
                    if (m_PathIndex < m_Pathfinding.Pathlist.Count)
                    {
                        if (!pLevel.IsClear(m_Pathfinding.Pathlist[m_PathIndex].position.X,
                            m_Pathfinding.Pathlist[m_PathIndex].position.Y) ||
                            pLevel.IsObjectAt(m_Pathfinding.Pathlist[m_PathIndex].position.X,
                                              m_Pathfinding.Pathlist[m_PathIndex].position.Y, typeof(NewBox)))
                        {
                            m_Pathfinding.UpdateStart(new Point((int)Position.X / pLevel.TMXLevel.TileWidth,
                                                                (int)Position.Y / pLevel.TMXLevel.TileHeight));

                            m_Pathfinding.UpdateGoal(new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                                                               (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight));
                            m_Pathfinding.Replan();
                            m_PathIndex = 0;
                            m_TraversalTimeBegin = Environment.TickCount;
                        }


                        Rotation = RotateTo(new Vector2(m_Pathfinding.Pathlist[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                                        m_Pathfinding.Pathlist[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32));
                        _currentposition += MoveTo(Rotation, m_MovementSpeed);

                        // Increase the path index and continue as normal.
                        if (WithinRange(new Vector2(m_Pathfinding.Pathlist[m_PathIndex].position.X * pLevel.TMXLevel.TileWidth + 32,
                                                    m_Pathfinding.Pathlist[m_PathIndex].position.Y * pLevel.TMXLevel.TileHeight + 32), m_CheckRadius))
                        {
                            m_Pathfinding.Pathlist[m_PathIndex].visited = true;
                            m_PathIndex++;
                        }
                    }
                    else
                    {
                        MainGame.Instance.GameState.YellowAntTraversalTime = Environment.TickCount - m_TraversalTimeBegin;
                        NotificationText.Notifications.Add(new NotificationText(true, "PATH COMPLETE!", Position, true, Color.White, true));
                        this.Dead = true;

                    }
                }
                else
                {
                    this.m_Pathfinding = new JPSAStarPath(pLevel, this);
                    m_Pathfinding.Initialize(new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                        (int)this.Position.Y / pLevel.TMXLevel.TileHeight),
                                             new Point((int)this.m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                                                       (int)this.m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight));
                    m_Pathfinding.Replan();
                }

            }
            else
            {
                this.m_Pathfinding = new JPSAStarPath(pLevel, this);
                m_Pathfinding.Initialize(new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                    (int)this.Position.Y / pLevel.TMXLevel.TileHeight),
                                         new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                   (int)this.Position.Y / pLevel.TMXLevel.TileHeight));
                m_Pathfinding.Replan();
            }

            #region Collision Checking
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
                this.Position = new Vector2(_currentposition.X, Position.Y);
            }

            #endregion
           // base.Update(pGameTime, pInputHandler, pLevel);
        }
        #endregion

        // Draw the sprite image for the ant in question
        public override void Draw(SpriteBatch pSpriteBatch)
        {
            m_Pathfinding.DrawPathfinding(pSpriteBatch);
            base.Draw(pSpriteBatch);
        }
    }
}
