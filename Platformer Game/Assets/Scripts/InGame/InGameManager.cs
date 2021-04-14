using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameManager : MonoBehaviour
{
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
        if (uid == null) return;
        GameObject player = Instantiate(playerModel);
        Vector3 vec = player.transform.position;
        vec.x = 0f;
        vec.y = -4f;
        vec.z = -1f;
        player.transform.position = vec;
        player.transform.parent = playerGroup.transform;
        player.GetComponent<EntityPlayer>().SetIsMe(TcpManager.instance.GetUserID().Equals(uid));
        InGameDataManager.instance.players.Add(uid, player.GetComponent<EntityPlayer>());
    }

    public void Respawn(string uid) {
        if (uid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(uid, out player)) {
            Vector3 vec = player.transform.position;

            vec.x = 0f;
            vec.y = -4f;

            player.transform.position = vec;
            player.Respawn();
        }
    }

    public void Recarvery(string uid) {
        if (uid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(uid, out player)) {
            player.currentFill = 1f;
        }
    }

    public void Healing(string uid, string scale) {
        if (uid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(uid, out player)) {
            float s = player.currentFill;
            float.TryParse(scale, out s);
            player.currentFill = s;
        }
    }

    public EntityMonster SpawnMonster(string eid) {
        if (eid == null) return null;
        GameObject monster = Instantiate(monsterModel);
        monster.GetComponent<EntityMonster>().SetEntityID(eid);
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("MonsterSpawner");
        GameObject spawner = spawners[Random.Range(0, spawners.Length)];

        Vector3 vec = monster.transform.position;
        vec.x = spawner.transform.position.x;
        vec.y = spawner.transform.position.y;
        vec.z = -1f;
        monster.transform.position = vec;

        monster.transform.parent = monsterGroup.transform;

        InGameDataManager.instance.monsters.Add(eid, monster.GetComponent<EntityMonster>());
        return monster.GetComponent<EntityMonster>();
    }

    public EntityMonster SpawnMonster(string eid, float x, float y, float rY) {
        if (eid == null) return null;
        GameObject monster = Instantiate(monsterModel);
        monster.GetComponent<EntityMonster>().SetEntityID(eid);

        Vector3 vec = monster.transform.position;
        vec.x = x;
        vec.y = y;
        vec.z = -1f;
        monster.transform.position = vec;

        monster.transform.parent = monsterGroup.transform;

        Quaternion rot = monster.transform.rotation;
        rot.y = rY;
        monster.transform.rotation = rot;

        InGameDataManager.instance.monsters.Add(eid, monster.GetComponent<EntityMonster>());
        return monster.GetComponent<EntityMonster>();
    }

    public void SendLocationPacket() {
        if (InGameDataManager.instance.me != null) {
            Transform transform = InGameDataManager.instance.me.transform;
            sendLocationTimer += Time.deltaTime;
            float beforeX = Mathf.Round(position.x * 1000) / 1000;
            float beforeY = Mathf.Round(position.y * 1000) / 1000;
            float nowX = Mathf.Round(transform.position.x * 1000) / 1000;
            float nowY = Mathf.Round(transform.position.y * 1000) / 1000;
            float nowRY = Mathf.Round(transform.rotation.y * 1000) / 1000;
            if (sendLocationTimer >= 0.02f && (position == null || beforeX != nowX || beforeY != nowY || rY != nowRY)) {
                JsonSetting json = new JsonSetting();
                sendLocationTimer = 0f;
                position = transform.position;
                rY = nowRY;
                TcpManager.instance.SocketSend(json.Add("x", nowX).Add("y", nowY).Add("rY", nowRY).Add("type", "Location").ToString());
            }
        }
    }

    public void UpdatePlayerLocation(JsonSetting json) {
        string uid = json.Get("id");
        if (uid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(uid, out player)) {
            Vector3 vec = player.transform.position;
            Quaternion rot = player.transform.rotation;
            float x = 0, y = 0, rY = 0;
            float.TryParse(json.Get("x"), out x);
            float.TryParse(json.Get("y"), out y);
            float.TryParse(json.Get("rY"), out rY);

            vec.x = x;
            vec.y = y;
            rot.y = rY;

            player.transform.position = vec;
            player.transform.rotation = rot;
        }
    }

    public void UpdateMonsterLocation(JsonSetting json) {
        string eid = json.Get("id");
        if (eid == null) return;
        EntityMonster monster;
        float x = 0, y = 0, rY = 0;
        float.TryParse(json.Get("x"), out x);
        float.TryParse(json.Get("y"), out y);
        float.TryParse(json.Get("rY"), out rY);
        if (InGameDataManager.instance.monsters.TryGetValue(eid, out monster)) {
            Vector3 vec = monster.transform.position;
            Quaternion rot = monster.transform.rotation;
            vec.x = x;
            vec.y = y;
            rot.y = rY;
            monster.transform.position = vec;
            monster.transform.rotation = rot;
        }else {
            SpawnMonster(eid, x, y, rY);
        }
    }

    public void EntityDied(string eid) {
        EntityMonster monster;
        if (InGameDataManager.instance.monsters.TryGetValue(eid, out monster)) {
            monster.Die();
            InGameDataManager.instance.monsters.Remove(eid);
        }
    }

    public void PlayerDied(string pid) {
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(pid, out player)) {
            player.Die();
        }
    }

    public void EntityDamaged(string eid) {
        EntityMonster monster;
        if (InGameDataManager.instance.monsters.TryGetValue(eid, out monster)) {
            monster.Damaged();
        }
    }

    public void PlayerDamaged(string pid, string hps) {
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(pid, out player)) {
            float hp = player.currentFill;
            float.TryParse(hps, out hp);
            player.currentFill = hp;
            player.Damaged();
        }
    }

    public void MonsterTarget(string eid) {
        EntityMonster monster;
        if(InGameDataManager.instance.monsters.TryGetValue(eid, out monster)) {
            monster.locationSend = true;
        }else {
            monster = SpawnMonster(eid);
            monster.locationSend = true;
        }
    }

    public void AttackMotionStart(string pid) {
        if (pid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(pid, out player)) {
            player.GetComponent<Animator>().SetBool("isAttack", true);
        }
    }

    public void AttackMotionEnd(string pid) {
        if (pid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(pid, out player)) {
            player.GetComponent<Animator>().SetBool("isAttack", false);
        }
    }

    public void RemovePlayer(string pid) {
        if (pid == null) return;
        EntityPlayer player;
        if (InGameDataManager.instance.players.TryGetValue(pid, out player)) {
            Destroy(player.hpBar.gameObject);
            Destroy(player.gameObject);
        }
    }

    public void GameEnd(JsonSetting json) {
        gameOverImage.gameObject.SetActive(true);
        int stage = 1;
        int.TryParse(json.Get("stage"), out stage);
        int kills = 0, death = 0;
        int.TryParse(json.Get("kills"), out kills);
        int.TryParse(json.Get("death"), out death);
        stageInfo.text = stage + " STAGE";
        otherInfo.text = "죽은 횟수\n"+death+"\n\n죽인 엔티티 수\n"+kills;
    }

    public void GameOverCompleteBtnClickEvent() {
        SceneManager.LoadScene("SampleScene");
    }
}
