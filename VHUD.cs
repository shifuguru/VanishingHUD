using GTA;
using GTA.UI;
using GTA.Math;
using GTA.Native;
using Screen = GTA.UI.Screen;
using LemonUI;
using LemonUI.Menus;
using iFruitAddon2;
using System;
using System.IO;
using System.Windows.Forms;

namespace VanishingHUD
{
    // Version 1.6a
    // Removed Phone method
    // Fixed issue relating to Minimap showing after pressing 's', was to do with the Phone method, which didn't even work. Gone. 
    // Hm, guess not. It wasn't responsible for the 's' key bug... 
    // Function.Call(Hash.SET_BIGMAP_ACTIVE, true, false);
    // Function.Call(Hash.SET_RADAR_ZOOM, 6000);
    // RADAR ZOOM 
    // Function.Call(Hash.IS_INTERIOR_SCENE());
    // 
    // Version 1.5
    // TODO: Fix Phone is Open only when in First Person.. Phone detection not working in 3rd person... 
    // Find out why this mod's LemonUI menu allows the phone controls to be disabled
    // 
    // Version 1.4 
    // Fixed "Mod Enabled" menu button, now properly restores radar after disabling mod. 
    // Added Wanted Radar - Enable Radar while wanted.
    // Added Phone Radar - Enable Radar while holding Phone.
    // 
    // Version 1.3.1 
    // Hot-fix of Contact text showing as "{ModVersion}", should have had $ infront of string
    //
    // Version 1.3
    //
    // Additions/Changes: 
    // All waypoint types now included 
    // Mod Menu behaviour has been corrected. 
    // Tidied Logic, dusted cobwebs 
    // 
    // 
    public class VanishingHUD : Script
    {
        // Pre-Settings:
        public static string modName = "Vanishing HUD";
        public static string modVer = "Version 1.6a";

        public static ScriptSettings settings; // Settings .ini for the mod 
        public static int timerStartTime; // Time when the Script was Loaded/Start countdown to hide radar

        public static Keys menuToggleKey = Keys.J; // Shows mod menu 
        public static Keys radarToggleKey = Keys.Z; // Shows Radar/Delay Countdown Timer (if active) 
        public static Keys radarCycleKey;

        public static bool modEnabled; // is Mod Enabled 
        public static bool debugEnabled; // is Debug Enabled 

        public static bool radarVisible; // Is Radar currently visible 
        public static bool bigMapEnabled;
        public static bool radarZoomEnabled;
        public static int bigMapZoom = 0;
        public static int onFootZoom = 840;
        public static int vehicleZoom = 1200;
        public static int inBuildingZoom = 0;
        public static int minSpeedRequired = 15;
        public static int showDuration = 15; // Default: 15 ; Total Time in seconds that the radar should remain visible when the script starts. It's used in calculating the showTime.
        public static int showTimeRemaining; // Time remaining in milliseconds before radar is hidden. Displays countdown timer above the radar.
        public static string timerText; // showTimeRemaining value as a string (displays when Debug is Enabled) 

        // Options: 
        public static bool footRadarEnabled; // Enable On Foot Radar
        public static bool vehicleRadarEnabled; // Enable Vehicle Radar
        public static bool waypointRadarEnabled; // Enable Waypoint Radar 
        public static bool waypointActive; // Is Player's Waypoint active
        public static bool objectiveWaypointActive; // Are any other waypoints active
        public static bool missionRadarEnabled; // Enable Mission Radar
        public static bool wantedRadarEnabled; // Enabled Radar during police chases
        public static bool playerWanted; // Is Player currently wanted by police

        CustomiFruit _iFruit;

