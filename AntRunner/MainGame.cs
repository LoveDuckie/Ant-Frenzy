using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

using AntRunner.Utility;
using AntRunner.States;
using AntRunner.Entity;
using AntRunner.Menu;

namespace AntRunner
{
    public class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public static bool DEBUG = false;

        private StateValue m_StateValue;

        private InputHandler m_InputHandler;

        #region Required States
        private MenuState m_MenuState;
        private SettingsState m_SettingsState;

        public SettingsState SettingsState
        {
            get { return m_SettingsState; }
            set { m_SettingsState = value; }
        }
        private HelpState m_HelpState;
        private GameState m_GameState;

        public HelpState HelpState
        {
            get { return m_HelpState; }
            set { m_HelpState = value; }
        }

        public GameState GameState
        {
            get { return m_GameState; }
            set { m_GameState = value; }
        }
        #endregion

        private static MainGame _instance;

        // Content htat is to be loaded into the game.
        private Dictionary<string, SpriteFont> m_Fonts;
        private Dictionary<string, Texture2D> m_Textures;
        private Dictionary<string, SoundEffect> m_Sound;

        #region Properties
        public StateValue StateValue
        {
            get { return m_StateValue; }
            set { m_StateValue = value; }
        }

        public Dictionary<string, Texture2D> Textures
        {
            get { return m_Textures; }
            set { m_Textures = value; }
        }

        public Dictionary<string, SoundEffect> Sounds
        {
            get { return m_Sound; }
            set { m_Sound = value; }
        }

        public Dictionary<string, SpriteFont> Fonts
        {
            get { return m_Fonts; }
            set { m_Fonts = value; }
        }

        public static MainGame Instance
        {
            get { return MainGame._instance; }
            set { MainGame._instance = value; }
        }

        public SpriteBatch SpriteBatch
        {
            get { return _spriteBatch; }
            set { _spriteBatch = value; }
        }
        #endregion

        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            m_StateValue = StateValue.Menu;

            _instance = this;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Containers for the content that we are going to load.
            m_Fonts = new Dictionary<string, SpriteFont>();
            m_Textures = new Dictionary<string, Texture2D>();
            m_Sound = new Dictionary<string, SoundEffect>();

            // This won't work for some reason as it's a full-screen application :<
            this.Window.AllowUserResizing = true;
            
            // Generic menu stuff.
            m_Textures.Add("arrow_select", Content.Load<Texture2D>("arrow_select"));
            m_Textures.Add("shotgun_fire", Content.Load<Texture2D>("shotgun_fire"));
            m_Textures.Add("tower_head", Content.Load<Texture2D>("tower_head"));
            m_Textures.Add("bullet_single", Content.Load<Texture2D>("bullet_single"));
            m_Textures.Add("bullet", Content.Load<Texture2D>("bullet"));
            m_Sound.Add("menu_select", Content.Load<SoundEffect>("Blip_Select"));
            m_Sound.Add("shoot_sound", Content.Load<SoundEffect>("shoot_sound"));
            m_Sound.Add("menu_music", Content.Load<SoundEffect>("main_menu")); // Include the main menu music for super awesome fun times!
            m_Sound.Add("hit", Content.Load<SoundEffect>("hit"));
            m_Sound.Add("pickup", Content.Load<SoundEffect>("pickup"));
            m_Sound.Add("pickup2", Content.Load<SoundEffect>("pickup2"));
            m_Textures.Add("ant_sprite", Content.Load<Texture2D>("ant_spritesheet"));
            m_Textures.Add("game_logo", Content.Load<Texture2D>("AntRunnerLogo"));
            
            // For demonstrating the other methods of pathfinding.
            m_Textures.Add("ant_spritesheet_green", Content.Load<Texture2D>("ant_spritesheet_green"));
            m_Textures.Add("ant_spritesheet_blue", Content.Load<Texture2D>("ant_spritesheet_blue"));
            m_Textures.Add("ant_spritesheet_red", Content.Load<Texture2D>("ant_spritesheet_red"));

            // Pathfinding debug images.
            m_Textures.Add("open_grid", Content.Load<Texture2D>("open_grid"));
            m_Textures.Add("close_grid", Content.Load<Texture2D>("close_grid"));
            m_Textures.Add("path_dot", Content.Load<Texture2D>("path_dot"));
            m_Textures.Add("blank_grid", Content.Load<Texture2D>("blank_grid"));
            m_Textures.Add("bezier_dot", Content.Load<Texture2D>("bezier_dot"));
            m_Textures.Add("speech_kill", Content.Load<Texture2D>("speech_kill"));
            m_Textures.Add("speech_cake", Content.Load<Texture2D>("speech_cake"));
            m_Textures.Add("speech_wandering", Content.Load<Texture2D>("speech_wandering"));
            m_Textures.Add("narrator", Content.Load<Texture2D>("narrator"));
            m_Textures.Add("collect_items", Content.Load<Texture2D>("collect_items"));
            m_Textures.Add("debug_dot", Content.Load<Texture2D>("debug_dot"));
            m_Textures.Add("pulse", Content.Load<Texture2D>("pulse"));
            m_Textures.Add("selection_arrow", Content.Load<Texture2D>("selection_arrow"));
            m_Textures.Add("ant_menu_icon", Content.Load<Texture2D>("ant_menu_icon"));
            m_Textures.Add("selection_cursor",Content.Load<Texture2D>("selection_cursor"));

