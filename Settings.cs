using System;
using System.Collections.Generic;
using System.Linq;
using CustomUI.Settings;
using UnityEngine;
using System.IO;


namespace RandomSabers
{
    /// <summary>
    /// This class is in charge of the settings for the plugin. You can use it to check if this plugin is enabled, and get which sabers are currently enabled. 
    /// </summary>
    public static class Settings
    {
        static int SabersPerMenu => 7;

        static Dictionary<string, bool> isSaberEnabled = new Dictionary<string, bool>();

        public static string PrefsModSection => "Random Sabers";
        public static string PrefsModEnabled => "Enabled";
        public static string PrefsModListsEnabled => "ListsEnabled";
        public static string PrefsEnabledSabersSection => "Random Sabers | EnabledSabers";
        private static string SelectedSaberOnStartup;

        /// <summary>
        /// Check whether a saber is enabled. 
        /// </summary>
        /// <param name="SaberPath">The path to the saber. Can also be the name of the saber (including the ".saber" extension). </param>
        /// <returns>True if the saber is enabled, false if it is not, and null if no such saber exists or it was added after the game was launched. </returns>
        public static bool? IsSaberEnabled(string SaberPath)
        {
            if (!SaberPath.Contains("/") || !SaberPath.Contains("\\"))
            {
                SaberPath = Path.Combine(Plugin.FolderPath, SaberPath);
            }
            if (isSaberEnabled.ContainsKey(SaberPath))
                return isSaberEnabled[SaberPath];
            return null;
        }

        /// <summary>
        /// Returns true if the mod is enabled in the in-game settings menu. 
        /// </summary>
        public static bool Enabled
        {
            get
            {
                return PlayerPrefs.GetInt(PrefsModSection + PrefsModEnabled) == 1;
            }
        }

        /// <summary>
        /// Returns an array of the paths of all the sabers that are enabled in the in-game expandable menus. When a new saber is added, it is set to enabled by default. 
        /// </summary>
        public static string[] EnabledSabers
        {
            get
            {
                return Plugin.AllSabers.Where((x) => { bool? en = IsSaberEnabled(x); return (en.HasValue && en.Value); }).ToArray();
            }
        }

        internal static void PlayerPrefsSetup()
        {
            //Console.WriteLine("------------------------------------Random Sabers Dictionary Setup-----------------------------------");
            SelectedSaberOnStartup = CustomSaber.Plugin._currentSaberPath;
            if (!PlayerPrefs.HasKey(PrefsModSection + PrefsModEnabled))
            {
                PlayerPrefs.SetInt(PrefsModSection + PrefsModEnabled, 1);
                //Console.WriteLine("Added " + PrefsModSection + PrefsModEnabled);
            }
            else
            {
                //Console.WriteLine("Has " + PrefsModSection + PrefsModEnabled);
            }

            if (!PlayerPrefs.HasKey(PrefsModSection + PrefsModListsEnabled))
                PlayerPrefs.SetInt(PrefsModSection + PrefsModListsEnabled, 0);


            int folderPathLength = Plugin.FolderPath.Length;
            foreach (string saberPath in Plugin.AllSabers)
            {
                string saberName = saberPath.Substring(folderPathLength);
                if (!PlayerPrefs.HasKey(PrefsEnabledSabersSection + saberName))
                {
                    PlayerPrefs.SetInt(PrefsEnabledSabersSection + saberName, 1);
                    //Console.WriteLine("Added PlayerPrefs: " + PrefsEnabledSabersSection + saberName + " = 1. " );
                }
                else
                {
                    //Console.WriteLine("Has PlayerPrefs: " + PrefsEnabledSabersSection + saberName + " = " + PlayerPrefs.GetInt(PrefsEnabledSabersSection+saberName) + ". " );
                }


                isSaberEnabled.Add(saberPath, PlayerPrefs.GetInt(PrefsEnabledSabersSection+saberName) == 1);

                //Console.WriteLine(saberName + " Enabled: " + isSaberEnabled[saberPath]);
            }
            //Console.WriteLine("------------------------------------Random Sabers Dictionary Setup-----------------------------------");
        }

        private static int setAllValue = 0;

