using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace AntRunner.States
{
    class ScoreState : State
    {
        private const string SCORE_GET_URL = "http://lucshelton.com/antrunner/scores.php";

        public ScoreState()
        {

        }

        public override void Initialize()
        {
            base.Initialize();
        }

        // Member functions for rendering and updating.
        public override void Update(GameTime pGameTime, InputHandler pInputHandler)
        {
            base.Update(pGameTime, pInputHandler);
        }

        public override void Draw(SpriteBatch pSpriteBatch)
        {
            base.Draw(pSpriteBatch);
        }

    }
}
