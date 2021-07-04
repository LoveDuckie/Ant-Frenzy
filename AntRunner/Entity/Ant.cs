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

// required for dropping items appropriately
using AntRunner.Entity.Items;

namespace AntRunner.Entity
{
    #region Enumerations
    public enum AntState
    {
        Attacking = 1,
        Wandering, // Moving random
        MoveToPlayer,
        MoveToPath,
        Idle,
        Returning // Going back to the spawn with the cake
    }

    public enum MovementDirection
    {
        Up = 1,
        Down,
        Left,
        Right,
        None
    }
    #endregion

    // Custom event handler for the states.
    public delegate void StateHandler(Level pLevel, GameTime pGameTime, InputHandler pInputHandler);

    // Struct to be used for the finite state machine
    public struct FiniteState
    {
        public string m_StateName { get; set; }
        public FiniteStateEnum m_StateType { get; set; }
        public StateHandler m_Action { get; set; }
        public StateHandler m_OnBegin { get; set; }
        public StateHandler m_OnSuspend { get; set; }
    }

    /// <summary>
    /// Value for identifying what state the ant is currently in
    /// </summary>
    public enum FiniteStateEnum
    {
        Wandering = 1,
        MoveToPath,
        Returning,
        Attacking
    }

    // The base ant class that contains the finite state machines etc.
    public class Ant : Character
    {
        // The states that the ant will make use of.
        protected Dictionary<string, FiniteState> m_States = new Dictionary<string, FiniteState>();
        protected StateHandler OnPathEnd;
        protected StateHandler OnPathBegin;
        protected StateHandler OnPathReplan;

        protected int m_PlanningTime = 0;
        protected int m_PlanningTimeBegin = 0;

        // Every 2 seconds the ant will run out of water, and this will affect the HTN.
        protected int m_Water = 50;

        // The sprites that are available for usage.
        public readonly Point[] ANT_SPRITES = { new Point(0, 0), 
                                                new Point(0, 1), 
                                                new Point(0, 2), 
                                                new Point(0, 3) };


        // A map that determines all the points that are considered to be dangerous in the environment
        private DangerMap m_DangerMap;

        private float m_CheckPathCounter;
        private const float CHECK_PATH_INTERVAL = 5000f;

        // Used for deleting the ant if it's stuck for too long
        private float m_CheckStuckTimer = 0f;
        private float m_CheckStuckDuration = 2000f; // How many seconds do we check again to determine that it's stuck?

        protected bool m_ApplyPhysics = false;

        protected int m_TraversalTimeBegin = 0;
        protected int m_TraversalTimeEnd = 0;

        private Point m_LastInsertedDangerPoint = Point.Zero;

        // This will determine whether or not
        protected bool m_DisplayPhysicsDebug = true;

        private float m_RandomDirectionCounter;

        #region Constants
        // The min and max used for the counter
        private const float RANDOM_DIR_MIN = 3000f;
        private const float RANDOM_DIR_MAX = 7500f;
        private const float MAX_VELOCITY = 5f;
        private const int MAX_DROPS = 4;

        private SoundEffectInstance m_HitSound;

        // How far from the path node that we're aiming for do we have to be
        // before we must consider regenerating?
        public const float REGENERATE_PATH_DISTANCE = 10.5f;
        public const float PATH_DISTANCE = 8f;
        public const float CAKE_RANGE = 5f;

        // For applying steering forces.
        private const float MAX_SPEED = 1.0f;

        // The movement speed for when they are 
        private const float DANGER_MOVEMENT_SPEED = 11f;
        private const float NORMAL_MOVEMENT_SPEED = 3f;
        #endregion

        #region Pathfinding
        protected Point m_Goal = Point.Zero;
        protected Point m_Start = Point.Zero;
        #endregion

        protected Color               m_HurtColor;
        protected int                 m_HurtRedOpacity;
        protected bool                m_IsHurt;
        protected bool                m_CanFocus;
        protected bool                m_CarryingCake;

        // Steering behaviour
        protected Vector2             m_Desired = Vector2.Zero;
        
        // Distances
        protected float               m_ChaseRadius;
        protected float               m_AttackRadius;
        protected float               m_CheckRadius;

        protected bool                m_IsOnIce = false;
        protected bool                m_ReturningSpawn = false;
        
        // States etc.
        protected MovementDirection   m_MovementDirection;
        protected AntState            m_AntState;
        protected Entity              m_Focus;
        protected string              m_CurrentState;

        // State notification
        protected bool                m_DisplayNotification;
        protected float               m_DisplayNotificationTimer;
        protected float               m_DisplayNotificationDuration;


        // The point at which a notification is displayed 
        // because the ant is being sucked into the black hole
        protected float               m_GravityWarningMagnitude = 5f;
        
        // Temporary variable used for determining that the next path node to take is safe
        protected bool                m_CheckedNextPathNode = false;

        protected SpawnPoint          m_SpawnFrom;

        protected Cake m_FocusCake;

        // For determining if the ant can attack
        // If so, for how long?
        protected bool m_CanAttack;
        protected float m_CanAttackCounter;

        protected bool m_CanMove;

        // List of path nodes that we're heading for
        protected List<PathNode> m_Pathlist;
        protected int m_PathIndex = 0;