        // Menu
        private static readonly ObjectPool pool = new ObjectPool();
        private static readonly NativeMenu menu = new NativeMenu($"{modName}", $"{modVer}", " ");
        // Main Toggles: 
        private static readonly NativeCheckboxItem ModToggleMenuItem = new NativeCheckboxItem("Mod Enabled: ", "Enables/Disables the Mod.", modEnabled);
        private static readonly NativeCheckboxItem DebugToggleMenuItem = new NativeCheckboxItem("Debug Enabled: ", "Enables Debug Notifications. Recommended: False", debugEnabled);
        // Show Duration
        public NativeDynamicItem<int> ShowDurationDynamicMenuItem = new NativeDynamicItem<int>("Show Duration: ", "Time in seconds before hiding the Radar, 0-60 seconds", 10);
        // Map State
        private static readonly NativeDynamicItem<int> OnFootMapZoomLevelDynamicMenuItem = new NativeDynamicItem<int>("On Foot Map Zoom Level: ", "Sets the Radar Zoom Level.", onFootZoom);
        private static readonly NativeDynamicItem<int> VehicleMapZoomLevelDynamicMenuItem = new NativeDynamicItem<int>("Vehicle Map Zoom Level: ", "Sets the Radar Zoom Level.", vehicleZoom);
        private static readonly NativeDynamicItem<int> BuildingMapZoomLevelDynamicMenuItem = new NativeDynamicItem<int>("In Building Map Zoom Level: ", "Sets the Radar Zoom Level.", inBuildingZoom);
        private static readonly NativeDynamicItem<int> BigMapZoomLevelDynamicMenuItem = new NativeDynamicItem<int>("Big Map Zoom Level: ", "Sets the Radar Zoom Level.", bigMapZoom);
        // Toggles: 
        private static readonly NativeCheckboxItem BigMapToggleMenuItem = new NativeCheckboxItem("Big Map Enabled: ", "Enables the Big Map from GTA: Online", bigMapEnabled);
        private static readonly NativeCheckboxItem WaypointRadarToggleMenuItem = new NativeCheckboxItem("Waypoint Radar Enabled: ", "Enables Radar while you have a Waypoint. Also works on foot with active waypoint. Recommended: True", waypointRadarEnabled);
        private static readonly NativeCheckboxItem FootRadarToggleMenuItem = new NativeCheckboxItem("Pedestrian Radar Enabled: ", "Enables Radar while on Foot. Recommended: False", footRadarEnabled);
        private static readonly NativeCheckboxItem VehicleRadarToggleMenuItem = new NativeCheckboxItem("Vehicle Radar Enabled: ", "Enables Radar while in any Vehicle. Recommended: True", vehicleRadarEnabled);
        private static readonly NativeCheckboxItem WantedRadarToggleMenuItem = new NativeCheckboxItem("Wanted Radar Enabled: ", "Enables Radar while police are actively searching for you. Recommended: True", wantedRadarEnabled);
        private static readonly NativeCheckboxItem MissionRadarToggleMenuItem = new NativeCheckboxItem("Mission Radar Enabled: ", "Enables Radar during Missions. Recommended: True", missionRadarEnabled);


        // INITIATE SCRIPT
        //
        public VanishingHUD()
        {
            LoadSettings();
            LoadMenuItems();
            LoadiFruitAddon();
            SaveSettings();

            Tick += OnTick;
            KeyDown += OnKeyDown;
            Aborted += OnAborted;
        }

