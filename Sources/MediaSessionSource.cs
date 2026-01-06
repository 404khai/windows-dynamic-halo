using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;
using System.IO;
using System.Windows.Media.Imaging;

namespace WindowsDynamicHalo.Sources
{
    public class MediaInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public bool IsPlaying { get; set; }
        public byte[]? AlbumArtBytes { get; set; }
        public TimeSpan Duration { get; set; } = TimeSpan.Zero;
        public TimeSpan Position { get; set; } = TimeSpan.Zero;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
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
                _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
            }

            _currentSession = session;

            if (_currentSession != null)
            {
                // Subscribe to new session events
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                
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

        private void OnTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession sender, TimelinePropertiesChangedEventArgs args)
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
                var timeline = _currentSession.GetTimelineProperties();

                bool isPlaying = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

                if (props != null)
                {
                    byte[]? artBytes = null;
                    try
                    {
                        var thumb = props.Thumbnail;
                        if (thumb != null)
                        {
                            var ras = await thumb.OpenReadAsync();
                            using var netStream = WindowsRuntimeStreamExtensions.AsStreamForRead(ras);
                            using var ms = new MemoryStream();
                            await netStream.CopyToAsync(ms);
                            artBytes = ms.ToArray();
                        }
                    }
                    catch (System.Exception exThumb)
                    {
                        Debug.WriteLine($"Thumbnail read failed: {exThumb.Message}");
                    }

                    MediaInfoChanged?.Invoke(this, new MediaInfo
                    {
                        Title = props.Title,
                        Artist = props.Artist,
                        IsPlaying = isPlaying,
                        AlbumArtBytes = artBytes,
                        Duration = timeline.EndTime,
                        Position = timeline.Position,
                        LastUpdated = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UpdateMediaInfo Failed: {ex.Message}");
            }
        }

        public async Task TryPlayAsync()
        {
            if (_currentSession != null)
            {
                try { await _currentSession.TryPlayAsync(); } catch (Exception ex) { Debug.WriteLine($"TryPlayAsync failed: {ex.Message}"); }
            }
        }

        public async Task TryPauseAsync()
        {
            if (_currentSession != null)
            {
                try { await _currentSession.TryPauseAsync(); } catch (Exception ex) { Debug.WriteLine($"TryPauseAsync failed: {ex.Message}"); }
            }
        }

        public async Task TrySeekAsync(TimeSpan position)
        {
            if (_currentSession != null)
            {
                try { await _currentSession.TryChangePlaybackPositionAsync(position.Ticks); } catch (Exception ex) { Debug.WriteLine($"TrySeekAsync failed: {ex.Message}"); }
            }
        }

        public async Task TrySkipNextAsync()
        {
            if (_currentSession != null)
            {
                try { await _currentSession.TrySkipNextAsync(); } catch (Exception ex) { Debug.WriteLine($"TrySkipNextAsync failed: {ex.Message}"); }
            }
        }

        public async Task TrySkipPreviousAsync()
        {
            if (_currentSession != null)
            {
                try { await _currentSession.TrySkipPreviousAsync(); } catch (Exception ex) { Debug.WriteLine($"TrySkipPreviousAsync failed: {ex.Message}"); }
            }
        }
    }
}
