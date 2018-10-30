﻿using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BonVoyage
{
    /// <summary>
    /// Part module of Bon Voyage
    /// </summary>
    class BonVoyageModule : PartModule
    {
        #region KSP Fields

        /// <summary>
        /// BonVoyage controller is active (moving vessel)
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool active = false;

        /// <summary>
        /// BonVoyage controller is shutdown
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool shutdown = false;

        /// <summary>
        /// Vessel type - 0 - rover, 1 - ship
        /// </summary>
        // localize, when ship part is ready
        [KSPField(isPersistant = true, guiActive = false, guiActiveEditor = false, guiName = "Vessel type")]
        [UI_ChooseOption(scene = UI_Scene.None, options = new[] { "0", "1" }, display = new[] { "Rover", "Ship" })]
        public string vesselType = "0";

        /// <summary>
        /// Target latitude
        /// </summary>
        [KSPField(isPersistant = true)]
        public double targetLatitude = 0;

        /// <summary>
        /// Target longitude
        /// </summary>
        [KSPField(isPersistant = true)]
        public double targetLongitude = 0;

        /// <summary>
        /// Distance to target
        /// </summary>
        [KSPField(isPersistant = true)]
        public double distanceToTarget = 0;

        /// <summary>
        /// Distance travelled
        /// </summary>
        [KSPField(isPersistant = true)]
        public double distanceTravelled = 0;

        /// <summary>
        /// Encoded path
        /// </summary>
        [KSPField(isPersistant = true)]
        public string pathEncoded = "";

        /// <summary>
        /// Average vessel speed
        /// </summary>
        [KSPField(isPersistant = true)]
        public double averageSpeed = 0;

        /// <summary>
        /// Average vessel speed at night
        /// </summary>
        [KSPField(isPersistant = true)]
        public double averageSpeedAtNight = 0;

        /// <summary>
        /// Last time when were fields updated
        /// </summary>
        [KSPField(isPersistant = true)]
        public double lastTimeUpdated = 0;

        /// <summary>
        /// Vessel is only solar powered
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool solarPowered = false;

        /// <summary>
        /// Vessel is manned
        /// </summary>
        [KSPField(isPersistant = true)]
        public bool manned = false;

        /// <summary>
        /// Root part height from the terrain
        /// </summary>
        [KSPField(isPersistant = true)]
        public double vesselHeightFromTerrain = 0;

        #endregion


        /// <summary>
        /// Return info about module
        /// </summary>
        /// <returns>Module info</returns>
        public override string GetInfo()
        {
            return "Bon Voyage Controller";
        }


        /// <summary>
        /// Module start
        /// </summary>
        /// <param name="state">Start state</param>
        public override void OnStart(PartModule.StartState state)
        {
            Events["ToggleBVController"].guiName = (!shutdown ? Localizer.Format("#LOC_BV_ContextMenu_Shutdown") : Localizer.Format("#LOC_BV_ContextMenu_Activate"));
            if (HighLogic.LoadedSceneIsEditor)
            {
                //Fields["vesselType"].guiActive = !shutdown;
            }
            if (HighLogic.LoadedSceneIsFlight)
            {
                //Fields["vesselType"].guiActive = !shutdown;
                Events["BVControlPanel"].guiActive = !shutdown;
                Events["BVControlPanel"].guiName = Localizer.Format("#LOC_BV_ContextMenu_Panel");
            }
        }


        #region KSP Events

        /// <summary>
        /// Shutdown/Activate BV controller
        /// </summary>
        [KSPEvent(guiActive = true, guiActiveEditor = true, guiName = "Shutdown Bon Voyage Controller", category = "Bon Voyage")]
        public void ToggleBVController()
        {
            shutdown = !shutdown;
            Events["ToggleBVController"].guiName = (!shutdown ? Localizer.Format("#LOC_BV_ContextMenu_Shutdown") : Localizer.Format("#LOC_BV_ContextMenu_Activate"));
            if (!HighLogic.LoadedSceneIsEditor)
            {
                //Fields["vesselType"].guiActive = !shutdown;
                Events["BVControlPanel"].guiActive = !shutdown;
                if (shutdown)
                {
                    //if (active)
                    //    Deactivate();
                    //BonVoyage.Instance.HideModuleControl();
                }
                BonVoyage.Instance.SetShutdownState(this.vessel.id, shutdown);
            }
            else
            {
                //Fields["vesselType"].guiActiveEditor = !shutdown;
            }
        }


        /// <summary>
        /// Show BV control panel
        /// </summary>
        [KSPEvent(guiActive = true, guiName = "Bon Voyage Control Panel", category = "Bon Voyage")]
        public void BVControlPanel()
        {
            BonVoyage.Instance.ToggleControlWindow();
        }

        #endregion


        /// <summary>
        /// Pick target in the map mode
        /// </summary>
        void OnGUI()
        {
            if (BonVoyage.Instance.GamePaused && !BonVoyage.Instance.ShowUI)
                return;

            // Pick target in the map mode
            if (BonVoyage.Instance.MapMode)
            {
                if (!MapView.MapIsEnabled)
                {
                    BonVoyage.Instance.MapMode = false;
                    return;
                }

                double[] latLon = Tools.PlaceTargetAtCursor(vessel.mainBody);
                if (latLon[0] != double.MinValue)
                {
                    GUI.Label(
                        new Rect(Input.mousePosition.x + 20, Screen.height - Input.mousePosition.y, 200, 55),
                        Localizer.Format("#LOC_BV_Control_Lat") + ": " + latLon[0].ToString("F") + "\n" +
                        Localizer.Format("#LOC_BV_Control_Lon") + ": " + latLon[1].ToString("F") + "\n" +
                        Localizer.Format("#LOC_BV_Control_Biome") + ": " + ScienceUtil.GetExperimentBiome(vessel.mainBody, latLon[0], latLon[1])
                    );
                    // On left mouse click send coordinates and exit map mode
                    if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
                    {
                        if (BonVoyage.Instance.ControlModel != null)
                        {
                            BonVoyage.Instance.ControlModel.Latitude = latLon[0].ToString();
                            BonVoyage.Instance.ControlModel.Longitude = latLon[1].ToString();
                        }
                        BonVoyage.Instance.MapMode = false;
                        MapView.ExitMapView();
                    }
                }
            }
        }

    }

}