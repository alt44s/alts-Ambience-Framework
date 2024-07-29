using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using System;

namespace altsAmbientSounds
{
    public class AmbienceSoundDef : SoundDef
	{
        public List<string> customTags = new List<string>();
    }

    public class MapComponent_AmbientSoundPlayer : MapComponent
    {
        private int nextSoundTick;
        private Dictionary<string, List<AmbienceSoundDef>> loadedSounds;

        public MapComponent_AmbientSoundPlayer(Map map) : base(map)
        {
            nextSoundTick = Find.TickManager.TicksGame + AAEMod.GetRandomTickInterval();
            LoadSounds();
        }

        private void LoadSounds()
        {
            loadedSounds = new Dictionary<string, List<AmbienceSoundDef>>();

            foreach (var soundDef in DefDatabase<AmbienceSoundDef>.AllDefs)
            {
                if (AAEMod.settings.soundEnabled.TryGetValue(soundDef.defName, out bool isEnabled) && isEnabled)
                {
                    foreach (var tag in soundDef.customTags)
                    {
                        if (!loadedSounds.ContainsKey(tag))
                        {
                            loadedSounds[tag] = new List<AmbienceSoundDef>();
                        }
                        loadedSounds[tag].Add(soundDef);
                    }
                }
            }
        }

        private bool ShouldPlaySound(AmbienceSoundDef soundDef)
        {
            if (map == null)
            {
                return false;
            }

            bool CheckComplexTag(string complexTag)
            {
                var conditions = complexTag.Split(new[] { " AND " }, StringSplitOptions.None);
                foreach (var condition in conditions)
                {
                    if (!CheckCondition(condition))
                    {
                        return false;
                    }
                }
                return true;
            }

            bool CheckCondition(string condition)
            {
                if (condition == "Any")
                {
                    return true;
                }

                if (condition == "Weather_Clear" && map.weatherManager.curWeather.defName == "Clear")
                {
                    return true;
                }

                if (condition == "Weather_Rain" && map.weatherManager.curWeather.rainRate > 0.5f)
                {
                    return true;
                }

                if (condition == "Weather_Snow" && map.weatherManager.curWeather.snowRate > 0.5f)
                {
                    return true;
                }

                if (condition == "Weather_Fog" && map.weatherManager.curWeather.defName == "Fog")
                {
                    return true;
                }

                if (condition == "Time_Night" && (GenLocalDate.HourInteger(map) >= 23 || GenLocalDate.HourInteger(map) <= 5))
                {
                    return true;
                }

                if (condition == "Time_Day" && !(GenLocalDate.HourInteger(map) >= 23 || GenLocalDate.HourInteger(map) <= 5))
                {
                    return true;
                }

                Season currentSeason = GenLocalDate.Season(map);
                if (condition == "Season_Spring" && currentSeason == Season.Spring)
                {
                    return true;
                }
                if (condition == "Season_Summer" && currentSeason == Season.Summer)
                {
                    return true;
                }
                if (condition == "Season_Fall" && currentSeason == Season.Fall)
                {
                    return true;
                }
                if (condition == "Season_Winter" && currentSeason == Season.Winter)
                {
                    return true;
                }

                if (condition.StartsWith("Biome_") && condition == "Biome_" + map.Biome.defName)
                {
                    return true;
                }

                return false;
            }

            foreach (var tag in soundDef.customTags)
            {
                if (CheckComplexTag(tag))
                {
                    return true;
                }
            }

            return false;
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            if (Find.TickManager.TicksGame >= nextSoundTick)
            {
                PlayRandomAmbientSound();
                nextSoundTick = Find.TickManager.TicksGame + AAEMod.GetRandomTickInterval();
            }
        }

        private void PlayRandomAmbientSound()
        {
            var applicableSounds = new List<AmbienceSoundDef>();

            foreach (var category in loadedSounds)
            {
                if (ShouldPlaySound(category.Value.FirstOrDefault()))
                {
                    applicableSounds.AddRange(category.Value);
                }
            }

            if (applicableSounds.Any())
            {
                var soundDef = applicableSounds.RandomElement();
                soundDef?.PlayOneShotOnCamera();
            }
        }
    }
}
