using AI_Helper.Games;
using System.Collections.ObjectModel;

namespace AI_Helper.ViewModel
{
    internal class MainViewModel
    {
        public static _gamesList GamesList { get; set; } = new();

        public class _gamesList
        {
            public _gamesList()
            {
                Selected = Games[0];
            }

            public ObservableCollection<GameBase> Games { get; set; } = new() { new Koikatsu(), new HoneySelect() };
            public GameBase Selected { get; set; }
        }

        public static bool IsWorking { get => GamesList.Selected != null; }
        public static int TabIndex { get; set; } = 0;

        private RelayCommand? onPrepareGame_Click;
        public RelayCommand OnPrepareGame_Click => onPrepareGame_Click ??= new RelayCommand(obj => { });

        private RelayCommand? onGameButton_Click;
        public RelayCommand OnGameButton_Click => onGameButton_Click ??= new RelayCommand(obj => { });

        private RelayCommand? onManagerButton_Click;
        public RelayCommand OnManagerButton_Click => onManagerButton_Click ??= new RelayCommand(obj => { });

        private RelayCommand? onStudioButton_Click;
        public RelayCommand OnStudioButton_Click => onStudioButton_Click ??= new RelayCommand(obj => { });

        private RelayCommand? onSettingsButton_Click;
        public RelayCommand OnSettingsButton_Click => onSettingsButton_Click ??= new RelayCommand(obj => { });
    }
}
