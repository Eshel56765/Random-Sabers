#define RandomSabers
using IPA;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using SemVer;

namespace RandomSabers
{
    /// <summary>
    /// This class is in charge of the plugin behavior. 
    /// <para>
    /// To check if this plugin is present in other plugins, use the #IF RandomSabers #endif block. 
    /// </para>
    /// </summary>
    public class Plugin : IBeatSaberPlugin
    {
        /// <summary>
        /// The Name of the plugin. Returns "Random Sabers Plugin".
        /// </summary>
        public static string PluginName => "Random Sabers Plugin";
        /// <summary>
        /// The current version of the plugin. 
        /// </summary>
        public static SemVer.Version PluginVersion => new SemVer.Version(1, 2, 0);
        /// <summary>
        /// The name of the in-game menu scene. 
        /// </summary>
        public const string MenuSceneName = "MenuCore";
        /// <summary>
        /// The name of the in-game play scene. 
        /// </summary>
        public const string GameSceneName = "GameCore";

        private static string folderPath;

        /// <summary>
        /// The absolute path to the CustomSabers folder (including "CustomSabers/"). 
        /// </summary>
        public static string SabersFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    string[] children = Directory.GetDirectories(Directory.GetCurrentDirectory());
                    folderPath = children.First(x => x.EndsWith("CustomSabers"));
                    folderPath += "\\";
                }
                return folderPath;
            }
        }

        private static List<string> allSaberNames;

        /// <summary>
        /// Returns an array of all the names of the saber files, without the preceding path to the game folder. 
        /// </summary>
        public static string[] AllSaberNames
        {
            get
            {
                InitAllSaberNames();
                return allSaberNames.ToArray();
            }
        }

        /// <summary>
        /// The amount of sabers present. 
        /// </summary>
        public static int SaberCount
        {
            get
            {
                InitAllSaberNames();
                return allSaberNames.Count;
            }
        }
        private static void InitAllSaberNames()
        {
            if (null == allSaberNames)
            {
                int folderPathLength = SabersFolderPath.Length;
                allSaberNames = new List<string>();
                foreach (string saberPath in Directory.EnumerateFiles(SabersFolderPath))
                {
                    allSaberNames.Add(Path.GetFileNameWithoutExtension(saberPath));
                }
            }
        }

        internal static string GetSaberName(int Index)
        {
            InitAllSaberNames();
            return allSaberNames[Index];
        }

        /// <summary>
        /// The last selected saber. 
        /// </summary>
        public static string LastSelectedSaber { get; private set; } = "";

        //int saberIndex = 0;
        void IBeatSaberPlugin.OnApplicationStart()
        {
            Settings.PlayerPrefsSetup();
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
            Console.WriteLine("Hey thanks for using my mod! If you're enjoying random sabers, or encounter any issues that you don't encounter without this mod, ");
            Console.WriteLine("you are very welcome to tell me at Eshel56765#4962 on Discord!");
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
        }



        void IBeatSaberPlugin.OnActiveSceneChanged(Scene prevScene, Scene newScene)
        {
            /*
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
            Console.WriteLine("Moved from Scene " + prevScene.buildIndex + "(" + prevScene.name + ") to Scene " + newScene.buildIndex + "(" + newScene.name + "). ");
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
            */
            if (newScene.name == MenuSceneName && Settings.Enabled)
            {
                PrivSelectRandomSaber(true);
            }
            if (newScene.name == GameSceneName && Settings.Enabled && Settings.DisplaySelectedSaber)
            {
                SaberNameText.DisplayText(CustomSaber.Plugin._currentSaberName);
            }
        }

        /// <summary>
        /// This function selects a random saber from <see cref="Settings.EnabledSabers"/> and sets that saber into <see cref="CustomSaber.Plugin._currentSaberName"/>. 
        /// <para>
        /// This can be called at any time, and will only affect the next time a saber is loaded. This will not change the sabers mid-way through a song, for example. 
        /// </para>
        /// </summary>
        public static void SelectRandomSaber()
        {
            Console.WriteLine("RandomSabers.SelectRandomSaber(), called by " + new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName + ". If the saber selection is giving you trouble, try removing the other mod first. ");
            PrivSelectRandomSaber(false);
        }

        private static void PrivSelectRandomSaber(bool internalCall)
        {
            if (internalCall)
                Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");

            string[] enabledSabers = Settings.EnabledSabers;
            Console.WriteLine("Amount of Enabled Sabers: " + enabledSabers.Length);
            if (0 == enabledSabers.Length)
            {
                Console.WriteLine("All sabers disabled. ");
                return;
            }
            DateTime now = DateTime.Now;
            int newSeed = now.Millisecond + now.Second * 1000 + now.Minute * 60000 + now.Hour * 3600000 + now.Day * 86400000;
            UnityEngine.Random.InitState(newSeed);
            int SaberIndex;
            do
            {
                SaberIndex = UnityEngine.Random.Range(0, enabledSabers.Length);
            } while (enabledSabers[SaberIndex] == LastSelectedSaber && enabledSabers.Length < 2);
            LastSelectedSaber = enabledSabers[SaberIndex];
            Console.WriteLine("Loading Random Saber #" + SaberIndex + ':' + LastSelectedSaber);
            CustomSaber.Plugin._currentSaberName = LastSelectedSaber;
            if (internalCall)
                Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
        }

        void IBeatSaberPlugin.OnApplicationQuit()
        {
        }

        void IBeatSaberPlugin.OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == MenuSceneName)
            {
                Settings.CreateMenu();
            }
        }

        void IBeatSaberPlugin.OnUpdate()
        {
        }

        void IBeatSaberPlugin.OnFixedUpdate()
        {
        }

        void IBeatSaberPlugin.OnSceneUnloaded(Scene scene)
        {
        }
    }
}
// 