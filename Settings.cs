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

        private const string PrefsModSection = "Random Sabers";
        private const string PrefsModEnabled = "Enabled";
        private const string PrefsDisplaySelectedSaberEnabled = "DisplaySelectedSaber";
        private const string PrefsModListsEnabled = "ListsEnabled";
        private const string PrefsEnabledSabersSection = "Random Sabers | EnabledSabers";
        private static string SelectedSaberOnStartup;

        /// <summary>
        /// Check whether a saber is enabled. 
        /// </summary>
        /// <param name="SaberName">The name of the saber (including the ".saber" extension). </param>
        /// <returns>True if the saber is enabled, false if it is not, and null if no such saber exists or it was added after the game was launched. </returns>
        public static bool? IsSaberEnabled(string SaberName)
        {
            if (isSaberEnabled.ContainsKey(SaberName))
                return isSaberEnabled[SaberName];
            return null;
        }

        /// <summary>
        /// Returns true if the mod is enabled in the in-game settings menu. 
        /// </summary>
        public static bool Enabled =>
            PlayerPrefs.GetInt(PrefsModSection + PrefsModEnabled) == 1;

        /// <summary>
        /// Whether the Display Selected Saber option is set to On in the in-fame settings menu. 
        /// </summary>
        public static bool DisplaySelectedSaber =>
            PlayerPrefs.GetInt(PrefsModSection + PrefsDisplaySelectedSaberEnabled) == 1;

        /// <summary>
        /// Returns an array of the paths of all the sabers that are enabled in the in-game expandable menus. When a new saber is added, it is set to enabled by default. 
        /// </summary>
        public static string[] EnabledSabers
        {
            get
            {
                return Plugin.AllSaberNames.Where((x) => { bool? en = IsSaberEnabled(x); return (en.HasValue && en.Value); }).ToArray();
            }
        }

        internal static void PlayerPrefsSetup()
        {
            //Console.WriteLine("------------------------------------Random Sabers Dictionary Setup-----------------------------------");
            SelectedSaberOnStartup = CustomSaber.Plugin._currentSaberName;
            if (!PlayerPrefs.HasKey(PrefsModSection + PrefsModEnabled))
            {
                PlayerPrefs.SetInt(PrefsModSection + PrefsModEnabled, 1);
            }
            if (!PlayerPrefs.HasKey(PrefsModSection + PrefsDisplaySelectedSaberEnabled))
            {
                PlayerPrefs.SetInt(PrefsModSection + PrefsDisplaySelectedSaberEnabled, 1);
            }

            if (!PlayerPrefs.HasKey(PrefsModSection + PrefsModListsEnabled))
                PlayerPrefs.SetInt(PrefsModSection + PrefsModListsEnabled, 0);


            foreach (string saberName in Plugin.AllSaberNames)
            {
                if (!PlayerPrefs.HasKey(PrefsEnabledSabersSection + saberName))
                {
                    PlayerPrefs.SetInt(PrefsEnabledSabersSection + saberName, 1);
                    //Console.WriteLine("Added PlayerPrefs: " + PrefsEnabledSabersSection + saberName + " = 1. " );
                }
                else
                {
                    //Console.WriteLine("Has PlayerPrefs: " + PrefsEnabledSabersSection + saberName + " = " + PlayerPrefs.GetInt(PrefsEnabledSabersSection+saberName) + ". " );
                }


                isSaberEnabled.Add(saberName, PlayerPrefs.GetInt(PrefsEnabledSabersSection + saberName) == 1);

                //Console.WriteLine(saberName + " Enabled: " + isSaberEnabled[saberPath]);
            }
            //Console.WriteLine("------------------------------------Random Sabers Dictionary Setup-----------------------------------");
        }

        private static int setAllValue = 0;

        internal static void CreateMenu()
        {
            SubMenu GeneralMenu = SettingsUI.CreateSubMenu("Random Sabers");
            int folderPathLength = Plugin.SabersFolderPath.Length;
            BoolViewController enabledController = GeneralMenu.AddBool("Enable Random Sabers", "Random sabers on song startup. ");
            enabledController.GetValue += () => { return PlayerPrefs.GetInt(PrefsModSection + PrefsModEnabled) == 1; };
            enabledController.SetValue += (val) => { PlayerPrefs.SetInt(PrefsModSection + PrefsModEnabled, val ? 1 : 0); if (!val) { CustomSaber.Plugin.LoadNewSaber(SelectedSaberOnStartup); } };
            BoolViewController displaySelectedSaberController = GeneralMenu.AddBool("Display Selected Saber", "Displays the selected Saber's name when starting the song.");
            displaySelectedSaberController.GetValue += () => { return PlayerPrefs.GetInt(PrefsModSection + PrefsDisplaySelectedSaberEnabled) == 1; };
            displaySelectedSaberController.SetValue += (val) => { PlayerPrefs.SetInt(PrefsModSection + PrefsDisplaySelectedSaberEnabled, val ? 1 : 0); };
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
                    for (int i = 0; i < Plugin.SaberCount; i++)
                    {
                        PlayerPrefs.SetInt(PrefsEnabledSabersSection + Plugin.GetSaberName(i), 1);
                        isSaberEnabled[Plugin.GetSaberName(i)] = true;
                    }
                }
                else if (2 == val)
                {
                    for (int i = 0; i < Plugin.SaberCount; i++)
                    {
                        PlayerPrefs.SetInt(PrefsEnabledSabersSection + Plugin.GetSaberName(i), 0);
                        isSaberEnabled[Plugin.GetSaberName(i)] = false;
                    }
                }
            };
            //Add option lists in groups of SabersPerMenu. 
            if (1 == PlayerPrefs.GetInt(PrefsModSection + PrefsModListsEnabled))
            {
                int sabersCount = Plugin.SaberCount;
                //Console.WriteLine("------------------------------------Random Sabers Menu Setup-----------------------------------");
                int MenuCount = Mathf.CeilToInt((float)sabersCount / SabersPerMenu);
                //Console.WriteLine(sabersCount + " Sabers, " + MenuCount + " Menus. ");
                for (int i = 0; i < MenuCount; i++)
                {
                    int startingSaberIndex = i * SabersPerMenu;
                    string menuName = "Random Sabers (";
                    if (0 != i)
                    {
                        string StartSaberName = Plugin.GetSaberName(startingSaberIndex);
                        string previousSabername = Plugin.GetSaberName(startingSaberIndex - 1);
                        string firstLetter = StartSaberName.Substring(0, 1).ToUpper();
                        menuName += firstLetter;
                        if (previousSabername.Substring(0, 1).ToUpper() == firstLetter)
                        {
                            menuName += StartSaberName.Substring(1, 1).ToUpper();
                        }
                    }
                    else
                    {
                        menuName += Plugin.GetSaberName(startingSaberIndex).Substring(0, 1).ToUpper();
                    }
                    menuName += " - ";
                    menuName += Plugin.GetSaberName(Mathf.Min(sabersCount - 1, (i + 1) * SabersPerMenu)).Substring(0, 1).ToUpper();
                    menuName += ")";
                    //Console.WriteLine("Added Menu: " + menuName);
                    SubMenu subMenu = SettingsUI.CreateSubMenu(menuName);
                    for (int j = 0; j < SabersPerMenu && startingSaberIndex + j < sabersCount; j++)
                    {
                        string saberName = Plugin.GetSaberName(startingSaberIndex + j);
                        BoolViewController controller = subMenu.AddBool(saberName, "Add or remove " + saberName + " from the random sabers options. ");
                        controller.GetValue += () => {
                            if (1 == setAllValue)
                                return true;
                            else if (2 == setAllValue)
                                return false;
                            else return isSaberEnabled[saberName];
                        };
                        controller.SetValue += (val) =>
                        {
                            isSaberEnabled[saberName] = val;
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

    /// <summary>
    /// The Settings View Controller for the SetAllSabers option. 
    /// </summary>
    public class SetAllController : IntViewController
    {
        /// <summary>
        /// Returns the correct text for each value option. 
        /// </summary>
        /// <param name="value">The current value.</param>
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
