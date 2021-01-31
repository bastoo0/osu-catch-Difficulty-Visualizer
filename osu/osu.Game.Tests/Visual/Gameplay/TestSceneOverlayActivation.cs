﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Overlays;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneOverlayActivation : OsuPlayerTestScene
    {
        protected new OverlayTestPlayer Player => base.Player as OverlayTestPlayer;

        [Test]
        public void TestGameplayOverlayActivation()
        {
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
        }

        [Test]
        public void TestGameplayOverlayActivationPaused()
        {
            AddUntilStep("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("pause gameplay", () => Player.Pause());
            AddUntilStep("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationReplayLoaded()
        {
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("load a replay", () => Player.DrawableRuleset.HasReplayLoaded.Value = true);
            AddAssert("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
        }

        [Test]
        public void TestGameplayOverlayActivationBreaks()
        {
            AddAssert("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
            AddStep("seek to break", () => Player.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().StartTime));
            AddUntilStep("activation mode is user triggered", () => Player.OverlayActivationMode == OverlayActivation.UserTriggered);
            AddStep("seek to break end", () => Player.GameplayClockContainer.Seek(Beatmap.Value.Beatmap.Breaks.First().EndTime));
            AddUntilStep("activation mode is disabled", () => Player.OverlayActivationMode == OverlayActivation.Disabled);
        }

        protected override TestPlayer CreatePlayer(Ruleset ruleset) => new OverlayTestPlayer();

        protected class OverlayTestPlayer : TestPlayer
        {
            public new OverlayActivation OverlayActivationMode => base.OverlayActivationMode.Value;
        }
    }
}
