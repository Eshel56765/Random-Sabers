using IllusionPlugin;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;

namespace RandomSabers
{
    /// <summary>
    /// This class is in charge of the plugin behavior. 
    /// </summary>
    public class Plugin : IPlugin
    {
        public string Name => PluginName;
        /// <summary>
        /// The Name of the plugin. Returns "Random Sabers Plugin".
        /// </summary>
        public static string PluginName => "Random Sabers Plugin";
        public string Version => PluginVersion;
        /// <summary>
        /// The current version of the plugin. 
        /// </summary>
        public static string PluginVersion => "1.1.6";

        public string MenuSceneName => "MenuCore";

        private static string folderPath = "";

        /// <summary>
        /// The absolute path to the CustomSabers folder (including "CustomSabers/"). 
        /// </summary>
        public static string FolderPath
        {
            get
            {
                if ("" == folderPath)
                {
                    string[] children = Directory.GetDirectories(Directory.GetCurrentDirectory());
                    folderPath = children.First(x => x.EndsWith("CustomSabers"));
                    folderPath += "\\";
                }
                return folderPath;
            }
        }

        private static List<string> allSabers;

        /// <summary>
        /// Returns an array of all the sabers that were in the folder when the game started, including the absolute path. 
        /// </summary>
        public static string[] AllSabers
        {
            get
            {
                if (null == allSabers)
                {
                    allSabers = Directory.GetFiles(FolderPath).ToList();
                }
                return allSabers.ToArray();
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
                if (null == allSaberNames)
                {
                    int folderPathLength = FolderPath.Length;
                    allSaberNames = new List<string>();
                    foreach (string saberPath in AllSabers)
                    {
                        allSaberNames.Add(saberPath.Substring(folderPathLength));
                    }
                }
                return allSaberNames.ToArray();
            }
        }

        /// <summary>
        /// The last selected saber. 
        /// </summary>
        public static string LastSelectedSaber { get; private set; } = "";

        //int saberIndex = 0;
        void IPlugin.OnApplicationStart()
        {
            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            Settings.PlayerPrefsSetup();
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
            Console.WriteLine("Hey thanks for using my mod! If you're enjoying random sabers, or encounter any issues that you don't encounter without this mod, ");
            Console.WriteLine("you are very welcome to tell me at Eshel56765#4962 on Discord!");
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
        }



        private void SceneManagerOnActiveSceneChanged(Scene arg0, Scene arg1)
        {
            //Console.WriteLine("Moved from Scene " + arg0.buildIndex +"(" + arg0.name + ") to Scene " + arg1.buildIndex + "(" + arg1.name + "). ");

            if (Settings.Enabled)
                PrivSelectRandomSaber();
        }

        /// <summary>
        /// This function selects a random saber from <see cref="Settings.EnabledSabers"/> and sets that saber into <see cref="CustomSaber.Plugin._currentSaberPath"/>. 
        /// <para>
        /// This can be called at any time, and will only affect the next time a saber is loaded. This will not change the sabers mid-way through a song, for example. 
        /// </para>
        /// </summary>
        public static void SelectRandomSaber()
        {
            Console.WriteLine("RandomSabers.SelectRandomSaber, called by " + new StackTrace().GetFrame(1).GetMethod().DeclaringType.FullName + ". If the saber selection is giving you trouble, try removing the other mod first. ");
            string[] enabledSabers = Settings.EnabledSabers;
            if (0 == enabledSabers.Length)
            {
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
            CustomSaber.Plugin._currentSaberPath = LastSelectedSaber;
        }

        private static void PrivSelectRandomSaber()
        {
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
            Console.WriteLine("Loading Random Saber #" + SaberIndex + " - " + LastSelectedSaber.Substring(FolderPath.Length));
            CustomSaber.Plugin._currentSaberPath = LastSelectedSaber;
            Console.WriteLine("---------------------------------------------Random Sabers---------------------------------------------");
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            if (arg0.name == MenuSceneName)
            {
                Settings.CreateMenu();
            }
        }
        
        void IPlugin.OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }

        void IPlugin.OnLevelWasLoaded(int level)
        {

        }

        void IPlugin.OnLevelWasInitialized(int level)
        {
        }

        void IPlugin.OnUpdate()
        {
        }

        void IPlugin.OnFixedUpdate()
        {
        }
    }
}
