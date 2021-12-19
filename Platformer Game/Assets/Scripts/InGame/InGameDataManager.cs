using System.Collections.Generic;
using UnityEngine;

public class InGameDataManager : MonoBehaviour
{
    public static InGameDataManager Instance { get; private set; }
    public static InGameManager inGameManager;
    public EntityPlayer me;
    public readonly Dictionary<string, EntityPlayer> players = new Dictionary<string, EntityPlayer>();
    public readonly Dictionary<string, EntityMonster> monsters = new Dictionary<string, EntityMonster>();

    private void Awake() {
        Instance = this;
    }
}