        #region Constructors
        public Ant()
        {
            // Set it to white, until we want to hurt the ant and then make the red apparent.
            m_HurtColor = new Color(255, 255, 255, 255);
            m_CarryingCake = false;

            this.MovementSpeed = NORMAL_MOVEMENT_SPEED;

            this.m_FrameSize = new Point(43, 30);
            this.m_FrameSpeed = 250;
            this.Health = 50;

            m_ApplyPhysics = true;

            this.Initialize();
        }

        public Ant(Vector2 pPosition) : base(new Point(43,30),250)
        {
            this.m_Position = new Vector2(pPosition.X + (43 / 2), pPosition.Y + (30 / 2));
            this.m_SpawnFrom = null;
            this.AlignedBox = new AABB(this.Position, new Vector2(Position.X + Size.X,
                                                                 Position.Y + Size.Y), this);
            this.Initialize();
        }


        public Ant(Vector2 pPosition,SpawnPoint pSpawnPoint) : base(new Point(43,30),250)
        {
            // Do something creative with the position
            this.m_Position = new Vector2(pPosition.X + (43 / 2), pPosition.Y + (30 / 2));
            this.m_SpawnFrom = pSpawnPoint;

            // Generate the new danger map that is used for evaluating costs of paths.
            m_DangerMap = new DangerMap(MainGame.Instance.GameState.Level);

            // Generate a new bounding box for the item
            this.AlignedBox = new AABB(Vector2.Zero, Vector2.Zero, this);
            this.Initialize();
        }
#endregion

        #region Methods
        /// <summary>
        /// Generate a random direction for us to go in
        /// </summary>
        /// <returns>Returns the movement direction that we want to head in</returns>
        public MovementDirection RandomDirection()
        {
            return (MovementDirection)m_Random.Next(1, 4);
        }
        
        // Determine whether the other entity is within the radius that is specified.
        public bool CheckRadius(Entity pOther)
        {
            float _squaredistance = (float) Math.Sqrt(Math.Pow((this.m_Origin.X - pOther.Position.X),2) +
                                            Math.Sqrt(Math.Pow((this.m_Origin.Y - pOther.Position.Y),2)));

            return _squaredistance <= Math.Pow(this.m_CheckRadius, 2);
        }

        public override void Initialize()
        {
            // Set the opacity that is being used.
            this.m_HurtRedOpacity = 255;
            this.MovementSpeed = 2.5f;

            this.m_CheckRadius = 10f;
            this.m_ChaseRadius = 20f;

            this.m_CanMove = true;

            // Notification stuff
            m_DisplayNotificationTimer = 0f;
            m_DisplayNotification = false;
            m_DisplayNotificationDuration = 1500f;

            ChangeState("MoveToPath");

            // Load in the spritesheet that is going to be used.
            this.m_SpriteSheet = MainGame.Instance.Textures["ant_sprite"];
            this.Health = 10;
            this.m_FrameIndex = ANT_SPRITES[m_Random.Next(0, this.ANT_SPRITES.Length)];
            this.m_FrameSize = new Point(56, 30);
            this.Size = new Point(56, 30);
            
            this.m_AntState = AntState.Idle;
            this.m_MovementDirection = MovementDirection.Right;
            this.m_Origin = new Vector2(m_FrameSize.X / 2, m_FrameSize.Y / 2);
            this.Mass = 15f;

            m_Pathlist = new List<PathNode>();

            m_CanAttackCounter = 0f;
            m_CanAttack = true;

            // Generate the bounding box that we want to use
            // for determining collisions
            this.m_BoundingBox = new Rectangle((int)m_Position.X,
                                               (int) m_Position.Y, 
                                                m_FrameSize.X, 
                                                m_FrameSize.Y);

            // Add the finite states that we are going to be using
            this.m_States.Add("Wandering", new FiniteState() { m_Action = Wandering, m_StateName = "Wandering" });
            this.m_States.Add("Attacking", new FiniteState() { m_Action = Attacking, m_StateName = "Attacking" });
            this.m_States.Add("MoveToPlayer", new FiniteState() { m_Action = MoveToPlayer, m_StateName = "MoveToPlayer" });
            this.m_States.Add("MoveToPath", new FiniteState() { m_Action = MoveToPath, m_StateName = "MoveToPath" });
            this.m_States.Add("Returning", new FiniteState() { m_Action = Returning, m_StateName = "Returning" });

            // Add the appropriate forces that we want to use.
            // These will be added up and normalized in the end.
            this.Forces.Add("Gravity", Vector2.Zero);
            this.Forces.Add("Angular", Vector2.Zero);
            this.Forces.Add("Movement", Vector2.Zero);
            this.Forces.Add("Ice", Vector2.Zero);
            this.Forces.Add("Steering", Vector2.Zero);
            this.Forces.Add("Collision", Vector2.Zero); // Used for dealing with physics when colliding with ants
        }

        protected virtual void FollowPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Only move along the path
            if (m_Pathlist != null)
            {

            }
        }

