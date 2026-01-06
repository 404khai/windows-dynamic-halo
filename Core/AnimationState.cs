using System;

namespace WindowsDynamicHalo.Core
{
    // Represents interactive animation state for the island.
    // - IsExpanded: current visual state
    // - LastInteractionUtc: used for auto-collapse after inactivity
    public class AnimationState
    {
        public bool IsExpanded { get; set; }
        public DateTime LastInteractionUtc { get; set; } = DateTime.UtcNow;
    }
}

