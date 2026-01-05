using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;

namespace WinDynamicIsland.Sources
{
    public class MediaInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public bool IsPlaying { get; set; }
    }

    public class MediaSessionSource
    {
        private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
        private GlobalSystemMediaTransportControlsSession? _currentSession;

        public event EventHandler<MediaInfo>? MediaInfoChanged;

        public async Task InitializeAsync()
        {
            try
            {
                // Request the session manager from Windows
                _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                
                if (_sessionManager != null)
                {
                    _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                    UpdateSession(_sessionManager.GetCurrentSession());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MediaSessionSource Init Failed: {ex.Message}");
            }
        }

        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            UpdateSession(sender.GetCurrentSession());
        }

        private void UpdateSession(GlobalSystemMediaTransportControlsSession? session)
        {
            // Unsubscribe from old session events
            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            }

            _currentSession = session;

            if (_currentSession != null)
            {
                // Subscribe to new session events
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                
                // Initial update
                _ = UpdateMediaInfoAsync();
            }
            else
            {
                 // No active session
                 MediaInfoChanged?.Invoke(this, new MediaInfo { IsPlaying = false });
            }
        }

        private void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
        {
            _ = UpdateMediaInfoAsync();
        }

        private void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
        {
             _ = UpdateMediaInfoAsync();
        }

        private async Task UpdateMediaInfoAsync()
        {
            if (_currentSession == null) return;

            try
            {
                var info = _currentSession.GetPlaybackInfo();
                var props = await _currentSession.TryGetMediaPropertiesAsync();

                bool isPlaying = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

                if (props != null)
                {
                    MediaInfoChanged?.Invoke(this, new MediaInfo
                    {
                        Title = props.Title,
                        Artist = props.Artist,
                        IsPlaying = isPlaying
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateMediaInfo Failed: {ex.Message}");
            }
        }
    }
}