        #region LOAD SETTINGS 
        //
        private void LoadSettings()
        {
            radarVisible = true; // Reset Radar to be visible
            timerStartTime = Game.GameTime; // Reset Timer Start Time            
            showTimeRemaining = timerStartTime + (showDuration * 1000) - Game.GameTime; // Calc updated per tick

            // Player Settings
            settings = ScriptSettings.Load("scripts\\VanishingHUD.ini");
            // Keys 
            menuToggleKey = settings.GetValue<Keys>("Options", "MENU TOGGLE KEY", Keys.F9);
            radarToggleKey = settings.GetValue<Keys>("Options", "RADAR TOGGLE KEY", Keys.Z);
            radarCycleKey = settings.GetValue<Keys>("Options", "RADAR CYCLE KEY", Keys.N);
            // Options
            showDuration = settings.GetValue<int>("Options", "SHOW DURATION", 15);
            modEnabled = settings.GetValue<bool>("Options", "MOD ENABLED", true);
            debugEnabled = settings.GetValue<bool>("Options", "DEBUG ENABLED", false);
            footRadarEnabled = settings.GetValue<bool>("Options", "FOOT RADAR ENABLED", false);
            vehicleRadarEnabled = settings.GetValue<bool>("Options", "VEHICLE RADAR ENABLED", true);
            waypointRadarEnabled = settings.GetValue<bool>("Options", "WAYPOINT RADAR ENABLED", true);
            wantedRadarEnabled = settings.GetValue<bool>("Options", "WANTED RADAR ENABLED", true);
            //isMissionRadarEnabled = settings.GetValue<bool>("Options", "MISSION RADAR ENABLED", true);

            // BIG MAP
            onFootZoom = settings.GetValue<int>("Options", "ON FOOT MAP ZOOM LEVEL", onFootZoom);
            vehicleZoom = settings.GetValue<int>("Options", "IN VEHICLE MAP ZOOM LEVEL", vehicleZoom);
            inBuildingZoom = settings.GetValue<int>("Options", "IN BUILDING MAP ZOOM LEVEL", inBuildingZoom);
            bigMapZoom = settings.GetValue<int>("Options", "BIG MAP ZOOM LEVEL", bigMapZoom);
            bigMapEnabled = settings.GetValue<bool>("Options", "BIG MAP ENABLED", false);

            if (bigMapEnabled)
                Function.Call(Hash.SET_BIGMAP_ACTIVE, true, false);

            // Finished Loading 
            if (debugEnabled)
                Notification.Show($"Loaded {modName} : {modVer}", true);
        }
        public static void SaveSettings()
        {
            settings = ScriptSettings.Load("scripts\\VanishingHUD.ini");
            settings.SetValue<Keys>("Options", "MENU TOGGLE KEY", menuToggleKey);
            settings.SetValue<Keys>("Options", "RADAR TOGGLE KEY", radarToggleKey);
            settings.SetValue<Keys>("Options", "RADAR CYCLE KEY", radarCycleKey);
            settings.SetValue<bool>("Options", "MOD ENABLED", modEnabled);
            settings.SetValue<bool>("Options", "DEBUG ENABLED", debugEnabled);
            settings.SetValue<bool>("Options", "WAYPOINT RADAR ENABLED", waypointRadarEnabled);
            settings.SetValue<bool>("Options", "FOOT RADAR ENABLED", footRadarEnabled);
            settings.SetValue<bool>("Options", "VEHICLE RADAR ENABLED", vehicleRadarEnabled);
            settings.SetValue<bool>("Options", "MISSION RADAR ENABLED", missionRadarEnabled);
            settings.SetValue<int>("Options", "SHOW DURATION", showDuration);
            settings.SetValue<bool>("Options", "BIG MAP ENABLED", bigMapEnabled);
            settings.SetValue<int>("Options", "BIG MAP ZOOM LEVEL", bigMapZoom);
            settings.SetValue<int>("Options", "ON FOOT MAP ZOOM LEVEL", onFootZoom);
            settings.SetValue<int>("Options", "IN VEHICLE MAP ZOOM LEVEL", vehicleZoom);
            settings.SetValue<int>("Options", "IN BUILDING MAP ZOOM LEVEL", inBuildingZoom);
            settings.Save();
        }
        #endregion

        #region LOAD MENU
        //
        private void LoadMenuItems()
        {
            // INITIALISE ITEMS FOR MOD MENU 
            pool.Add(menu);
            menu.Add(ModToggleMenuItem);
            menu.Add(DebugToggleMenuItem);
            menu.Add(BigMapToggleMenuItem);
            menu.Add(WaypointRadarToggleMenuItem);
            menu.Add(FootRadarToggleMenuItem);
            menu.Add(VehicleRadarToggleMenuItem);
            //menu.Add(MissionRadarToggleMenuItem);
            menu.Add(WantedRadarToggleMenuItem);
            menu.Add(ShowDurationDynamicMenuItem);
            menu.Add(BigMapZoomLevelDynamicMenuItem);
            menu.Add(OnFootMapZoomLevelDynamicMenuItem);
            menu.Add(VehicleMapZoomLevelDynamicMenuItem);
            menu.Add(BuildingMapZoomLevelDynamicMenuItem);

            // ITEM METHODS - CALLED WHEN MENU ITEM IS CHANGED
            ModToggleMenuItem.Activated += ToggleMod;
            DebugToggleMenuItem.Activated += ToggleDebug;
            BigMapToggleMenuItem.Activated += ToggleBigMap;
            WaypointRadarToggleMenuItem.Activated += ToggleWaypointRadar;
            FootRadarToggleMenuItem.Activated += ToggleFootRadar;
            VehicleRadarToggleMenuItem.Activated += ToggleVehicleRadar;
            MissionRadarToggleMenuItem.Activated += ToggleMissionRadar;
            WantedRadarToggleMenuItem.Activated += ToggleWantedRadar;
            // Show Duration 
            ShowDurationDynamicMenuItem.ItemChanged += UpdateShowDurationValue;
            BigMapZoomLevelDynamicMenuItem.ItemChanged += UpdateBigMapZoom;
            OnFootMapZoomLevelDynamicMenuItem.ItemChanged += UpdateOnFootMapZoom;
            VehicleMapZoomLevelDynamicMenuItem.ItemChanged += UpdateVehicleMapZoom;
            BuildingMapZoomLevelDynamicMenuItem.ItemChanged += UpdateBuildingMapZoom;

            // Load Menu Settings 
            ModToggleMenuItem.Checked = modEnabled;
            DebugToggleMenuItem.Checked = debugEnabled;
            WaypointRadarToggleMenuItem.Checked = waypointRadarEnabled;
            FootRadarToggleMenuItem.Checked = footRadarEnabled;
            VehicleRadarToggleMenuItem.Checked = vehicleRadarEnabled;
            WantedRadarToggleMenuItem.Checked = wantedRadarEnabled;
            //MissionRadarToggleMenuItem.Checked = missionRadarEnabled;
            ShowDurationDynamicMenuItem.SelectedItem = showDuration;
            BigMapToggleMenuItem.Checked = bigMapEnabled;
            BigMapZoomLevelDynamicMenuItem.SelectedItem = bigMapZoom;
            OnFootMapZoomLevelDynamicMenuItem.SelectedItem = onFootZoom;
            VehicleMapZoomLevelDynamicMenuItem.SelectedItem = vehicleZoom;
            BuildingMapZoomLevelDynamicMenuItem.SelectedItem = inBuildingZoom;
        }
        #endregion

