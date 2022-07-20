using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIHelper.Games
{
    public interface IGame
    {
        string GameDisplayingName { get; }
        string GameExeName { get; }
        string GameStudioExeName { get; }
        string GameAbbreviation { get; }
        string ManifestGame { get; }
        BaseGamePyFileInfo BaseGamePyFile { get; }
    }
}
