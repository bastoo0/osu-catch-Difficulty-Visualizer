// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Difficulty.Preprocessing;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Catch.Difficulty.Skills
{
    public class Movement : Skill
    {
        private const float absolute_player_positioning_error = 16f;
        private const float normalized_hitobject_radius = 41.0f;
        private const double direction_change_bonus = 21.0;

        protected override double SkillMultiplier => 900;
        protected override double StrainDecayBase => 0.2;

        protected override double DecayWeight => 0.94;

        protected readonly float HalfCatcherWidth;

        private float? oldLastPlayerPosition;
        private float? lastPlayerPosition;

        private float oldLastDistanceMoved;
        private float lastDistanceMoved;
        private double lastStrainTime;

        private bool previousWasDirectionChange = false;

        public double DirectionChangeCount;

        public int FruitNumber = 0;

        public Movement(float halfCatcherWidth)
        {
            HalfCatcherWidth = halfCatcherWidth;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            FruitNumber++;
            var catchCurrent = (CatchDifficultyHitObject)current;

            lastPlayerPosition ??= catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                lastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - lastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime;

            // We do the base scaling according to the distance moved
            double distanceAddition = Math.Pow(Math.Abs(distanceMoved), 0.50) / 140;

            double edgeDashBonus = 0;

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                // Bonus increased
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 3.2;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= Math.Min(5, 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20) * Math.Pow(Math.Min(1.5 * catchCurrent.StrainTime, 265) / 265, 1.5) / catchCurrent.ClockRate); // Edge Dashes are easier at lower ms values            }
            }
            double distanceRatioBonus;
            // Gives weight to non-hyperdashes
            if (!catchCurrent.LastObject.HyperDash)
            {
                // Speed is the ratio between "1/strain time" and the distance moved

                //Give value to long and fast movements
                distanceRatioBonus = 2.5 * Math.Abs(distanceMoved) / weightedStrainTime;

                if (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved) && Math.Sign(lastDistanceMoved) != 0 && Math.Abs(distanceMoved) > 4)
                {
                    DirectionChangeCount += 1;
                    distanceRatioBonus *= 4.8;

                    // Give value to short movements if multiple direction changes (for wiggles)
                    if (Math.Abs(distanceMoved) < 120)
                    {
                        distanceRatioBonus *= 1.22;
                        if (previousWasDirectionChange)
                        {
                            distanceRatioBonus += (catchCurrent.BaseObject.HyperDash ? 0.7 : 1) *  Math.Log(120 / Math.Abs(distanceMoved), 1.40) * 280 / weightedStrainTime;
                        }
                    }
                    previousWasDirectionChange = true;
                }
                else previousWasDirectionChange = false;

            }
            else // Hyperdashes calculation
            {
                double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(lastDistanceMoved)) / 70, 0.38) * 2;
                bool directionChanged = (Math.Sign(distanceMoved) != Math.Sign(lastDistanceMoved));
                bool bonusFactor = previousWasDirectionChange && directionChanged;
                distanceRatioBonus = Math.Log(4.2 * Math.Abs(distanceMoved) / weightedStrainTime * antiflowFactor * (bonusFactor ? 1.2 : 1) * (directionChanged ? (catchCurrent.BaseObject.HyperDash ? 1.6 : 1) : 0.6) + 0.7, 1.75) + 0.7;
                //distance scaling (long distances nerf)
                double scaledDistance = Math.Abs(distanceMoved) / (CatchPlayfield.WIDTH / 2);
                distanceRatioBonus *= Math.Min(-0.22 * Math.Abs(scaledDistance) + 1, 1);
                previousWasDirectionChange = directionChanged;
            }
            double distanceRatioBonusFactor = 4.95;


            distanceAddition *= distanceRatioBonusFactor * distanceRatioBonus;

            lastPlayerPosition = playerPosition;
            lastDistanceMoved = distanceMoved;

            return distanceAddition / weightedStrainTime;
        }






        protected override double OldStrainValueOf(DifficultyHitObject current)
        {
            var catchCurrent = (CatchDifficultyHitObject)current;

            oldLastPlayerPosition ??= catchCurrent.LastNormalizedPosition;

            float playerPosition = Math.Clamp(
                oldLastPlayerPosition.Value,
                catchCurrent.NormalizedPosition - (normalized_hitobject_radius - absolute_player_positioning_error),
                catchCurrent.NormalizedPosition + (normalized_hitobject_radius - absolute_player_positioning_error)
            );

            float distanceMoved = playerPosition - oldLastPlayerPosition.Value;

            double weightedStrainTime = catchCurrent.StrainTime + 13 + (3 / catchCurrent.ClockRate);

            double distanceAddition = (Math.Pow(Math.Abs(distanceMoved), 1.3) / 510);
            double sqrtStrain = Math.Sqrt(weightedStrainTime);

            double edgeDashBonus = 0;

            // Direction change bonus.
            if (Math.Abs(distanceMoved) > 0.1)
            {
                if (Math.Abs(oldLastDistanceMoved) > 0.1 && Math.Sign(distanceMoved) != Math.Sign(oldLastDistanceMoved))
                {
                    double bonusFactor = Math.Min(50, Math.Abs(distanceMoved)) / 50;
                    double antiflowFactor = Math.Max(Math.Min(70, Math.Abs(oldLastDistanceMoved)) / 70, 0.38);

                    distanceAddition += direction_change_bonus / Math.Sqrt(lastStrainTime + 16) * bonusFactor * antiflowFactor * Math.Max(1 - Math.Pow(weightedStrainTime / 1000, 3), 0);
                }

                // Base bonus for every movement, giving some weight to streams.
                distanceAddition += 12.5 * Math.Min(Math.Abs(distanceMoved), normalized_hitobject_radius * 2) / (normalized_hitobject_radius * 6) / sqrtStrain;
            }

            // Bonus for edge dashes.
            if (catchCurrent.LastObject.DistanceToHyperDash <= 20.0f)
            {
                if (!catchCurrent.LastObject.HyperDash)
                    edgeDashBonus += 5.7;
                else
                {
                    // After a hyperdash we ARE in the correct position. Always!
                    playerPosition = catchCurrent.NormalizedPosition;
                }

                distanceAddition *= 1.0 + edgeDashBonus * ((20 - catchCurrent.LastObject.DistanceToHyperDash) / 20) * Math.Pow((Math.Min(catchCurrent.StrainTime * catchCurrent.ClockRate, 265) / 265), 1.5); // Edge Dashes are easier at lower ms values
            }

            oldLastPlayerPosition = playerPosition;
            oldLastDistanceMoved = distanceMoved;
            lastStrainTime = catchCurrent.StrainTime;

            return distanceAddition / weightedStrainTime;
        }
    }
}


