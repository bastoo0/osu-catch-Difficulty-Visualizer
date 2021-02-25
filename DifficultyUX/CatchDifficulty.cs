using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using System.IO;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Difficulty;

namespace DifficultyUX
{
    class CatchDifficulty
    {
        string[] Mods;
        Result parsed;
        ProcessorWorkingBeatmap beatmap;
        public void Execute(string path, string[] mods)
        {
            Mods = mods;

            beatmap = new ProcessorWorkingBeatmap(path);
            parsed = processBeatmap(beatmap);

        }

        private Result processBeatmap(WorkingBeatmap beatmap)
        {
            // Get the ruleset
            var ruleset = new CatchRuleset();
            var attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(getMods(ruleset).ToArray());

            var result = new Result
            {
                RulesetId = ruleset.RulesetInfo.ID ?? 0,
                Beatmap = $"{beatmap.BeatmapInfo.OnlineBeatmapID} - {beatmap.BeatmapInfo}",
                Stars = attributes.StarRating.ToString("N2")
            };

            switch (attributes)
            {

                case CatchDifficultyAttributes @catch:
                    result.AttributeData = new List<(string, object)>
                    {
                        ("max combo", @catch.MaxCombo),
                        ("approach rate", @catch.ApproachRate.ToString("N2"))
                    };

                    break;

            }

            return result;
        }

        public DifficultyAttributes getCatchDifficultyAttributes()
        {
            var ruleset = new CatchRuleset();
            var attributes = ruleset.CreateDifficultyCalculator(beatmap).Calculate(getMods(ruleset).ToArray());

            return attributes;
        }

        public List<Mod> getMods(Ruleset ruleset)
        {
            var mods = new List<Mod>();
            if (Mods == null)
                return mods;

            var availableMods = ruleset.GetAllMods().ToList();

            foreach (var modString in Mods)
            {
                Mod newMod = availableMods.FirstOrDefault(m => string.Equals(m.Acronym, modString, StringComparison.CurrentCultureIgnoreCase));
                if (newMod == null)
                    throw new ArgumentException($"Invalid mod provided: {modString}");

                mods.Add(newMod);
            }

            return mods;
        }

        public string toString()
        {
            string doc = new string("");
            doc += "Beatmap: " + parsed.Beatmap + "\n";
            doc += "Old Star Rating: " + parsed.Stars + "\n";

            return doc;
        }

        public ProcessorWorkingBeatmap getProcessedBeatmap()
        {
            return beatmap;
        }

        public struct Result
        {
            public int RulesetId;
            public string Beatmap;
            public string Stars;
            public List<(string name, object value)> AttributeData;
        }
    }
}
