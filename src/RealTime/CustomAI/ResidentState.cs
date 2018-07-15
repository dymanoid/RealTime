// <copyright file="ResidentState.cs" company="dymanoid">Copyright (c) dymanoid. All rights reserved.</copyright>

namespace RealTime.CustomAI
{
    /// <summary>Possible citizen's states.</summary>
    internal enum ResidentState : byte
    {
        /// <summary>The state could not be determined.</summary>
        Unknown,

        /// <summary>The citizen should be ignored, just a dummy traffic.</summary>
        Ignored,

        /// <summary>The citizen is moving to the home building.</summary>
        MovingHome,

        /// <summary>The citizen is in the home building.</summary>
        AtHome,

        /// <summary>The citizen is moving to a target that is not their home building.</summary>
        MovingToTarget,

        /// <summary>The citizen is in the school or work building.</summary>
        AtSchoolOrWork,

        /// <summary>The citizen has lunch time.</summary>
        AtLunch,

        /// <summary>The citizen is shopping in a commercial building.</summary>
        Shopping,

        /// <summary>The citizen is in a commercial leisure building or in a beautification building.</summary>
        AtLeisureArea,

        /// <summary>The citizen visits a building.</summary>
        Visiting,

        /// <summary>The citizen is on a guided tour.</summary>
        OnTour,

        /// <summary>The citizen is evacuating.</summary>
        Evacuating,

        /// <summary>The citizen is in a shelter building.</summary>
        InShelter
    }
}