        #region LOAD IFRUITADDON2 
        private void LoadiFruitAddon()
        {
            // Custom phone creation
            _iFruit = new CustomiFruit();

            // Phone customization (optional)
            /*
            _iFruit.CenterButtonColor = System.Drawing.Color.Orange;
            _iFruit.LeftButtonColor = System.Drawing.Color.LimeGreen;
            _iFruit.RightButtonColor = System.Drawing.Color.Purple;
            _iFruit.CenterButtonIcon = SoftKeyIcon.Fire;
            _iFruit.LeftButtonIcon = SoftKeyIcon.Police;
            _iFruit.RightButtonIcon = SoftKeyIcon.Website;
            */

            // New contact (wait 3 seconds (3000ms) before picking up the phone)
            iFruitContact contactVHUD = new iFruitContact("Vanishing HUD");
            contactVHUD.Answered += ContactAnswered;   // Linking the Answered event with our function
            contactVHUD.DialTimeout = 3000;            // Delay before answering
            contactVHUD.Active = true;                 // true = the contact is available and will answer the phone
            contactVHUD.Icon = ContactIcon.Blank;      // Contact's icon
            _iFruit.Contacts.Add(contactVHUD);         // Add the contact to the phone
        }

        private void ContactAnswered(iFruitContact contact)
        {
            // The contact has answered: 
            if (debugEnabled)
            {
                Notification.Show("Vanishing HUD Menu Opened");
            }

            if (!menu.Visible)
            {
                OpenMenu();
            }

            // We need to close the phone in a moment
            // We can close it as soon as the contact picks up by calling _iFruit.Close().
            // Here, we will close the phone in 5 seconds (5000ms). 
            _iFruit.Close();
        }
        #endregion

        #region ON TICK 
        private void OnTick(object sender, EventArgs e)
        {
            _iFruit.Update();
            pool.Process();
            
            if (menu.Visible) // DISABLE CONTROLS WHILE MENU IS OPEN
            {
                Game.DisableControlThisFrame(GTA.Control.VehicleDuck);
                Game.DisableControlThisFrame(GTA.Control.VehicleHeadlight);
                Game.DisableControlThisFrame(GTA.Control.VehicleRadioWheel);
                Game.DisableControlThisFrame(GTA.Control.VehicleRoof);
                Game.DisableControlThisFrame(GTA.Control.VehicleSelectNextWeapon);
                Game.DisableControlThisFrame(GTA.Control.VehicleSelectPrevWeapon);
                Game.DisableControlThisFrame(GTA.Control.VehicleFlyAttack);
            }

            if (!modEnabled) return;
            
            // D-Pad Down check: 
            if (Game.IsControlJustPressed(GTA.Control.MultiplayerInfo))
            {
                showRadar();
            }
            
            if (radarVisible)
            {
                TimerCountdown();
                RadarZoomUpdate();
            }

            UpdateRadar();

            /*
            // RESET RADAR VISIBILTY
            else
            {
                if (Function.Call<bool>(Hash.IS_RADAR_PREFERENCE_SWITCHED_ON, true))
                {
                    showRadar();
                }
                else
                {
                    hideRadar();
                }
            }
            */
        }
        #endregion

