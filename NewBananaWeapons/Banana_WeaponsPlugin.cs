using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ULTRAKILL.Portal;
using ULTRAKILL.Portal.Geometry;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;


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
        static List<GameObject> BundleArms = new List<GameObject>();
        static List<GameObject> addedWeapons = new List<GameObject>();
        List<GameObject> BundleWeapons = new List<GameObject>();

        static Dictionary<string, Sprite> armIcons = new Dictionary<string, Sprite>();
        static List<Sprite> armIconsList = new List<Sprite>();

        public static ConfigFile File;

        public static GameObject funnySecret;
        GameObject dttalkPrefab;

        public static Banana_WeaponsPlugin Instance { get; private set; }

        public static Dictionary<GameObject, float> cooldowns = new Dictionary<GameObject, float>();


        public static Dictionary<GameObject, ConfigEntry<bool>> WeaponsEnabled = new Dictionary<GameObject, ConfigEntry<bool>>();
        public Dictionary<GameObject, ConfigEntry<int>> WeaponsSlots = new Dictionary<GameObject, ConfigEntry<int>>();
        private void Awake()
        {
            File = Config;
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
                    foreach (var asset in loadedAssets)
                    {
                        string sectionName = asset.name;

                        WeaponsEnabled.Add(asset, Config.Bind<bool>(sectionName, "Enabled", true));
                        if (asset.GetComponentInChildren<BaseWeapon>())
                        {
                            asset.GetComponentInChildren<BaseWeapon>().SetupConfigs(sectionName);
                        }
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
                        foreach (var asset in loadedAssets)
                        {
                            string sectionName = asset.name;
                            if(asset.TryGetComponent<WeaponIcon>(out WeaponIcon wicn))
                            {
                                sectionName = wicn.weaponDescriptor.weaponName;
                            }
                            string weaponDescrip = "Enables the weapon";
                            
                            WeaponsSlots.Add(asset, Config.Bind<int>(sectionName, "Slot", 6, "1 is minimum, 6 is max"));
                            if (asset.GetComponentInChildren<BaseWeapon>())
                            {
                                weaponDescrip = asset.GetComponentInChildren<BaseWeapon>().GetWeaponDescription();
                                WeaponsEnabled.Add(asset, Config.Bind<bool>(sectionName, "Enabled", true, weaponDescrip));
                                asset.GetComponentInChildren<BaseWeapon>().SetupConfigs(sectionName);
                            }
                            else
                            {
                                WeaponsEnabled.Add(asset, Config.Bind<bool>(sectionName, "Enabled", true, weaponDescrip));
                            }
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

                        var loadedAssets = bundle.LoadAsset<GameObject>("bananaplush");
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

                // Load dttalk bundle
                using (var stream = assembly.GetManifestResourceStream("NewBananaWeapons.Bundles.dttalk"))
                {
                    if (stream == null)
                    {
                        Logger.LogError("Failed to load dttalk bundle stream: Resource not found");
                    }
                    else
                    {
                        var bundle = AssetBundle.LoadFromStream(stream);
                        if (bundle == null)
                        {
                            Logger.LogError("Failed to load dttalk bundle from stream");
                            return;
                        }

                        var loadedAssets = bundle.LoadAllAssets<GameObject>();
                        if (loadedAssets == null || loadedAssets.Length == 0)
                        {
                            Logger.LogError("No GameObjects found in dttalk bundle");
                            bundle.Unload(false);
                            return;
                        }

                        dttalkPrefab = loadedAssets[0];
                        bundle.Unload(false);
                        Logger.LogInfo("Successfully loaded dttalk bundle");
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
            cooldowns.Clear();
            if (AddressableManager.lightningBoltWindup == null)
            {
                AddressableManager.GetAssets();
            }
            if (ShaderManager.shaderDictionary.Count == 0)
            {
                StartCoroutine(ShaderManager.LoadShadersAsync());
            }

            // Instantiate dttalk prefab every scene
            if (dttalkPrefab != null)
            {
                GameObject dttalkInstance = Instantiate(dttalkPrefab);
                StartCoroutine(ShaderManager.ApplyShaderToGameObject(dttalkInstance));

            }
        }

        public static void LaunchPlayer(Vector3 dir, float mult = 8, bool ignoreMass = false)
        {
            NewMovement nm = NewMovement.Instance;

            if (nm.groundProperties && !nm.groundProperties.launchable)
            {
                return;
            }
            if (dir == Vector3.down && nm.gc.onGround)
            {
                return;
            }
            nm.jumping = true;
            nm.Invoke("NotJumping", 0.5f);
            nm.jumpCooldown = true;
            nm.Invoke("JumpReady", 0.2f);
            nm.boost = false;
            if (nm.gc.heavyFall)
            {
                nm.fallSpeed = 0f;
                nm.gc.heavyFall = false;
                if (nm.currentFallParticle != null)
                {
                    UnityEngine.Object.Destroy(nm.currentFallParticle);
                }
            }
            nm.rb.AddForce(Vector3.ClampMagnitude(dir, 1000f) * mult, ignoreMass ? ForceMode.VelocityChange : ForceMode.Impulse);
        }

        public static void ApplyKnockBack(EnemyIdentifier eid, Vector3 force)
        {
            try
            {
                if (eid.zombie)
                {
                    //eid.zombie.falling = true;
                    eid.zombie.KnockBack(force);
                    return;
                }
                if (eid.machine)
                {
                    //eid.machine.falling = true;
                    eid.machine.KnockBack(force);
                    return;
                }

                if (eid.drone)
                {
                    eid.drone.rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);
                    return;
                }


                if (eid.rb)
                    eid.rb.AddForce(force.normalized * (force.magnitude / 100f), ForceMode.Impulse);

            }
            catch(Exception e)
            {
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

                    if(gameObject.TryGetComponent<Collider>(out Collider col))
                    {
                        Destroy(col);
                    }

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
                if (ULTRAKILL.Cheats.NoWeaponCooldown.NoCooldown)
                {
                    cooldowns.Clear();
                }
                else
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


            }

            
            

            if (Input.GetKeyDown(KeyCode.Insert))
            {
                GameObject portalGun = AddMockGun(PrimitiveType.Cube);
                portalGun.AddComponent<PortalGun>();

                MakeGun(5, portalGun);

                Destroy(portalGun);
            }

            if (Input.GetKeyDown(KeyCode.End))
            {
                GameObject mockArm = new GameObject("mockArm");
                mockArm.SetActive(false);

                mockArm.AddComponent<LoaderArm>();

                AddArm(mockArm);
            }

            if (BundleArms == null || BundleArms.Count == 0) return;
            foreach (GameObject obj in BundleArms)
            {
                if (obj != null && !addedArms.Contains(obj))
                {
                    AddArm(obj);
                }
            }
            if (BundleWeapons == null || BundleWeapons.Count == 0) return;
            foreach (GameObject obj in BundleWeapons)
            {
                if (obj != null && !addedWeapons.Contains(obj) && WeaponsEnabled[obj].Value)
                {
                    addedWeapons.Add(obj);
                    StartCoroutine(ShaderManager.ApplyShaderToGameObject(MakeGun(WeaponsSlots[obj].Value - 1, obj)));

                }
            }
        }


        

        GameObject AddMockGun(PrimitiveType primitive)
        {
            GameObject mockGun = GameObject.CreatePrimitive(primitive);
            Destroy(mockGun.GetComponent<Collider>());
            mockGun.transform.position = new Vector3(0.7f, -0.8f, 0.8f);
            mockGun.SetActive(false);
            mockGun.AddComponent<WeaponIcon>();
            mockGun.AddComponent<WeaponIdentifier>();
            mockGun.AddComponent<WeaponPos>();
            mockGun.AddComponent<AudioSource>();
            Destroy(mockGun.GetComponent<Collider>());
            return mockGun;
        }
        public void AddArm(GameObject arm)
        {
            if (arm == null) return;
            if (WeaponsEnabled.ContainsKey(arm))
            {
                if (!WeaponsEnabled[arm].Value) return;
            }
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

                List<Sprite> useableSprites = new List<Sprite>();
                for (int i = 0; i < Banana_WeaponsPlugin.armIconsList.Count; i++)
                {
                    GameObject armEquipped = Banana_WeaponsPlugin.BundleArms[i];
                    if (WeaponsEnabled.ContainsKey(armEquipped))
                    {
                        if (WeaponsEnabled[armEquipped].Value) useableSprites.Add(Banana_WeaponsPlugin.armIconsList[i]);
                    }
                }

                __instance.fistFill.sprite = Banana_WeaponsPlugin.armIconsList[current - __instance.fistIcons.Length];
                __instance.fistBackground.sprite = Banana_WeaponsPlugin.armIconsList[current - __instance.fistIcons.Length];

                MonoSingleton<FistControl>.Instance.fistIconColor = MonoSingleton<ColorBlindSettings>.Instance.variationColors[0];

                return false;
            }
        }

        [HarmonyPatch(typeof(NewMovement), nameof(NewMovement.Respawn))]
        public static class NewMovement_ResetCooldowns_Patch
        {
            public static void Postfix()
            {
                Banana_WeaponsPlugin.cooldowns.Clear();
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

        // Style Things
        [HarmonyPatch(typeof(StyleCalculator), nameof(StyleCalculator.HitCalculator))]
        public class CustomStyle
        {
            public static void Prefix(StyleCalculator __instance, string hitter, string enemyType, string hitLimb, bool dead, EnemyIdentifier eid = null, GameObject sourceWeapon = null)
            {
                if (eid != null && eid.blessed)
                {
                    return;
                }
                if (MonoSingleton<PlayerTracker>.Instance.playerType == PlayerType.Platformer)
                {
                    return;
                }

                if (hitter == "Metal")
                {
                    __instance.AddPoints(10, "", eid, sourceWeapon);
                    if (dead)
                    {
                        __instance.AddPoints(150, "BLUNT FORCE", eid, sourceWeapon);
                    }
                }

                if (hitter == "pipe")
                {
                    __instance.AddPoints(24, "", eid, sourceWeapon);
                    if (dead)
                    {
                        __instance.AddPoints(125, "PIPE BASH", eid, sourceWeapon);
                    }
                }
                if (hitter == "repipe")
                {
                    __instance.AddPoints(36, "", eid, sourceWeapon);
                    if (dead)
                    {
                        __instance.AddPoints(100, "RE-PIPE", eid, sourceWeapon);
                    }
                }

                if (hitter == "car")
                {
                    if (dead)
                    {
                        __instance.AddPoints(130, "RUN-OVER", eid, sourceWeapon);
                    }
                    else
                    {
                        __instance.AddPoints(45, "CAR HIT", eid, sourceWeapon);
                    }
                }
                if (hitter == "table")
                {
                    __instance.AddPoints(76, "TABLED", eid, sourceWeapon);
                }

                if(hitter == "riskofrain2loaderreference")
                {
                    __instance.AddPoints(13, "", eid, sourceWeapon);
                    if (dead)
                    {
                        __instance.AddPoints(135, "CHARGED", eid, sourceWeapon);
                    }
                }

                if(hitter == "beam")
                {
                    __instance.AddPoints(3, "", eid, sourceWeapon);
                    if (dead)
                    {
                        __instance.AddPoints(175, "FRIED", eid, sourceWeapon);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Punch), nameof(Punch.TryParryProjectile))]
        public static class PunchPipe
        {
            [HarmonyPrefix]
            public static void Prefix(Punch __instance, Transform target, bool canProjectileBoost = false)
            {
                Banana_WeaponsPlugin.Log.LogInfo(target.gameObject.name + " is being checked");
                if (target.gameObject.TryGetComponent<PipeProjectile>(out PipeProjectile pipe))
                {
                    if (pipe.goingBackToPlayer == false) return;
                    pipe.goingBackToPlayer = false;
                    pipe.timesParried++;
                    pipe.transform.forward = CameraController.Instance.transform.forward;
                    pipe.Calculate();
                    pipe.timerWhereItHasToReturn = 5;
                    pipe.StopAllCoroutines();
                    MonoSingleton<TimeController>.Instance.ParryFlash();
                    __instance.anim.Play("Hook", 0, 0.065f);
                }

                if (target.gameObject.TryGetComponent<TableProjectile>(out TableProjectile table))
                {
                    if (table.parried) return;
                    table.parried = true;
                    table.damage *= 2;
                    table.GetComponent<Rigidbody>().velocity = CameraController.Instance.transform.forward * 100;
                    MonoSingleton<TimeController>.Instance.ParryFlash();
                    __instance.anim.Play("Hook", 0, 0.065f);
                }
            }
        }

        [HarmonyPatch(typeof(GunSetter), nameof(GunSetter.ResetWeapons))]
        public static class GunSetter_ResetWeapons_Patch
        {
            public static void Postfix()
            {
                addedWeapons.Clear();
            }
        }

        [HarmonyPatch(typeof(PlayerActivator), nameof(PlayerActivator.Activate))]
        public class Notification
        {
            static bool alrDone = false;
            public static void Postfix()
            {
                if (alrDone) return;
                alrDone = true;
                HudMessageReceiver.Instance.SendHudMessage("You can change weapon configs and see descriptions of weapons in the configgy menu", delay: 1);
            }
        }
    }
}