            // Ant icons for the menu
            m_Textures.Add("green_ant_icon", Content.Load<Texture2D>("green_ant_icon"));
            m_Textures.Add("yellow_ant_icon", Content.Load<Texture2D>("yellow_ant_icon"));
            m_Textures.Add("red_ant_icon", Content.Load<Texture2D>("red_ant_icon"));
            m_Textures.Add("blue_ant_icon", Content.Load<Texture2D>("blue_ant_icon"));

            // GUI icons for the clickwheel
            m_Textures.Add("spade", Content.Load<Texture2D>("spade"));
            m_Textures.Add("hammer_icon", Content.Load<Texture2D>("hammer_icon"));
            m_Textures.Add("cake_slice", Content.Load<Texture2D>("cake_slice"));

            // In-game gui stuff
            m_Textures.Add("healthbar_full", Content.Load<Texture2D>("healthbar_full_small"));
            m_Textures.Add("emptybar_small", Content.Load<Texture2D>("emptybar_small"));
            m_Textures.Add("ammobar_small", Content.Load<Texture2D>("ammobar_small"));
            m_Textures.Add("white_bar", Content.Load<Texture2D>("white_bar"));

            // Disable the appearance of the mouse within the game
            this.IsMouseVisible = true;

            /** STATES **/
            m_GameState = new GameState();
            m_HelpState = new HelpState();
            m_MenuState = new MenuState();
            m_SettingsState = new SettingsState();

            m_HelpState.Initialize();
            m_MenuState.Initialize();
            m_SettingsState.Initialize();

            /* INPUT HANDLING */
            m_InputHandler = new InputHandler();

            base.Initialize();
        }

        protected override void OnExiting(object sender, System.EventArgs args)
        {
            // Loop through the content and unload them before we close the application.
            foreach (KeyValuePair<string,Texture2D> item in m_Textures)
            {
                item.Value.Dispose();
            }

            foreach (KeyValuePair<string,SoundEffect> item in m_Sound)
            {
                item.Value.Dispose();
            }

            base.OnExiting(sender, args);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load in the fonts that are required
            m_Fonts.Add("debug_font", Content.Load<SpriteFont>("GameFont"));
            m_Fonts.Add("astar_font", Content.Load<SpriteFont>("astar_font"));
            m_Fonts.Add("medium_font", Content.Load<SpriteFont>("medium_game_font"));

            // Load in the textures that are required, chuck them at the dictionary.
            
            m_Textures.Add("terrain_tiles", Content.Load<Texture2D>("terrain_tiles"));
            
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here

            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            m_InputHandler.UpdatePreviousState();

            if (m_InputHandler.IsKeyDownOnce(Keys.Home))
            {
                Global.DEBUG = Global.DEBUG == false ? true : false; 
            }

            // For demonstrating separating debugging data for the paths in the environment.
            if (m_InputHandler.IsKeyDownOnce(Keys.End))
            {
                Global.PATH_DEBUG = Global.PATH_DEBUG == false ? true : false;
            }

            switch (m_StateValue)
            {
                case StateValue.Menu:
                    m_MenuState.Update(gameTime, m_InputHandler);
                break;

                case StateValue.Help:
                    m_HelpState.Update(gameTime, m_InputHandler);
                break;

                case StateValue.Game:
                    m_GameState.Update(gameTime, m_InputHandler);
                break;

                case StateValue.Settings:
                    m_SettingsState.Update(gameTime, m_InputHandler);
                break;
            }

            m_InputHandler.UpdateCurrentState();

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Determine which state to draw presuming that we are in the right one
            switch (m_StateValue)
            {
                case StateValue.Game:
                    m_GameState.Draw(_spriteBatch);
                break;

                case StateValue.Help:
                    m_HelpState.Draw(_spriteBatch);
                break;

                case StateValue.Menu:
                    m_MenuState.Draw(_spriteBatch);
                break;

                case StateValue.Settings:
                    m_SettingsState.Draw(_spriteBatch);
                break;
            }

#if DEBUG
            _spriteBatch.Begin();
                _spriteBatch.DrawString(m_Fonts["debug_font"], "DEBUG", new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(m_Fonts["debug_font"], string.Format("GAME STATE: {0}", m_StateValue.ToString()), new Vector2(0, 30), Color.White);
                _spriteBatch.DrawString(m_Fonts["debug_font"], string.Format("MOUSE COORDS: {0},{1}", Mouse.GetState().X, Mouse.GetState().Y), new Vector2(0, 60), Color.White);
            _spriteBatch.End();

#endif

            base.Draw(gameTime);
        }
    }
}
