using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;


// For loading in the xml files that are required
using System.Xml.Schema;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AntRunner.Utility
{

    public class Tileset
    {
        #region Members
        public string ImageSource { get; set; }
        public string Name { get; set; }
        public int FirstGID { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<TiledImage> Tiles { get; set; }
        public List<TiledObject> Objects { get; set; }
        #endregion
    }

    public class TiledObject
    {
        #region Members
        public int Width { get; set; }
        public int Height { get; set; }
        public string Name { get; set; }
        public int GID { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public bool HorizontalFlip { get; set; }
        public bool VerticalFlip { get; set; }
        public bool DiagonalFlip { get; set; }
        #endregion
    }

    public class ObjectGroup
    {
        #region Members
        public int ID { get; set; }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<TiledObject> Objects { get; set; }
        #endregion

        #region Methods
        // Logic goes here
        #endregion

    }

    public class TiledLayerData
    {
        public List<TiledImage> TiledImages { get; set; }
    }

    public class TiledLayer
    {
        #region Members
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public List<TiledLayerData> Data { get; set; }
        #endregion
    }

    // Used by the TSX and TMX 
    public class TiledImage
    {
        #region Members
        public int ID { get; set; } // The gid that is set.
        public Dictionary<string, string> Properties { get; set; } // name & value
        public bool HorizontalFlip { get; set; }
        public bool VerticalFlip { get; set; }
        public bool DiagonalFlip { get; set; }
        #endregion

    }

    // Level object that is going to store the information that is required
    public class TMXLevel
    {
        #region Members
        public List<Tileset> Tilesets { get; set; }
        public List<ObjectGroup> ObjectGroups { get; set; }
        public List<TiledLayer> Layers { get; set; }

        // This will be primarily used by the level rendering to make it easier
        // Additionally for collision
        public TiledImage[,,] Grid { get; set; }

        // The properties that are stored as a part of the map
        public Dictionary<string, string> Properties { get; set; }

        public string FileName { get; set; }

        // Return the name of the level if it's stored in the properties.
        public string LevelName
        {
            get
            {
                if (Properties.ContainsKey("level_name"))
                {
                    return Properties["level_name"].ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (Properties.ContainsKey("level_name"))
                {
                    Properties["level_name"] = value.ToString();
                }
            }
        }

        public int TileWidth { get; set; }
        public int TileHeight { get; set; }

        public int Height { get; set; }
        public int Width { get; set; }
        #endregion
    }

    /// <summary>
    /// Responsible for loading in TMX files for the level usage.
    /// </summary>
    public class TMXLoader
    {
        #region Methods
        private static async Task<Tileset> LoadTSX(StorageFolder pFileFolder, string pFilename)
        {
            // Determine the file exists.
            if (await DoesFileExist(pFileFolder, pFilename))
            {
                StorageFile _file = await pFileFolder.GetFileAsync(pFilename);
                Stream _filestream = await _file.OpenStreamForReadAsync();

                XDocument _tsxdocument = XDocument.Load(_filestream);
                XElement _root = (XElement)_tsxdocument.Root;

                Tileset _newtileset = new Tileset();
                _newtileset.Tiles = new List<TiledImage>();

                // Get the name of the tileset that we are going to be using
                if (_tsxdocument.Root.Attribute("name") != null &&
                    _tsxdocument.Root.Descendants("tile") != null &&
                    _tsxdocument.Root.Descendants("tile").Count() > 0)
                {
                    _newtileset.Name = _root.Attribute("name").Value;
                }

                // Grab the images that are used for the level.
                foreach (var item in _tsxdocument.Descendants("image"))
                {
                    if (item != null)
                    {
                        _newtileset.Width = int.Parse(item.Attribute("width").Value);
                        _newtileset.Height = int.Parse(item.Attribute("height").Value);
                    }
                }

                foreach (var item in _tsxdocument.Descendants("tile"))
                {
                    TiledImage _newtile = new TiledImage();
                    _newtile.Properties = new Dictionary<string, string>();
                    _newtile.ID = int.Parse(item.Attribute("id").Value);
                   

                    // Grab the properties that we want
                    foreach (var properties in item.Descendants("properties"))
                    {
                        foreach (var property in properties.Descendants("property"))
                        {
                            string name, value;
                            name = property.Attribute("name").Value;
                            value = property.Attribute("value").Value;
                            _newtile.Properties.Add(name, value);
                        }
                    }

                    _newtileset.Tiles.Add(_newtile);
                }

                return _newtileset;
            }

            return null;
        }

        /// <summary>
        /// Using the data that we've just loaded, do something magical.
        /// 
        /// Generate a grid so that the rendering of the level is considered as easier.
        /// </summary>
        /// <param name="pLevel">The level that we're loading from</param>
        public static void ConstructGrid(TMXLevel pLevel)
        {
            if (pLevel != null)
            {
                // Generate the grid that we want to use
                pLevel.Grid = new TiledImage[pLevel.Width, pLevel.Height,pLevel.Layers.Count];
                int _index = 0;

                // Loop through the list.
                for (int z = 0; z < pLevel.Layers.Count; z++)
                {
                    _index = 0;
                    for (int x = 0; x < pLevel.Width; x++)
                    {
                        for (int y = 0; y < pLevel.Height; y++)
                        {
                            // Make sure that the index is within the bounds of the array
                            if (_index < pLevel.Layers[z].Data[0].TiledImages.Count)
                            {
                                // Use the index to determine where we are in the list.
                                pLevel.Grid[y, x, z] = pLevel.Layers[z].Data[0].TiledImages[_index];
                                _index++;
                            }
                        }
                    }
                }

                // Loop through the layer list and add them to the grid / 2D array.
                //while (_row < pLevel.Height)
                //{
                //    if (_column % pLevel.Width != 0)
                //    {
                //        pLevel.Grid[_column, _row] = pLevel.Layers[0].Data[0].TiledImages[_column];
                //        _column++;
                //    }
                //    else
                //    {
                //        _row++;
                //    }
                //}
            }
        }

        /// <summary>
        /// Basic function for loading the TMX file that we require.
        /// 
        /// Assumes that there is a TSX file that was used externally.
        /// </summary>
        /// <param name="pFileFolder">The folder that we are looking for the file from</param>
        /// <param name="pFilename">The file that we are loading</param>
        /// <returns></returns>
        public static async Task<TMXLevel> LoadTMX(StorageFolder pFileFolder, string pFilename)
        {
            TMXLevel _level = new TMXLevel();

            // Ensure that the file exists first
            if (await DoesFileExist(pFileFolder,pFilename))
            {
                StorageFile _file = await pFileFolder.GetFileAsync(pFilename);
                Stream _filestream = await _file.OpenStreamForReadAsync();

                XDocument _tmxdocument = XDocument.Load(_filestream);
                XElement _root = (XElement)_tmxdocument.Root;
                //var _mapproperties;
                
                // Load in the required values for the level file
                _level.Width = int.Parse(_root.Attribute("width").Value);
                _level.Height = int.Parse(_root.Attribute("height").Value);
                _level.TileHeight = int.Parse(_root.Attribute("tileheight").Value);
                _level.TileWidth = int.Parse(_root.Attribute("tilewidth").Value);
                _level.Tilesets = new List<Tileset>();
                _level.Layers = new List<TiledLayer>();
                _level.Properties = new Dictionary<string, string>();
                _level.FileName = pFilename;

                // Go through the properties and add them to the list
                foreach (var item in _root.Descendants("properties"))
                {
                    IEnumerable<XElement> _descendants = item.Descendants("property");
                    foreach (var property in _descendants)
                    {
                        // So hacky.
                        if (property.Parent.Parent.Name == "map")
                        {
                            // Add that to the list
                            _level.Properties.Add(property.Attribute("name").Value.ToString(),
                                                  property.Attribute("value").Value.ToString());
                        }
                    }
                }

                // Loop through the tilesets and populate the content that we require
                foreach (var item in _tmxdocument.Descendants("tileset"))
	            {
                    int _firstgid = int.Parse(item.Attribute("firstgid").Value);
                    string _source = item.Attribute("source").Value;

                    // Grab the tileset.
                    Tileset _tileset = await TMXLoader.LoadTSX(pFileFolder,item.Attribute("source").Value.ToString());
                    _tileset.FirstGID = _firstgid;

                    // Load in the tilesets that we are after.
                    _level.Tilesets.Add(_tileset);
	            }

                // Loop through the layers
                foreach (var item in _tmxdocument.Descendants("layer"))
                {
                    // Setting up the new layer
                    TiledLayer _newlayer = new TiledLayer()
                    {
                         Height = int.Parse(item.Attribute("height").Value),
                         Width = int.Parse(item.Attribute("width").Value),
                         Name = item.Attribute("name").Value.ToString()
                    };
 
                    // Using LINQ, grab the images that we are after.
                    _newlayer.Data = (from data in item.Descendants("data")
                                            select new TiledLayerData()
                                            {
                                                TiledImages = (from tiles in data.Elements()
                                                               select new TiledImage()
                                                               {
                                                                   ID = int.Parse(tiles.Attribute("gid").Value) - 1
                                                               }).ToList()

                                            }).ToList();

                    _level.Layers.Add(_newlayer);
                }

                _level.ObjectGroups = new List<ObjectGroup>();

                // Loop through the object layers
                foreach (var item in _tmxdocument.Descendants("objectgroup"))
                {
                    ObjectGroup _objectgroup = new ObjectGroup();
                    _objectgroup.Name = item.Attribute("name").Value;
                    _objectgroup.Height = int.Parse(item.Attribute("height").Value);
                    _objectgroup.Width = int.Parse(item.Attribute("width").Value);

                    // Create the list of objects that we are going to be using.
                    _objectgroup.Objects = new List<TiledObject>();

                    // Loop through the items of the level
                    foreach (var objectitem in item.Descendants("object"))
                    {
                        TiledObject _newobject = new TiledObject() {
                           X = int.Parse(objectitem.Attribute("x").Value),
                           Y = int.Parse(objectitem.Attribute("y").Value) - _level.TileHeight,
                           GID = int.Parse(objectitem.Attribute("gid").Value) - 1
                        };

                        // Instantiate the appropriate dictionary that we are going to be 
                        // using.
                        _newobject.Properties = new Dictionary<string, string>();

                        // Loop through the properties of the given item
                        foreach (var properties in objectitem.Descendants("properties"))
                        {
                            foreach (var property in properties.Descendants("property"))
                            {
                                _newobject.Properties.Add(
                                    property.Attribute("name").Value.ToString(),
                                    property.Attribute("value").Value);
                            }
                        }

                        _objectgroup.Objects.Add(_newobject);
                    }

                    _level.ObjectGroups.Add(_objectgroup);
                }

                /*** SORT OUT THE ROTATIONS OF THE SPRITE IMAGES ***/
                // Forgetting this for now. Not relevant.
            }

            ConstructGrid(_level);

            return _level;
        }

        /// <summary>
        /// Simple method that determines whether the file exists in the 
        /// provided folder.
        /// </summary>
        /// <param name="pFileFolder">Folder that we are checking out.</param>
        /// <param name="pFilename">Name of the file that we are checking for.</param>
        /// <returns></returns>
        public static async Task<bool> DoesFileExist(StorageFolder pFileFolder, string pFilename)
        {
            // File that we are going to check against
            StorageFile _file;

            try
            {
                _file = await pFileFolder.GetFileAsync(pFilename);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