        #region  OnAborted
        private void OnAborted(object sender, EventArgs e)
        {
            try
            {
                pool.RefreshAll();

                if (Function.Call<bool>(Hash.IS_RADAR_PREFERENCE_SWITCHED_ON, true))
                {
                    showRadar();
                }
                else
                {
                    hideRadar();
                }
            }
            catch
            {
                
            }
        }
        #endregion
        //

        public static void RadarZoomUpdate()
        {
            bool isInBuilding = Function.Call<bool>(Hash.IS_INTERIOR_SCENE);
            
            if (debugEnabled)
            {
                Screen.ShowSubtitle($"Interior?: {isInBuilding}", 400);
            }

            int currentZoom;

            if (!isInBuilding)
            {
                bool isInVehicle = Game.Player.Character.IsInVehicle();
                if (bigMapEnabled)
                {
                    currentZoom = bigMapZoom;
                }
                else if (isInVehicle)
                {
                    currentZoom = vehicleZoom;
                }
                else
                {
                    currentZoom = onFootZoom;
                }
            }
            else
            {  
                currentZoom = inBuildingZoom; 
            }

            Function.Call(Hash.SET_RADAR_ZOOM, currentZoom);
        }

        private void OpenMenu()
        {
            menu.Visible = true;
        }

        private void CloseMenu()
        {
            menu.Visible = false;
            SaveSettings();
        }

        #region KEY DOWN 
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == menuToggleKey)
            {
                if (!menu.Visible)
                {
                    OpenMenu();
                }
                else
                {
                    CloseMenu();
                }
            }