        #region States
        /// <summary>
        /// Return back to the spawn point with the cake!
        /// </summary>
        /// <param name="pLevel">The level that we're interacting with</param>
        /// <param name="pGameTime">The delta time values that are being used</param>
        /// <param name="pInputHandler">The object that is used for dealing with input</param>
        protected virtual void Returning(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {   
            // Make sure that the spawn point is still there.
            if (m_SpawnFrom != null)
            {
                if (m_PathIndex < m_Pathlist.Count)
                {
                    // Have we arrived at the spawn point again? Is it worth returning?
                    if (WithinRange(m_SpawnFrom, 5f))
                    {
                        ChangeState("MoveToPath");
                    }
                    else
                    {
                        // Check if there is a box at the position that we want to go to
                        if (!pLevel.IsBoxAt(m_Pathlist[m_PathIndex].position.X, m_Pathlist[m_PathIndex].position.Y) &&
                            !pLevel.IsObjectAt((int)m_Pathlist[m_PathIndex].bezierPosition.X / (int)pLevel.TMXLevel.TileWidth,
                                               (int)m_Pathlist[m_PathIndex].bezierPosition.Y / pLevel.TMXLevel.TileHeight,
                                               typeof(Box)))
                        {
                            this.Rotation = RotateTo(m_Pathlist[m_PathIndex].bezierPosition);
                         
                            this.Forces["Movement"] += MoveTo(Rotation, MovementSpeed) / Mass;

                            if (WithinRangeOrigin(m_Pathlist[m_PathIndex].bezierPosition, PATH_DISTANCE))
                            {
                                m_Pathlist[m_PathIndex].visited = true;
                                m_PathIndex++;
                            }
                        }
                        else
                        {
                            // Generate a new path based on 
                            m_Pathlist.Clear();
                            this.m_Pathlist = AStarPath.ComputePath(new Point((int)m_SpawnFrom.Position.X / pLevel.TMXLevel.TileWidth,
                                                                              (int)m_SpawnFrom.Position.Y / pLevel.TMXLevel.TileHeight),
                                                                              new Point((int)m_Position.X / pLevel.TMXLevel.TileWidth,
                                                                                        (int)m_Position.Y / pLevel.TMXLevel.TileHeight), pLevel);
                            m_PathIndex = 0;
                            m_Pathlist.Reverse();
                        }
                    }
                }
                else
                {
                    ChangeState("MoveToPath");
                }
            }
            else
            {
                // Select a random spawn point to go to
                this.m_SpawnFrom = pLevel.SpawnPoints[m_Random.Next(0, pLevel.SpawnPoints.Count - 1)];
            }
        }

        /// <summary>
        /// This is activated when the Ant is heading towards the player to kill them off.
        /// </summary>
        /// <param name="pLevel">The level that we are interacting with</param>
        /// <param name="pGameTime">The delta time object</param>
        /// <param name="pInputHandler">The input manager</param>
        protected virtual void Attacking(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            Vector2 _currentposition = Position;

            // Determine if the player is within range with the given check radius
            if (WithinRange(m_Focus, m_ChaseRadius))
            {
                // Keep moving in the direction of the player, otherwise stop
                if (!WithinRange(m_Focus, 11f))
                {
                    this.m_Rotation = RotateTo(m_Focus); //+ MathHelper.ToRadians(90);
                    this.Forces["Movement"] = MoveTo(Rotation,NORMAL_MOVEMENT_SPEED);
                    //this.Velocity = CalculateForces();
                   // _currentposition += Velocity;

                    //// Check for collision on the X axis
                    //if (!pLevel.CheckCollision(_currentposition,
                    //                           new Rectangle((int)_currentposition.X,
                    //                                         (int)Position.Y,
                    //                                         Size.X,
                    //                                         Size.Y)))
                    //{
                    //    this.Position = new Vector2(_currentposition.X, Position.Y);
                    //}
                    //else
                    //{
                    //    // Bump into a wall and return to moving to a path
                    //    ChangeState("MoveToPath");
                    //}

                    //if (!pLevel.CheckCollision(_currentposition,
                    //                           new Rectangle((int)Position.X,
                    //                                         (int)_currentposition.Y,
                    //                                         Size.X,
                    //                                         Size.Y)))
                    //{
                    //    this.Position = new Vector2(Position.X, _currentposition.Y);
                    //}
                    //else
                    //{
                    //    // Get bored of the player and return to moving to the cake again
                    //    ChangeState("MoveToPath");
                    //}

                }
            }
            else
            {
                m_Focus = null;
                ChangeState("MoveToPath");
            }
        }
        
        /// <summary>
        /// When there is no particular goal set of the cake is not near by.
        /// </summary>
        /// <param name="pLevel">The level that is being observed and used to interact with</param>
        /// <param name="pGameTime">The delta time that is passed to the function</param>
        /// <param name="pInputHandler">The class that is responsible for dealing with the input</param>
        protected virtual void Wandering(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Used for determining if the ant can move to the next spot.
            Vector2 _currentPosition = Position;
            Vector2 _velocity = CalculateForces();

            this.m_CanMove = true;

            // Only able the ant to move in any random direction of this variable is set to true.
            if (m_CanMove)
            {
                // Based on the direction that is assigned to the person, get the ant moving.
                switch (m_MovementDirection)
                {
                    case MovementDirection.Down:
                        this.m_Rotation = MathHelper.ToRadians(90);
                        //_currentPosition += new Vector2(0, MovementSpeed);

                        _currentPosition += Forces["Movement"];

                        break;

                    case MovementDirection.Up:
                        this.m_Rotation = MathHelper.ToRadians(270);
                        _currentPosition += new Vector2(0, -MovementSpeed);
                        break;

                    case MovementDirection.Left:
                        this.m_Rotation = MathHelper.ToRadians(180);
                        _currentPosition += new Vector2(-MovementSpeed, 0);
                        break;

                    case MovementDirection.Right:
                        this.m_Rotation = 0f;
                        _currentPosition += new Vector2(MovementSpeed, 0);
                        break;
                }

                // Apply the appropriate force to the movement of the direction that has been set
                //                this.Forces["Movement"] += MoveTo(Rotation, m_MovementSpeed); 

                if (m_CheckPathCounter == 0f)
                {
                    m_CheckPathCounter = Environment.TickCount + CHECK_PATH_INTERVAL;
                }

                //// Change state again to moving to path as that is what we want to do
                //if (m_CheckPathCounter < Environment.TickCount)
                //{
                //    m_CheckPathCounter = 0f;
                //    ChangeState("MoveToPath");
                //    return;
                //}

                // Change the direction randomly.
                if (m_RandomDirectionCounter < Environment.TickCount)
                {
                    m_RandomDirectionCounter = Environment.TickCount + m_Random.Next((int)RANDOM_DIR_MIN, (int)RANDOM_DIR_MAX);
                    m_MovementDirection = (MovementDirection)m_Random.Next(0, 4);

                }

                // Check to see if there is a collision on the X axis
                if (pLevel.CheckCollision(_currentPosition, new Rectangle((int)_currentPosition.X,
                                                                         (int) Position.Y,
                                                                         Size.X,
                                                                         Size.Y)))
                {

                }

                // Check to see if there is a collision on the Y axis
                if (pLevel.CheckCollision(_currentPosition, new Rectangle((int)Position.X,
                                                                          (int)_currentPosition.Y,
                                                                          Size.X,
                                                                          Size.Y)))
                {

                }

                // Check to see if the ant is within the bounds of the level.
                if (!WithinLevelBounds(pLevel, new Vector2(_currentPosition.X, Position.Y)))
                {
                    ReverseDirection();
                }

                if (!WithinLevelBounds(pLevel, new Vector2(Position.X, _currentPosition.Y)))
                {
                    ReverseDirection();
                }

                if (MainGame.Instance.GameState.PlayerManager.Players.Count > 0)
                {
                    // If the player is within range, then change to attacking. nom nom nom.
                    if (WithinRange(MainGame.Instance.GameState.PlayerManager.Players[0], 10.0f))
                    {
                        if (m_Focus == null)
                        {
                            m_Focus = MainGame.Instance.GameState.PlayerManager.Players[0];
                        }

                        ChangeState("Attacking");
                    }
                }
            }
        }

        /// <summary>
        /// Called when the ant is dead and out of the game
        /// Is meant to drop items around the point that the ant died
        /// </summary>
        public void Die()
        {
            // Select a number of drops to be spawned by the ant
            int _alloweddrops = m_Random.Next(1, MAX_DROPS);

            int _allowedHealthDrops = m_Random.Next(1, 3);

            // Drop a certain amount of items for the player to collect.
            for (int i = 0; i < _alloweddrops; i++)
            {
                float _direction = MathHelper.ToRadians(m_Random.Next(0, 359));

                // Add the item to the entity list if the enemy is dead
                GameItems.Add(new Ammo(25,new Vector2((float)Math.Cos(_direction),
                                                     (float)Math.Sin(_direction)),
                                                     true,
                                                     Position));
            }

            // Keep generating until we hit the randomly generated number of allowed health drops in total
            for (int i = 0; i < _allowedHealthDrops; i++)
            {
                float _direction = MathHelper.ToRadians(m_Random.Next(0, 359));

                // Add the item to the entity list if the enemy is dead
                //GameItems.Add(new HealthPotion(25, new Vector2((float)Math.Cos(_direction),
                //                                     (float)Math.Sin(_direction)),
                //                                     true,
                //                                     Position));
            }
        }

        /// <summary>
        /// Follow the path that is generated for the ant.
        /// </summary>
        /// <param name="pLevel">The level that the ant is going to interact with</param>
        /// <param name="pGameTime">the delta time of the environment</param>
        /// <param name="pInputHandler">For dealing with controller input</param>
        private void MoveToPath(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            Velocity = Vector2.Zero; // set it to this for now

            // Determine first that there is more than one player within the environment.
            if (MainGame.Instance.GameState.PlayerManager.Players.Count > 0)
            {

                // Storage the value that we are after
                if (WithinRange(MainGame.Instance.GameState.PlayerManager.Players[0], 10f))
                {
                    // Change to the attacking state and make the focus the player
                    m_Focus = MainGame.Instance.GameState.PlayerManager.Players[0];
                    ChangeState("Attacking");
                }
            }

            // Determine if the path list is null or counting to 0 before we do anything
            if (m_Pathlist == null || m_Pathlist.Count == 0)
            {
                if (m_FocusCake == null)
                {
                    m_FocusCake = pLevel.GetRandomCake();
                    
                    // If there is still no cake to be found then keep wandering.
                    if (m_FocusCake == null)
                    {
                        // If no cake can be found then keep wandering
                        ChangeState("Wandering");
                        return;
                    }
                }

                // Generate the path if it's null
                m_Pathlist = AStarPath.ComputePath(new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth, 
                                                             (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight),
                                                   new Point((int)Position.X / pLevel.TMXLevel.TileWidth, 
                                                             (int)Position.Y / pLevel.TMXLevel.TileHeight),
                                                             pLevel);

                // For some reason due to the nature that the points are generated, 
                // I will have to reverse them.
                m_Pathlist.Reverse();
                m_PathIndex = 0;

            }

            // Change to wandering until it is possible to find a path to the cake in the middle
            if (m_Pathlist.Count == 0 || m_Pathlist.Count == 1)
            {
                ChangeState("Wandering");
            }

            // If we haven't reached the end of the paths then keep going
            if (m_PathIndex < m_Pathlist.Count)
            {
                // Make sure that minimally the next path in the list is within distance.
                // Dont bother regenderating if we're getting pulled in by gravitational forces at a ridiculous speed
                if (!WithinRangeOrigin(m_Pathlist[m_PathIndex].bezierPosition, REGENERATE_PATH_DISTANCE) 
                    && Forces["Gravity"].Length() < 1.5f)
                {
                    // Regenerate the path if we're out of range of the current path that we want
                    this.m_Pathlist = AStarPath.ComputePath(new Point((int)Position.X / pLevel.TMXLevel.TileWidth,
                                                                      (int)Position.Y / pLevel.TMXLevel.TileHeight),
                                                            new Point((int)m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth,
                                                                      (int)m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight), pLevel);
                    m_PathIndex = 0;
                }

                if (m_Pathlist.Count > 0)
                {
                    // Determine that there isn't a box at the next pathnode that we want to get to
                    if (!pLevel.IsObjectAt((int)m_Pathlist[m_PathIndex].bezierPosition.X / pLevel.TMXLevel.TileWidth,
                                           (int)m_Pathlist[m_PathIndex].bezierPosition.Y / pLevel.TMXLevel.TileHeight,
                                           typeof(Box)))
                    {
                        // Adjust the rotation of the ant so that it's facing the next path node that we have to move towards.
                        this.m_Rotation = RotateTo(m_Pathlist[m_PathIndex].bezierPosition);
                        this.Forces["Movement"] += MoveTo(Rotation, Forces["Gravity"].Length() < 5f ? NORMAL_MOVEMENT_SPEED : DANGER_MOVEMENT_SPEED) / Mass;

                        if (WithinRangeOrigin(m_Pathlist[m_PathIndex].bezierPosition, PATH_DISTANCE))
                        {
                            m_Pathlist[m_PathIndex].visited = true;
                            m_PathIndex++;
                        }
                    }
                    else
                    {
                        m_Pathlist = AStarPath.ComputePath(new Point((int)(m_FocusCake.Position.X / pLevel.TMXLevel.TileWidth), (int)(m_FocusCake.Position.Y / pLevel.TMXLevel.TileHeight)),
                                                           new Point((int)(Position.X / (int)pLevel.TMXLevel.TileWidth), (int)(Position.Y / pLevel.TMXLevel.TileHeight)), pLevel);
                        m_PathIndex = 0;
                        m_Pathlist.Reverse();
                    }
                }
            }
            else
            {
                // See if the ake is within range
                if (WithinRange(m_FocusCake, 7f))
                {
                    m_FocusCake.TakeCake(5);
                }
                
                ChangeState("Returning");
            }

        }

        // For when to attack the player
        public void MoveToPlayer(Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            if (m_Focus != null && m_Focus is Player)
            {
                if (!WithinRange(m_Focus, m_CheckRadius))
                {
                    // If the player is not within range then continue looking for the caje
                    ChangeState("MoveToPath");
                }
                else
                {
                    this.m_Rotation = this.RotateTo(pLevel.GameStateRef.PlayerManager.Players[0]);
                }
            }
        }
        #endregion

        /// <summary>
        /// Call the finite state that we want to use
        /// </summary>
        /// <param name="pStateName">The name of the finite state that is to be used</param>
        public void CallState(string pStateName, Level pLevel, GameTime pGameTime, InputHandler pInputHandler)
        {
            // Determine first that the state name is correct.
            if (m_States.ContainsKey(pStateName))
            {
                m_States[pStateName].m_Action.Invoke(pLevel, pGameTime, pInputHandler);
            }
        }


        public virtual void DrawPathfinding(SpriteBatch pSpriteBatch)
        {
            if (m_Pathlist != null && m_Pathlist.Count > 0)
            {
                for (int i = 0; i < m_Pathlist.Count; i++)
                {
                    // Do we draw the curvature dot or something else?
                    if (!m_Pathlist[i].isBezier)
                    {
                        pSpriteBatch.Draw(MainGame.Instance.Textures["blank_grid"], new Vector2(m_Pathlist[i].position.X * 64, m_Pathlist[i].position.Y * 64), m_Pathlist[i].visited == true ? Color.Blue : Color.Red);
                    }
                    else
                    {
                        pSpriteBatch.Draw(MainGame.Instance.Textures["bezier_dot"], m_Pathlist[i].bezierPosition, m_Pathlist[i].visited == true ? Color.Blue : Color.Red); 
                    }
                }
            }
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            //pSpriteBatch.Draw(m_SpriteSheet,m_Position,new Rectangle(m
#if DEBUG
            //m_Pathfinding.Draw(pSpriteBatch);

            //pSpriteBatch.Draw(MainGame.Instance.Textures["bezier_dot"], Position, Color.Red);


            // Only display debug information if the global variable has been set accordingly.
            if (Global.DEBUG)
            {
                DrawPathfinding(pSpriteBatch);
            
                if (m_FocusCake != null)
                {
                    DebugDraw.DrawDebugLine(Position, m_FocusCake.Position, pSpriteBatch, Color.White, 5, false);
                }
            }

            if (Global.DEBUG)
            {
                if (m_DangerMap != null)
                {
                    m_DangerMap.Draw(pSpriteBatch);
                }
            }

#endif
            // Display notification if it is required of the ant.
            if (m_DisplayNotification)
            {
                switch (m_CurrentState)
                {
                    case "Wandering":
                        pSpriteBatch.Draw(MainGame.Instance.Textures["speech_wandering"],new Vector2(Position.X - 70, Position.Y - MainGame.Instance.Textures["speech_wandering"].Height - 32), Color.White);
                    break;

                    case "MoveToPath":
                        pSpriteBatch.Draw(MainGame.Instance.Textures["speech_cake"], new Vector2(Position.X - 70, Position.Y - MainGame.Instance.Textures["speech_kill"].Height - 32), Color.White);
                    break;

                    case "Attacking":
                        pSpriteBatch.Draw(MainGame.Instance.Textures["speech_kill"], new Vector2(Position.X - 70, Position.Y - MainGame.Instance.Textures["speech_kill"].Height - 32), Color.White);
                    break;

                    case "Returning":
                        pSpriteBatch.Draw(MainGame.Instance.Textures["speech_wandering"], new Vector2(Position.X - 70, Position.Y - MainGame.Instance.Textures["speech_wandering"].Height - 32), Color.White);
                    break;
                }
            }

            #region Rendering Ant
            // Ant rendering
            pSpriteBatch.Draw(
                m_SpriteSheet, 
                m_Position, 
                new Rectangle(m_FrameIndex.X * m_FrameSize.X,
                              m_FrameIndex.Y * m_FrameSize.Y,
                              m_FrameSize.X, m_FrameSize.Y), 
                              new Color(255,m_HurtRedOpacity,m_HurtRedOpacity,255),
                              m_Rotation,
                              m_Origin,
                              1.0f,
                              SpriteEffects.None,0);
            #endregion


            if (Global.DEBUG)
            {
                // Make sure that we are meant to display the physics debug.
                if (m_DisplayPhysicsDebug)
                {
                    int _count = 0;
                    foreach (var item in Forces)
                    {
                        ShadowText.Draw(string.Format("{0}: {1}", item.Key.ToString(), item.Value.ToString()), pSpriteBatch, new Vector2(Position.X + Size.X, Position.Y + Size.Y + (14 * _count)));

                        // Increase the count
                        _count++;
                    }
                }

                base.Draw(pSpriteBatch);
            }
        }

        // Determine the amount of health to lose
        // if it goes past the amount that's left, then consider the ant dead.
        public void LoseHealth(int pAmount)
        {
            if ((Health - pAmount) < 0)
            {
                m_Dead = true;
               Die();
            }
            else
            {
                Health -= pAmount;
            }
        }

        // Determine whether or not we're within the level
        public bool WithinLevelBounds(Level pLevel, Vector2 pNewPosition)
        {
            return ((pNewPosition.X + m_FrameSize.X) < pLevel.TMXLevel.Width * pLevel.TMXLevel.TileWidth &&
                    (pNewPosition.Y + m_FrameSize.Y) < pLevel.TMXLevel.Height * pLevel.TMXLevel.TileHeight &&
                    pNewPosition.X > 0 &&
                    pNewPosition.Y > 0);
        }

        /// <summary>
        /// Done when the state is no currently active.
        /// </summary>
        /// <param name="pStateName">The name of the state that we want to change to</param>
        public virtual void OnSuspend(string pStateName)
        {
            switch (pStateName)
            {
                case "Wandering":
                    
                break;

                case "MoveToPath":

                break;

                case "MoveToPlayer":

                break;

                case "Attacking":
                    // Set it back to nothing.
                    Forces["Steering"] = Vector2.Zero;
                break;

                case "Returning":
                    // Get rid of the spawn from point. We want a new one for every time that the state has to start again
                    m_SpawnFrom = null;
                break;

            }
            
        }

        /// <summary>
        /// Function that is called once when the state is changed
        /// </summary>
        /// <param name="pStateName">The name of the state that we are going to execute conditions based on</param>
        public virtual void OnBeginState(string pStateName)
        {
            Level pLevel = MainGame.Instance.GameState.Level;

            switch (pStateName)
            {
                case "Wandering":

                break;

                case "MoveToPath":
                    // Clear out all the pathfinding stuff and generate a new one
                    // When the state is called for the first time
                    this.m_PathIndex = 0;
                    
                    if (m_Pathlist != null)
                    {
                        this.m_Pathlist.Clear();
                    }
                break;

                case "MoveToPlayer":

                break;

                case "Attacking":
                    m_Pathlist.Clear();
                    m_PathIndex = 0;
                break;

                case "Returning":

                // If there is a box there, then clear out the path list and try again
                if (m_Pathlist != null)
                {
                    m_SpawnFrom = pLevel.SpawnPoints[m_Random.Next(0,pLevel.SpawnPoints.Count)];

                    m_Pathlist.Clear();
                    m_PathIndex = 0;
                    m_Pathlist = AStarPath.ComputePath(new Point((int)m_SpawnFrom.Position.X / pLevel.TMXLevel.TileWidth,
                                                                 (int)m_SpawnFrom.Position.Y / pLevel.TMXLevel.TileHeight),
                                                        new Point((int)Position.X / pLevel.TMXLevel.TileWidth,
                                                                  (int)Position.Y / pLevel.TMXLevel.TileHeight), pLevel);
                    m_Pathlist.Reverse();
                }

                break;

            }
        }

        public void ReverseDirection()
        {
            // Determine if we're walking left to right or not
            if (m_MovementDirection == MovementDirection.Left || m_MovementDirection == MovementDirection.Right)
            {
                this.m_MovementDirection = m_MovementDirection == MovementDirection.Left ? MovementDirection.Right : MovementDirection.Left;
            }
            else if (m_MovementDirection == MovementDirection.Up || m_MovementDirection == MovementDirection.Down)
            {
                this.m_MovementDirection = m_MovementDirection == MovementDirection.Up ? MovementDirection.Down : MovementDirection.Up;
            }
        }

        public void ChangeState(string pStateName)
        {
            OnSuspend(m_CurrentState);
            this.m_CurrentState = pStateName;
            m_DisplayNotification = true;
            this.OnBeginState(pStateName);
        }

        // Check to see if the ant is within the bounds of the level
        // if not then
        public void CheckBounds(Level pLevel)
        {
            // Check the bounds of 
            if (Position.X > pLevel.TMXLevel.Width * pLevel.TMXLevel.TileWidth ||
                Position.Y > pLevel.TMXLevel.Height * pLevel.TMXLevel.TileHeight ||
                Position.X < 0 ||
                Position.Y < 0)
            {
                this.Dead = true;
            }
        }

        /// <summary>
        /// Inverse the direction that we are aiming to head in
        /// </summary>
        /// <param name="pDirection">The current direction that the ant is going in</param>
        /// <returns>The new direction that we are heading in</returns>
        public virtual MovementDirection InverseDirection(MovementDirection pDirection)
        {
            // Based on the direction, reverse it appropriately.
            switch (pDirection)
            {
                case MovementDirection.Down: return MovementDirection.Up;
                case MovementDirection.Left: return MovementDirection.Right;
                case MovementDirection.Up: return MovementDirection.Down;
                case MovementDirection.Right: return MovementDirection.Left;
            }

            return MovementDirection.None;
        }

        // Another version of the function so that we can determine whether the ant has exceeded
        // the level bounds.
        public override void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            AlignedBox.Update(pGameTime, pInputHandler);
            Vector2 _currentposition = Position;
            this.WorldOrigin = Position + Origin;
            CheckBounds(pLevel);

            // Used for processing physics in some areas.
            float dt = (float)pGameTime.ElapsedGameTime.TotalSeconds;
            this.m_FrameCounter += (float)pGameTime.ElapsedGameTime.TotalMilliseconds;

            if (m_HurtRedOpacity < 254)
            {
                m_HurtRedOpacity += 10;
            }

            #region Ice Checking
            // Determine that we are on ice.
            if (pLevel.GetTileType((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                   (int)this.Position.Y / pLevel.TMXLevel.TileHeight) == TileType.Ice)
            {
                this.m_IsOnIce = true;
            }
            else
            {
                this.m_IsOnIce = false;
            }
            #endregion


            #region Gravity Checking
            // Check for a gravitational pull
            // Loop through the black holes and determine something near by that is pulling the ants
            // closer.
            foreach (var item in pLevel.BlackHoles)
            {
                if (WithinRange(item, item.PullRadius))
                {
                    Vector2 _distanceVector = (item.Position + item.Origin) - this.Position;
                    _distanceVector.Normalize();

                    this.Forces["Gravity"] = MathUtilities.Gravity(MathUtilities.GRAVITY_CONST, item.Mass, this.Mass, this.Position - item.WorldOrigin);
                    

                    // Plot on the danger map where the approximation is 
                    if (this.Forces["Gravity"].Length() > 1.0f)
                    {
                        // Determine that we haven't already inserted a point here
                        if (m_LastInsertedDangerPoint != new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                                   (int)this.Position.Y / pLevel.TMXLevel.TileHeight))
                        {
                            // Generate a new danger point and then propagate to adjacent nodes.
                            pLevel.DangerMap.InsertDangerPoint(new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                                         (int)this.Position.Y / pLevel.TMXLevel.TileHeight), 1);

                            this.m_LastInsertedDangerPoint = new Point((int)this.Position.X / pLevel.TMXLevel.TileWidth,
                                                                       (int)this.Position.Y / pLevel.TMXLevel.TileHeight);
                        }
                        
                    }

                    break;
                }

                this.Forces["Gravity"] = Vector2.Zero;
            }
            #endregion


