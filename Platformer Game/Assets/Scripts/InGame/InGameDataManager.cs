using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameDataManager : MonoBehaviour
{
    public static InGameDataManager instance;
    public static InGameManager inGameManager;
    public EntityPlayer me;
    public Dictionary<string, EntityPlayer> players = new Dictionary<string, EntityPlayer>();
    public Dictionary<string, EntityMonster> monsters = new Dictionary<string, EntityMonster>();

    private void Awake() {
        instance = this;
    }
}
