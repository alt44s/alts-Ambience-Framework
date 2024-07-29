using Verse;
using UnityEngine;
using System.Collections.Generic;
using RimWorld;

namespace altsAmbientSounds
{
    public class AAEModSettings : ModSettings
    {
        public int minTickIntervalTicks = 5000;
        public int maxTickIntervalTicks = 15000;
        public Dictionary<string, bool> soundEnabled = new Dictionary<string, bool>();
        public float soundVolumeMin = 20f;
        public float soundVolumeMax = 20f;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref minTickIntervalTicks, "minTickIntervalTicks", 5000);
            Scribe_Values.Look(ref maxTickIntervalTicks, "maxTickIntervalTicks", 15000);
            Scribe_Collections.Look(ref soundEnabled, "soundEnabled", LookMode.Value, LookMode.Value);
            Scribe_Values.Look(ref soundVolumeMin, "soundVolumeMin", 20f);
            Scribe_Values.Look(ref soundVolumeMax, "soundVolumeMax", 20f);
        }
    }

    public class AAEMod : Mod
    {
        public static AAEModSettings settings;
        private Vector2 scrollPosition = Vector2.zero;
        private bool messageShown = true;
        private List<BiomeDef> biomeDefs;
        private float biomeLabelHeight;

        public AAEMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<AAEModSettings>();
            biomeDefs = DefDatabase<BiomeDef>.AllDefsListForReading;

            if (settings.soundEnabled == null)
            {
                settings.soundEnabled = new Dictionary<string, bool>();
            }

            foreach (var soundDef in DefDatabase<AmbienceSoundDef>.AllDefs)
            {
                if (!settings.soundEnabled.ContainsKey(soundDef.defName))
                {
                    if (soundDef.customTags.Contains("DisableByDefault"))
                    {
                        settings.soundEnabled[soundDef.defName] = false;
                    }
                    else
                    {
                        settings.soundEnabled[soundDef.defName] = true;
                    }
                }
            }
        }

        public override string SettingsCategory() => "alt's Ambience Framework";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            if (biomeLabelHeight == 0)
			{
                biomeLabelHeight = Text.CalcHeight("Biome_Label", inRect.width);
			}

            float viewHeight = 180f + settings.soundEnabled.Count * 30f;
            viewHeight += biomeLabelHeight * (biomeDefs.Count + 1);

            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(inRect, ref scrollPosition, viewRect);

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);

            listingStandard.Gap();

            float minTickIntervalHours = settings.minTickIntervalTicks / 2500f;
            listingStandard.Label($"{"AAE_MinSoundTick".Translate()} " + minTickIntervalHours.ToString("F1"));
            minTickIntervalHours = listingStandard.Slider(minTickIntervalHours, 0.1f, 24f);

            settings.minTickIntervalTicks = Mathf.RoundToInt(minTickIntervalHours * 2500);

            float maxTickIntervalHours = settings.maxTickIntervalTicks / 2500f;     
            listingStandard.Label($"{"AAE_MaxSoundTick".Translate()} " + maxTickIntervalHours.ToString("F1"));
            maxTickIntervalHours = listingStandard.Slider(maxTickIntervalHours, 0.1f, 24f);        

            settings.maxTickIntervalTicks = Mathf.RoundToInt(maxTickIntervalHours * 2500);

            if (settings.minTickIntervalTicks > settings.maxTickIntervalTicks)
            {
                settings.minTickIntervalTicks = settings.maxTickIntervalTicks;
            }

            listingStandard.Gap();

            listingStandard.Label($"{"AAE_MinSoundVol".Translate()} " + settings.soundVolumeMin.ToString("F0"));
            settings.soundVolumeMin = Mathf.RoundToInt(listingStandard.Slider(settings.soundVolumeMin, 0f, 150f));

            listingStandard.Label($"{"AAE_MaxSoundVol".Translate()} " + settings.soundVolumeMax.ToString("F0"));
            settings.soundVolumeMax = Mathf.RoundToInt(listingStandard.Slider(settings.soundVolumeMax, 0f, 150f));

            if (settings.soundVolumeMin > settings.soundVolumeMax)
            {
                settings.soundVolumeMin = settings.soundVolumeMax;
            }

            listingStandard.Gap();

            foreach (var soundDef in DefDatabase<AmbienceSoundDef>.AllDefs)
            {
                if (!settings.soundEnabled.TryGetValue(soundDef.defName, out bool enabled))
                {
                    enabled = true;
                    settings.soundEnabled[soundDef.defName] = enabled;
                }

                bool previousEnabledState = enabled;

                string tagsString = string.Join(", ", soundDef.customTags);

                string label = $"{"AAE_Enable".Translate()} {soundDef.defName}  <color=grey>({tagsString.ToLower()})</color>";

                listingStandard.CheckboxLabeled(label, ref enabled);
                settings.soundEnabled[soundDef.defName] = enabled;

                if (previousEnabledState != enabled)
                {
                    messageShown = false;
                }

                listingStandard.Gap(2);
            }

            listingStandard.Gap();

            listingStandard.Label("Available Biome Tags:");
            foreach (var biomeDef in biomeDefs)
            {
                string biomeTag = $"Biome_{biomeDef.defName}";
                listingStandard.Label($" - <color=grey>{biomeTag}</color>");
            }

            listingStandard.Gap();

            if (listingStandard.ButtonText("AAE_ResetToDefault".Translate()))
            {
                ResetSettings();
            }

            listingStandard.End();
            Widgets.EndScrollView();
        }

        public static int GetRandomTickInterval()
        {
            return Random.Range(settings.minTickIntervalTicks, settings.maxTickIntervalTicks + 1);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();

            if (!messageShown)
            {
                Find.WindowStack.Add(new Dialog_MessageBox("AAE_CheckboxInfo".Translate()));
                messageShown = true;
            }
        }

        public void ResetSettings()
        {
            settings.minTickIntervalTicks = 5000;
            settings.maxTickIntervalTicks = 15000;

            settings.soundVolumeMin = 20f;
            settings.soundVolumeMax = 20f;

            foreach (var soundDef in DefDatabase<AmbienceSoundDef>.AllDefs)
            {
                bool disableByDefault = soundDef.customTags.Contains("DisableByDefault");

                if (settings.soundEnabled[soundDef.defName] != !disableByDefault)
                {
                    messageShown = false;
                }

                settings.soundEnabled[soundDef.defName] = !disableByDefault;
            }
        }
    }

    [StaticConstructorOnStartup]
    public static class SoundSettingsApplier
    {
        static SoundSettingsApplier()
        {
            AAEModSettings settings = AAEMod.settings;

            foreach (var soundDef in DefDatabase<AmbienceSoundDef>.AllDefs)
            {
                foreach (var subSoundDef in soundDef.subSounds)
				{
                    subSoundDef.volumeRange.min = Mathf.Clamp(settings.soundVolumeMin, 0f, settings.soundVolumeMax);
                    subSoundDef.volumeRange.max = Mathf.Clamp(settings.soundVolumeMax, settings.soundVolumeMin, 1f);
				}
            }
        }
    }
}
