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

        public MediaViewModel(MediaSessionSource source)
        {
            _mediaSource = source;
            _mediaSource.MediaInfoChanged += OnMediaInfoChanged;
            PlayPauseCommand = new DelegateCommand(async () => await TogglePlayPauseAsync());
        }

        public string Title { get => _title; private set { _title = value; OnPropertyChanged(); } }
        public string Artist { get => _artist; private set { _artist = value; OnPropertyChanged(); } }
        public bool IsPlaying { get => _isPlaying; private set { _isPlaying = value; OnPropertyChanged(); } }
        public ImageSource? AlbumArt { get => _albumArt; private set { _albumArt = value; OnPropertyChanged(); } }

        public ICommand PlayPauseCommand { get; }

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

