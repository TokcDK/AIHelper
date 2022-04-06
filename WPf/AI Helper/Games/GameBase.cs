namespace AIHelper.Games
{
    public abstract class GameBase
    {
        public abstract string GameName { get; }

        /// <summary>
        /// game's studio exe name of selected game
        /// </summary>
        /// <returns></returns>
        public virtual string GetGameStudioExeName()
        {
            return string.Empty;
        }
    }
}
