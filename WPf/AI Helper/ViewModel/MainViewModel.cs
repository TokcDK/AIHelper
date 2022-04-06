using AIHelper.Games;
using AIHelper.SharedData;
using System.Collections.ObjectModel;
using System.IO;

namespace AIHelper.ViewModel
{
    internal class MainViewModel
    {
        public static GameData GamesList { get; set; } = new();
        public static bool CanUserRunStudio { get => File.Exists(GamesList.Game.GetGameStudioExeName()); }

        public static bool HasAnyGame { get => GamesList.Game != null; }

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

        public static string AppDir { get; internal set; } = System.IO.Path.GetDirectoryName(System.Environment.ProcessPath);

        // strings
        public LocaleStrings Loc { get; } = new LocaleStrings();
    }
}
