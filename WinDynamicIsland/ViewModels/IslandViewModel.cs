using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WinDynamicIsland.Core;
using WinDynamicIsland.Sources;

namespace WinDynamicIsland.ViewModels
{
    public class IslandViewModel : INotifyPropertyChanged
    {
        private readonly MediaSessionSource _mediaSource;
        private string _title = "Idle";
        private string _artist = "";
        private bool _hasMedia = false;
        private double _width = 120; // Default idle width
        private double _height = 35; // Default idle height

        public IslandViewModel()
        {
            _mediaSource = new MediaSessionSource();
            _mediaSource.MediaInfoChanged += OnMediaInfoChanged;
            
            // Initialize Async
            _ = _mediaSource.InitializeAsync();
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public string Artist
        {
            get => _artist;
            set { _artist = value; OnPropertyChanged(); }
        }

        public bool HasMedia
        {
            get => _hasMedia;
            set 
            { 
                _hasMedia = value; 
                OnPropertyChanged(); 
                UpdateDimensions();
            }
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }

        private void UpdateDimensions()
        {
            if (HasMedia)
            {
                Width = 350; // Expanded for media
                Height = 80;
                StateManager.Instance.SetMode(IslandMode.MediaPlaying);
            }
            else
            {
                Width = 120; // Compact pill
                Height = 35;
                StateManager.Instance.SetMode(IslandMode.Idle);
            }
        }

        private void OnMediaInfoChanged(object? sender, MediaInfo e)
        {
            // Marshal to UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (e.IsPlaying)
                {
                    Title = e.Title;
                    Artist = e.Artist;
                    HasMedia = true;
                }
                else
                {
                    Title = "Idle";
                    Artist = "";
                    HasMedia = false;
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
