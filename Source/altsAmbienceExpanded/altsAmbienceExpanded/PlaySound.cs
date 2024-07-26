using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

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
            if (map?.weatherManager?.curWeather?.defName == null)
			{
                return false;
			}

            if (soundDef.customTags.Contains("Any"))
            {
                return true;
            }

            if (map.weatherManager.curWeather.defName == "Clear" && soundDef.customTags.Contains("Weather_Clear"))
            {
                return true;
            }

            if (map.weatherManager.curWeather.rainRate > 0.5f && soundDef.customTags.Contains("Weather_Rain"))
            {
                return true;
            }

            if (map.weatherManager.curWeather.snowRate > 0.5f && soundDef.customTags.Contains("Weather_Snow"))
            {
                return true;
            }

            if (map.weatherManager.curWeather.defName == "Fog" && soundDef.customTags.Contains("Weather_Fog"))
            {
                return true;
            }

            if (GenLocalDate.HourInteger(map) >= 23 || GenLocalDate.HourInteger(map) <= 5)
            {
                if (soundDef.customTags.Contains("Time_Night"))
                {
                    return true;
                }
            }
            else
            {
                if (soundDef.customTags.Contains("Time_Day"))
                {
                    return true;
                }
            }

            Season currentSeason = GenLocalDate.Season(map);
            switch (currentSeason)
            {
                case Season.Spring:
                    if (soundDef.customTags.Contains("Season_Spring"))
                        return true;
                    break;
                case Season.Summer:
                    if (soundDef.customTags.Contains("Season_Summer"))
                        return true;
                    break;
                case Season.Fall:
                    if (soundDef.customTags.Contains("Season_Fall"))
                        return true;
                    break;
                case Season.Winter:
                    if (soundDef.customTags.Contains("Season_Winter"))
                        return true;
                    break;
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
