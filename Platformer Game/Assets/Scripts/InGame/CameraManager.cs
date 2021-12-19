using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour {
    private float moveSpeed = 5.0f;

    // Update is called once per frame
    void FixedUpdate() {
        CameraMove();
        SpecterMode();
    }
    
    private void CameraMove() {
        if (InGameDataManager.Instance.me != null && InGameDataManager.Instance.me.gameObject.activeSelf) {
            GameObject player = InGameDataManager.Instance.me.gameObject;
            Vector3 target = transform.position;
            target.x = player.transform.position.x;
            target.y = player.transform.position.y;
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * player.GetComponent<EntityPlayer>().GetSpeed());
        }
    }

    private void SpecterMode() {
        if (InGameDataManager.Instance.me == null || !InGameDataManager.Instance.me.gameObject.activeSelf) {
            float posY = Input.GetAxis("Vertical");
            float posX = Input.GetAxis("Horizontal");
            transform.Translate((posX * Vector3.right + posY * Vector3.up) * moveSpeed * Time.deltaTime);
        }
    }
}
