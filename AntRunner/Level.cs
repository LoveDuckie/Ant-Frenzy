using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.Storage.Search;
using Windows.Storage;
using Windows.Storage.Streams;


// Include the entire XML namespace as this is required
// for loading in the tmx files that we want to use.
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

using Windows.Data.Xml;
using Windows.ApplicationModel;

using Windows.ApplicationModel.Core;

using AntRunner.Entity;
using AntRunner.Utility;
using AntRunner.States;
using AntRunner.Cameras;

namespace AntRunner
{
    #region Enumerators
    public enum SpawnType
    {
        BlueAnt = 1,
        RedAnt,
        GreenAnt,
        YellowAnt,
        Player,
        None
    }

    public enum TimePeriod
    {
        Short = 1,
        Medium,
        Long
    }

    public enum TileType
    {
        Poison = 1,
        Grass,
        Dirt,
        Rock,
        Ice,
        None
    }

    public enum LevelType
    {
        Tmx,
        None
    }

    public enum CollisionType
    {
        IsAbove = 1,
        Intersected
    }
    #endregion

    public class SpawnPoint : Entity.Entity
    {
        private Type m_AntType = null;
        private SpawnType m_SpawnType = SpawnType.GreenAnt;
        private TimePeriod m_SpawnPeriod = TimePeriod.Long;

        // Time based stuff if we want this spawn point ot generate a new ant
        // every few seconds
        private int m_Timer = -1;
        private int m_NextTimer = 0;
        private bool m_Timerbased = false;

        #region Properties
        public SpawnType SpawnType
        {
            get { return m_SpawnType; }
            set { m_SpawnType = value; }
        }


        public Type AntType
        {
            get { return m_AntType; }
            set { m_AntType = value; }
        }

        #endregion

        #region Constructors
        public SpawnPoint()
        {
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
        }

        public SpawnPoint(SpawnType pSpawnType, bool pTimerbased)
        {
            m_SpawnType = pSpawnType;
            m_Timerbased = pTimerbased;
        }

        public SpawnPoint(Type pSpawnType)
        {
            m_AntType = pSpawnType;
            this.m_SpriteSheet = MainGame.Instance.Textures["terrain_tiles"];
        }

        public SpawnPoint(SpawnType pSpawnType)
        {
            this.m_SpawnType = pSpawnType;
        }
        #endregion

        #region Methods
        public override void Initialize()
        {
            this.m_SpawnPeriod = TimePeriod.Long;
            this.m_Timer = -1;

            base.Initialize();
        }

        // Change the spawn interval timing
        public void ToggleTimer()
        {
            
        }
        
        /// <summary>
        /// Called whenever we want to spawn an ant.
        /// </summary>
        public void SpawnAnt()
        {
            switch (m_SpawnType)
            {
                case AntRunner.SpawnType.BlueAnt:
                    Entity.Entity.Entities.Add(new BlueAnt(Position,this));
                break;

                case AntRunner.SpawnType.GreenAnt:
                    Entity.Entity.Entities.Add(new GreenAnt(Position,this));
                break;

                case AntRunner.SpawnType.RedAnt:
                    Entity.Entity.Entities.Add(new RedAnt(Position,this));
                break;

            }
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            this.CollisionBox = new BoundingBox(new Vector3(Position, 0), new Vector3(Position.X + 64, Position.Y + 64, 0));
            this.BoundingBox = new Rectangle((int)Position.X, (int)Position.Y, 64, 64);

            // If a timer has been specified, then spawn at the given interval
            if (m_Timer != -1)
            {
                if (Environment.TickCount >= m_NextTimer)
                {
                    SpawnAnt();
                }
            }
            
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            // Write more of the rendering code here for the spawn point

            pSpriteBatch.Draw(m_SpriteSheet, m_Position, new Rectangle(8 * 64, 10 * 64, 64, 64), Color.White);
            base.Draw(pSpriteBatch);
        }
        #endregion
    }

    public class Tileset
    {
        #region Members
        private string m_TilesetName;
        private int m_Height;
        private int m_Width;
        private int m_TileHeight;
        private int m_TileWidth;
        private Texture2D m_Spritesheet;
        private int m_FirstGID;
        private string m_ImageSource;
        #endregion

        #region Properties
        public int TileHeight
        {
            get { return m_TileHeight; }
            set { m_TileHeight = value; }
        }

        public string ImageSource
        {
            get { return m_ImageSource; }
            set { m_ImageSource = value; }
        }

        public int FirstGID
        {
            get { return m_FirstGID; }
            set { m_FirstGID = value; }
        }

        public int TileWidth
        {
            get { return m_TileWidth; }
            set { m_TileWidth = value; }
        }

        public string TilesetName
        {
            get { return m_TilesetName; }
            set { m_TilesetName = value; }
        }

        public int Height
        {
            get { return m_Height; }
            set { m_Height = value; }
        }

        public int Width
        {
            get { return m_Width; }
            set { m_Width = value; }
        }
        #endregion

        #region Constructor
        public Tileset()
        {

        }

