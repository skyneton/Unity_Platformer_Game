using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
    private static readonly float LocationSendTerm = 30 / 1000.0f;
    public GameObject playerModel;
    public GameObject monsterModel;
    private float sendLocationTimer = 0f;
    private Vector3 position;
    private float rY = 0f;
    private GameObject playerGroup;
    private GameObject monsterGroup;

    public Image gameOverImage;
    public Text stageInfo;
    public Text otherInfo;
    private static readonly int IsAttack = Animator.StringToHash("isAttack");

    void Start()
    {
        InGameDataManager.inGameManager = this;
        playerGroup = GameObject.Find("EntityPlayerGroup");
        monsterGroup = GameObject.Find("EntityMonsterGroup");
    }

    // Update is called once per frame
    void Update()
    {
        SendLocationPacket();
    }

    public void SpawnPlayer(string uid) {
        var player = Instantiate(playerModel, playerGroup.transform, true).GetComponent<EntityPlayer>();
        player.transform.position = new Vector3(0, -4, -1);
        player.SetIsMe(NetworkManager.Instance.userID == uid);
        InGameDataManager.Instance.players.Add(uid, player);
    }

    public void Respawn(string uid) {
        if (uid == null) return;
        if (!InGameDataManager.Instance.players.TryGetValue(uid, out var player)) return;
        
        var vec = player.transform.position;

        vec.x = 0f;
        vec.y = -4f;

        player.transform.position = vec;
        player.Respawn();
    }

    public void PlayerHealthUpdate(string uid, float scale) {
        if (InGameDataManager.Instance.players.TryGetValue(uid, out var player)) {
            if(player.currentFill > scale)
                player.Damaged();
            player.currentFill = scale;
        }
    }

    public EntityMonster SpawnMonster(Guid eid, float x, float y, int direction = 0) {
        var monster = Instantiate(monsterModel).GetComponent<EntityMonster>();
        monster.entityID = eid;

        monster.transform.position = new Vector3(x, y, -1f);

        monster.transform.parent = monsterGroup.transform;

        var rot = monster.transform.rotation;
        rot.y = direction == -1 ? 0 : 1;
        monster.transform.rotation = rot;

        InGameDataManager.Instance.monsters.Add(eid.ToString(), monster);
        return monster;
    }

    public void SendLocationPacket()
    {
        if (InGameDataManager.Instance.me == null) return;
        var pos = InGameDataManager.Instance.me.transform.position;
        
        sendLocationTimer += Time.deltaTime;
        if (!(sendLocationTimer >= LocationSendTerm) || (!(Vector2.Distance(pos, position) > 0.01f))) return;
        sendLocationTimer = 0f;
        position = pos;

        NetworkManager.Instance.SendPacket(new PacketPlayerLocation(pos.x, pos.y,
            (int) Mathf.Round(InGameDataManager.Instance.me.transform.rotation.y * -2 + 1)));
    }

    public void UpdatePlayerLocation(string uid, float x, float y, int direction)
    {
        if (!InGameDataManager.Instance.players.TryGetValue(uid, out var player)) return;
        var vec = player.transform.position;
        var rot = player.transform.rotation;

        vec.x = x;
        vec.y = y;
        rot.y = direction == -1 ? 1 : 0;

        player.transform.position = vec;
        player.transform.rotation = rot;
    }

    public void UpdateMonsterLocation(string eid, float x, float y, int direction)
    {
        if (!InGameDataManager.Instance.monsters.TryGetValue(eid, out var monster)) return;
        var vec = monster.transform.position;
        var rot = monster.transform.rotation;
        vec.x = x;
        vec.y = y;
        rot.y = direction == -1 ? 0 : 1;
        monster.transform.position = vec;
        monster.transform.rotation = rot;
    }

    public void EntityDied(string eid) {
        if (InGameDataManager.Instance.monsters.TryGetValue(eid, out var monster)) {
            monster.Die();
            InGameDataManager.Instance.monsters.Remove(eid);
        }
    }

    public void PlayerDied(string pid) {
        if (InGameDataManager.Instance.players.TryGetValue(pid, out var player)) {
            player.Die();
        }
    }

    public void EntityDamaged(string eid) {
        if (InGameDataManager.Instance.monsters.TryGetValue(eid, out var monster)) {
            monster.Damaged();
        }
    }

    public void AttackMotionStart(string pid) {
        if (InGameDataManager.Instance.players.TryGetValue(pid, out var player)) {
            player.GetComponent<Animator>().SetBool(IsAttack, true);
        }
    }

    public void AttackMotionEnd(string pid) {
        if (InGameDataManager.Instance.players.TryGetValue(pid, out var player)) {
            player.GetComponent<Animator>().SetBool(IsAttack, false);
        }
    }

    public void RemovePlayer(string pid) {
        if (InGameDataManager.Instance.players.TryGetValue(pid, out var player)) {
            Destroy(player.hpBar.gameObject);
            Destroy(player.gameObject);
        }
    }

    public void GameEnd(int stage, int kills, int deaths) {
        gameOverImage.gameObject.SetActive(true);
        stageInfo.text = stage + " STAGE";
        otherInfo.text = "죽은 횟수\n"+deaths+"\n\n죽인 엔티티 수\n"+kills;
    }

    public void GameOverCompleteBtnClickEvent() {
        SceneManager.LoadScene("MainScene");
    }
}
