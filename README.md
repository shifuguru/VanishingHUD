# VanishingHUD

VanishingHUD is a Grand Theft Auto V script which customises when to show or hide the Radar/Minimap. 

## Getting Started

### Prerequisites
```
dinput8.dll
ScripthookV.dll
ScriptHookVDotNet3.asi
LemonUI.dll
iFruitAddon2.dll
```

### Installing
```
1. Download and install ScriptHookV.
2. Download and install ScriptHookVDotNet 3.
3. Download and install LemonUI.
4. Place VanishingHUD.dll in ..//Grand Theft Auto/scripts
```

## Features: 
```
Once in-game summon the VanishingHUD menu by:
- dialling the VanishingHUD Contact found in the Player's Phone
- pressing F9 (customisable change key in .ini)
```

## Built With

* [Visual Studio]
* [.NET 4.8 Framework]
* [C# Language]

## Versioning

Current Progress: 

// Version 1.6 
```
Set Large Map from Online: // Function.Call(Hash.SET_BIGMAP_ACTIVE, true, false);

Set Zoom Levels: // Function.Call(Hash.SET_RADAR_ZOOM, 6000);

Detect Interiors: // Function.Call(Hash.IS_INTERIOR_SCENE());

TODO: Fix Phone is Open only when in First Person.. Phone detection not working in 3rd person... 
```
Releases: 

// Version 1.5
```
Disabled phone controls while LemonMenu open. 
```


// Version 1.4 
```
Fixed "Mod Enabled" menu button, now properly restores radar after disabling mod. 

Added Wanted Radar - Enable Radar while wanted.

Added Phone Radar - Enable Radar while holding Phone.
```
 
// Version 1.3.1 
```
Hot-fix of Contact text showing as "{ModVersion}", should have had $ infront of string
```

// Version 1.3
```
All waypoint types now included 

Mod Menu behaviour has been corrected. 

Tidied Logic, dusted cobwebs 
```

## Authors

* **Sonny / Shifuguru**

https://www.gta5-mods.com/users/shifuguru
https://www.youtube.com/channel/UCsiK5dOPsmjZ_D1V9-U1GTg




