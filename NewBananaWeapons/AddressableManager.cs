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
        public static void GetAssets()
        {
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset("Assets/Particles/Environment/LightningBoltWindupFollow Variant.prefab",
                (result) =>
                {
                    lightningBoltWindup = result;
                }));
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset("Assets/Prefabs/Attacks and Projectiles/Explosions/Lightning Strike Explosive.prefab",
                (result) =>
                {
                    lightningBolt = result;
                }));
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Railcannon Beam Malicious.prefab",
                (result) =>
                {
                    mauriceBeam = result;
                }));
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset("Assets/Prefabs/Attacks and Projectiles/Hitscan Beams/Revolver Beam.prefab",
                (result) =>
                {
                    normalBeam = result;
                }));
            Banana_WeaponsPlugin.Instance.StartCoroutine(
                loadAsset("Assets/Prefabs/Sandbox/Manipulated Object Particles.prefab",
                (result) =>
                {
                    manipulationEffect = result;
                }));
        }

        static IEnumerator loadAsset(string path, Action<GameObject> onDone)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(path);
            yield return new WaitUntil(() => handle.IsDone);
            onDone.Invoke(handle.Result);
        }
    }
}
