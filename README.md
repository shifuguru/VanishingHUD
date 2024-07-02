# VanishingHUD

Description: Highly customisable script to Show / Hide the Radar based on different circumstances, such as; being on-foot, in a vehicle, having an active waypoint, being wanted by the police, and customise the time it takes to hide the Radar. 

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

dinput8.dll
ScripthookV.dll
ScriptHookVDotNet3.asi
LemonUI.dll
iFruitAddon2.dll

```
Give examples
```

### Installing

1. Download and install ScriptHookV.
2. Download and install ScriptHookVDotNet 3.
3. Download and install LemonUI.
4. Place VanishingHUD.dll in ..//Grand Theft Auto/scripts

Once in-game you can summon the VanishingHUD menu by dialling the VanishingHUD Contact found in the Player's Phone, or by pressing F9 (customisable change key in .ini). 

## Features: 

Explain how to run the automated tests for this system

## Built With

* [Visual Studio]
* [.NET 4.8 Framework]
* [C# Language]

## Contributing

Provide feedback as desired. 

## Versioning

See Version history below. 

## Authors

* **Sonny / Shifuguru**

https://www.youtube.com/channel/UCsiK5dOPsmjZ_D1V9-U1GTg

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone whose code was used
* Inspiration
* etc




Current Progress: 

// Version 1.6 

Set Large Map from Online: // Function.Call(Hash.SET_BIGMAP_ACTIVE, true, false);

Set Zoom Levels: // Function.Call(Hash.SET_RADAR_ZOOM, 6000);

Detect Interiors: // Function.Call(Hash.IS_INTERIOR_SCENE());

TODO: Fix Phone is Open only when in First Person.. Phone detection not working in 3rd person... 

Releases: 

// Version 1.5

// Find out why this mod's LemonUI menu allows the phone controls to be disabled



// Version 1.4 

// Fixed "Mod Enabled" menu button, now properly restores radar after disabling mod. 

// Added Wanted Radar - Enable Radar while wanted.

// Added Phone Radar - Enable Radar while holding Phone.

 
// Version 1.3.1 

// Hot-fix of Contact text showing as "{ModVersion}", should have had $ infront of string



// Version 1.3

// All waypoint types now included 

// Mod Menu behaviour has been corrected. 

// Tidied Logic, dusted cobwebs 

