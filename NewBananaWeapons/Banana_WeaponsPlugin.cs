using UnityEngine;
using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using System.Linq;


namespace NewBananaWeapons
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class Banana_WeaponsPlugin : BaseUnityPlugin
    {
        private const string MyGUID = "com.banana.Banana_Weapons";
        private const string PluginName = "NewBananaWeapons";
        private const string VersionString = "1.0.0";
        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);
        static List<GameObject> addedArms = new List<GameObject>();
        List<GameObject> BundleArms = new List<GameObject>();
        static List<GameObject> addedWeapons = new List<GameObject>();
        List<GameObject> BundleWeapons = new List<GameObject>();

        static Dictionary<string, Sprite> armIcons = new Dictionary<string, Sprite>();
        static List<Sprite> armIconsList = new List<Sprite>();

        GameObject funnySecret;

        public static Banana_WeaponsPlugin Instance { get; private set; }

        public static Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();
        private void Awake()
        {
            Instance = this;
            gameObject.hideFlags = HideFlags.DontSaveInEditor;
            
            try
            {
                Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");

                // Apply Harmony patches
                Harmony.PatchAll();
                Log = Logger;

                // Load assets from bundle
                var assembly = Assembly.GetExecutingAssembly();
                using (var stream = assembly.GetManifestResourceStream("NewBananaWeapons.Bundles.arms"))
                {
                    if (stream == null)
                    {
                        Logger.LogError("Failed to load asset bundle stream: Resource not found");
                        return;
                    }

                    var bundle = AssetBundle.LoadFromStream(stream);
                    if (bundle == null)
                    {
                        Logger.LogError("Failed to load asset bundle from stream");
                        return;
                    }

                    var loadedAssets = bundle.LoadAllAssets<GameObject>();
                    if (loadedAssets == null || loadedAssets.Length == 0)
                    {
                        Logger.LogError("No GameObjects found in asset bundle");
                        bundle.Unload(false);
                        return;
                    }

                    BundleArms.AddRange(loadedAssets);
                    bundle.Unload(false);
                }
                Logger.LogInfo($"Successfully loaded {BundleArms.Count} arms from bundle");


                foreach (string resourceName in assembly.GetManifestResourceNames())
                {
                    if (!resourceName.StartsWith("NewBananaWeapons.ArmIcons"))
                        continue;

                    using (var stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null)
                            continue;

                        byte[] data = new byte[stream.Length];
                        stream.Read(data, 0, data.Length);

                        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                        tex.LoadImage(data);

                        Sprite sprite = Sprite.Create(
                            tex,
                            new Rect(0, 0, tex.width, tex.height),
                            new Vector2(0.5f, 0.5f)
                        );

                        const string iconPrefix = "NewBananaWeapons.ArmIcons.";

                        string fileName = resourceName.Substring(iconPrefix.Length);


                        armIcons[fileName] = sprite;
                        armIconsList.Add(sprite);
                    }
                }
                Logger.LogInfo($"Loaded {armIcons.Count} arm icons");
                using (var stream = assembly.GetManifestResourceStream("NewBananaWeapons.Bundles.weapons"))
                {
                    if (stream == null)
                    {
                        Logger.LogError("Failed to load asset bundle stream: Resource not found");

                    }
                    else
                    {
                        var bundle = AssetBundle.LoadFromStream(stream);
                        if (bundle == null)
                        {
                            Logger.LogError("Failed to load asset bundle from stream");
                            return;
                        }

                        var loadedAssets = bundle.LoadAllAssets<GameObject>();
                        if (loadedAssets == null || loadedAssets.Length == 0)
                        {
                            Logger.LogError("No GameObjects found in asset bundle");
                            bundle.Unload(false);
                            return;
                        }

                        BundleWeapons.AddRange(loadedAssets);
                        bundle.Unload(false);
                    }

                    
                }

                using (var stream = assembly.GetManifestResourceStream("NewBananaWeapons.Bundles.funnysecret"))
                {
                    if (stream == null)
                    {
                        Logger.LogError("Failed to load asset bundle stream: Resource not found");

                    }
                    else
                    {
                        var bundle = AssetBundle.LoadFromStream(stream);
                        if (bundle == null)
                        {
                            Logger.LogError("Failed to load asset bundle from stream");
                            return;
                        }

                        var loadedAssets = bundle.LoadAsset<GameObject>("FunnySecret");
                        if (loadedAssets == null)
                        {
                            Logger.LogError("No GameObjects found in asset bundle");
                            bundle.Unload(false);
                            return;
                        }

                        funnySecret = loadedAssets;
                        bundle.Unload(false);
                    }


                }

                Logger.LogInfo($"Successfully loaded {BundleWeapons.Count} weapons from bundle");
                Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error during plugin initialization: {ex.Message}\n{ex.StackTrace}");
            }
            var assembly2 = Assembly.GetExecutingAssembly();
            AddressableManager.GetAssets();
            //testCar = AssetBundle.LoadFromStream(assembly2.GetManifestResourceStream("Banana_Weapons.Bundles.car")).LoadAllAssets()[0] as GameObject;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }



        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            addedArms.Clear();
            addedWeapons.Clear();
            if(AddressableManager.lightningBoltWindup == null)
            {
                AddressableManager.GetAssets();
            }
            if(ShaderManager.shaderDictionary.Count == 0)
            {
                StartCoroutine(ShaderManager.LoadShadersAsync());
            }
        }

        public static GameObject MakeGun(int var, GameObject original)
        {
            int num = var;
            // Making sure it isnt null to prevent errors
            bool flag = MonoSingleton<GunControl>.Instance == null || MonoSingleton<StyleHUD>.Instance == null;
            bool flag2 = flag;
            // defining result
            GameObject result;
            if (flag2)
            {
                result = null;
            }
            else
            {
                // Checking everything so we dont get any errors
                bool flag3 = !MonoSingleton<GunControl>.Instance.enabled || !MonoSingleton<StyleHUD>.Instance.enabled;
                bool flag4 = flag3;
                if (flag4)
                {
                    result = null;
                }
                else
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(original);
                    bool flag5 = gameObject == null;
                    bool flag6 = flag5;
                    if (flag6)
                    {
                        result = null;
                    }
                    else
                    {
                        Vector3 pos = gameObject.transform.position;
                        Quaternion rot = gameObject.transform.rotation;
                        // Assigning the transforms
                        gameObject.transform.parent = MonoSingleton<GunControl>.Instance.transform;
                        gameObject.transform.localPosition = pos;
                        gameObject.transform.localRotation = rot;
                        // Adding it to the slots
                        MonoSingleton<GunControl>.Instance.slots[num].Add(gameObject);
                        MonoSingleton<GunControl>.Instance.allWeapons.Add(gameObject);
                        MonoSingleton<GunControl>.Instance.slotDict.Add(gameObject, num);
                        MonoSingleton<StyleHUD>.Instance.weaponFreshness.Add(gameObject, 10f);
                        // Setting the object inactive as default
                        gameObject.SetActive(false);
                        // Setting noweapons to false and doing yesweapons
                        MonoSingleton<GunControl>.Instance.noWeapons = false;
                        MonoSingleton<GunControl>.Instance.YesWeapon();
                        // Setting every child inactive
                        for (int k = 0; k < MonoSingleton<GunControl>.Instance.transform.childCount; k++)
                        {
                            MonoSingleton<GunControl>.Instance.transform.GetChild(k).gameObject.SetActive(false);
                        }
                        result = gameObject;
                    }
                }
            }
            return result;
        }
        void Update()
        {

            if (cooldowns.Count > 0)
            {
                // Copy keys to avoid modifying collection while iterating
                var keys = new List<GameObject>(cooldowns.Keys);

                foreach (var key in keys)
                {
                    cooldowns[key] -= Time.deltaTime;

                    // Optional: remove finished cooldowns
                    if (cooldowns[key] <= 0f)
                        cooldowns.Remove(key);
                }
            }


            if (funnySecret != null)
            {
                if (Input.GetKeyDown(KeyCode.End))
                {

                    GameObject funny = Instantiate(funnySecret, NewMovement.instance.transform.position, Quaternion.identity);

                    StartCoroutine(ShaderManager.ApplyShaderToGameObject(funny));
                }
            }

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                GameObject mockGun = new GameObject("mockgun");
                mockGun.SetActive(false);
                mockGun.AddComponent<WeaponIcon>();
                mockGun.AddComponent<WeaponIdentifier>();
                mockGun.AddComponent<WeaponPos>();

                mockGun.AddComponent<KeyboardWeapon>();

                MakeGun(5, mockGun);
            }


            if (BundleArms == null || BundleArms.Count == 0) return;
            foreach (GameObject obj in BundleArms)
            {
                if (obj != null && !addedArms.Contains(obj))
                {
                    AddArm(obj, 3);
                }
            }
            if (BundleWeapons == null || BundleWeapons.Count == 0) return;
            foreach (GameObject obj in BundleWeapons)
            {
                if (obj != null && !addedWeapons.Contains(obj))
                {
                    addedWeapons.Add(obj);
                    StartCoroutine(ShaderManager.ApplyShaderToGameObject(MakeGun(5, obj)));
                    
                }
            }
        }

        public void AddArm(GameObject arm, int slot)
        {
            if (arm == null) return;

            addedArms.Add(arm);
            var fistControl = MonoSingleton<FistControl>.Instance;
            if (fistControl != null)
            {
                fistControl.ResetFists();
            }
        }

        [HarmonyPatch(typeof(HudController), nameof(HudController.UpdateFistIcon))]
        public static class Display_Correct_Icon
        {
            public static bool Prefix(HudController __instance, int current)
            {
                Log.LogInfo("Arm icon number: " + current);
                if (__instance.fistIcons.Length >= current + 1) return true;
                __instance.fistFill.sprite = Banana_WeaponsPlugin.armIconsList[current - __instance.fistIcons.Length];
                __instance.fistBackground.sprite = Banana_WeaponsPlugin.armIconsList[current - __instance.fistIcons.Length];

                MonoSingleton<FistControl>.Instance.fistIconColor = MonoSingleton<ColorBlindSettings>.Instance.variationColors[0];

                return false;
            }
        }

        [HarmonyPatch(typeof(FistControl), nameof(FistControl.ResetFists))]
        public static class FistControl_ResetFists_Patch
        {
            public static void Postfix(FistControl __instance)
            {
                if (__instance == null || addedArms == null) return;

                for (int i = 0; i < addedArms.Count; i++)
                {
                    if (addedArms[i] != null)
                    {
                        Banana_WeaponsPlugin.Log.LogInfo("Adding... " + addedArms[i].name);
                        GameObject item = UnityEngine.Object.Instantiate(addedArms[i], __instance.transform);
                        __instance.StartCoroutine(ShaderManager.ApplyShaderToGameObject(item));
                        __instance.spawnedArms.Add(item);
                        __instance.spawnedArmNums.Add(i + 3);
                        /*
                        string iconKey = addedArms[i].name + ".png";


                        if (armIcons.TryGetValue(iconKey, out Sprite icon))
                        {
                            foreach (var hudcontroller in FindObjectsByType<HudController>(FindObjectsSortMode.None))
                            {
                                Sprite[] oldIcons = hudcontroller.fistIcons;
                                Sprite[] newIcons = new Sprite[oldIcons.Length + 1];

                                for (int j = 0; j < oldIcons.Length; j++)
                                {
                                    newIcons[j] = oldIcons[j];
                                }

                                // add your icon at the end
                                newIcons[newIcons.Length - 1] = icon;

                                hudcontroller.fistIcons = newIcons;
                            }
                        }*/


                        Banana_WeaponsPlugin.Log.LogInfo("Added... " + addedArms[i].name + "!");

                    }
                }
            }
        }
    }
}
