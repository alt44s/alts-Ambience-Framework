# alt's Ambience Framework Documentation

## Features

### Sound Configuration

**alt's Ambience Framework** allows you to configure ambient sounds using the `AmbienceSoundDef` class, which extends RimWorld’s `SoundDef`.

Any mod using this framework **MUST** be loaded after it with it being in the active modlist.

#### XML Configuration

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Defs>
	<altsAmbientSounds.AmbienceSoundDef>
		<defName>*your defname</defName>
		<context>MapOnly</context>
		<subSounds>
		  <li>
			<onCamera>true</onCamera>
			<grains>
			  <li Class="AudioGrain_Clip">
				<clipPath>*sound file name</clipPath>
			  </li>
			</grains>
			<volumeRange>20~20</volumeRange>
		  </li>
		</subSounds>
		<customTags>
		  <li>*insert appropriate tags here</li>
                  <!-- Insert more tags if necessary -->
		</customTags>
	</altsAmbientSounds.AmbienceSoundDef>
</Defs>
```
- `<defName>`: Unique name for the sound definition.
- `<customTags>`: Specify conditions for sound playback (`Any`, `Weather_Clear`, `Time_Night`, etc.).

Custom Tags

  `Any`: Plays regardless of conditions.
  
  `Weather_Clear`: Plays when the weather is clear.
  
  `Weather_Rain`: Plays when it’s raining.
  
  `Weather_Snow`: Plays when it’s snowing.
  
  `Weather_Fog`: Plays when it’s foggy.
  
  `Time_Night`: Plays during the night.
  
  `Time_Day`: Plays during the day.
  
  `Season_Spring`: Plays in spring.
  
  `Season_Summer`: Plays in summer.
  
  `Season_Fall`: Plays in fall.
  
  `Season_Winter`: Plays in winter.
  
**Note:** When using multiple tags in the `<customTags>` field, the selection works on an OR basis. This means that the sound will be eligible for playback if any one of the specified tags (e.g., `Weather_Rain` OR `Time_Night`) is present. However, you must specify at least one tag in the `<customTags>` field; otherwise, the sound will not play. The sound does not require all tags to be present, only one of the specified tags is needed for the sound to be selected.

## Mod Settings

- Adjust **Tick Interval Ranges** and **Sound Volume Ranges** in mod settings.
- Disable sounds individually via settings menu.