        internal static void CreateMenu()
        {
            SubMenu GeneralMenu = SettingsUI.CreateSubMenu("Random Sabers");
            int folderPathLength = Plugin.FolderPath.Length;
            BoolViewController enabledController = GeneralMenu.AddBool("Enable Random Sabers", "Random sabers on song startup. ");
            enabledController.GetValue += () => { return PlayerPrefs.GetInt(PrefsModSection + PrefsModEnabled) == 1; };
            enabledController.SetValue += (val) => { PlayerPrefs.SetInt(PrefsModSection + PrefsModEnabled, val ? 1 : 0); if (!val) { CustomSaber.Plugin.LoadNewSaber(SelectedSaberOnStartup); } };
            BoolViewController listsEnabledController = GeneralMenu.AddBool("Enable Saber Menus", "Enables secondary menus to add or remove sabers from the selection pool. Settings are saved even if the menus are turned off. ");
            listsEnabledController.GetValue += () => { return PlayerPrefs.GetInt(PrefsModSection + PrefsModListsEnabled) == 1; };
            listsEnabledController.SetValue += (val) => { PlayerPrefs.SetInt(PrefsModSection + PrefsModListsEnabled, val ? 1 : 0); };
            SetAllController SetAllController = GeneralMenu.AddIntSetting<SetAllController>("Set All Sabers To ", "Turn all sabers on, off, or do nothing.");
            SetAllController.SetValues(0, 2, 1);
            SetAllController.GetValue += () => { return 0; };
            SetAllController.SetValue += (val) => {
                setAllValue = val;
                if (1 == val)
                {
                    for (int i = 0; i < Plugin.AllSabers.Length; i++)
                    {
                        PlayerPrefs.SetInt(PrefsEnabledSabersSection + Plugin.AllSaberNames[i], 1);
                        isSaberEnabled[Plugin.AllSabers[i]] = true;
                    }
                }
                if (2 == val)
                {
                    for (int i = 0; i < Plugin.AllSabers.Length; i++)
                    {
                        PlayerPrefs.SetInt(PrefsEnabledSabersSection + Plugin.AllSaberNames[i], 0);
                        isSaberEnabled[Plugin.AllSabers[i]] = false;
                    }
                }
            };
            //Add option lists in groups of SabersPerMenu. 
            if (1 == PlayerPrefs.GetInt(PrefsModSection + PrefsModListsEnabled))
            {
                int sabersCount = Plugin.AllSabers.Length;
                //Console.WriteLine("------------------------------------Random Sabers Menu Setup-----------------------------------");
                int MenuCount = Mathf.CeilToInt((float)sabersCount / SabersPerMenu);
                //Console.WriteLine(sabersCount + " Sabers, " + MenuCount + " Menus. ");
                for (int i = 0; i < MenuCount; i++)
                {
                    int startingSaberIndex = i * SabersPerMenu;
                    string menuName = "Random Sabers (";
                    if (0 != i)
                    {
                        string StartSaberName = Plugin.AllSaberNames[startingSaberIndex];
                        string previousSabername = Plugin.AllSaberNames[startingSaberIndex - 1];
                        string firstLetter = StartSaberName.Substring(0, 1).ToUpper();
                        menuName += firstLetter;
                        if (previousSabername.Substring(0, 1).ToUpper() == firstLetter)
                        {
                            menuName += StartSaberName.Substring(1, 1).ToUpper();
                        }
                    }
                    else
                    {
                        menuName += Plugin.AllSaberNames[startingSaberIndex].Substring(0, 1).ToUpper();
                    }
                    menuName += " - ";
                    menuName += Plugin.AllSaberNames[Mathf.Min(sabersCount - 1, (i + 1) * SabersPerMenu)].Substring(0, 1).ToUpper();
                    menuName += ")";
                    //Console.WriteLine("Added Menu: " + menuName);
                    SubMenu subMenu = SettingsUI.CreateSubMenu(menuName);
                    for (int j = 0; j < SabersPerMenu && startingSaberIndex + j < sabersCount; j++)
                    {
                        string saberName = Plugin.AllSaberNames[startingSaberIndex + j];
                        string SaberPath = Plugin.AllSabers[startingSaberIndex + j];
                        BoolViewController controller = subMenu.AddBool(saberName, "Add or remove " + saberName.Substring(0, saberName.Length - 6) + " from the random sabers options. ");
                        controller.GetValue += () => {
                            if (1 == setAllValue)
                                return true;
                            else if (2 == setAllValue)
                                return false;
                            else return isSaberEnabled[SaberPath]; };
                        controller.SetValue += (val) =>
                        {
                            isSaberEnabled[SaberPath] = val;
                            PlayerPrefs.SetInt(PrefsEnabledSabersSection + saberName, val ? 1 : 0);
                            //controller.ApplySettings();
                            Console.WriteLine("Set " + saberName + " to " + PlayerPrefs.GetInt(PrefsEnabledSabersSection + saberName));
                        };
                    }
                }
                //Console.WriteLine("------------------------------------Random Sabers Menu Setup-----------------------------------");
            }
        }
    }

    public class SetAllController : IntViewController
    {
        protected override string TextForValue(int value)
        {
            switch (value)
            {
                case 1:
                    return "Enable All";
                case 2:
                    return "Disable All";
                default:
                    return "[Do nothing]";
            }
        }
    }
}
