// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty;

namespace osu.Game.Rulesets.Catch.Difficulty
{
    public class CatchDifficultyAttributes : DifficultyAttributes
    {
        public double ApproachRate;
        public List<double> DifficultyFactor;
        public List<double> RawDiff;
        public List<double> NewDiff;
        public double NewStarRating;

    }
}
