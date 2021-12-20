using UnityEngine;
using System.Net.Sockets;
using System;
using Network;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour {
    public string serverIP = "127.0.0.1";
    public int serverPort = 3000;
    public static NetworkManager Instance { get; private set; }
    public long Timeout = 1500;

    private TcpClient client;

    public string userID;

    public PlayType PlayType { get; internal set; }

    private long LastPacketMillis = TimeManager.CurrentTimeMillis;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            Init();

            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this) Destroy(gameObject);
    }

    // Start is called before the first frame update
    private void Init()
    {
        try
        {
            client = new TcpClient(serverIP, serverPort)
            {
                NoDelay = true,
                SendTimeout = 500
            };
        } catch(Exception e) {
            print(e);
            WaitingSceneDataManager.instance.errorMessage.text = "서버 연결에 문제가 발생하였습니다. 인터넷이 연결되어 있는지 확인해주세요.";
        }
    }

    private void Update()
    {
        SocketConnectedUpdate();
        PacketUpdate();
        KeepAliveUpdate();
    }

    private void SocketConnectedUpdate()
    {
        if (client.Connected) return;
        if (WaitingSceneDataManager.instance != null)
        {
            WaitingSceneDataManager.instance.errorMessage.text = "서버 연결에 문제가 발생하였습니다. 인터넷이 연결되어 있는지 확인해주세요.";
        }
        else
        {
            if(!SceneManager.GetActiveScene().name.EndsWith("MainScene"))
                SceneManager.LoadScene("MainScene");
        }
    }

    private void PacketUpdate()
    {
        while (client.Connected && client.Available > 0)
        {
            var bytes = new byte[ByteBuf.ReadVarInt(client.GetStream())];
            client.GetStream().Read(bytes, 0, bytes.Length);

            PacketManager.Handle(this, new ByteBuf(bytes));
        }
    }

    private void KeepAliveUpdate()
    {
        if (client.Connected && TimeManager.CurrentTimeMillis - LastPacketMillis >= Timeout)
        {
            SendPacket(new PacketOutKeepAlive());
        }
    }

    public void SendPacket(Packet packet)
    {
        if (!client.Connected) return;
        var buf = new ByteBuf();
        packet.Write(buf);

        var data = buf.Flush();

        try
        {
            client.GetStream().WriteAsync(data, 0, data.Length);
            client.GetStream().Flush();
            
            LastPacketMillis = TimeManager.CurrentTimeMillis;
        }
        catch (Exception)
        {
            client.Close();
        }
    }

    public void OnDestroy()
    {
        client?.Close();
    }

    public static void Log(object s)
    {
        print(s);
    }
}
