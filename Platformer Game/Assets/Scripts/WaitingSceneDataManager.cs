using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingSceneDataManager : MonoBehaviour {
    public GameStayManager gameStayManager;
    public static WaitingSceneDataManager instance;
    public Sprite waitingRoomImage1;
    public Sprite waitingRoomImage2;
    public Sprite waitingRoomImage3;
    public Sprite waitingRoomImage4;
    public Text errorMessage;

    private void Awake() {
        instance = this;
    }
}