        public Tileset(string pTilename, int pTileHeight, int pTileWidth, int pHeight, int pWidth)
        {
            this.m_Height = pHeight;
            this.m_Width = pWidth;
            this.m_TileWidth = pTileWidth;
            this.m_TileHeight = pTileHeight;
        }
        #endregion


    }

    // Level object for our class
    public class Level : IEntity
    {
        #region Members
        private GameState m_GameStateRef;

        private string m_LevelName;
        private TMXLevel m_TMXLevel;
        private Texture2D m_SpriteSheet;
        private TileType[,] m_LevelTiles;
        private Point m_TileSize;
        private Point m_LevelSize;
        #endregion

        #region Properties
        public List<Cake> Cakes
        {
            get { return m_Cakes; }
            set { m_Cakes = value; }
        }

        public DangerMap DangerMap
        {
            get { return m_DangerMap; }
            set { m_DangerMap = value; }
        }

        public GameState GameStateRef
        {
            get { return m_GameStateRef; }
            set { m_GameStateRef = value; }
        }

        public TMXLevel TMXLevel
        {
            get { return m_TMXLevel; }
            set { m_TMXLevel = value; }
        }

        public Point LevelSize
        {
            get { return m_LevelSize; }
            set { m_LevelSize = value; }
        }

        public Point TileSize
        {
            get { return m_TileSize; }
            set { m_TileSize = value; }
        }

        public TileType[,] LevelTiles
        {
            get { return m_LevelTiles; }
            set { m_LevelTiles = value; }
        }

        public List<SpawnPoint> SpawnPoints
        {
            get { return m_SpawnPoints; }
            set { m_SpawnPoints = value; }
        }

        public int AntsKillAmount
        {
            get { return m_AntsKillAmount; }
            set { m_AntsKillAmount = value; }
        }

        public List<SpawnPoint> PlayerSpawnPoints
        {
            get { return m_PlayerSpawnPoints; }
            set { m_PlayerSpawnPoints = value; }
        }

        public List<BlackHole> BlackHoles
        {
            get { return m_BlackHoles; }
            set { m_BlackHoles = value; }
        }

        public int AntsKilled
        {
            get { return m_AntsKilled; }
            set { m_AntsKilled = value; }
        }
        #endregion

        private Random m_Random = new Random();

        private static StorageFolder LEVEL_FOLDER;

        private List<SpawnPoint> m_PlayerSpawnPoints;
        private List<SpawnPoint> m_SpawnPoints;

        private DangerMap m_DangerMap;

        // For interactive items within the environment.
        private List<Cake> m_Cakes;
        private List<BlackHole> m_BlackHoles = new List<BlackHole>(); // Used for influencing the gravitational forces.

        // Slighty adjustment to get the kind of behaviour that we are after
        public const float ICE_FRICTION = 1.05f;
        public const float  DIRT_FRICTION = 0.94f;

        private int m_AntsKilled = 0;
        private int m_AntsKillAmount = 0;

        #region Constructors
        public Level()
        {
            this.Initialize();
        }
         
        public Level(int pWidth, int pHeight, Point pTileSize)
        {
            m_TileSize = pTileSize;
            m_LevelSize = new Point(pWidth, pHeight);

            m_DangerMap = new DangerMap(this);

            this.Initialize();
        }
        #endregion

        #region Destructors
        ~Level()
        {
            this.m_Cakes.Clear();
            this.m_PlayerSpawnPoints.Clear();
            this.m_SpawnPoints.Clear();
        }
        #endregion

        // Return a list of files that are responsible for the games levels.
        public static async Task<List<TMXLevel>> GetLevelList()
        {
            var _return = await LEVEL_FOLDER.GetFilesAsync();
            List<TMXLevel> _levelsReturn = new List<TMXLevel>();

            // Loop through the files that are in the folder and load them as an object.
            // After that, add them to the list for returning
            foreach (var item in _return.Where(n => n.Name.Contains(".tmx")))
            {
                // The level that we want to load in
                TMXLevel _level = await TMXLoader.LoadTMX(LEVEL_FOLDER,item.Name);
                _levelsReturn.Add(_level);
            }

            // Send back the levels that we just loaded.
            return _levelsReturn;
        }

        public static async Task<IEnumerable<StorageFile>> GetLevelFiles()
        {
            var _return = await LEVEL_FOLDER.GetFilesAsync();
            List<StorageFile> _levels = new List<StorageFile>();

            // Return the list of levels
            return _return;
        }

