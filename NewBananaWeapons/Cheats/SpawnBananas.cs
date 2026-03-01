using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewBananaWeapons.Cheats
{
    public class SpawnBananas : ICheat
    {
        public string LongName => "SPAWN BANANAS";
        public string Identifier => "ultrakill.spawn.bananas";
        public string ButtonEnabledOverride => "Spawn";
        public string ButtonDisabledOverride => "Spawn";
        public string Icon => "death";
        public bool DefaultState => false;
        public StatePersistenceMode PersistenceMode => StatePersistenceMode.NotPersistent;
        public bool IsActive => false;

        public void Disable() { }

        public void Enable(CheatsManager manager)
        {
            for (int i = 0; i < 100; i++)
            {
                Vector3 pos = NewMovement.Instance.transform.position;
                GameObject funny = GameObject.Instantiate(Banana_WeaponsPlugin.funnySecret, pos, Quaternion.identity);
                manager.StartCoroutine(ShaderManager.ApplyShaderToGameObject(funny));
            }
        }
        public IEnumerator Coroutine(CheatsManager manager)
        {
            Enable(manager);
            yield break;
        }
    }

    [HarmonyPatch(typeof(CheatsManager), "Start")]
    public class Patch
    {
        public static void Prefix(CheatsManager __instance)
        {
            __instance.RegisterExternalCheat(new SpawnBananas());
        }
    }
}