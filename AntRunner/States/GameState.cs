using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

using AntRunner.Entity;
using AntRunner.Menu;
using AntRunner.States;
using AntRunner.Cameras;
using AntRunner.Utility;

namespace AntRunner.States
{
    // Basic class container for the players in the game.
    public class PlayerManager : IEntity
    {
        private List<Player> m_Players;
        private int m_Score;

        public List<Player> Players
        {
            get { return m_Players; }
            set { m_Players = value; }
        }

        public PlayerManager()
        {
            m_Players = new List<Player>();
        }

        public void AddPlayer(Player pNewPlayer)
        {
            if (m_Players.Count < 4)
            {
                m_Players.Add(pNewPlayer);
            }
        }

        public void RemovePlayer(Player pPlayerRemove)
        {
            if (m_Players.Count > 0)
            {
                m_Players.Remove(pPlayerRemove);
            }
        }

        public void Initialize()
        {
        
        }

        // Camera is going to be causing the aiming thing more problems so this is something that we have to take into consideration.
        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel, CameraManager pCamera)
        {
            for (int i = 0; i < m_Players.Count; i++)
            {
                m_Players[i].Update(pGameTime, pLevel, pInputHandler,pCamera);
            }
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {
            for (int i = 0; i < m_Players.Count; i++)
            {
                m_Players[i].Update(pGameTime, pLevel, pInputHandler);
            }
        }

        public void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            for (int i = 0; i < m_Players.Count; i++)
            {
                m_Players[i].Update(pGameTime, pInputHandler);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            for (int i = 0; i < m_Players.Count; i++)
            {
                m_Players[i].Draw(pSpriteBatch);
            }
        }

        // Return the first player that is within the game.
        public Player PrimaryPlayer
        {
            get
            {
                if (m_Players.Count > 0)
                {
                    return m_Players[0];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (m_Players.Count > 0)
                {
                    m_Players[0] = value;
                }
            }
        }
    }

    public enum GameMode
    {
        Play,
        Spectator
    }

    public class GameState : State
    {
        #region Members
        private bool m_GamePaused;
        private Level m_Level;

        private GameMode m_GameMode = GameMode.Play;

        private CameraManager m_CameraManager;
        private const float CAMERA_CONTROL_SPEED = 5f;

        /// <summary>
        /// The two constants responsible for determine the values in which the ants will spawn between
        /// </summary>
        private const float MAX_SPAWN_TIME = 3000f;
        private const float MIN_SPAWN_TIME = 1500f;

        private int m_WhiteBarHealthAlpha = 1;
        private int m_WhiteBarAmmoAlpha = 1;

        // The amount of ants that are to be spawned left.
        private int m_AntSpawnCount = 0;
        private float m_AntSpawnCounter = 0f;

        // Used for generating random variables.
        private Random m_Random;
        private MenuManager m_PauseMenu;
        private PlayerManager m_PlayerManager;

        public int m_BluePlanningTime = 0;
        public int m_RedPlanningTime = 0;
        public int m_GreenPlanningTime = 0;

        // New clickwheel menu for dictating ants to do things.
        private Clickwheel m_ClickwheelMenu = null;

        private bool m_SpawnedPlayer = false;
        private bool m_SpawnedAnt = false;

        private Texture2D m_SelectionMarker = null;
        private Point m_SelectionArea = Point.Zero;
        private Ant m_SelectedAnt = null;
        private Vector2 m_SelectionPoint = Vector2.Zero;

        private SpawnType m_PreviouslySpawned = SpawnType.BlueAnt;

        private int m_BlueAntTraversalTime = 0;
        private int m_RedAntTraversalTime = 0;
        private int m_GreenAntTraversalTime = 0;
        private int m_YellowAntTraversalTime = 0;


        // Basic variable for determining whether or not the ants are allowed to spawn
        private bool m_AntsCanSpawn = true;
        private const int MAX_ANTS = 0;
        private bool m_GameStart;
        #endregion

        #region Properties
        public int RedAntTraversalTime
        {
            get { return m_RedAntTraversalTime; }
            set { m_RedAntTraversalTime = value; }
        }

        public int GreenAntTraversalTime
        {
            get { return m_GreenAntTraversalTime; }
            set { m_GreenAntTraversalTime = value; }
        }

        public int BlueAntTraversalTime
        {
            get { return m_BlueAntTraversalTime; }
            set { m_BlueAntTraversalTime = value; }
        }

        public int YellowAntTraversalTime
        {
            get { return m_YellowAntTraversalTime; }
            set { m_YellowAntTraversalTime = value; }
        }

        public Vector2 SelectionPoint
        {
            get { return m_SelectionPoint; }
            set { m_SelectionPoint = value; }
        }

        public bool GamePaused
        {
            get { return m_GamePaused; }
            set { m_GamePaused = value; }
        }

        public bool SpawnedPlayer
        {
            get { return m_SpawnedPlayer; }
            set { m_SpawnedPlayer = value; }
        }

        public PlayerManager PlayerManager
        {
            get { return m_PlayerManager; }
            set { m_PlayerManager = value; }
        }

        public CameraManager CameraManager
        {
            get { return m_CameraManager; }
            set { m_CameraManager = value; }
        }

        public Level Level
        {
            get { return m_Level; }
            set { m_Level = value; }
        }
        #endregion

        public GameState()
        {
            m_Level = new Level() { GameStateRef = this } ;

            // Generate a new random object.
            this.m_Random = new Random();
            
            m_GamePaused = false;
            m_GameStart = false;

            m_PreviouslySpawned = SpawnType.None;

            m_PlayerManager = new PlayerManager();
            m_ClickwheelMenu = new Clickwheel(Vector2.Zero, m_Level);

            // Create a new texture that is used for plotting the marker.
            m_SelectionMarker = Utility.ColourTexture.Create(MainGame.Instance.GraphicsDevice,64,64, Color.White);

            Initialize();
        }

        // Restart the game so that it's back to normal.
        public void RestartState()
        {
            // Clear out the list of entities
            Entity.Entity.Entities.Clear();
        }

        /// <summary>
        /// Generate a random time value that is used for counting in the next ant into the world
        /// </summary>
        /// <param name="pMin">The minimum time required</param>
        /// <param name="pMax">The max random time value that we are using</param>
        /// <returns>Returns the new random value</returns>
        public float GenerateRandomSpawnTime(float pMin, float pMax)
        {
            // Return a random number that is amplified by the max number
            return pMin + (float)m_Random.NextDouble() * (pMax - pMin);
        }

        public override void Initialize()
        {
            // Create a new menu for pausing
            if (m_Level != null)
            {
                m_PlayerManager.AddPlayer(new Player(0));
            }
            m_PauseMenu = new MenuManager("Main Menu", new Vector2((MainGame.Instance.Window.ClientBounds.Width / 2) - 125, MainGame.Instance.Window.ClientBounds.Height / 2));
            m_PauseMenu.Active = false;
            
            // Manager that is responsible for dealing with the viewing angles
            if (m_PlayerManager.Players.Count > 0)
            {
                m_CameraManager = new CameraManager();
                m_CameraManager.AddCamera(new Camera(m_PlayerManager.Players[0], 1.0f, 0.0f));
            }

            #region Menu Item Loading
            m_PauseMenu.AddMenuItem(new MenuItem()
            {
                Message = "RESUME",
                Action = delegate(object sender, EventArgs e)
                {
                    m_GamePaused = false;
                    m_PauseMenu.Active = false;
                }
            });
            
            m_PauseMenu.AddMenuItem(new MenuItem()
            {
                Message = "QUIT",
                Action = delegate(object sender, EventArgs e)
                {
                    MainGame.Instance.StateValue = StateValue.Menu;
                }
            });

            m_PauseMenu.AddMenuItem(new MenuItem()
            {
                Message = "SETTINGS",
                Action = delegate(object sender, EventArgs e)
                {
                    MainGame.Instance.StateValue = StateValue.Settings;
                }
            });

            m_ClickwheelMenu.Active = true;
            // Add in the new items that are going to appear on the click wheel.
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Move cake", MainGame.Instance.Textures["cake_slice"])
                {
                    Enabled = true
                });
            //this.Items.Add(new ClickwheelItem("Move here", MainGame.Instance.Textures[" "]));
            //this.Items.Add(new ClickwheelItem("Gather", MainGame.Instance.Textures[""]));
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Build Shack", MainGame.Instance.Textures["hammer_icon"])
                {
                    Enabled = true
                });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Dig here", MainGame.Instance.Textures["spade"])
                {
                    Enabled = true
                });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Force Replan", MainGame.Instance.Textures["spade"])
                {
                    Enabled = true
                });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Spawn Box", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {

                    // Determine that the part of the level is clear.
                    if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                        (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                    {
                        Entity.Entity.Entities.Add(new NewBox(this.m_SelectionPoint, 1f, 0f));
                    }
                    else
                    {
                        NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN THERE", m_SelectionPoint, true, Color.White, true));
                    }
                }
            });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Select Target", MainGame.Instance.Textures["spade"])
                {
                    Enabled = true
                });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Add Spawn Point", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    m_ClickwheelMenu.DisplayChild(1);
                }
            });
            m_ClickwheelMenu.Items.Add(new ClickwheelItem("Spawn Ant", MainGame.Instance.Textures["ant_menu_icon"])
                {
                    Enabled = true,
                    Action = delegate(object sender, EventArgs e)
                    {
                        // Display the ant spawn menu.
                        m_ClickwheelMenu.DisplayChild(0);
                    }
                });
            m_ClickwheelMenu.Children.Add(new Clickwheel(m_ClickwheelMenu.Position, m_Level));

            // The items for generating new ants within the environment.
            m_ClickwheelMenu.Children[0].AddMenuItem(new ClickwheelItem("Spawn Yellow Ant",MainGame.Instance.Textures["yellow_ant_icon"])
                {
                    Enabled = true,
                    Action = delegate(object sender, EventArgs e)
                    {
                        if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                            (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                        {
                            m_PreviouslySpawned = SpawnType.YellowAnt;

                            // Generate a new ant at the given position
                            Entity.Entity.Entities.Add(new YellowAnt(m_SelectionPoint, null));
                        }
                        else
                        {
                            NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN THERE", m_SelectionPoint, true, Color.White, true));
                        }
                    }
                });
            m_ClickwheelMenu.Children[0].AddMenuItem(new ClickwheelItem("Spawn Red Ant",MainGame.Instance.Textures["red_ant_icon"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {

                    if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                        (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                    {
                        m_PreviouslySpawned = SpawnType.RedAnt;
                        Entity.Entity.Entities.Add(new RedAnt(m_SelectionPoint, null));

                        // Generate a new ant at the given position
                        // Entity.Entity.Entities.Add(new RedAnt(m_SelectionPoint, MainGame.Instance.Textures["ant_spritesheet"]));
                    }
                    else
                    {
                        NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN THERE", m_SelectionPoint, true, Color.White, true));
                    }
                }
            });
            m_ClickwheelMenu.Children[0].AddMenuItem(new ClickwheelItem("Spawn Green Ant",MainGame.Instance.Textures["green_ant_icon"])
                {
                    Enabled = true,
                    Action = delegate(object sender, EventArgs e)
                    {
                        if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                            (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                        {
                            m_PreviouslySpawned = SpawnType.GreenAnt;
                            Entity.Entity.Entities.Add(new GreenAnt(m_SelectionPoint,m_Level));
                        }
                        else
                        {
                            NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN THERE", m_SelectionPoint, true, Color.White, true));
                        }
                    }
                });
            m_ClickwheelMenu.Children[0].AddMenuItem(new ClickwheelItem("Spawn Blue Ant", MainGame.Instance.Textures["blue_ant_icon"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                        (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                    {
                        m_PreviouslySpawned = SpawnType.BlueAnt;
                        Entity.Entity.Entities.Add(new BlueAnt(m_SelectionPoint));
                    }
                    else
                    {
                        NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN THERE", m_SelectionPoint, true, Color.White, true));
                    }
                }
            });

            // For displaying the child menus
            m_ClickwheelMenu.Children.Add(new Clickwheel(m_ClickwheelMenu.Position, m_Level));
            m_ClickwheelMenu.Children[1].AddMenuItem(new ClickwheelItem("Player Spawn Point", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    // Generate a new spawn point for the player
                    this.Level.PlayerSpawnPoints.Add(new SpawnPoint(SpawnType.Player));
                }
            });

            m_ClickwheelMenu.Children[1].AddMenuItem(new ClickwheelItem("Red Ant Spawn Point", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    // Generate a new spawn point for the player
                    this.Level.PlayerSpawnPoints.Add(new SpawnPoint(SpawnType.RedAnt));
                }
            });

            m_ClickwheelMenu.Children[1].AddMenuItem(new ClickwheelItem("Blue Ant Spawn Point", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    // Generate a new spawn point for the player
                    this.Level.PlayerSpawnPoints.Add(new SpawnPoint(SpawnType.BlueAnt));
                }
            });

            m_ClickwheelMenu.Children[1].AddMenuItem(new ClickwheelItem("Green Ant Spawn Point", MainGame.Instance.Textures["spade"])
            {
                Enabled = true,
                Action = delegate(object sender, EventArgs e)
                {
                    // Generate a new spawn point for the player
                    this.Level.PlayerSpawnPoints.Add(new SpawnPoint(SpawnType.GreenAnt));
                }
            });

           // m_ClickwheelMenu.Children[1].AddMenuItem(new ClickwheelItem("Add Player Spawn Point", MainGame.Instance.Textures[""]));

            #endregion

            //m_Level.Initialize();
            base.Initialize();
        }

        /// <summary>
        /// The type of map that we are going to be using
        /// </summary>
        /// <param name="pGameMode">The game mode enumerator</param>
        public void SetGameType(GameMode pGameMode)
        {
            // Set the required parameters of the game accordingly.
            switch (pGameMode)
            {
                case GameMode.Play:
                    // Get rid of any previously existing cameras before hand
                    m_CameraManager.Cameras.Clear();
                    if (m_PlayerManager.Players.Count == 0)
                    {
                        m_PlayerManager.Players.Add(new Player(0));
                    }
                    m_CameraManager.AddCamera(new Camera(m_PlayerManager.Players[0],1f,0f));;
                    m_GameMode = GameMode.Play;
                break;

                case GameMode.Spectator:
                    // Add a new camera that we can use.
                    m_CameraManager.Cameras.Clear();
                    m_CameraManager.AddCamera(new Camera() { Position = new Vector2(0f,0f), Rotation = 0f, Scale = 1f });
                    m_GameMode = GameMode.Spectator;

                    // Clear out the players as we no longer reuqire them.
                    m_PlayerManager.Players.Clear();
                
                break;
            }
        }

        // Generate a random spawn interval that is going to be used for timing the next spawn
        public float NextRandomSpawnTime()
        {
            return  MIN_SPAWN_TIME + (float) m_Random.NextDouble() * (MAX_SPAWN_TIME - MIN_SPAWN_TIME);
        }

        /// <summary>
        /// Make the health bar flash white temporarily when there is a health potion that is picked up
        /// </summary>
        public void FlashHealthHUD()
        {
            m_WhiteBarHealthAlpha = 255;
        }

        /// <summary>
        /// Make the ammo bar flash white temporarily
        /// </summary>
        public void FlashAmmoHUD()
        {
            m_WhiteBarAmmoAlpha = 255;
        }


        /// <summary>
        /// Place a new ant in the environment
        /// </summary>
        public void SpawnAnt()
        {
            // Determine that we have not reached the limit of ants that are to be spawned in the
            // environment.
            if (Entity.Entity.Entities.Where(n => n is Ant).ToList().Count < MAX_ANTS)
            {
                SpawnPoint _randomSpawnPoint = Level.SpawnPoints[m_Random.Next(0, Level.SpawnPoints.Count)];

                switch (_randomSpawnPoint.SpawnType)
                {
                    case SpawnType.YellowAnt:
                        Entity.Entity.Entities.Add(new Ant(_randomSpawnPoint.Position, _randomSpawnPoint));
                    break;

                    case SpawnType.GreenAnt:
                        Entity.Entity.Entities.Add(new GreenAnt(_randomSpawnPoint.Position, _randomSpawnPoint));
                    break;

                    case SpawnType.BlueAnt:
                        // Generate a new ant that is responsible for Hierarchical Task Networks
                        Entity.Entity.Entities.Add(new BlueAnt(_randomSpawnPoint.Position,_randomSpawnPoint));
                    break;

                    case SpawnType.RedAnt:
                        Entity.Entity.Entities.Add(new RedAnt(_randomSpawnPoint.Position, _randomSpawnPoint));
                    break;
                }
                //Entity.Entity.Entities.Add(new Ant(_randomSpawnPoint.Position, _randomSpawnPoint));
            }
        }

        /// <summary>
        /// Where all the game logic within the game happens
        /// </summary>
        /// <param name="pGameTime">The delta time object that is used</param>
        /// <param name="pInputHandler">The input handler object.</param>
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {

            m_Level.Update(pGameTime, pInputHandler);

            // Register the input for the mouse if we're considered to be in the Spectator mode.
            if (m_GameMode == GameMode.Spectator)
            {
                if (m_SelectedAnt != null)
                {
                    if (pInputHandler.IsKeyDownOnce(Keys.Delete))
                    {
                        m_SelectedAnt.Dead = true;
                        NotificationText.Entities.Add(new NotificationText(true, "KILLED!", m_SelectedAnt.Position, true, Color.Red, true));
                    }
                }

                // Update the clickwheel menu for any further interactions
                m_ClickwheelMenu.Update(pGameTime, pInputHandler);

                // Make where on the map that it has been selected.
                if (!m_ClickwheelMenu.IsActive() &&
                    pInputHandler.IsLeftMouseButtonDownOnce())
                {
                    m_SelectionPoint = pInputHandler.GetMouseToWorld(m_CameraManager.ActiveCamera().GetMatrix());
                    m_SelectionArea = new Point((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                                (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight);
                }

                // Determine if the mouse button is down once
                if (pInputHandler.IsLeftMouseButtonDownOnce())
                {
                    Rectangle _mouseCollisionBox = new Rectangle((int)m_SelectionPoint.X, (int)m_SelectionPoint.Y, 32, 32);

                    // Loop through the entities and find the one that collides with the mouse coords
                    foreach (var item in Entity.Entity.Entities)
                    {
                        if (item is Ant)
                        {
                            if (item.BoundingBox.Intersects(_mouseCollisionBox))
                            {
                                m_SelectedAnt = (Ant)item;
                                break;
                            }
                        }
                        m_SelectedAnt = null;
                    }
                }

            }
            else
            {
                m_ClickwheelMenu.Active = false;
            }

            // For re-spawning the same ant more than once.
            // spamm!!!
            if (pInputHandler.IsKeyDownOnce(Keys.Space))
            {
                // Determine first that the selected area is not in fact blocked.
                if (m_Level.IsClear((int)m_SelectionPoint.X / m_Level.TMXLevel.TileWidth,
                                    (int)m_SelectionPoint.Y / m_Level.TMXLevel.TileHeight))
                {
                    switch (m_PreviouslySpawned)
                    {
                        case SpawnType.BlueAnt:
                            Entity.Entity.Entities.Add(new BlueAnt(m_SelectionPoint));
                        break;

                        case SpawnType.GreenAnt:
                            Entity.Entity.Entities.Add(new GreenAnt(m_SelectionPoint, m_Level));            
                        break;

                        case SpawnType.RedAnt:
                            Entity.Entity.Entities.Add(new RedAnt(m_SelectionPoint,null));
                        break;

                        case SpawnType.YellowAnt:
                            Entity.Entity.Entities.Add(new YellowAnt(m_SelectionPoint,null));
                        break;
                    }
                }
                else
                {
                    NotificationText.Entities.Add(new NotificationText(true, "CAN'T SPAWN HERE!", pInputHandler.GetMouseToWorld(m_CameraManager.ActiveCamera().GetMatrix()),true,Color.White,true));
                }
            }

            // Get that gradual fading effect that we want.
            if (m_WhiteBarAmmoAlpha > 0)
                m_WhiteBarHealthAlpha -= 5;
            else
                m_WhiteBarAmmoAlpha = 0;

            if (m_WhiteBarHealthAlpha > 0)
                m_WhiteBarHealthAlpha -= 5;
            else
                m_WhiteBarHealthAlpha = 0;

            // Only allow the player to control and do something
            if (m_GameStart)
            {
                //m_Player.Update(pGameTime, pInputHandler);
            }

            // Determine first whether or not ants can spawn
            if (m_AntsCanSpawn)
            {
                if (Environment.TickCount > m_AntSpawnCounter)
                {
                    SpawnAnt();
                    m_AntSpawnCounter = Environment.TickCount + NextRandomSpawnTime();
                }
            }

            // Do this once
            if (!m_SpawnedPlayer)
            {
                // Only do this if there are no players within the environment.
                if (m_PlayerManager.Players.Count > 0)
                {
                    foreach (var item in m_Level.PlayerSpawnPoints)
                    {
                        m_PlayerManager.Players[0].Position = item.Position;
                        break;
                    }
                    m_SpawnedPlayer = true;
                }
            }

            if (!m_SpawnedAnt)
            {
                //foreach (var item in m_Level.SpawnPoints)
                //{
                //    Entity.Entity.Entities.Add(new Ant(item.Position,item));
                //    break;
                //}

                //Entity.Entity.Entities.Add(new Ant(m_Level.SpawnPoints[1].Position,m_Level.SpawnPoints[1]));

                m_SpawnedAnt = true;
            }

            // Show the menu again if the user wants to bail!
            if (pInputHandler.KeyboardButtonPressed(Keys.Escape))
            {
                m_PauseMenu.Active = m_PauseMenu.Active == true ? false : true;
                m_GamePaused = m_GamePaused == true ? false : true;
            }
            if (!m_PauseMenu.Active)
            {
                m_CameraManager.Update(pGameTime, pInputHandler);
            }
            // Update the text
            List<NotificationText> _deadtext = new List<NotificationText>();
            foreach (var item in NotificationText.Notifications)
            {
                item.Update(pGameTime, pInputHandler);
                if (item.Dead)
                    _deadtext.Add(item);
            }

            // Loop through the list that is meant to be text that is to be cleared
            foreach (var item in _deadtext)
            {
                NotificationText.Notifications.Remove(item); 
            }

            // Update the particle emitters that exist.
            foreach (var item in Particles.ParticleEmitter.Emitters)
            {
                item.Update(pGameTime, pInputHandler);
            }

            m_PlayerManager.Update(pGameTime, pInputHandler, this.m_Level, m_CameraManager);
            m_Level.Update(pGameTime, pInputHandler);

            // List of dead entities that are going to be cleared up.
            List<Entity.Entity> _deadgameitems = new List<Entity.Entity>();
            foreach (var item in Entity.Entity.GameItems)
            {
                if (item.Dead)
                {
                    _deadgameitems.Add(item);
                    continue;
                }

                item.Update(pGameTime, pInputHandler, this.m_Level);
            }

            List<Entity.Entity> _deadentities = new List<Entity.Entity>();
            foreach (var item in Entity.Entity.Entities)
            {
                // Important that we do this before anything else
                if (item.Dead)
                {
                    _deadentities.Add(item);
                    continue;
                }

                if (!(item is Bullet) && !(item is Ant))
                {

                    item.Update(pGameTime, pInputHandler);
                }

                if (item is Bullet) 
                {
                    // Determine that it's a bullet object we're dealing with. If it is, then pass through the level parameter.
                    ((Bullet)item).Update(pGameTime, pInputHandler, this.m_Level);
                }

                if (item is Ant)
                {
                    ((Ant)item).Update(pGameTime, pInputHandler, this.m_Level);
                }

                if (item is Box)
                {
                    ((Box)item).Update(pGameTime, pInputHandler, this.m_Level);
                }

                if (item is NewBox)
                {
                    ((NewBox)item).Update(pGameTime, pInputHandler, this.m_Level);
                }
            }

            // Go through the temporary list of dead entities and clean them up
            foreach (var item in _deadentities)
            {
                item.Dispose();
            }

            m_PauseMenu.Update(pGameTime, pInputHandler);

            base.Update(pGameTime, pInputHandler);
        }

        /// <summary>
        /// All the rendering for the GameState is carried out here
        /// </summary>
        /// <param name="pSpriteBatch">The object that is required for rendering</param>
        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Draw them appropriately.
            pSpriteBatch.Begin(SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                null,
                null,
                null,
                null,
                m_CameraManager.ActiveCamera().GetMatrix());

            pSpriteBatch.GraphicsDevice.Clear(new Color(121,85,58,255));

            m_Level.Draw(pSpriteBatch);
            m_PlayerManager.Draw(pSpriteBatch);

            // Loop through the specific game items in the environment and update them accordingly.
            foreach (var item in Entity.Entity.GameItems)
            {
                if (!item.Dead)
                    item.Draw(pSpriteBatch);
            }

            foreach (var item in Entity.Entity.Entities)
            {
                if (!item.Dead)
                {
                    item.Draw(pSpriteBatch);
                }
            }

            foreach (var item in NotificationText.Notifications)
            {
                item.Draw(pSpriteBatch);
            }

            // Loop through the emitters and draw them as is
            foreach (var item in Particles.ParticleEmitter.Emitters)
            {
                item.Draw(pSpriteBatch);
            }

            // Return whether or not an ant has been selected
            if (m_SelectedAnt != null)
            {
                pSpriteBatch.Draw(MainGame.Instance.Textures["selection_arrow"], new Vector2(m_SelectedAnt.Position.X - 15, m_SelectedAnt.Position.Y - 75), Color.White);
            }


            if (m_SelectionPoint != Vector2.Zero && m_GameMode == GameMode.Spectator)
            {
                pSpriteBatch.Draw(MainGame.Instance.Textures["selection_cursor"], new Vector2(m_SelectionArea.X * m_Level.TMXLevel.TileWidth,
                                                                                              m_SelectionArea.Y * m_Level.TMXLevel.TileHeight), Color.White);
            }

            pSpriteBatch.End();

            pSpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            /** DRAW THE GUI HERE **/

            pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"], "HEALTH", new Vector2(25, MainGame.Instance.Window.ClientBounds.Height - 75), Color.White);

            int _healthbar = 0;
            int _ammobar = 0;

            if (m_PlayerManager.PrimaryPlayer != null)
            {
                _healthbar = (MainGame.Instance.Textures["healthbar_full"].Width / m_PlayerManager.Players[0].MaxHealth) * m_PlayerManager.Players[0].Health;
                _ammobar = (MainGame.Instance.Textures["ammobar_small"].Width / m_PlayerManager.Players[0].MaxAmmo) * m_PlayerManager.Players[0].Ammo;
            }

            pSpriteBatch.Draw(MainGame.Instance.Textures["emptybar_small"], new Vector2(25, MainGame.Instance.Window.ClientBounds.Height - 50), Color.White);
            pSpriteBatch.Draw(MainGame.Instance.Textures["healthbar_full"], new Vector2(25, MainGame.Instance.Window.ClientBounds.Height - 50), new Rectangle(0, 0, _healthbar, MainGame.Instance.Textures["healthbar_full"].Height), Color.White);
           // pSpriteBatch.Draw(MainGame.Instance.Textures["white_bar"], new Vector2(25, MainGame.Instance.Window.ClientBounds.Height - 50), new Color(255,255,255,m_WhiteBarHealthAlpha));

            // Output the current weapon name 
            if (PlayerManager != null && PlayerManager.Players != null)
            {
                if (PlayerManager.Players.Count > 0)
                {
                    Player _player = PlayerManager.Players[0];
                    pSpriteBatch.DrawString(
                        MainGame.Instance.Fonts["debug_font"],
                        _player.Weapons[_player.WeaponActive].m_WeaponName.ToString(),
                        new Vector2(MainGame.Instance.Window.ClientBounds.Width - MainGame.Instance.Textures["emptybar_small"].Width - 25, MainGame.Instance.Window.ClientBounds.Height - 100),
                        Color.White);
                }
            }
            pSpriteBatch.DrawString(MainGame.Instance.Fonts["debug_font"], "AMMO", new Vector2(MainGame.Instance.Window.ClientBounds.Width - MainGame.Instance.Textures["emptybar_small"].Width - 25, MainGame.Instance.Window.ClientBounds.Height - 75), Color.White);

            pSpriteBatch.Draw(MainGame.Instance.Textures["emptybar_small"], new Vector2(MainGame.Instance.Window.ClientBounds.Width - MainGame.Instance.Textures["emptybar_small"].Width - 25, MainGame.Instance.Window.ClientBounds.Height - 50), Color.White);
            pSpriteBatch.Draw(MainGame.Instance.Textures["ammobar_small"], new Vector2(MainGame.Instance.Window.ClientBounds.Width - MainGame.Instance.Textures["emptybar_small"].Width - 25, MainGame.Instance.Window.ClientBounds.Height - 50), new Rectangle(0, 0, _ammobar, MainGame.Instance.Textures["ammobar_small"].Height), Color.White);
          //  pSpriteBatch.Draw(MainGame.Instance.Textures["white_bar"], new Vector2(MainGame.Instance.Window.ClientBounds.Width - MainGame.Instance.Textures["emptybar_small"].Width - 25, MainGame.Instance.Window.ClientBounds.Height - 50), new Color(255, 255, 255, m_WhiteBarAmmoAlpha));

            // Render the position of the camera that we are using to navigate the game.
            ShadowText.Draw(m_CameraManager.ActiveCamera().Position.ToString(), pSpriteBatch, new Vector2(0, 250));

            // Display that we are in the spectator mode.
            if (m_GameMode == GameMode.Spectator)
            {
                
                Vector2 _textvector = Vector2.Zero;
                ShadowText.Draw("SPECTATOR MODE", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width / 2, _textvector.Y));

                ShadowText.Draw("RED ANTS", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y),Color.Red);

                _textvector += new Vector2(0, 20);
                int _redants = Entity.Entity.Entities.Where(n => n.GetType() == typeof(RedAnt)).ToList().Count;
                ShadowText.Draw(_redants.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);
                
                _textvector += new Vector2(0, 20);
                ShadowText.Draw("BLUE ANTS", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Blue);

                _textvector += new Vector2(0, 20);
                int _blueants = Entity.Entity.Entities.Where(n => n.GetType() == typeof(BlueAnt)).ToList().Count;
                ShadowText.Draw(_blueants.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw("YELLOW ANTS", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Yellow);
                _textvector += new Vector2(0, 20);
                int _yellowants = Entity.Entity.Entities.Where(n => n.GetType() == typeof(YellowAnt)).ToList().Count;
                ShadowText.Draw(_yellowants.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);
                
                _textvector += new Vector2(0, 20);
                int _greenants = Entity.Entity.Entities.Where(n => n.GetType() == typeof(GreenAnt)).ToList().Count;
                ShadowText.Draw("GREEN ANTS", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Green);
                _textvector += new Vector2(0, 20);
                
                ShadowText.Draw(_greenants.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw("RED ANTS TIME", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Red);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw(this.RedAntTraversalTime.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw("BLUE ANTS TIME", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Blue);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw(this.BlueAntTraversalTime.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw("YELLOW ANTS TIME", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Yellow);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw(this.YellowAntTraversalTime.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw("GREEN ANTS TIME", pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.Green);

                _textvector += new Vector2(0, 20);
                ShadowText.Draw(this.GreenAntTraversalTime.ToString(), pSpriteBatch, new Vector2(MainGame.Instance.Window.ClientBounds.Width - 250, _textvector.Y), Color.White);

                //ShadowText.Draw(
            }


            if (m_ClickwheelMenu.Active || m_ClickwheelMenu.DisplayActiveChild)
            {
                m_ClickwheelMenu.Draw(pSpriteBatch);
            }

            // Only display the pause menu if it's actually paused.
            if (m_GamePaused)
            {
                m_PauseMenu.Draw(pSpriteBatch);
            }

            pSpriteBatch.End();

            base.Draw(pSpriteBatch);
        }
    }
}
