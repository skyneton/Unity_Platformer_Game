using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class TcpManager : MonoBehaviour {
    private static string SERVER_IP = "127.0.0.1";
    private static int SERVER_PORT = 3000;
    public static TcpManager instance = null;

    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;
    private bool socketReady = false;

    private string USER_ID;
    private Queue<string> packets = new Queue<string>();
    private bool isPlaying = false;

    private void Start() {
        if (instance == null) {
            instance = this;
            Init();
        }
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Init()
    {
        try {
            socket = new TcpClient(SERVER_IP, SERVER_PORT);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);
            socket.NoDelay = true;
            socketReady = true;
        } catch {
            WaitingSceneDataManager.instance.errorMessage.text = "서버 연결에 문제가 발생하였습니다. 인터넷이 연결되어 있는지 확인해주세요.";
        }
    }

    private void Update() {
        if (socketReady) {
            while (stream.DataAvailable) {
                string data = reader.ReadLine();
                if (data != null) DistinguishPacketType(data);
            }
        }
        if(!socket.Connected) {
            if (WaitingSceneDataManager.instance != null) {
                WaitingSceneDataManager.instance.errorMessage.text = "서버 연결에 문제가 발생하였습니다. 인터넷이 연결되어 있는지 확인해주세요.";
            }else {
                SceneManager.LoadScene("SampleScene");
            }
        }
    }

    public bool IsConnected() {
        return socket.Connected;
    }

    public void DistinguishPacketType(string packet) {
        switch (packet) {
            case "JoinRoomPlayer": {
                WaitingSceneDataManager.instance.gameStayManager.AddPlayer();
                break;
            }
            case "GameStartTimerCancel": {
                WaitingSceneDataManager.instance.gameStayManager.GameStartTimerCancel();
                break;
            }
            case "GameStart": {
                if (!isPlaying) {
                    isPlaying = true;
                    WaitingSceneDataManager.instance.gameStayManager.GameStart();
                    SocketSend("InGameSceneLoaded");
                }
                break;
            }
            default: {
                DistinguishJsonType(new JsonSetting().loadJsonString(packet));
                break;
            }
        }
    }

    public void DistinguishJsonType(JsonSetting json) {
        if (json.Get("type") == null) return;
        switch (json.Get("type")) {
            case "RoomInfo": {
                int users = 1;
                int.TryParse(json.Get("PlayerNumbers"), out users);
                WaitingSceneDataManager.instance.gameStayManager.DrawStayImage(users);
                USER_ID = json.Get("id");
                isPlaying = false;
                break;
            }
            case "LeaveRoomPlayer": {
                if (!isPlaying)
                    WaitingSceneDataManager.instance.gameStayManager.RemovePlayer();
                else
                    InGameDataManager.inGameManager.RemovePlayer(json.Get("id"));
                break;
            }
            case "GameStartTimer": {
                int timer = 1;
                int.TryParse(json.Get("timer"), out timer);
                WaitingSceneDataManager.instance.gameStayManager.GameStartTimer(timer);
                break;
            }
            case "EntityPlayerSpawn": {
                if (isPlaying)
                    InGameDataManager.inGameManager.SpawnPlayer(json.Get("id"));
                break;
            }
            case "Location": {
                if (isPlaying)
                    InGameDataManager.inGameManager.UpdatePlayerLocation(json);
                break;
                }
            case "EntityDied": {
                if (isPlaying)
                    InGameDataManager.inGameManager.EntityDied(json.Get("id"));
                break;
            }
            case "PlayerDied": {
                if (isPlaying)
                    InGameDataManager.inGameManager.PlayerDied(json.Get("id"));
                break;
            }
            case "EntityDamaged": {
                if (isPlaying)
                    InGameDataManager.inGameManager.EntityDamaged(json.Get("id"));
                break;
            }
            case "PlayerDamaged": {
                if (isPlaying)
                    InGameDataManager.inGameManager.PlayerDamaged(json.Get("id"), json.Get("hp"));
                break;
            }
            case "MonsterTarget": {
                if (isPlaying)
                    InGameDataManager.inGameManager.MonsterTarget(json.Get("id"));
                break;
            }
            case "MonsterLocation": {
                if (isPlaying)
                    InGameDataManager.inGameManager.UpdateMonsterLocation(json);
                break;
                }
            case "AttackMotionStart": {
                if (isPlaying)
                    InGameDataManager.inGameManager.AttackMotionStart(json.Get("id"));
                break;
            }
            case "AttackMotionEnd": {
                if (isPlaying)
                    InGameDataManager.inGameManager.AttackMotionEnd(json.Get("id"));
                break;
            }
            case "GameEnd": {
                if (isPlaying)
                    InGameDataManager.inGameManager.GameEnd(json);
                    isPlaying = false;
                    break;
                }
            case "Respawn": {
                if (isPlaying) {
                    InGameDataManager.inGameManager.Respawn(json.Get("id"));
                }
                break;
            }
            case "Recarvery": {
                if(isPlaying) {
                    InGameDataManager.inGameManager.Recarvery(json.Get("id"));
                }
                break;
            }
            case "Healing": {
                if(isPlaying) {
                    InGameDataManager.inGameManager.Healing(json.Get("id"), json.Get("scale"));
                }
                break;
            }
        }
    }

    public void SocketSend(String str) {
        if (str == null || !socketReady) return;
        writer.WriteLine(str);
        writer.Flush();
    }

    public string GetUserID() {
        return USER_ID;
    }

    public void OnDestroy() {
        if(socket.Connected) {
            SendMessage("Quit");
            socket.Close();
        }
    }
}