            #region Collision Box Updating
            // Update the collision box of the ant.
            this.m_CollisionBox = new BoundingBox(new Vector3(Position - Origin, 0),
                                                  new Vector3(this.m_Position.X + Origin.X, 
                                                              this.m_Position.Y + Origin.Y, 0));
            this.m_BoundingBox = new Rectangle((int)Position.X - (int)m_Origin.X, 
                                               (int)Position.Y - (int)m_Origin.Y, 
                                               m_FrameSize.X - 2, m_FrameSize.Y - 2);
            #endregion

            // From the finite state machine, call the appropriate state.
            CallState(m_CurrentState,pLevel,pGameTime,pInputHandler);

            // Truncate the speed in question
            //this.Forces["MovementSpeed"] = Truncate(this.Forces["MovementSpeed"], NORMAL_MOVEMENT_SPEED);

            this.Forces["Movement"] *= m_IsOnIce == true ? 0.98f : 0.93f;
            this.Forces["Gravity"] *= m_IsOnIce == true ? 0.98f : 0.93f;
            this.Forces["Collision"] *= m_IsOnIce == true ? 0.98f : 0.93f;

            // Generate the new velocity that is going to be applied.
            this.Velocity = CalculateForces();
            _currentposition += Velocity;


            #region Collision Box Checking
            //foreach (var item in Entities)
            //{
            //    // Make sure that it's an ant that we are dealing with here.
            //    if (item is Ant)
            //    {
            //        // Generate a new bounding box
            //        var _temporaryboundingBox = new BoundingBox(new Vector3(Position, 0),
            //                                                    new Vector3(Position.X + Size.X,
            //                                                                Position.Y + Size.Y, 0));
            //        if (_temporaryboundingBox.Contains(item.CollisionBox) != ContainmentType.Disjoint)
            //        {
            //            ApplyImpulse(item);
            //        }
            //    }
            //}
            #endregion

