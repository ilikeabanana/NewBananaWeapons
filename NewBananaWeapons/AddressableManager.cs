using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace NewBananaWeapons
{
    public class AddressableManager
    {
        public static GameObject lightningBoltWindup;
        public static GameObject lightningBolt;
        public static GameObject mauriceBeam;
        public static GameObject normalBeam;
        public static GameObject manipulationEffect;
        public static GameObject explosion;
        public static GameObject blueFlash;
        public static GameObject rageEffect;
        public static void GetAssets()
        {
            loadAddressable("Assets/Particles/Environment/LightningBoltWindupFollow Variant.prefab",
                (result) =>
                {
                    lightningBoltWindup = result;
                });
            loadAddressable("Assets/Prefabs/Attacks and Projectiles/Explosions/Lightning Strike Explosive.prefab",
                (result) =>
                {
                    lightningBolt = result;
                });
            loadAddressable("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Railcannon Beam Malicious.prefab",
                (result) =>
                {
                    mauriceBeam = result;
                });
            loadAddressable("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Revolver Beam.prefab",
                (result) =>
                {
                    normalBeam = result;
                });
            loadAddressable("Assets/Prefabs/Sandbox/Manipulated Object Particles.prefab",
                (result) =>
                {
                    manipulationEffect = result;
                });
            loadAddressable("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion Big.prefab",
                (result) =>
                {
                    explosion = result;
                });
            loadAddressable("Assets/Particles/Flashes/V2FlashUnparriable.prefab",
                (result) =>
                {
                    blueFlash = result;
                });
            loadAddressable("Assets/Particles/Enemies/RageEffect.prefab",
                (result) =>
                {
                    rageEffect = result;
                });
        }

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
        }
    }
}
