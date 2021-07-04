using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntRunner.Entity
{
    /// <summary>
    /// Function used to making sure that certain objects within the game world are actually usable.
    /// </summary>
    public interface IToggable
    {
        void Toggle(Entity pOther);
    }
}