        /// <summary>
        /// Return whether or not the given grid co-ordinate is considered to be clear.
        /// </summary>
        /// <param name="pX">X co-ordinate</param>
        /// <param name="pY">Y co-ordinate</param>
        /// <returns>Whether or not the co-ordinate is clear</returns>
        public bool IsClear(int pX, int pY)
        {
            // Make sure that the coordinate is within the bounds
            if (pX >= 0 && pX < TMXLevel.Width &&
                pY >= 0 && pY < TMXLevel.Height)
            {
                return (this.TMXLevel.Grid[pX, pY, 0].ID == 146);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Determine whether the provided coordinates are within the bounds of the map.
        /// </summary>
        /// <param name="pX">X axis</param>
        /// <param name="pY">Y axis</param>
        /// <returns>Returns whether or not it's within the bounds.</returns>
        public bool WithinBounds(int pX, int pY)
        {
            if (pX < 0 || pX >= TMXLevel.Width ||
                pY < 0 || pY >= TMXLevel.Height)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Take in world coordinates and return whether or not there is a collision on the grid
        /// </summary>
        /// <param name="pX">The x coordinate</param>
        /// <param name="pY">The y coordinate</param>
        /// <returns>Returns whether that coordinate is considered clear.</returns>
        public bool IsClearWorld(int pX, int pY)
        {
            int _pointX = pX / TMXLevel.TileWidth;
            int _pointY = pY / TMXLevel.TileHeight;

            // Make sure that the coordinate is within the bounds
            if (pX >= 0 && pX <= TMXLevel.Width &&
                pY >= 0 && pY <= TMXLevel.Height)
            {
                return (this.TMXLevel.Grid[pX, pY, 0].ID == 146);
            }
            else
            {
                return false;
            }
        }

        // Reload the level into the game
        public void LoadLevel(TMXLevel pLevel)
        {
            m_TMXLevel = pLevel;
            Entity.Entity.GameItems.Clear();
            NotificationText.Entities.Clear();
            Entity.Entity.Entities.Clear();
            m_Cakes.Clear();
            m_BlackHoles.Clear();
            m_DangerMap = new DangerMap(this);
            m_SpawnPoints.Clear();
            m_PlayerSpawnPoints.Clear();

            // Determine that the key exists before trying to access it first.
            if (pLevel.Properties.ContainsKey("level_name"))
            {
                this.m_LevelName = m_TMXLevel.Properties["level_name"];
            }

            // Determine if the property that we want is in fact there
            if (pLevel.Properties.ContainsKey("ants_kill"))
            {
                // Parse the amount of ants that have to be killed as an integer to the level.
                m_AntsKillAmount = int.Parse(pLevel.Properties["ants_killed"].ToString());
            }

            // Load in the objects that are being loaded from the map.
            foreach (var group in pLevel.ObjectGroups)
            {
                // Based on what they are, then create the appropriate ones in the map.
                foreach (var item in group.Objects)
                {
                    // Based on the items GID, then load the appropriate object into the environment.
                    switch (item.GID)
                    {
                        case 37:
                            m_BlackHoles.Add(new BlackHole(new Vector2(item.X, item.Y)));
                        break;

                        case 199:
                            Entity.Entity.Entities.Add(new Box(new Vector2(item.X, item.Y)));
                        break;

                        case 198:
                            Entity.Entity.Entities.Add(new NewBox(new Vector2(item.X,item.Y),1f,0f));
                        break;

                        case 140:
                            m_Cakes.Add(new Cake(new Vector2(item.X, item.Y)));
                        break;
                        
                        // Toggable Door
                        case 10:
                         
                            Entity.Entity.Entities.Add(new DoorBlock(new Vector2(item.X, item.Y), 0f, 1f));
                        break;

                            // Changeable Switch
                        case 23:
                            string _name = ""; // Name of the switch that is going to be used for relations between doors
                            if (item.Properties.ContainsKey("name"))
                            {
                                _name = item.Properties["name"];
                            }

                            // Add a new entity to the list that is the switch!
                            Entity.Entity.Entities.Add(new Switch(new Vector2(item.X, item.Y), 1f, 0f, _name));
                        break;

                            // Gold
                        case 32:
                            int _goldamount = 0;
                            if (item.Properties.ContainsKey("amount"))
                            {
                                int.TryParse(item.Properties["amount"], out _goldamount);
                            }

                            if (_goldamount == 0)
                            {
                                _goldamount = 25;
                            }

                            // Add the object.
                            Entity.Entity.Entities.Add(new Gold(new Vector2(item.X, item.Y), 1f, 0f, _goldamount));
                        break;

                        // Diamond
                        case 50:
                            int _diamondamount = 0;
                            if (item.Properties.ContainsKey("amount"))
                            {
                                int.TryParse(item.Properties["amount"], out _diamondamount);
                            }

                            // Change the value from 0 to 25, if it remains that way after parsing.
                            if (_diamondamount == 0)
                            {
                                _diamondamount = 25;
                            }

                            Entity.Entity.Entities.Add(new Diamond(new Vector2(item.X, item.Y), 0f,1f, _diamondamount));
                        break;
                        
                        // Door block
                        case 5:
                        string _switchName = "";
                            if (item.Properties.ContainsKey("name"))
                            {
                                _switchName = item.Properties["name"];
                            }

                            // Add the door block to the list.
                            Entity.Entity.Entities.Add(new DoorBlock(new Vector2(item.X, item.Y), 0f, 1f));
                        break;

                        case 168:
                            // Determine what kind of spawn point it is.
                            // Player or Ant?!
                            if (item.Properties.Count > 0)
                            {
                                SpawnPoint _spawnpoint = new SpawnPoint() { Position = new Vector2(item.X,item.Y) };
                                SpawnType _type;
                                // Make sure that some type has been returned
                                if (item.Properties.ContainsKey("anttype"))
                                {
                                    // Determine what type of ant is going to be spawned here.
                                    _type = (SpawnType)Enum.Parse(typeof(SpawnType), item.Properties["anttype"]);
                                    _spawnpoint.SpawnType = _type;
                                }
                                else
                                {
                                    _type = SpawnType.YellowAnt;
                                }

                                // Determine if it contains the spawn type property that we want.
                                if (item.Properties.ContainsKey("spawntype"))
                                {
                                    // Determine what kind of spawn type it is
                                    switch (item.Properties["spawntype"])
                                    {
                                        case "player":
                                            _spawnpoint.SpawnType = SpawnType.Player;
                                            this.m_PlayerSpawnPoints.Add(_spawnpoint);
                                            break;

                                        case "ant":
                                            // Add the spawn point to the environment
                                            this.m_SpawnPoints.Add(_spawnpoint);
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                // Default to this
                                m_SpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y), SpawnType = SpawnType.YellowAnt});
                            }
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Somewhat scruffy way of dealing with this...
        /// </summary>
        /// <param name="pLevel">The level object that we are going to be working with</param>
        /// <returns>Returns the new level object with stuff added onto it</returns>
        public Level ParseObjects(Level pLevel)
        {
            return null;
        }

        // Load in a new level and clear out the current objects.
        public async Task LoadLevel(string pLevelName)
        {
            this.TMXLevel = await TMXLoader.LoadTMX(LEVEL_FOLDER, pLevelName);
            m_Cakes.Clear();
            m_BlackHoles.Clear();
            m_DangerMap = new DangerMap(this);
            m_SpawnPoints.Clear();
            m_PlayerSpawnPoints.Clear();
            
            // Clear out the players as we won't be requiring them
            MainGame.Instance.GameState.PlayerManager.Players.Clear();

            // Determine that the key exists before trying to access it first.
            if (m_TMXLevel.Properties.ContainsKey("level_name"))
            {
                this.m_LevelName = m_TMXLevel.Properties["level_name"];
            }

            // Determine if the property that we want is in fact there
            if (m_TMXLevel.Properties.ContainsKey("ants_kill"))
            {
                // Parse the amount of ants that have to be killed as an integer to the level.
                m_AntsKillAmount = int.Parse(m_TMXLevel.Properties["ants_killed"].ToString());
            }

            // Load in the objects that are being loaded from the map.
            foreach (var group in m_TMXLevel.ObjectGroups)
            {
                // Based on what they are, then create the appropriate ones in the map.
                foreach (var item in group.Objects)
                {
                    // This is a really sloppy way of doing things, that's for sure.
                    switch (item.GID)
                    {
                        case 37:
                            m_BlackHoles.Add(new BlackHole(new Vector2(item.X, item.Y)));
                        break;

                        case 199:
                            Entity.Entity.Entities.Add(new Box(new Vector2(item.X, item.Y)));
                        break;

                        case 140:
                            m_Cakes.Add(new Cake(new Vector2(item.X, item.Y)));
                        break;

                        // Chest
                        case 28:
                            
                        break;

                        // Switch
                        case 132:
                            string _name = "";
                            if (item.Properties.ContainsKey("name"))
                            {
                                _name = item.Properties["name"];
                            }

                            // Assign the name of the switch
                            Entity.Entity.Entities.Add(new Switch(new Vector2(item.X, item.Y), 1f, 0f,_name));
                        break;

                        case 2:
                            int _damageamount;

                            // Return the amount of damage that the block can take before it's destroyed
                            if (item.Properties.ContainsKey("damage_amount"))
                            {
                                int.TryParse(item.Properties["damage_amount"].ToString(),out _damageamount);
                            }

                            /// Insert the new Dirt Block into the terrain.
                            Entity.Entity.Entities.Add(new DirtBlock(new Vector2(item.X,item.Y),0f,1f));

                        break;

                        case 32:

                            int _goldamount = 0;
                            if (item.Properties.ContainsKey("amount"))
                            {
                                int.TryParse(item.Properties["amount"], out _goldamount);
                            }

                            if (_goldamount == 0)
                            {
                                _goldamount = 25;
                            }

                            Entity.Entity.Entities.Add(new Gold(new Vector2(item.X, item.Y), 1f, 0f, _goldamount));
                        break;
                         
                        case 25:
                            
                        break;

                        // Gold
                        case 50:
                            int _diamondamount = 0;
                            if (item.Properties.ContainsKey("amount"))
                            {
                                int.TryParse(item.Properties["amount"], out _diamondamount);
                            }

                            // Change the value from 0 to 25, if it remains that way after parsing.
                            if (_diamondamount == 0)
                            {
                                _diamondamount = 25;
                            }

                            Entity.Entity.Entities.Add(new Diamond(new Vector2(item.X, item.Y), 1f, 0f, _diamondamount));
                        break;

                        case 168:
                            // Determine what kind of spawn point it is.
                            // Player or Ant?!
                            if (item.Properties.Count > 0)
                            {
                                // Determine if it contains the spawn type property that we want.
                                if (item.Properties.ContainsKey("spawntype"))
                                {
                                    // Determine what kind of spawn type it is
                                    switch (item.Properties["spawntype"])
                                    {
                                        case "player":
                                            this.m_PlayerSpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                                        break;

                                        case "ant":
                                            this.m_SpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Default to this
                                m_SpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                            }
                            break;
                    }
                }
            }

        }

        

        /// <summary>
        /// Return the friction property to apply based on what position and size of the entity we're
        /// talking about here
        /// 
        /// Function assumes that "Size" has been defined properly by the entity when it's spawned.
        /// </summary>
        /// <param name="pOther">The entity that we're going to determine the position of</param>
        /// <returns></returns>
        public float TileFriction(Entity.Entity pOther)
        {
            // The friction property of that given tile.
            float _highest = DIRT_FRICTION;
            int _count = 0;

            int _minX, _minY, _maxX, _maxY;
            
            // Grab the areas that the sprite is within at the moment.
            _minX = Math.Max(0, (int)pOther.Position.X / this.m_TMXLevel.TileWidth);
            _minY = Math.Max(0, (int)pOther.Position.Y / this.m_TMXLevel.TileHeight);
            _maxX = Math.Min(m_TMXLevel.Width, ((int)pOther.Position.X + pOther.Size.X) / TMXLevel.TileWidth);
            _maxY = Math.Min(m_TMXLevel.Height, ((int)pOther.Position.Y + pOther.Size.Y) / TMXLevel.TileHeight);

            // Loop through the tiles and determine the highest friction property to return
            for (int x = _minX; x < _maxX; x++)
            {
                for (int y = _minY; y < _maxY; y++)
                {
                    // Make sure that we're not dealing with a null value here.
                    if (m_TMXLevel.Grid[x, y, 1].ID == -1)
                        continue;

                    _highest = ICE_FRICTION;
 
                    // Used for calculating the average friction that is applied
                    _count++;
                }
            }

            return _highest;
        }

        /// <summary>
        /// Return the type of tile that is at the given X and Y coordinates.
        /// </summary>
        /// <param name="pX">The X coordinate that we're checking against</param>
        /// <param name="pY">The Y coordinate that we're checking against</param>
        /// <returns></returns>
        public TileType GetTileType(int pX, int pY)
        {
            int _checkX = Math.Max(Math.Min(pX, TMXLevel.Width - 1),0);
            int _checkY = Math.Max(Math.Min(pY, TMXLevel.Height - 1), 0);

            int _tileID = TMXLevel.Grid[_checkX, _checkY,1].ID;
            TileType _returntype;

            // Determine what type of tile we're dealing with
            switch (_tileID)
            {
                case 67:
                    _returntype = TileType.Ice;
                break;

                default:
                    _returntype = TileType.None;
                break;
            }

            return _returntype;
        }

        /// <summary>
        /// Wrapper method stub for the actual method in question
        /// </summary>
        /// <param name="pPoint">Point that we are checking</param>
        /// <param name="pType">Type of object that we are checking against</param>
        /// <returns>Return whether or not the object is occupying the point in question</returns>
        public bool IsObjectOccupying(Point pPoint, Type pType)
        {
            return IsObjectOccupying(pPoint.X, pPoint.Y, pType);
        }

        /// <summary>
        /// Using OccupyingGridSpaces() we determine whether or not the coordinate provided is occupied
        /// </summary>
        /// <param name="pX">The x co-ordinate</param>
        /// <param name="pY">The y co-ordinate</param>
        /// <param name="pType">The type that we are getting information about</param>
        /// <returns>Returns whether or not an object is occupying the point in question</returns>
        public bool IsObjectOccupying(int pX, int pY, Type pType)
        {
            foreach (var item in Entity.Entity.Entities)
            {
                // Keep looping if the item is not quite what we are after.
                if (item.GetType() != pType)
                    continue;

                // Return the points that it should be occupying in total.
                Point[] _occupying = item.OccupyingGridSpace();

                if (_occupying.Contains(new Point(pX,pY)))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Temporarily generate a bounding box that is going to check the intersection
        /// </summary>
        /// <param name="pX">(X Co-ordinate) The tile on the map that we want to check against</param>
        /// <param name="pY">(Y Co-ordinate) The tile on the map that we want to check against</param>
        /// <returns></returns>
        public bool IsObjectAt(int pX, int pY, Type pType)
        {
            if (pX > 0 && pY > 0 &&
                TMXLevel.Width > pX &&
                TMXLevel.Height > pY)
            {

                BoundingBox _temporarybb = new BoundingBox(new Vector3(pX * TMXLevel.TileWidth, pY * TMXLevel.TileHeight, 0),
                                                           new Vector3((pX * TMXLevel.TileWidth) + TMXLevel.TileWidth - 16,
                                                                       (pY * TMXLevel.TileHeight) + TMXLevel.TileHeight - 16, 0));

                // Loop through the boxes in the environment and then determine if there is a collision
                foreach (var item in Entity.Entity.Entities)
                {
                    // Return whether or not there is a collision on that block square.
                    if (item.CollisionBox.Intersects(_temporarybb) && item.GetType() == pType)
                    {
                        return true;
                    }
                }

                // Loop through the black holes as well. Definitely don't want to walk into those
                foreach (var item in MainGame.Instance.GameState.Level.BlackHoles)
                {
                    if (item.CollisionBox.Intersects(_temporarybb) && item.GetType() == pType)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        // Determine that there is a box there.
        public bool IsBoxAt(int pX, int pY)
        {
            // Linear check through the entities list to determine if the next approach is applicable
            foreach (var item in Entity.Entity.Entities)
            {
                if (item is Box)
                {
                    int _checkminX, _checkmaxX;
                    int _checkminY, _checkmaxY;

                    int _centerX, _centerY;

                    // Get the mins and maxes
                    _checkminX = Math.Max(0, (int)(item.Position.X + 32) / TMXLevel.TileWidth);
                    _checkminY = Math.Max(0, (int)(item.Position.Y + 32) / TMXLevel.TileHeight);

                    _checkmaxX = Math.Min(TMXLevel.Width, (int)(item.Position.X + item.Size.X + 32) / m_TMXLevel.TileWidth);
                    _checkmaxY = Math.Min(TMXLevel.Height, (int)(item.Position.Y + item.Size.Y + 32) / TMXLevel.TileHeight);

                    _centerX = _checkminX + (_checkmaxX - _checkminX);
                    _centerY = _checkminY + (_checkmaxY - _checkminY);

                    for (int x = _checkminX; x < _checkmaxX; x++)
                    {
                        for (int y = _checkminY; y < _checkmaxY; y++)
                        {
                            if (x == pX && y == pY)
                            {
                                return true;
                            }

                            if (_centerX == pX && _centerY == pY)
                            {
                                return true;
                            }
                        }
                    }
                
                }
            }

            return false;
        }

        /// <summary>
        /// Return the cake with the highest output
        /// </summary>
        /// <param name="pOther">The other distance that we are comparing against.</param>
        /// <returns>Returns the cake in question</returns>
        public Cake GetFarthestCake(Vector2 pOther)
        {
            Cake _farthest = null;
            float _highestDistanceTotal = 0f;

            // Loop through the cakes in question to find the farthest one away possible
            // Loop through the cakes and determine the lowest
            for (int i = 0; i < m_Cakes.Count; i++)
            {
                // If the lowest distance is at 0 anyway, then go with that.
                if (_highestDistanceTotal == 0f)
                {
                    _highestDistanceTotal = (Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y));
                    _farthest = m_Cakes[i];                
                }
                else if ((Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y)) > _highestDistanceTotal)
                {
                    // Retreive the lowest distance cost.
                    _highestDistanceTotal = (Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y));
                    _farthest = m_Cakes[i];     
                }
            }
            
            // Return the cake to the sender of this function call.
            return _farthest;
        }

        /// <summary>
        /// Return the cake that is the nearest to the given Vector
        /// 
        /// To be used for ants when detecting the cake with the shortest distance to get to
        /// </summary>
        /// <param name="pOther">Vector of the other entity to check against</param>
        /// <returns>Relevant Cake object</returns>
        public Cake GetNearestCake(Vector2 pOther)
        {
            Cake _lowest = null;
            float _lowestDistanceTotal = 0f;

            // Loop through the cakes and determine the lowest
            for (int i = 0; i < m_Cakes.Count; i++)
            {
                // If the lowest distance is at 0 anyway, then go with that.
                if (_lowestDistanceTotal == 0f)
                {
                    _lowestDistanceTotal = (Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y));
                    _lowest = m_Cakes[i];                
                }
                else if ((Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y)) < _lowestDistanceTotal)
                {
                    // Retreive the lowest distance cost.
                    _lowestDistanceTotal = (Math.Abs(m_Cakes[i].Position.X - pOther.X) + Math.Abs(m_Cakes[i].Position.Y - pOther.Y));
                    _lowest = m_Cakes[i];     
                }
            }

            return _lowest;
        }

        /// <summary>e
        /// Returns a random cake that is to be used by the game state
        /// </summary>
        /// <returns>The fantastic cake in question</returns>
        public Cake GetRandomCake()
        {
            return m_Cakes.Count == 0 ? null : m_Cakes[m_Random.Next(0, m_Cakes.Count)];
        }


        // Determine at which point on the map are we detecting a collision
        public Vector2 CheckCollisionForElastic(BoundingBox pBoundingBox)
        {
            // Several sets of local variables.
            int _checkareaX = (int)Math.Max(0,pBoundingBox.Min.X / TMXLevel.TileWidth), _checkareaY = (int)Math.Max(0,pBoundingBox.Min.Y / TMXLevel.TileHeight);
            int _checkareaX2 = (int)Math.Min(TMXLevel.Width,pBoundingBox.Max.X), _checkareaY2 = (int)Math.Min(TMXLevel.Height,pBoundingBox.Max.Y);

            // Loop through the relevant area to the bounding box that we are looking at
            for (int x = _checkareaX; x < _checkareaX2; x++)
            {
                for (int y = _checkareaY; y < _checkareaY2; y++)
                {
                    // Make sure that what we are dealing with here is legit
                    if (TMXLevel.Grid[x, y, 0].ID == 146)
                        continue;

                    BoundingBox _tempboundingBox = new BoundingBox(new Vector3(x * TMXLevel.TileWidth, y * TMXLevel.TileHeight, 0),
                                                                   new Vector3((x * TMXLevel.TileWidth) + TMXLevel.TileWidth,
                                                                               (y * TMXLevel.TileHeight) + TMXLevel.TileHeight,
                                                                               0));

                    // Determine that it's the right kind of containment that we're using here.
                    if (_tempboundingBox.Contains(pBoundingBox) != ContainmentType.Disjoint)
                    {
                        // Return the tile that we want to deal with on the impulse
                        return new Vector2(x * TMXLevel.TileWidth, y * TMXLevel.TileHeight);
                    }
                }
            }

            // Only return the point at which we collided. 
            // We require this for producing the impulse on the other end.
            return new Vector2(-1f, -1f);
        }

        // Determine that there is a collision at a given point.
        public bool CheckCollision(Vector2 pPosition, Rectangle pBounds)
        {
            int _checkareaX, _checkareaY;
            int _checkareaX2, _checkareaY2;

            // Get tbe bounds for looping through
            _checkareaX = Math.Max(0, (pBounds.X / m_TMXLevel.TileWidth));
            _checkareaY = Math.Max(0, (pBounds.Y / m_TMXLevel.TileHeight));

            // Ensure that the minimum value is at least within the bounds of the map 
            _checkareaX2 = Math.Min(m_TMXLevel.Width, _checkareaX + ((pBounds.X + pBounds.Width) / m_TMXLevel.TileWidth));
            _checkareaY2 = Math.Min(m_TMXLevel.Height, _checkareaY + ((pBounds.Y + pBounds.Height) / m_TMXLevel.TileHeight));

            BoundingBox _newboundingarea = new BoundingBox(new Vector3(new Vector2(pBounds.X, pBounds.Y), 0), new Vector3(pBounds.X + pBounds.Width, pBounds.Y + pBounds.Height, 0));

            // Loop through the area in which we want to check for a collision in.
            for (int i = _checkareaX; i < _checkareaX2; i++)
            {
                for (int j = _checkareaY; j < _checkareaY2; j++)
                {
                    // Check to see that it's a grass area.
                    if (m_TMXLevel.Grid[i, j,0].ID == 146)
                        continue;

                    // Creating the bounding box that we are going to compare against.
                    BoundingBox _temptilebox = 
                        new BoundingBox(
                            new Vector3(
                                i * m_TMXLevel.TileWidth, 
                                j * m_TMXLevel.TileHeight, 0), 
                            new Vector3(
                                (i * m_TMXLevel.TileWidth) + m_TMXLevel.TileWidth, 
                                (j * m_TMXLevel.TileHeight) + m_TMXLevel.TileHeight, 0));

                    // Ensure that it's either contain or intersecting
                    // with the bounding box for the tile.
                    if (_temptilebox.Contains(_newboundingarea) != ContainmentType.Disjoint)
                    {
                        return true;
                    }
                }
            }

            return false;

        }

        /// <summary>
        /// Basic function that returns whether or not the level file exists.
        /// </summary>
        /// <param name="pLevelName"></param>
        /// <returns></returns>
        public async Task<bool> DoesLevelExist(string pLevelName)
        {
            StorageFile _storagefile;

            try
            {
                _storagefile = await LEVEL_FOLDER.GetFileAsync(pLevelName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Old upate method. Not currently being used.
        /// </summary>
        /// <param name="pGameTime">The delta time value that is used</param>
        public void Update(GameTime pGameTime)
        {

        }

        /// <summary>
        /// Set up all the required objects for the level to render appropriately.
        /// </summary>
        public async void Initialize()
        {
            m_SpriteSheet = MainGame.Instance.Content.Load<Texture2D>("terrain_tiles");

            //IEnumerable<StorageFile> _files = await GetLevelList();
            //IRandomAccessStream _stream = await _files.ElementAt(0).OpenAsync(FileAccessMode.Read);

            // Get the level folder that we want.
            LEVEL_FOLDER = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
            LEVEL_FOLDER = await LEVEL_FOLDER.GetFolderAsync("Levels");

            // Initialize the lists that are going to contain information regarding the spawn points.
            m_SpawnPoints = new List<SpawnPoint>();
            m_PlayerSpawnPoints = new List<SpawnPoint>();

            m_Cakes = new List<Cake>();

            // Load in the file in question
           m_TMXLevel = await TMXLoader.LoadTMX(LEVEL_FOLDER, "testing.tmx");

            // Create the new dangermap on top
           m_DangerMap = new DangerMap(this);

            // Determine that the key exists before trying to access it first.
            if (m_TMXLevel.Properties.ContainsKey("level_name"))
            {
                this.m_LevelName = m_TMXLevel.Properties["level_name"];
            }

            // Determine if the property that we want is in fact there
            if (m_TMXLevel.Properties.ContainsKey("ants_kill"))
            {

            }

            // Load in the objects that are being loaded from the map.
            foreach (var group in m_TMXLevel.ObjectGroups)
            {
                // Based on what they are, then create the appropriate ones in the map.
                foreach (var item in group.Objects)
                {
                    // Determine what kind of object that it is
                    switch(item.GID)
                    {
                        // For generating the black holes
                        case 37:
                            m_BlackHoles.Add(new BlackHole(new Vector2(item.X, item.Y)));
                        break;
                        case 199:
                            Entity.Entity.Entities.Add(new Box(new Vector2(item.X, item.Y)));
                        break;
                        case 140:
                            m_Cakes.Add(new Cake(new Vector2(item.X, item.Y)));
                        break;
                        case 168:
                            // Determine what kind of spawn point it is.
                            // Player or Ant?!
                            if (item.Properties.Count > 0)
                            {
                                // Determine if it contains the spawn type property that we want.
                                if (item.Properties.ContainsKey("spawntype"))
                                {
                                    // Determine what kind of spawn type it is
                                    switch (item.Properties["spawntype"])
                                    {
                                        case "player":
                                            this.m_PlayerSpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                                        break;

                                        case "ant":
                                            this.m_SpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Default to this
                                m_SpawnPoints.Add(new SpawnPoint() { Position = new Vector2(item.X, item.Y) });
                            }
                        break;
                    }   
                }
            }
        }

        public virtual void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            // Loop through the spawn points and update them
            for (int i = 0; i < m_SpawnPoints.Count; i++)
            {
                m_SpawnPoints[i].Update(pGameTime, pInputHandler);
            }

            // Loop through the cakes and update them
            for (int i = 0; i < m_Cakes.Count; i++)
            {
                m_Cakes[i].Update(pGameTime, pInputHandler);
            }

            for (int i = 0; i < m_BlackHoles.Count; i++)
            {
                m_BlackHoles[i].Update(pGameTime, pInputHandler);
            }
        }

        public void Draw(SpriteBatch pSpriteBatch)
        {
            //if (TMXLevel.Properties.ContainsKey("kill_amount"))
            //   pSpriteBatch.DrawString(string.Format("ANTS KILLED: {0}/{1}",m_TMXLevel.

            // Loop through the environment and render appropriately.
            for (int z = 0; z < m_TMXLevel.Grid.GetLength(2); z++)
            {
                for (int x = 0; x < m_TMXLevel.Grid.GetLength(0); x++)
                {
                    for (int y = 0; y < m_TMXLevel.Grid.GetLength(1); y++)
                    {
                        if (m_TMXLevel.Grid[x, y, z].ID != -1)
                        {
                            // Grab the sprite coords from the GID.
                            int _columns = m_TMXLevel.Tilesets[0].Width / m_TMXLevel.TileWidth;
                            int _spriteX = m_TMXLevel.Grid[x, y, z].ID == 0 ? 0 : m_TMXLevel.Grid[x, y, z].ID % _columns; // Make sure that it's 0 before actually performing any operations on it.
                            int _spriteY = m_TMXLevel.Grid[x, y, z].ID / _columns;

                            // Draw the tiles to the screen because that's fun.
                            pSpriteBatch.Draw(m_SpriteSheet, new Vector2(x * m_TMXLevel.TileWidth, y * m_TMXLevel.TileHeight),
                                                            new Rectangle(_spriteX * m_TMXLevel.TileWidth, _spriteY * m_TMXLevel.TileHeight,
                                                                          m_TMXLevel.TileWidth, m_TMXLevel.TileHeight), Color.White);
                        }
                    }
                }
            }

            // Update the other items that are in the level
            for (int i = 0; i < m_Cakes.Count; i++)
            {
                m_Cakes[i].Draw(pSpriteBatch);
            }

            for (int j = 0; j < m_SpawnPoints.Count; j++)
            {
                m_SpawnPoints[j].Draw(pSpriteBatch);
            }

            for (int i = 0; i < m_BlackHoles.Count; i++)
            {
                m_BlackHoles[i].Draw(pSpriteBatch);
            }

            if (Global.DEBUG)
            {
                if (m_DangerMap != null)
                {
                    m_DangerMap.Draw(pSpriteBatch);
                }
            }
        }


        public void Update(GameTime pGameTime, InputHandler pInputHandler, Level pLevel)
        {

        }
    }
}
