using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NewBananaWeapons
{
    public class AddressableManager
    {
        // Prefabs
        public static GameObject lightningBoltWindup;
        public static GameObject lightningBolt;
        public static GameObject mauriceBeam;
        public static GameObject normalBeam;
        public static GameObject manipulationEffect;
        public static GameObject explosion;
        public static GameObject bigExplosion;
        public static GameObject blueFlash;
        public static GameObject rageEffect;
        public static GameObject rubbleBig;
        public static GameObject blackholekaboom;

        // Sounds
        public static AudioClip negativeNotifi;
        public static AudioClip blackholeLaunch;

        // Material shit
        public static Material lineMat;
        public static Shader unlit;
        public async static void GetAssets()
        {
            lightningBoltWindup = await LoadAddressable<GameObject>("Assets/Particles/Environment/LightningBoltWindupFollow Variant.prefab");
            lightningBolt = await LoadAddressable<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Lightning Strike Explosive.prefab");
            mauriceBeam = await LoadAddressable<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Railcannon Beam Malicious.prefab");
            normalBeam = await LoadAddressable<GameObject>("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Revolver Beam.prefab");
            manipulationEffect = await LoadAddressable<GameObject>("Assets/Prefabs/Sandbox/Manipulated Object Particles.prefab");
            bigExplosion = await LoadAddressable<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Big.prefab");
            explosion = await LoadAddressable<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
            blueFlash = await LoadAddressable<GameObject>("Assets/Particles/Flashes/V2FlashUnparriable.prefab");
            rageEffect = await LoadAddressable<GameObject>("Assets/Particles/Enemies/RageEffect.prefab");
            rubbleBig = await LoadAddressable<GameObject>("Assets/Particles/RubbleBigDistant.prefab");
            blackholekaboom = await LoadAddressable<GameObject>("Assets/Particles/BlackHoleExplosion.prefab");

            lineMat = await LoadAddressable<Material>("Assets/Materials/Sprites/SpitLine.mat");
            unlit = await LoadAddressable<Shader>("Assets/Shaders/Main/ULTRAKILL-unlit.shader");

            negativeNotifi = await LoadAddressable<AudioClip>("Assets/Sounds/UI/Negative_Notification_25.wav");
            blackholeLaunch = await LoadAddressable<AudioClip>("Assets/Sounds/Weapons/BlackHoleLaunch.wav");
        }

        public static async Task<T> LoadAddressable<T>(string path)
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(path);
            await handle.Task;
            return handle.Result;
        }
        /*
        static void loadAddressable(string path, Action<GameObject> onDone)
        {
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset(path,
                onDone));
        }

        static IEnumerator loadAsset(string path, Action<GameObject> onDone)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(path);
            yield return new WaitUntil(() => handle.IsDone);
            onDone.Invoke(handle.Result);
        }*/
    }
}
