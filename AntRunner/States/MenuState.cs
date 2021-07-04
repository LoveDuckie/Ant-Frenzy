using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AntRunner.Cameras;
using AntRunner.Menu;
using AntRunner.States;
using AntRunner.Tower;
using AntRunner.Utility;

using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AntRunner.States
{
    // The first thing that the players see when they launch the game.
    public class MenuState : State
    {
        /** Controls the menu that is going to be used by the player. **/
        private MenuManager m_MenuManager;

        private SoundEffectInstance m_MainMenuMusic;

        private float m_MainImageScale;
        private bool m_ScaleDirection;

        private const float SCALE_AMOUNT = 0.005f;

        public MenuState()
        {
            m_MainImageScale = 1f;
            // Set up the menu that is going to be used with the items
            // that are going to be displayed in it.
            m_MenuManager = new MenuManager("Main Menu", new Vector2((MainGame.Instance.Window.ClientBounds.Width / 2) - 125, MainGame.Instance.Window.ClientBounds.Height / 2));
            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "PLAY",
                Action = delegate(object sender, EventArgs e)
                {
                  // MainGame.Instance.StateValue = StateValue.Game;
                   m_MenuManager.DisplayChild(0); // Display the menu for the levels before playing the game
                }
                
            });
            
            m_MenuManager.Children.Add(new MenuManager("SELECT LEVEL",m_MenuManager.Position)
            {
                Active = false
            });

            m_MenuManager.Children[0].Items = new List<MenuItem>();

            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "SETTINGS",
                Action = delegate(object sender, EventArgs e)
                {
                    MainGame.Instance.StateValue = StateValue.Settings;
                }
            });

            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "HELP",
                Action = delegate(object sender, EventArgs e)
                {
                    MainGame.Instance.StateValue = StateValue.Help;
                }
            });

            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "HIGHSCORES",
                Action = delegate(object sender, EventArgs e)
                {
                    App.Current.Exit();
                }
            });


            m_MenuManager.AddMenuItem(new MenuItem()
            {
                Message = "TEST STATS",
                Action = delegate(object sender, EventArgs e)
                {

                }
            });
          //  this.Initialize();
        }

        #region Member Methods
        public override async void Initialize()
        {
            m_MainMenuMusic = MainGame.Instance.Sounds["menu_music"].CreateInstance();

            var _levellist = await Level.GetLevelList();

            // Sort the list by level name.
            _levellist.Sort((x, y) => x.LevelName.CompareTo(y.LevelName));

            // If the list of levels is not null, then do something about it
            if (_levellist != null)
            {
                foreach (var item in _levellist)
	            {
                    // Loop through the menu items and add them to the list.
	                m_MenuManager.Children[0].Items.Add(new MenuItem()
                    {
                        Action = delegate(object sender, EventArgs e)
                        {
                            // Attempt to determine what game type it is
                            GameMode _type = (GameMode)Enum.Parse(typeof(GameMode), item.Properties["mode"].ToString());

                            MainGame.Instance.GameState.SetGameType(_type);
                            MainGame.Instance.GameState.Level.LoadLevel(item);
                            MainGame.Instance.StateValue = StateValue.Game;
                            MainGame.Instance.GameState.SpawnedPlayer = false;
                            m_MenuManager.DisableChild();
                        },
                        Message = item.LevelName.ToUpper().ToString()
                    });	 
	            }
            }

            // Continuously play the music!
            m_MainMenuMusic.IsLooped = true;
           // m_MainMenuMusic.Play();

            m_ScaleDirection = false;

            base.Initialize();
        }

        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            m_MenuManager.Update(pGameTime, pInputHandler);

            #region Scale Bouncing
            if (!m_ScaleDirection)
            {
                if (m_MainImageScale < 1f)
                {
                    m_MainImageScale += SCALE_AMOUNT;
                }
                else
                {
                    m_ScaleDirection = true;
                }
            }
            else
            {
                if (m_MainImageScale > 0.85f)
                {
                    m_MainImageScale -= SCALE_AMOUNT;
                }
                else
                {
                    m_ScaleDirection = false;
                }
            }
            #endregion

            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            pSpriteBatch.Begin(SpriteSortMode.Deferred,BlendState.AlphaBlend);

            int _renderGreenX = MainGame.Instance.Window.ClientBounds.Width / 64;
            int _renderGreenY = (MainGame.Instance.Window.ClientBounds.Height / 64) + 1 ;

            for (int x = 0; x < _renderGreenX; x++)
            {
                // Draw the green back ground just for funsies.
                for (int y = 0; y < _renderGreenY; y++)
                {
                    pSpriteBatch.Draw(MainGame.Instance.Textures["terrain_tiles"], new Vector2(x * 64, y * 64), new Rectangle(2 * 64, 9 * 64, 64, 64), Color.White);
                }
            }

            // Draw the game logo!
            pSpriteBatch.Draw(MainGame.Instance.Textures["game_logo"], new Vector2((MainGame.Instance.Window.ClientBounds.Width / 2), 250f),null, Color.White,0f,new Vector2(251,100),m_MainImageScale,SpriteEffects.None,0);

            // Render the text demonstrating it as a demo
            ShadowText.Draw("GAMES BEHAVIOUR - ASSIGNMENT PART TWO", pSpriteBatch, new Vector2((MainGame.Instance.Window.ClientBounds.Width / 2) - 250, 400f));

            m_MenuManager.Draw(pSpriteBatch);

            pSpriteBatch.End();
            base.Draw(pSpriteBatch);
        }
        #endregion

    }
}