            // Check if the Radar Toggle Key is pressed (you can change this to another key/button)
            if (e.KeyCode == radarToggleKey)
            {
                if (modEnabled)
                {
                    showRadar();
                }
            }
        }
        #endregion
        
        // UPDATE EVENTS
        //

        // UPDATE RADAR
        // Tick Event to determine Radar Visibility
        private void UpdateRadar()
        {
            UpdateWaypointRadar();
            UpdateFootRadar();
            UpdateVehicleRadar();
            UpdateWantedRadar();
        }
        // SHOW RADAR
        // Command to Show Radar immediately
        private void showRadar()
        {
            if (Function.Call<bool>(Hash.IS_RADAR_PREFERENCE_SWITCHED_ON))
            {
                Function.Call(Hash.DISPLAY_RADAR, true);
                radarVisible = true;
            }
            timerStartTime = Game.GameTime;
        }
        // HIDE RADAR
        // Command to begin Countdown Timer and to Hide Radar when Timer reaches 0 seconds
        private void hideRadar()
        {
            if (radarVisible)
            {
                Function.Call(Hash.DISPLAY_RADAR, false);
                // if (hideHUD) {Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);}
                radarVisible = false;
            }
        }
        
        // TIMERS: 
        // TIMER COUNTDOWN. 
        // Performed per tick 
        private void TimerCountdown()
        {
            // Only performed if timerStartTime is greater than 0, preventing memory leaks 
            if (timerStartTime > 0)
            {
                // Calculate the time elapsed
                int TimeElapsed = Game.GameTime - timerStartTime;
                // Calculate the time remaining in milliseconds 
                showTimeRemaining = (showDuration * 1000) - TimeElapsed;
                // Ensure the time remaining is not negative
                showTimeRemaining = Math.Max(0, showTimeRemaining);
                // Format the time remaining as a string (for debuggin)
                timerText = $"Hiding in: {showTimeRemaining / 1000} seconds.";
            }
            
            // Check if it's time to hide the radar
            if (showTimeRemaining <= 0)
            {
                // Hide Radar here,
                // All other methods conditionally Show Radar
                hideRadar();
                timerStartTime = 0;
            }
        }

        // UPDATE WAYPOINT RADAR
        private void UpdateWaypointRadar()
        {
            if (waypointRadarEnabled)
            {
                // OBJECTIVE WAYPOINT ACTIVE:
                Blip[] ActiveBlips = World.GetAllBlips();
                // Get Array of All Blips on Map
                foreach (var Blip in ActiveBlips)
                {
                    // Stops foreach once it finds an existing Blip with a GPS route 
                    if (Function.Call<bool>(Hash.DOES_BLIP_EXIST, Blip) && Function.Call<bool>(Hash.DOES_BLIP_HAVE_GPS_ROUTE, Blip))
                    {
                        objectiveWaypointActive = true;
                        break;
                    }
                    else
                    {
                        objectiveWaypointActive = false;
                    }
                }

                // Check if Player waypoint is active. Old code, have yet to test if Blip[] also includes player waypoint 
                waypointActive = Function.Call<bool>(Hash.IS_WAYPOINT_ACTIVE, true) || objectiveWaypointActive;

                // Show/Hide Radar, to test player waypoint remove 'wapointActive'
                if (waypointActive)
                {
                    showRadar();
                }
            }
        }

        // UPDATE FOOT RADAR
        private void UpdateFootRadar()
        {
            if (footRadarEnabled)
            {
                bool onFoot = Game.Player.Character.IsOnFoot;

                if (onFoot)
                {
                    showRadar();
                }
            }
        }

        // UPDATE VEHICLE RADAR
        private void UpdateVehicleRadar()
        {
            if (vehicleRadarEnabled)
            {
                bool inVeh = Game.Player.Character.IsSittingInVehicle();

                if (inVeh)
                {
                    showRadar();
                }
            }
        }

        // UPDATE WANTED RADAR
        private void UpdateWantedRadar()
        {
            if (wantedRadarEnabled)
            {
                if (Game.Player.WantedLevel > 0)
                {
                    showRadar();
                }
            }
        }

        // TOGGLES 
        private void ToggleMod(object sender, EventArgs e)
        {
            modEnabled = !modEnabled;
            ModToggleMenuItem.Checked = modEnabled;
            timerStartTime = Game.GameTime;
            Notification.Show($"VanishingHUD enabled {modEnabled}", false);
            SaveSettings();
        }
        private void ToggleDebug(object sender, EventArgs e)
        {
            debugEnabled = !debugEnabled;
            DebugToggleMenuItem.Checked = debugEnabled;
            Notification.Show($"Debug Enabled: {debugEnabled}", false);
            SaveSettings();
        }
        private void ToggleBigMap(object sender, EventArgs e)
        {
            bigMapEnabled = !bigMapEnabled;
            BigMapToggleMenuItem.Checked = bigMapEnabled;
            SaveSettings();
            if (bigMapEnabled)
                Function.Call(Hash.SET_BIGMAP_ACTIVE, true, false);
            else
                Function.Call(Hash.SET_BIGMAP_ACTIVE, false, false);

            if (debugEnabled)
                Notification.Show($"Big Map Enabled: {bigMapEnabled}");
        }
        private void ToggleWaypointRadar(object sender, EventArgs e)
        {
            waypointRadarEnabled = !waypointRadarEnabled;
            WaypointRadarToggleMenuItem.Checked = waypointRadarEnabled;
            SaveSettings();

            if (debugEnabled)
                Notification.Show($"Waypoint Radar Enabled: {waypointRadarEnabled}", false);
        }
        private void ToggleFootRadar(object sender, EventArgs e)
        {
            footRadarEnabled = !footRadarEnabled;
            FootRadarToggleMenuItem.Checked = footRadarEnabled;
            SaveSettings();

            if (debugEnabled)
                Notification.Show($"Foot Radar Enabled: {footRadarEnabled}", false);
        }
        private void ToggleVehicleRadar(object sender, EventArgs e)
        {
            vehicleRadarEnabled = !vehicleRadarEnabled;
            VehicleRadarToggleMenuItem.Checked = vehicleRadarEnabled;
            SaveSettings();

            if (debugEnabled)
                Notification.Show($"Vehicle Radar Enabled: {vehicleRadarEnabled}", false);
        }
        private void ToggleMissionRadar(object sender, EventArgs e)
        {
            missionRadarEnabled = !missionRadarEnabled;
            MissionRadarToggleMenuItem.Checked = missionRadarEnabled;
            SaveSettings();

            if (debugEnabled)
                Notification.Show($"Mission Radar Enabled: {missionRadarEnabled}", false);
        }
        private void ToggleWantedRadar(object sender, EventArgs e)
        {
            wantedRadarEnabled = !wantedRadarEnabled;
            WantedRadarToggleMenuItem.Checked = wantedRadarEnabled;
            SaveSettings();

            if (debugEnabled)
                Notification.Show($"Wanted Radar Enabled: {wantedRadarEnabled}");
        }
        
        // Show Duration:
        private void UpdateShowDurationValue(object sender, ItemChangedEventArgs<int> e)
        {
            const int maxDuration = 60;
            const int minDuration = 0;

            int increment = 1; // Default increment 

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -1;
            }
            else if (e.Direction == Direction.Right)
            {
                increment = 1;
            }

            // Adjust the show duration within the specified range 
            e.Object = (e.Object + increment - minDuration + (maxDuration - minDuration + 1)) % (maxDuration - minDuration + 1) + minDuration;
            showDuration = e.Object;

            if (debugEnabled)
            {
                //Notification.Show($"{showDuration}", true);
            }

            SaveSettings();
        }

        public static void UpdateBigMapZoom(object sender, ItemChangedEventArgs<int> e)
        {
            bigMapZoom = e.Object;
            int maxZoom = 5000;
            int minZoom = 0;
            int increment = 0; // Reset increment 

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -50;
            }
            else if (e.Direction == Direction.Right)
            {
                increment = 50;
            }

            int range = maxZoom - minZoom;
            bigMapZoom = (bigMapZoom + increment - minZoom + range) % range + minZoom;

            if (bigMapZoom < minZoom)
            {
                bigMapZoom = maxZoom;
            }
            else if (bigMapZoom > maxZoom)
            {
                bigMapZoom = minZoom;
            }

            e.Object = bigMapZoom;

            SaveSettings();

            // e.Object = ChangeMenuItem(currentZoom, increment, minZoom, maxZoom);
        }

        public static void UpdateOnFootMapZoom(object sender, ItemChangedEventArgs<int> e)
        {
            onFootZoom = e.Object;
            int maxZoom = 1410;
            int minZoom = 840;
            int increment = 10; // Reset increment 

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -10;
            }

            int range = maxZoom - minZoom;
            onFootZoom = (onFootZoom + increment - minZoom + range) % range + minZoom;

            if (onFootZoom < minZoom)
            {
                onFootZoom = maxZoom;
            }
            else if (onFootZoom > maxZoom)
            {
                onFootZoom = minZoom;
            }

            e.Object = onFootZoom; 

            SaveSettings();

            // e.Object = ChangeMenuItem(currentZoom, increment, minZoom, maxZoom);
        }

        public static void UpdateVehicleMapZoom(object sender, ItemChangedEventArgs<int> e)
        {
            vehicleZoom = e.Object;
            int maxZoom = 1410;
            int minZoom = 840;
            int increment = 0; // Reset increment 

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -10;
            }
            else if (e.Direction == Direction.Right)
            {
                increment = 10;
            }
            int range = maxZoom - minZoom;
            vehicleZoom = (vehicleZoom + increment - minZoom + range) % range + minZoom;

            if (vehicleZoom < minZoom)
            {
                vehicleZoom = maxZoom;
            }
            else if (vehicleZoom > maxZoom)
            {
                vehicleZoom = minZoom;
            }

            e.Object = vehicleZoom;
            SaveSettings();

           // e.Object = ChangeMenuItem(currentZoom, increment, minZoom, maxZoom);
        }

        public static void UpdateBuildingMapZoom(object sender, ItemChangedEventArgs<int> e)
        {
            inBuildingZoom = e.Object;
            int maxZoom = 1400;
            int minZoom = 0;
            int increment = 25; // Init increment 

            if (inBuildingZoom <= 25)
            {
                increment = 1;
            }

            // Determine the increment based on the menu direction
            if (e.Direction == Direction.Left)
            {
                increment = -increment;
            }

            inBuildingZoom += increment;

            if (inBuildingZoom > maxZoom)
            {
                inBuildingZoom = minZoom + (inBuildingZoom - maxZoom - increment);
            }
            else if (inBuildingZoom < minZoom)
            {
                inBuildingZoom = maxZoom;
            }

            e.Object = inBuildingZoom;
            SaveSettings();

            // e.Object = ChangeMenuItem(currentZoom, increment, minZoom, maxZoom);
        }

        public static int ChangeMenuItem(int current, int increment, int min, int max)
        {
            int range = max - min;
            current = (current + increment - min + range) % range + min;

            if (current < min)
            {
                current = max;
            }
            else if (current > max)
            {
                current = min;
            }
            
            SaveSettings();
            return current;
        }
    }
}
