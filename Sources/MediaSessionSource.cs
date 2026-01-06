using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Control;
using Windows.Storage.Streams;
using System.IO;
using System.Windows.Media.Imaging;
using WindowsDynamicHalo.Core;
using System.Windows.Threading;

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
        private DispatcherTimer _pollTimer;

        public event EventHandler<MediaInfo>? MediaInfoChanged;

        public MediaSessionSource()
        {
            _pollTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _pollTimer.Tick += OnPollTimerTick;
            _pollTimer.Start();
        }

        private void OnPollTimerTick(object? sender, EventArgs e)
        {
            // Fallback: If no session, or just to ensure we are in sync, check session manager
            if (_sessionManager != null)
            {
                var current = _sessionManager.GetCurrentSession();
                if (current != null && (_currentSession == null || current.SourceAppUserModelId != _currentSession.SourceAppUserModelId))
                {
                    Logger.Log($"Poll: Detected new session: {current.SourceAppUserModelId}");
                    UpdateSession(current);
                }
                else if (_currentSession != null)
                {
                    // Force update periodically if we have a session, just in case events missed
                    _ = UpdateMediaInfoAsync();
                }
            }
            else
            {
                // Try re-requesting manager if it failed initially
                 _ = InitializeAsync();
            }
        }

        public async Task InitializeAsync()
        {
            if (_sessionManager != null) return;

            try
            {
                Logger.Log("MediaSessionSource: Requesting SessionManager...");
                // Request the session manager from Windows
                _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
                
                if (_sessionManager != null)
                {
                    Logger.Log("MediaSessionSource: SessionManager acquired.");
                    _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                    UpdateSession(_sessionManager.GetCurrentSession());
                }
                else
                {
                    Logger.Log("MediaSessionSource: SessionManager returned NULL.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"MediaSessionSource Init Failed: {ex.Message}");
                Debug.WriteLine($"MediaSessionSource Init Failed: {ex.Message}");
            }
        }

        private void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
        {
            Logger.Log("MediaSessionSource: OnCurrentSessionChanged fired.");
            UpdateSession(sender.GetCurrentSession());
        }

        private void UpdateSession(GlobalSystemMediaTransportControlsSession? session)
        {
            // Unsubscribe from old session events
            if (_currentSession != null)
            {
                try
                {
                    _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                    _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                    _currentSession.TimelinePropertiesChanged -= OnTimelinePropertiesChanged;
                }
                catch { /* Ignore if object is disposed/invalid */ }
            }

            _currentSession = session;

            if (_currentSession != null)
            {
                Logger.Log($"MediaSessionSource: Updating Session -> {_currentSession.SourceAppUserModelId}");
                // Subscribe to new session events
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
                _currentSession.TimelinePropertiesChanged += OnTimelinePropertiesChanged;
                
                // Initial update
                _ = UpdateMediaInfoAsync();
            }
            else
            {
                 Logger.Log("MediaSessionSource: No active session.");
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
            if (_currentSession == null) 
            {
                Logger.Log("UpdateMediaInfoAsync: _currentSession is null. Aborting.");
                return;
            }

            try
            {
                Logger.Log("UpdateMediaInfoAsync: Fetching info...");
                var info = _currentSession.GetPlaybackInfo();
                var props = await _currentSession.TryGetMediaPropertiesAsync();
                var timeline = _currentSession.GetTimelineProperties();

                bool isPlaying = info.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
                Logger.Log($"UpdateMediaInfoAsync: PlaybackStatus={info.PlaybackStatus}, IsPlaying={isPlaying}");

                if (props != null)
                {
                    Logger.Log($"UpdateMediaInfoAsync: Props found. Title='{props.Title}', Artist='{props.Artist}'");
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
                            Logger.Log($"UpdateMediaInfoAsync: Thumbnail loaded ({artBytes.Length} bytes).");
                        }
                        else
                        {
                            Logger.Log("UpdateMediaInfoAsync: Thumbnail is null.");
                        }
                    }
                    catch (Exception exThumb)
                    {
                        Logger.Log($"Thumbnail read failed: {exThumb.Message}");
                    }

                    MediaInfoChanged?.Invoke(this, new MediaInfo
                    {
                        Title = props.Title,
                        Artist = props.Artist,
                        IsPlaying = isPlaying,
                        AlbumArtBytes = artBytes,
                        Duration = timeline?.EndTime ?? TimeSpan.Zero,
                        Position = timeline?.Position ?? TimeSpan.Zero,
                        LastUpdated = DateTime.UtcNow
                    });
                }
                else
                {
                    Logger.Log("UpdateMediaInfoAsync: Props are null. Retrying once...");
                    // Retry once if props are null but session exists
                    await Task.Delay(100);
                    props = await _currentSession.TryGetMediaPropertiesAsync();
                    if (props != null)
                    {
                         Logger.Log($"UpdateMediaInfoAsync: Retry success. Title='{props.Title}'");
                         // Recursively call or handle here? simpler to just proceed
                         // For now, if still null, we just send playing status
                         MediaInfoChanged?.Invoke(this, new MediaInfo { IsPlaying = isPlaying });
                    }
                    else
                    {
                        Logger.Log("UpdateMediaInfoAsync: Retry failed. Props still null.");
                        MediaInfoChanged?.Invoke(this, new MediaInfo { IsPlaying = isPlaying });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"UpdateMediaInfo Failed: {ex.Message}");
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
