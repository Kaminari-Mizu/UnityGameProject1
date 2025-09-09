using System;
using UnityEngine;

[Serializable]
public class SaveMeta
{
    public string fileName;   // actual filename on disk (unique per user)
    public string saveName;   // friendly name (or null)
    public string timestamp;  // ISO timestamp
    public string sceneName;
    public float health;
    public float mana;
}
