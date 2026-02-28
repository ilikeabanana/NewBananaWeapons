using BepInEx.Configuration;
using NewBananaWeapons;
using System.Collections;
using UnityEngine;


public abstract class BaseWeapon : MonoBehaviour
{
    public virtual void SetupConfigs(string sectionName)
    {
        
    }

    public virtual string GetWeaponDescription()
    {
        return string.Empty;
    }
}

public class ConfigVar<T>
{
    public ConfigEntry<T> entry;

    public ConfigVar(string section, string name, T defaultValue, string description = "")
    {
        entry = Banana_WeaponsPlugin.File.Bind<T>(section, name, defaultValue, description);
    }

    public T Value
    {
        get
        {
            return entry.Value;
        }
        set
        {
            entry.Value = value;
        }
    }
}
