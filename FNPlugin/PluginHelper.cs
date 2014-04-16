using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace FNPlugin {
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class PluginHelper : MonoBehaviour {
        public const double FIXED_SAT_ALTITUDE = 13599840256;

        public static Dictionary<string, float> DEFAULT_SCIENCE = new Dictionary<string, float>();
        public static Dictionary<string, float> DEFAULT_SCIENCE_LANDED = new Dictionary<string, float>();
        public static Dictionary<string, float> DEFAULT_IMPACTOR = new Dictionary<string, float>();
        public static Dictionary<string, float> DEFAULT_MAGNETIC_SCALING = new Dictionary<string, float>();

        public const int REF_BODY_KERBOL = 0;
        public const int REF_BODY_KERBIN = 1;
        public const int REF_BODY_MUN = 2;
        public const int REF_BODY_MINMUS = 3;
        public const int REF_BODY_MOHO = 4;
        public const int REF_BODY_EVE = 5;
        public const int REF_BODY_DUNA = 6;
        public const int REF_BODY_IKE = 7;
        public const int REF_BODY_JOOL = 8;
        public const int REF_BODY_LAYTHE = 9;
        public const int REF_BODY_VALL = 10;
        public const int REF_BODY_BOP = 11;
        public const int REF_BODY_TYLO = 12;
        public const int REF_BODY_GILLY = 13;
        public const int REF_BODY_POL = 14;
        public const int REF_BODY_DRES = 15;
        public const int REF_BODY_EELOO = 16;

        public static string hydrogen_resource_name = "LiquidFuel";
        public static string oxygen_resource_name = "Oxidizer";
        public static string aluminium_resource_name = "Aluminium";
        public static string methane_resource_name = "LqdMethane";
        public static string argon_resource_name = "Argon";
        public static string water_resource_name = "LqdWater";
        public static string hydrogen_peroxide_resource_name = "H2Peroxide";
        public static string ammonia_resource_name = "Ammonia";
        public static bool using_toolbar = false;

        public const int interstellar_major_version = 10;
        public const int interstellar_minor_version = 0;
        
		protected static bool plugin_init = false;
		protected static bool is_thermal_dissip_disabled_init = false;
		protected static bool is_thermal_dissip_disabled = false;
        protected static GameDatabase gdb;
        protected static bool resources_configured = false;
        protected static bool tech_checked = false;
        protected static TechUpdateWindow tech_window = null;
        protected static int installed_tech_tree_version_id = 0;
        protected static int new_tech_tree_version_id = 0;

        private void SetupDefaultScience()
        {
            Debug.Log("[KSP Interstellar] Setting up default science multipliers");
            DEFAULT_SCIENCE.Add("Sun", 15f);
            DEFAULT_SCIENCE.Add("Moho", 20f);
            DEFAULT_SCIENCE.Add("Eve", 5f);
            DEFAULT_SCIENCE.Add("Gilly", 5f);
            DEFAULT_SCIENCE.Add("Kerbin", 1f);
            DEFAULT_SCIENCE.Add("Mun", 2.5f);
            DEFAULT_SCIENCE.Add("Minmus", 2.5f);
            DEFAULT_SCIENCE.Add("Duna", 5f);
            DEFAULT_SCIENCE.Add("Ike", 5f);
            DEFAULT_SCIENCE.Add("Dres", 7.5f);
            DEFAULT_SCIENCE.Add("Jool", 10f);
            DEFAULT_SCIENCE.Add("Laythe", 12f);
            DEFAULT_SCIENCE.Add("Vall", 12f);
            DEFAULT_SCIENCE.Add("Bop", 10f);
            DEFAULT_SCIENCE.Add("Tylo", 10f);
            DEFAULT_SCIENCE.Add("Pol", 10f);
            DEFAULT_SCIENCE.Add("Eeloo", 20f);

            DEFAULT_SCIENCE_LANDED.Add("Eve", 12.5f);
            DEFAULT_SCIENCE_LANDED.Add("Tylo", 30f);

            DEFAULT_IMPACTOR.Add("Moho", 14f);
            DEFAULT_IMPACTOR.Add("Eve", 7f);
            DEFAULT_IMPACTOR.Add("Gilly", 7f);
            DEFAULT_IMPACTOR.Add("Kerbin", 0.5f);
            DEFAULT_IMPACTOR.Add("Mun", 5f);
            DEFAULT_IMPACTOR.Add("Minmus", 5f);
            DEFAULT_IMPACTOR.Add("Duna", 7f);
            DEFAULT_IMPACTOR.Add("Ike", 7f);
            DEFAULT_IMPACTOR.Add("Dres", 8f);
            DEFAULT_IMPACTOR.Add("Jool", 9f);
            DEFAULT_IMPACTOR.Add("Laythe", 11f);
            DEFAULT_IMPACTOR.Add("Vall", 11f);
            DEFAULT_IMPACTOR.Add("Bop", 9f);
            DEFAULT_IMPACTOR.Add("Tylo", 9f);
            DEFAULT_IMPACTOR.Add("Pol", 9f);
            DEFAULT_IMPACTOR.Add("Eeloo", 14f);

            DEFAULT_MAGNETIC_SCALING.Add("Tylo", 7f);
            DEFAULT_MAGNETIC_SCALING.Add("Laythe", 5f);
            DEFAULT_MAGNETIC_SCALING.Add("Moho", 2f);
            DEFAULT_MAGNETIC_SCALING.Add("Jool", 3f);
            DEFAULT_MAGNETIC_SCALING.Add("Eve", 2f);
            DEFAULT_MAGNETIC_SCALING.Add("Mun", 0.2f);
            DEFAULT_MAGNETIC_SCALING.Add("Ike", 0.2f);
            DEFAULT_MAGNETIC_SCALING.Add("Gilly", 0.05f);
            DEFAULT_MAGNETIC_SCALING.Add("Bop", 0.05f);
            DEFAULT_MAGNETIC_SCALING.Add("Pol", 0.05f);
        }
        
        
        public static string getPluginSaveFilePath() {
            return KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/WarpPlugin.cfg";
        }

        public static string getTechTreeFilePath() {
            return KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/tree.cfg";
        }

        public static string getNewTechTreeFilePath() {
            return KSPUtil.ApplicationRootPath + "GameData/WarpPlugin/tree.cfg";
        }

        public static string getPluginSettingsFilePath() {
            return KSPUtil.ApplicationRootPath + "GameData/WarpPlugin/WarpPluginSettings.cfg";
        }

		public static bool isThermalDissipationDisabled() {
			return is_thermal_dissip_disabled;
		}

		public static bool hasTech(string techid) {
			try{
				string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
				ConfigNode config = ConfigNode.Load (persistentfile);
				ConfigNode gameconf = config.GetNode ("GAME");
				ConfigNode[] scenarios = gameconf.GetNodes ("SCENARIO");
				foreach (ConfigNode scenario in scenarios) {
					if (scenario.GetValue ("name") == "ResearchAndDevelopment") {
						ConfigNode[] techs = scenario.GetNodes ("Tech");
						foreach (ConfigNode technode in techs) {
							if (technode.GetValue ("id") == techid) {
								return true;
							}
						}
					}
				}
				return false;
			} catch (Exception ex) {
				return false;
			}
		}

		public static float getKerbalRadiationDose(int kerbalidx) {
			try{
				string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
				ConfigNode config = ConfigNode.Load (persistentfile);
				ConfigNode gameconf = config.GetNode ("GAME");
				ConfigNode crew_roster = gameconf.GetNode("ROSTER");
				ConfigNode[] crew = crew_roster.GetNodes("CREW");
				ConfigNode sought_kerbal = crew[kerbalidx];
				if(sought_kerbal.HasValue("totalDose")) {
					float dose = float.Parse(sought_kerbal.GetValue("totalDose"));
					return dose;
				}
				return 0.0f;
			}catch (Exception ex) {
				print (ex);
				return 0.0f;
			}
		}

		public static ConfigNode getKerbal(int kerbalidx) {
			try{
				string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
				ConfigNode config = ConfigNode.Load (persistentfile);
				ConfigNode gameconf = config.GetNode ("GAME");
				ConfigNode crew_roster = gameconf.GetNode("ROSTER");
				ConfigNode[] crew = crew_roster.GetNodes("CREW");
				ConfigNode sought_kerbal = crew[kerbalidx];
				return sought_kerbal;
			}catch (Exception ex) {
				print (ex);
				return null;
			}
		}

		public static void saveKerbalRadiationdose(int kerbalidx, float rad) {
			try{
				string persistentfile = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/persistent.sfs";
				ConfigNode config = ConfigNode.Load (persistentfile);
				ConfigNode gameconf = config.GetNode ("GAME");
				ConfigNode crew_roster = gameconf.GetNode("ROSTER");
				ConfigNode[] crew = crew_roster.GetNodes("CREW");
				ConfigNode sought_kerbal = crew[kerbalidx];
				if(sought_kerbal.HasValue("totalDose")) {
					sought_kerbal.SetValue("totalDose",rad.ToString("E"));
				}else{
					sought_kerbal.AddValue("totalDose",rad.ToString("E"));
				}
				config.Save(persistentfile);
			}catch (Exception ex) {
				print (ex);
			}
		}

		public static ConfigNode getPluginSaveFile() {
			ConfigNode config = ConfigNode.Load (PluginHelper.getPluginSaveFilePath ());
			if (config == null) {
				config = new ConfigNode ();
				config.AddValue("writtenat",DateTime.Now.ToString());
				config.Save(PluginHelper.getPluginSaveFilePath ());
			}
			return config;
		}

        public static ConfigNode getPluginSettingsFile() {
            ConfigNode config = ConfigNode.Load(PluginHelper.getPluginSettingsFilePath());
            if (config == null) {
                config = new ConfigNode();
            }
            return config;
        }

        public static ConfigNode getTechTreeFile() {
            ConfigNode config = ConfigNode.Load(PluginHelper.getTechTreeFilePath());
            return config;
        }

        public static ConfigNode getNewTechTreeFile() {
            ConfigNode config = ConfigNode.Load(PluginHelper.getNewTechTreeFilePath());
            return config;
        }

        public static bool lineOfSightToSun(Vessel vess) {
            Vector3d a = vess.transform.position;
            Vector3d b = FlightGlobals.Bodies[0].transform.position;
            foreach (CelestialBody referenceBody in FlightGlobals.Bodies) {
                if (referenceBody.flightGlobalsIndex == 0) { // the sun should not block line of sight to the sun
                    continue;
                }
                Vector3d refminusa = referenceBody.position - a;
                Vector3d bminusa = b - a;
                if (Vector3d.Dot(refminusa, bminusa) > 0) {
                    if (Vector3d.Dot(refminusa, bminusa.normalized) < bminusa.magnitude) {
                        Vector3d tang = refminusa - Vector3d.Dot(refminusa, bminusa.normalized) * bminusa.normalized;
                        if (tang.magnitude < referenceBody.Radius) {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

		public static float getMaxAtmosphericAltitude(CelestialBody body) {
			if (!body.atmosphere) {
				return 0;
			}
			return (float) -body.atmosphereScaleHeight * 1000.0f * Mathf.Log(1e-6f);
		}

        public static string getPropertyOverrideForBody(string name, string type)
        {
            ConfigNode scienceOverrides = getPluginSettingsFile().GetNode("WARP_PLUGIN_PLANET_OVERRIDES");
            if (scienceOverrides != null)
            {
                ConfigNode scienceOverride = scienceOverrides.GetNode(name);
                if (scienceOverride != null && scienceOverride.HasValue(type))
                    return scienceOverride.GetValue(type);
            }
            return null;
        }

        public static float getScienceMultiplier(string name, bool landed) {
            float multiplier = 0f;
            if (DEFAULT_SCIENCE.ContainsKey(name)) multiplier = DEFAULT_SCIENCE[name];

            string scienceOverride = getPropertyOverrideForBody(name, "scienceBase");
            if (scienceOverride != null) multiplier = float.Parse(scienceOverride);

			if (landed) {
                scienceOverride = getPropertyOverrideForBody(name, "scienceLanded");
                if (scienceOverride != null) {
                    multiplier = float.Parse(scienceOverride);
                } else if (DEFAULT_SCIENCE_LANDED.ContainsKey(name)) {
					multiplier = DEFAULT_SCIENCE_LANDED[name];
				} else {
					multiplier = multiplier*2f;
				}
			}

            return multiplier;
        }

        public static float getImpactorScienceMultiplier(string name)
        {
            float multiplier = 0f;
            if (DEFAULT_IMPACTOR.ContainsKey(name)) multiplier = DEFAULT_IMPACTOR[name];

            string scienceOverride = getPropertyOverrideForBody(name, "scienceImpactor");
            if (scienceOverride != null) multiplier = float.Parse(scienceOverride);

            return multiplier;
        }

        public static double getSpecialMagneticFieldScaling(string name)
        {
            float scaling = DEFAULT_MAGNETIC_SCALING.ContainsKey(name) ? DEFAULT_MAGNETIC_SCALING[name] : 1.0f;
            string scalingOverride = getPropertyOverrideForBody(name, "magneticScaling");
            if (scalingOverride != null) scaling = float.Parse(scalingOverride);
            return scaling;
        }

        public void Start() {
            SetupDefaultScience();

            tech_window = new TechUpdateWindow();
            tech_checked = false;

            if (!tech_checked) {
                ConfigNode tech_nodes = PluginHelper.getTechTreeFile();
                ConfigNode new_tech_nodes = PluginHelper.getNewTechTreeFile();

                if (tech_nodes != null) {
                    if (tech_nodes.HasNode("VERSION")) {
                        ConfigNode version_node = tech_nodes.GetNode("VERSION");
                        if (version_node.HasValue("id")) {
                            installed_tech_tree_version_id = Convert.ToInt32(version_node.GetValue("id"));
                        }
                    }
                }
                if (new_tech_nodes != null) {
                    if (new_tech_nodes.HasNode("VERSION")) {
                        ConfigNode version_node2 = new_tech_nodes.GetNode("VERSION");
                        if (version_node2.HasValue("id")) {
                            new_tech_tree_version_id = Convert.ToInt32(version_node2.GetValue("id"));
                        }
                    }
                }
                if (new_tech_tree_version_id > installed_tech_tree_version_id) {
                    tech_window.Show();
                }

                tech_checked = true;
            }
        }

		public void Update() {
            this.enabled = true;
            AvailablePart intakePart = PartLoader.getPartInfoByName("CircularIntake");
            if (intakePart != null) {
                if (intakePart.partPrefab.FindModulesImplementing<AtmosphericIntake>().Count <= 0 && PartLoader.Instance.IsReady()) {
                    plugin_init = false;
                }
            }

            if (!resources_configured) {
                ConfigNode plugin_settings = GameDatabase.Instance.GetConfigNode("WarpPlugin/WarpPluginSettings/WarpPluginSettings");
                if (plugin_settings != null) {
                    if (plugin_settings.HasValue("HydrogenResourceName")) {
                        PluginHelper.hydrogen_resource_name = plugin_settings.GetValue("HydrogenResourceName");
                        Debug.Log("[KSP Interstellar] Hydrogen resource name set to " + PluginHelper.hydrogen_resource_name);
                    }
                    if (plugin_settings.HasValue("OxygenResourceName")) {
                        PluginHelper.oxygen_resource_name = plugin_settings.GetValue("OxygenResourceName");
                        Debug.Log("[KSP Interstellar] Oxygen resource name set to " + PluginHelper.oxygen_resource_name);
                    }
                    if (plugin_settings.HasValue("AluminiumResourceName")) {
                        PluginHelper.aluminium_resource_name = plugin_settings.GetValue("AluminiumResourceName");
                        Debug.Log("[KSP Interstellar] Aluminium resource name set to " + PluginHelper.aluminium_resource_name);
                    }
                    if (plugin_settings.HasValue("MethaneResourceName")) {
                        PluginHelper.methane_resource_name = plugin_settings.GetValue("MethaneResourceName");
                        Debug.Log("[KSP Interstellar] Methane resource name set to " + PluginHelper.methane_resource_name);
                    }
                    if (plugin_settings.HasValue("ArgonResourceName")) {
                        PluginHelper.argon_resource_name = plugin_settings.GetValue("ArgonResourceName");
                        Debug.Log("[KSP Interstellar] Argon resource name set to " + PluginHelper.argon_resource_name);
                    }
                    if (plugin_settings.HasValue("WaterResourceName")) {
                        PluginHelper.water_resource_name = plugin_settings.GetValue("WaterResourceName");
                        Debug.Log("[KSP Interstellar] Water resource name set to " + PluginHelper.water_resource_name);
                    }
                    if (plugin_settings.HasValue("HydrogenPeroxideResourceName")) {
                        PluginHelper.hydrogen_peroxide_resource_name = plugin_settings.GetValue("HydrogenPeroxideResourceName");
                        Debug.Log("[KSP Interstellar] Hydrogen Peroxide resource name set to " + PluginHelper.hydrogen_peroxide_resource_name);
                    }
                    if (plugin_settings.HasValue("AmmoniaResourceName")) {
                        PluginHelper.ammonia_resource_name = plugin_settings.GetValue("AmmoniaResourceName");
                        Debug.Log("[KSP Interstellar] Ammonia resource name set to " + PluginHelper.ammonia_resource_name);
                    }
                    if (plugin_settings.HasValue("ThermalMechanicsDisabled")) {
                        PluginHelper.is_thermal_dissip_disabled = bool.Parse(plugin_settings.GetValue("ThermalMechanicsDisabled"));
                        Debug.Log("[KSP Interstellar] ThermalMechanics set to enabled: " + !PluginHelper.is_thermal_dissip_disabled);
                    }
                    resources_configured = true;
                } else {
                    showInstallationErrorMessage();
                }
                
            }

            

			if (!plugin_init) {
                gdb = GameDatabase.Instance;
				plugin_init = true;

                AvailablePart kerbalRadiationPart = PartLoader.getPartInfoByName("kerbalEVA");
                if (kerbalRadiationPart.partPrefab.Modules != null) {
                    if (kerbalRadiationPart.partPrefab.FindModulesImplementing<FNModuleRadiation>().Count == 0) {
                        kerbalRadiationPart.partPrefab.gameObject.AddComponent<FNModuleRadiation>();
                    }
                } else {
                    kerbalRadiationPart.partPrefab.gameObject.AddComponent<FNModuleRadiation>();
                }

				List<AvailablePart> available_parts = PartLoader.LoadedPartsList;
				foreach (AvailablePart available_part in available_parts) {
					Part prefab_available_part = available_part.partPrefab;
					try {
						if(prefab_available_part.Modules != null) {
														
							if(prefab_available_part.FindModulesImplementing<ModuleResourceIntake>().Count > 0) {
								ModuleResourceIntake intake = prefab_available_part.Modules["ModuleResourceIntake"] as ModuleResourceIntake;
								if(intake.resourceName == "IntakeAir") {
									Type type = AssemblyLoader.GetClassByName(typeof(PartModule), "AtmosphericIntake");
									AtmosphericIntake pm = null;
									if(type != null) {
										pm = prefab_available_part.gameObject.AddComponent(type) as AtmosphericIntake;
										prefab_available_part.Modules.Add(pm);
										pm.area = intake.area*intake.unitScalar*intake.maxIntakeSpeed/20;
									}

									PartResource intake_air_resource = prefab_available_part.Resources["IntakeAir"];

                                    if (intake_air_resource != null && !prefab_available_part.Resources.Contains("IntakeAtm")) {
										ConfigNode node = new ConfigNode("RESOURCE");
										node.AddValue("name", "IntakeAtm");
										node.AddValue("maxAmount", intake_air_resource.maxAmount);
										node.AddValue("amount", intake_air_resource.amount);
										prefab_available_part.AddResource(node);
									}
								}

							}

                            if (prefab_available_part.FindModulesImplementing<ModuleDeployableSolarPanel>().Count > 0) {
                                ModuleDeployableSolarPanel panel = prefab_available_part.Modules["ModuleDeployableSolarPanel"] as ModuleDeployableSolarPanel;
                                if (panel.chargeRate > 0) {
                                    Type type = AssemblyLoader.GetClassByName(typeof(PartModule), "FNSolarPanelWasteHeatModule");
                                    FNSolarPanelWasteHeatModule pm = null;
                                    if (type != null) {
                                        pm = prefab_available_part.gameObject.AddComponent(type) as FNSolarPanelWasteHeatModule;
                                        prefab_available_part.Modules.Add(pm);
                                    }
                                }
                                
                                                                
                                if (!prefab_available_part.Resources.Contains("WasteHeat") && panel.chargeRate > 0) {
                                    ConfigNode node = new ConfigNode("RESOURCE");
                                    node.AddValue("name", "WasteHeat");
                                    node.AddValue("maxAmount", panel.chargeRate * 100);
                                    node.AddValue("amount", 0);
                                    PartResource pr = prefab_available_part.AddResource(node);

                                    if (available_part.resourceInfo != null && pr != null) {
                                        if (available_part.resourceInfo.Length == 0) {
                                            available_part.resourceInfo = pr.resourceName + ":" + pr.amount + " / " + pr.maxAmount;
                                        } else {
                                            available_part.resourceInfo = available_part.resourceInfo + "\n" + pr.resourceName + ":" + pr.amount + " / " + pr.maxAmount;
                                        }
                                    }
                                }

                            }

							if(prefab_available_part.FindModulesImplementing<ElectricEngineController>().Count() > 0) {
								available_part.moduleInfo = prefab_available_part.FindModulesImplementing<ElectricEngineController>().First().GetInfo();
                                available_part.moduleInfos.RemoveAll(modi => modi.moduleName == "Engine");
                                AvailablePart.ModuleInfo mod_info = available_part.moduleInfos.Where(modi => modi.moduleName == "Electric Engine Controller").First();
                                mod_info.moduleName = "Electric Engine";
							}

							if(prefab_available_part.FindModulesImplementing<FNNozzleController>().Count() > 0) {
								available_part.moduleInfo = prefab_available_part.FindModulesImplementing<FNNozzleController>().First().GetInfo();
                                available_part.moduleInfos.RemoveAll(modi => modi.moduleName == "Engine");
                                AvailablePart.ModuleInfo mod_info = available_part.moduleInfos.Where(modi => modi.moduleName == "FNNozzle Controller").First();
                                mod_info.moduleName = "Thermal Nozzle";
							}
                            
							if(prefab_available_part.CrewCapacity > 0 || prefab_available_part.FindModulesImplementing<ModuleCommand>().Count > 0) {
								Type type = AssemblyLoader.GetClassByName(typeof(PartModule), "FNModuleRadiation");
								FNModuleRadiation pm = null;
								if(type != null) {
									pm = prefab_available_part.gameObject.AddComponent(type) as FNModuleRadiation;
									prefab_available_part.Modules.Add(pm);
									double rad_hardness = prefab_available_part.mass /(Math.Max(prefab_available_part.CrewCapacity,0.1))*7.5;
									pm.rad_hardness = rad_hardness;
                                    AvailablePart.ModuleInfo minfo = new AvailablePart.ModuleInfo();
                                    minfo.moduleName = "Radiation Status";
                                    minfo.info = pm.GetInfo();
                                    available_part.moduleInfos.Add(minfo);
								}
                                print("Adding ModuleRadiation to " + prefab_available_part.name);
							}
						}
					}catch(Exception ex) {
                        if (prefab_available_part != null) {
                            print("[KSP Interstellar] Exception caught adding to: " + prefab_available_part.name + " part: " + ex.ToString());
                        } else {
                            print("[KSP Interstellar] Exception caught adding to unknown module");
                        }
					}


				}
			}

			//Destroy (this);
		}
        		
		protected static bool warning_displayed = false;

		public static void showInstallationErrorMessage() {
			if (!warning_displayed) {
				PopupDialog.SpawnPopupDialog ("KSP Interstellar Installation Error", "KSP Interstellar is unable to detect files required for proper functioning.  Please make sure that this mod has been installed to [Base KSP directory]/GameData/WarpPlugin.", "OK", false, HighLogic.Skin);
				warning_displayed = true;
			}
		}
        
    }
}
