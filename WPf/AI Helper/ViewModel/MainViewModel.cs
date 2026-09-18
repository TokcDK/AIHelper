using AIHelper.SharedData;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace AIHelper.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private int _tabIndex;

        private RelayCommand? onPrepareGame_Click;
        private RelayCommand? onGameButton_Click;
        private RelayCommand? onManagerButton_Click;
        private RelayCommand? onStudioButton_Click;
        private RelayCommand? onSettingsButton_Click;

        public MainViewModel() => GamesList.PropertyChanged += OnGamesListPropertyChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public GameData GamesList { get; } = new();

        public bool HasAnyGame => GamesList.Game != null;

        public bool CanUserRunStudio =>
            GamesList.Game != null && File.Exists(GamesList.Game.GetGameStudioExeName());

        public int TabIndex
        {
            get => _tabIndex;
            set
            {
                if (_tabIndex == value)
                {
                    return;
                }

                _tabIndex = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OnPrepareGame_Click => onPrepareGame_Click ??= new RelayCommand(obj => { });

        public RelayCommand OnGameButton_Click => onGameButton_Click ??= new RelayCommand(obj => { });

        public RelayCommand OnManagerButton_Click => onManagerButton_Click ??= new RelayCommand(obj => { });

        public RelayCommand OnStudioButton_Click => onStudioButton_Click ??= new RelayCommand(obj => { });

        public RelayCommand OnSettingsButton_Click => onSettingsButton_Click ??= new RelayCommand(obj => { });

        public static string AppDir { get; internal set; } =
            Path.GetDirectoryName(Environment.ProcessPath) ?? AppContext.BaseDirectory;

        // strings
        public LocaleStrings Loc { get; } = new LocaleStrings();

        private void OnGamesListPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameData.Game))
            {
                OnPropertyChanged(nameof(HasAnyGame));
                OnPropertyChanged(nameof(CanUserRunStudio));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
