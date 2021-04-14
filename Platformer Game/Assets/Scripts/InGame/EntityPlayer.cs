using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityPlayer : MonoBehaviour {
    private bool isDamaged = false;
    private float damagedTimer = 0f;

    private float moveSpeed = 5.0f;
    private float jumpPower = 7.0f;

    private Rigidbody2D rigid;
    bool isGround = true;
    bool canJump = true;

    public Transform groundChecker;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private float attackTimer = 0f;

    private Animator anim;
    private SoundManager soundManager;

    public bool isMe = false;

    private string PLAYER_ID;

    private Vector3 beforeLoc;
    private float moveTimer = 0f;
    private bool beforeMove = false;

    private long beforeDamagedTimeInClient = 0;

    public GameObject prfHpBar;
    public Image hpBar;
    private Image realHpBar;
    public float height = 1.2f;
    public float hpBarSpeed = 2f;

    public float healthPoint {
        get {
            return healthPoint;
        }

        set {
            if (value > 1f) value = 1f;
            else if (value < 0f) value = 0f;
        }
    }

    public float currentFill = 1f;

    // Start is called before the first frame update
    void Start() {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        beforeLoc = transform.position;
        if(isMe == false) {
            rigid.gravityScale = 0f;
        }
        hpBar = Instantiate(prfHpBar, GameObject.Find("HPCanvas").transform).GetComponent<Image>();
        realHpBar = hpBar.transform.GetChild(0).GetComponent<Image>();
    }

    // Update is called once per frame
    void Update() {
        DamagedShow();
        IsOnGround();
        HPBarPositionSetting();
        if (isMe) {
            if (InGameDataManager.instance.me != this)
                InGameDataManager.instance.me = this;
            Jump();
            AttackMotion();
        }
        isMoving();
    }

    public void HPBarPositionSetting() {
        Vector3 hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(transform.position.x, transform.position.y + height, 1));
        hpBar.transform.position = hpBarPos;

        if(currentFill != realHpBar.fillAmount) {
            realHpBar.fillAmount = Mathf.Lerp(realHpBar.fillAmount, currentFill, Time.deltaTime * hpBarSpeed);
        }
    }

    public void isMoving() {
        Vector3 nowLoc = transform.position;

        if (beforeLoc.x != nowLoc.x) {
            anim.SetBool("isRun", true);
            beforeLoc = nowLoc;
            beforeMove = true;
            moveTimer = 0f;
        }
        if (beforeMove) {
            moveTimer += Time.deltaTime;
            beforeMove = true;
            if (moveTimer >= 0.03f) anim.SetBool("isRun", false);
        }
    }

    private void FixedUpdate() {
        if (isMe) Move();
    }

    void IsOnGround() {
        isGround = Physics2D.OverlapCircle(groundChecker.position, groundRadius, groundLayer);
        if (isGround) {
            canJump = true;
        }
    }

    void AttackMotion() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            anim.SetBool("isAttack", true);
            moveSpeed = 2.5f;
            jumpPower = 5.2f;
            soundManager.PlaySwordSwingSound();

            attackTimer = 0f;
            AttackAround();
            TcpManager.instance.SocketSend("AttackMotionStart");
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            anim.SetBool("isAttack", false);
            moveSpeed = 5f;
            jumpPower = 7f;
            TcpManager.instance.SocketSend("AttackMotionEnd");
        }

        if(Input.GetKey(KeyCode.LeftControl)) {
            attackTimer += Time.deltaTime;
            if(attackTimer >= 0.3f) {
                attackTimer = 0f;
                soundManager.PlaySwordSwingSound();
            }
            AttackAround();
        }
    }

    private void AttackAround() {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 2, (1 << LayerMask.NameToLayer("EntityMonster")));

        if (col != null) {
            AttackEntity(col.gameObject.GetComponent<EntityMonster>());
        }
    }

    void Jump() {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && (canJump || isGround)) {
            Vector2 velocity = rigid.velocity;
            velocity.y = 0;
            rigid.velocity = velocity;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            if (isGround)
                isGround = false;
            else
                canJump = false;
        }
    }

    void Move() {
        float posX = Input.GetAxis("Horizontal");
        if (posX != 0) {
            if (posX >= 0) {
                QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
            } else {
                QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
            }
        }
        transform.Translate(Mathf.Abs(posX) * Vector3.right * moveSpeed * Time.deltaTime);
    }

    private void AttackEntity(EntityMonster enemy) {
        enemy.Attacked();
    }

    public void Attacked(string eid) {
        if (!isMe) return;
        long now = TimeUtils.CurrentTimeInMillis();
        if (now - beforeDamagedTimeInClient > 500) {
            TcpManager.instance.SocketSend(new JsonSetting().Add("entity", eid).Add("type", "PlayerDamagedByEntity").ToString());
            beforeDamagedTimeInClient = now;
        }
    }

    public float GetSpeed() {
        return moveSpeed;
    }

    public void SetIsMe(bool isMe) {
        InGameDataManager.instance.me = this;
        Vector3 vec = transform.position;
        vec.z = -1f;
        if(isMe) {
            this.isMe = isMe;
            vec.z = -2f;
            if (rigid != null) rigid.gravityScale = 1f;
        }else if(rigid != null) {
            rigid.gravityScale = 0f;
        }
        transform.position = vec;
        TransparentSetting();
    }
    public void Damaged() {
        damagedTimer = 0f;
        isDamaged = true;
    }

    private void DamagedShow() {
        if (isDamaged) {
            damagedTimer += Time.deltaTime;
            SpriteRenderer render = GetComponent<SpriteRenderer>();
            if (damagedTimer >= 1f) {
                isDamaged = false;
                damagedTimer = 0f;
                render.color = new Color(1, 1, 1);
            } else if (damagedTimer >= 0.9f) {
                render.color = new Color(1, 0.8f, 0.8f, 0.7f);
            } else if (damagedTimer >= 0.7f) {
                render.color = new Color(1, 1, 1);
            } else if (damagedTimer >= 0.6f) {
                render.color = new Color(1, 0.8f, 0.8f, 0.7f);
            } else if (damagedTimer >= 0.4f) {
                render.color = new Color(1, 1, 1);
            } else if (damagedTimer >= 0.3f) {
                render.color = new Color(1, 0.8f, 0.8f, 0.7f);
            } else if (damagedTimer >= 0.1f) {
                render.color = new Color(1, 1, 1);
            } else {
                render.color = new Color(1, 0.8f, 0.8f, 0.7f);
            }
            TransparentSetting();
        }
    }

    private void TransparentSetting() {
        if (!isMe) {
            SpriteRenderer render = GetComponent<SpriteRenderer>();
            Color color = render.color;
            color.a -= 0.2f;
            render.color = color;
        }
    }

    public void Die() {
        gameObject.SetActive(false);
        hpBar.gameObject.SetActive(false);
        if (isMe) {
            Camera.main.backgroundColor = new Color(0.4f, 0f, 0f, 0.2f);
        }
    }

    public void Respawn() {
        gameObject.SetActive(true);
        hpBar.gameObject.SetActive(true);
        currentFill = 1f;
        if (isMe) {
            Vector3 vec = transform.position;
            vec.x = 0f;
            vec.y = -4f;
            transform.position = vec;
            Camera.main.backgroundColor = new Color(0, 0, 0, 0);
        }
    }

    public void QuaternionChange(float x, float y, float z) {
        Quaternion q = transform.rotation;
        q.x = x;
        q.y = y;
        q.z = z;
        transform.rotation = q;
    }
}
