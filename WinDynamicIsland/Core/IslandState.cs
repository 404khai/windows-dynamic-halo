namespace WinDynamicIsland.Core
{
    public enum IslandMode
    {
        Idle,
        MediaPlaying
    }

    public class IslandState
    {
        public IslandMode Mode { get; set; } = IslandMode.Idle;
    }
}