            #region Tiled Map Collision Checking
            // Check for collision on the X axis
            if (!pLevel.CheckCollision(_currentposition, new Rectangle((int)_currentposition.X - (int)Origin.X,
                                                                      (int)Position.Y - (int)Origin.Y,
                                                                      Size.X,
                                                                      Size.Y)))
            {
                this.Position = new Vector2(_currentposition.X, Position.Y);
            }
            else
            {
                if (m_CurrentState == "Attacking")
                {
                    ChangeState("MoveToPath");
                }
            }

            // Check for level collision on the Y axis
            if (!pLevel.CheckCollision(_currentposition, new Rectangle((int)Position.X - (int)Origin.X,
                                                                       (int)_currentposition.Y - (int)Origin.Y,
                                                                       Size.X,
                                                                       Size.Y)))
            {
                this.Position = new Vector2(Position.X, _currentposition.Y);
            }
            else
            {
                if (m_CurrentState == "Attacking")
                {
                    ChangeState("MoveToPath");
                }
            }

            #endregion


            #region Notification Handling
            // Allow for the notification to be displayed.
            if (m_DisplayNotification)
            {
                if (m_DisplayNotificationTimer == 0f)
                {
                    m_DisplayNotificationTimer = Environment.TickCount + m_DisplayNotificationDuration;
                }

                if (Environment.TickCount > m_DisplayNotificationTimer)
                {
                    m_DisplayNotificationTimer = 0f;
                    m_DisplayNotification = false;
                }
            }
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

            
            #region Collision Checking
            // Loop through the items within that.
            foreach (var item in Entity.Entities)
            {
                // Only want to deal with it if it is in fact a bullet
                if (!(item is Bullet))
                    continue;

                // Determine if there is some kind of collision at all with the bullet in question
                if (item.BoundingBox.Intersects(m_BoundingBox))
                {
                    m_IsHurt = true;
                    m_HurtRedOpacity = 0;

                    // Notify the player that some damage has been done
                    NotificationText.Notifications.Add(new NotificationText(true,"DAMAGE!", m_Position, true, Color.White, true));

                    // Apply a loss of health to the ant
                    LoseHealth(5);

                    // Bump the ant death count by one!
                    if (Dead)
                    {
                        pLevel.AntsKilled++;
                        ((Bullet)item).WeaponOrigin.Owner.OnKilledAnt(this, null);
                    }
                    item.Dead = true;
                }

            }

            #endregion

            base.Update(pGameTime, pInputHandler, pLevel);
        }

        // Clamp the speed for the vector in question
        public Vector2 Truncate(Vector2 pToTruncate, float pMax)
        {
            float i;
            i = pMax / pToTruncate.Length();
            i = i < 1.0f ? 1.0f : i;

            return pToTruncate * i;
        }

        /// <summary>
        /// Generates steering velocity towards the target in question
        /// </summary>
        /// <param name="pTarget">The target taht we are steering towards</param>
        /// <returns>Returns the final velocity that we are going to be applying</returns>
        public Vector2 Seek(Vector2 pTarget)
        {
            Vector2 _desiredvelocity = (pTarget - Position);
            _desiredvelocity.Normalize();
            _desiredvelocity *= MAX_SPEED;

            Vector2 _steeringforce = _desiredvelocity - m_Velocity;
            _steeringforce /= Mass;

            return _steeringforce;
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
           

            base.Update(pGameTime, pInputHandler);
        }
        #endregion
    }
}
