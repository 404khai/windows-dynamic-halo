using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using WindowsDynamicHalo.Core;
using WindowsDynamicHalo.Sources;
using System.Windows.Input;
using System.Windows.Threading;

namespace WindowsDynamicHalo.ViewModels
{
    public class IslandViewModel : INotifyPropertyChanged
    {
        private readonly MediaSessionSource _mediaSource;
        private readonly AnimationState _animState = new AnimationState();
        private readonly DispatcherTimer _autoCollapseTimer;
        private string _title = "Idle";
        private string _artist = "";
        private bool _hasMedia = false;
        private bool _isExpanded = false;
        private double _width = 80; // Collapsed width
        private double _height = 35; // Collapsed height

        private static int _instanceCount = 0;
        public int InstanceId { get; }

        public IslandViewModel()
        {
            InstanceId = ++_instanceCount;
            Logger.Log($"IslandViewModel: Created Instance #{InstanceId}");
            _mediaSource = new MediaSessionSource();
            _mediaSource.MediaInfoChanged += OnMediaInfoChanged;
            
            Media = new MediaViewModel(_mediaSource);

            ToggleExpandCommand = new DelegateCommand(() =>
            {
                if (HasMedia)
                {
                    IsExpanded = !IsExpanded;
                    Touch();
                }
            });

            _autoCollapseTimer = new DispatcherTimer
            {
                Interval = System.TimeSpan.FromSeconds(5)
            };
            _autoCollapseTimer.Tick += (s, e) =>
            {
                if ((System.DateTime.UtcNow - _animState.LastInteractionUtc).TotalSeconds >= 5 && IsExpanded)
                {
                    IsExpanded = false;
                }
            };
            _autoCollapseTimer.Start();
        }

        public async Task InitializeAsync()
        {
            await _mediaSource.InitializeAsync();
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
                if (_hasMedia != value)
                {
                    _hasMedia = value; 
                    Logger.Log($"IslandViewModel: HasMedia changed to {value}");
                    OnPropertyChanged(); 
                    UpdateDimensions();
                }
            }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    _animState.IsExpanded = value;
                    Logger.Log($"IslandViewModel: IsExpanded changed to {value}");
                    OnPropertyChanged();
                    UpdateDimensions();
                }
            }
        }

        public MediaViewModel Media { get; }

        public ICommand ToggleExpandCommand { get; }

        public double Width
        {
            get => _width;
            set 
            {
                if (System.Math.Abs(_width - value) > 0.1)
                {
                    _width = value; 
                    Logger.Log($"IslandViewModel: Width changed to {value}");
                    OnPropertyChanged(); 
                }
            }
        }

        public double Height
        {
            get => _height;
            set 
            { 
                if (System.Math.Abs(_height - value) > 0.1)
                {
                    _height = value; 
                    Logger.Log($"IslandViewModel: Height changed to {value}");
                    OnPropertyChanged(); 
                }
            }
        }

        private void UpdateDimensions()
        {
            if (HasMedia)
            {
                if (IsExpanded)
                {
                    // Expanded Media State (Full Card)
                    Width = 350;
                    Height = 160; 
                    StateManager.Instance.SetMode(IslandMode.ExpandedMedia);
                }
                else
                {
                    // Compact Media State (Pill with content)
                    Width = 350;
                    Height = 46; 
                    StateManager.Instance.SetMode(IslandMode.CompactMedia);
                }
            }
            else
            {
                // Idle State (Small Pill)
                Width = 80; 
                Height = 35;
                StateManager.Instance.SetMode(IslandMode.Idle);
            }
        }

        private void OnMediaInfoChanged(object? sender, MediaInfo e)
        {
            // Marshal to UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Logger.Log($"IslandViewModel [#{InstanceId}]: OnMediaInfoChanged received. IsPlaying={e.IsPlaying}, Title='{e.Title}'");
                    if (e.IsPlaying)
                    {
                        Title = e.Title;
                        Artist = e.Artist;
                        HasMedia = true;
                        
                        // Force update if dimensions are desynced
                        if (HasMedia && Width < 100)
                        {
                            Logger.Log($"IslandViewModel [#{InstanceId}]: Force updating dimensions (Width was {Width})");
                            UpdateDimensions();
                        }
                        
                        Touch();
                    }
                    else
                    {
                        // Keep media state if paused, but clear if stopped/closed logic could go here
                        // For now, we trust the session. 
                        // If session is cleared, we go idle.
                        // If just paused, we stay in media mode but show pause icon.
                        Title = e.Title;
                        Artist = e.Artist;
                        HasMedia = !string.IsNullOrEmpty(e.Title); // Only true if we actually have a track
                        
                        if (!HasMedia)
                        {
                             Title = "Idle";
                             Artist = "";
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.Log($"IslandViewModel: Error in OnMediaInfoChanged: {ex.Message}");
                }
            });
        }

        private void Touch()
        {
            _animState.LastInteractionUtc = System.DateTime.UtcNow;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
