using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDestroyManager : MonoBehaviour {
    public static GameDestroyManager instance = null;
    private void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) GameDestroy();
    }

    public void GameDestroy() {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
