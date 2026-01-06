namespace WindowsDynamicHalo.Core
{
    public enum IslandMode
    {
        Idle,
        CompactMedia,  // Width 100, Shows everything horizontally
        ExpandedMedia  // Height increases, shows seek slider
    }

    public class IslandState
    {
        public IslandMode Mode { get; set; } = IslandMode.Idle;
    }
}
