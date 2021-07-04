using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace AntRunner.Utility
{
    /// <summary>
    /// The leaf task that is used for the behavoural tree.
    /// </summary>
    public abstract class BTTask
    {
        public abstract void Start();

        public abstract void End();

        public abstract bool CheckConditions();

        public abstract void ExecuteTask();
    }

    public abstract class BTTaskController
    {

    }

    public abstract class BTTaskDecorator : BTTask
    {

    }
}
