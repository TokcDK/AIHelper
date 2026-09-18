using AIHelper.Games;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AIHelper.SharedData
{
    public class GameData : INotifyPropertyChanged
    {
        private GameBase? _game;

        public GameData()
        {
            _game = Games.Count > 0 ? Games[0] : null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// List of valid games
        /// </summary>
        public List<GameBase> Games { get; set; } = new() { new Koikatsu(), new HoneySelect() };

        /// <summary>
        /// Current selected game
        /// </summary>
        public GameBase? Game
        {
            get => _game;
            set
            {
                if (ReferenceEquals(_game, value))
                {
                    return;
                }

                _game = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
