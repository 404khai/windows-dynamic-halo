using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsDynamicHalo.Core;
using WindowsDynamicHalo.Sources;

namespace WindowsDynamicHalo.ViewModels
{
    // Media-focused VM: controls playback, holds metadata and album art.
    // Exposes Play/Pause command bound from UI.
    public class MediaViewModel : INotifyPropertyChanged
    {
        private readonly MediaSessionSource _mediaSource;
        private string _title = "";
        private string _artist = "";
        private bool _isPlaying = false;
        private ImageSource? _albumArt;

        private TimeSpan _duration = TimeSpan.Zero;
        private TimeSpan _position = TimeSpan.Zero;
        private bool _isDragging = false;

        public MediaViewModel(MediaSessionSource source)
        {
            _mediaSource = source;
            _mediaSource.MediaInfoChanged += OnMediaInfoChanged;
            PlayPauseCommand = new DelegateCommand(async () => await TogglePlayPauseAsync());
            SkipNextCommand = new DelegateCommand(async () => await _mediaSource.TrySkipNextAsync());
            SkipPreviousCommand = new DelegateCommand(async () => await _mediaSource.TrySkipPreviousAsync());
        }

        public string Title { get => _title; private set { _title = value; OnPropertyChanged(); } }
        public string Artist { get => _artist; private set { _artist = value; OnPropertyChanged(); } }
        public bool IsPlaying { get => _isPlaying; private set { _isPlaying = value; OnPropertyChanged(); } }
        public ImageSource? AlbumArt { get => _albumArt; private set { _albumArt = value; OnPropertyChanged(); } }
        
        public TimeSpan Duration 
        { 
            get => _duration; 
            private set 
            { 
                _duration = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(TotalDurationSeconds));
            } 
        }

        public TimeSpan Position 
        { 
            get => _position; 
            private set 
            { 
                _position = value; 
                OnPropertyChanged(); 
                if (!_isDragging)
                {
                    OnPropertyChanged(nameof(CurrentPositionSeconds));
                }
            } 
        }

        public double TotalDurationSeconds => Duration.TotalSeconds;

        public double CurrentPositionSeconds
        {
            get => Position.TotalSeconds;
            set
            {
                // This setter is called by UI binding (TwoWay)
                if (!_isDragging)
                {
                    // If not dragging, we might want to seek immediately or ignore if it's just a small update
                    // But typically we use Drag events to control this.
                    // For now, let's allow immediate seek if it's a direct set (e.g. click on track)
                     _ = SeekToAsync(value);
                }
            }
        }

        public bool IsDragging
        {
            get => _isDragging;
            set => _isDragging = value;
        }

        public ICommand PlayPauseCommand { get; }
        public ICommand SkipNextCommand { get; }
        public ICommand SkipPreviousCommand { get; }

        public async Task SeekToAsync(double seconds)
        {
            await _mediaSource.TrySeekAsync(TimeSpan.FromSeconds(seconds));
        }

        private async Task TogglePlayPauseAsync()
        {
            if (IsPlaying) await _mediaSource.TryPauseAsync();
            else await _mediaSource.TryPlayAsync();
        }

        private void OnMediaInfoChanged(object? sender, MediaInfo e)
        {
            Title = e.Title;
            Artist = e.Artist;
            IsPlaying = e.IsPlaying;
            Duration = e.Duration;
            Position = e.Position;

            if (e.AlbumArtBytes != null && e.AlbumArtBytes.Length > 0)
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = new MemoryStream(e.AlbumArtBytes);
                    bmp.EndInit();
                    bmp.Freeze();
                    AlbumArt = bmp;
                }
                catch
                {
                    AlbumArt = null;
                }
            }
            else
            {
                AlbumArt = null;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

