using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_Helper.Games
{
    public abstract class GameBase
    {
        public abstract string GameName { get; }

        public abstract string ShortName { get; }
    }
}
