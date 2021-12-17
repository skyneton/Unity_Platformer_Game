using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStayManager : MonoBehaviour
{
    private bool isReady = false;
    public Image waiting;
    private int users = 0;
    private float timer;
    private bool isReadyTimer = false;
    public Text waitingTimer;
    // Start is called before the first frame update
    void Start()
    {
        WaitingSceneDataManager.instance.gameStayManager = this;
    }

    private void Update() {
        if(isReadyTimer) {
            int temp = Mathf.FloorToInt(timer);
            timer -= Time.deltaTime;
            if(Mathf.FloorToInt(timer) != temp) {
                waitingTimer.text = Mathf.FloorToInt(timer).ToString();
            }

            if(timer <= 0) {
                GameStartTimerCancel();
            }
        }
    }

    public void OnPlayBtnClick() {
        if (isReady == false) {
            isReady = true;
            GameReady();
        }
    }

    private static void GameReady() {
        NetworkManager.Instance.SendPacket(new PacketOutGameReady());
    }

    public void AddPlayer() {
        if(isReady) {
            DrawStayImage(users + 1);
        }
    }

    public void RemovePlayer() {
        if(isReady) {
            DrawStayImage(users - 1);
        }
    }

    public void GameStartTimer(long time) {
        timer = time / 1000.0f;
        isReadyTimer = true;
        waitingTimer.text = Mathf.FloorToInt(timer).ToString();
        waitingTimer.gameObject.SetActive(isReadyTimer);
    }

    public void GameStartTimerCancel() {
        isReadyTimer = false;
        waitingTimer.gameObject.SetActive(isReadyTimer);
    }

    public void GameStart() {
        SceneManager.LoadSceneAsync("InGameScene").completed += delegate(AsyncOperation operation)
        {
            NetworkManager.Instance.SendPacket(new PacketGameStart());
        };
    }

    public void DrawStayImage(int users) {
        this.users = users;
        waiting.gameObject.SetActive(true);
        switch(users) {
            case 2: {
                    waiting.sprite = WaitingSceneDataManager.instance.waitingRoomImage2;
                    break;
                }
            case 3: {
                    waiting.sprite = WaitingSceneDataManager.instance.waitingRoomImage3;
                    break;
                }
            case 4: {
                    waiting.sprite = WaitingSceneDataManager.instance.waitingRoomImage4;
                    break;
                }
            default: {
                    waiting.sprite = WaitingSceneDataManager.instance.waitingRoomImage1;
                    break;
                }
        }
    }
}
