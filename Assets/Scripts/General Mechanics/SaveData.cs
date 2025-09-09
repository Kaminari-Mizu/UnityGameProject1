using System;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string saveName;
    public string timestamp;  // ISO 8601 UTC string (set by SaveSystem)
    public string sceneName;
    public Vector3 playerPosition;

    //The existing state
    public bool isRunning;
    public bool isSwimming;
    public bool isFloating;
    public bool isPhysicalAttacking;
    public int physicalComboIndex;
    public bool isMagicalAttacking;
    public int magicalComboIndex;
    public float health;
    public float mana;
}
