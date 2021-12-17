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
    public bool IsGround { get; private set; } = true;
    public bool CanJump { get; private set; } = true;

    public Transform groundChecker;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;

    private float attackTimer = 0f;

    private Animator anim;
    private SoundManager soundManager;
    private SpriteRenderer render;

    public bool isMe;

    private string playerID;

    private Vector3 beforeLoc;
    private float moveTimer = 0f;
    private bool beforeMove = false;

    private long beforeDamagedTimeInClient = 0;

    public GameObject prfHpBar;
    public Image hpBar;
    private Image realHpBar;
    public float height = 1.2f;
    public float hpBarSpeed = 2f;

    public float currentFill = 1f;
    private static readonly int IsRun = Animator.StringToHash("isRun");
    private static readonly int IsAttack = Animator.StringToHash("isAttack");

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
        render = GetComponent<SpriteRenderer>();
        
        rigid.gravityScale = isMe ? 1 : 0;
    }

    // Update is called once per frame
    void Update() {
        DamagedShow();
        IsOnGround();
        HPBarPositionSetting();
        if (isMe) {
            Jump();
            AttackMotion();
        }
        isMoving();
    }

    public void HPBarPositionSetting() {
        var position = transform.position;
        var hpBarPos = Camera.main.WorldToScreenPoint(new Vector3(position.x, position.y + height, 1));
        hpBar.transform.position = hpBarPos;

        if(currentFill != realHpBar.fillAmount) {
            realHpBar.fillAmount = Mathf.Lerp(realHpBar.fillAmount, currentFill, Time.deltaTime * hpBarSpeed);
        }
    }

    public void isMoving() {
        Vector3 nowLoc = transform.position;

        if (Math.Abs(beforeLoc.x - nowLoc.x) > 0.05f) {
            anim.SetBool(IsRun, true);
            beforeLoc = nowLoc;
            beforeMove = true;
            moveTimer = 0f;
        }
        if (beforeMove) {
            moveTimer += Time.deltaTime;
            beforeMove = true;
            if (moveTimer >= 0.03f) anim.SetBool(IsRun, false);
        }
    }

    private void FixedUpdate() {
        if (isMe) Move();
    }

    private void IsOnGround() {
        IsGround = Physics2D.Raycast(groundChecker.position, Vector2.down, groundRadius, groundLayer);
        if (IsGround) {
            CanJump = true;
        }
    }

    void AttackMotion() {
        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            anim.SetBool(IsAttack, true);
            moveSpeed = 2.5f;
            jumpPower = 5.2f;
            soundManager.PlaySwordSwingSound();

            attackTimer = 0f;
            AttackAround();
            //TODO: Action
            // TcpManager.instance.SocketSend("AttackMotionStart");
        }
        if (Input.GetKeyUp(KeyCode.LeftControl)) {
            anim.SetBool(IsAttack, false);
            moveSpeed = 5f;
            jumpPower = 7f;
            // TcpManager.instance.SocketSend("AttackMotionEnd");
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
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow)) && (CanJump || IsGround)) {
            var velocity = rigid.velocity;
            velocity.y = 0;
            rigid.velocity = velocity;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            if (IsGround)
                IsGround = false;
            else
                CanJump = false;
        }
    }

    void Move() {
        var posX = Input.GetAxis("Horizontal");
        if (posX != 0) {
            var rotation = transform.rotation;
            if (posX >= 0)
            {
                QuaternionChange(rotation.x, 0, rotation.z);
            } else {
                QuaternionChange(transform.rotation.x, 180, rotation.z);
            }
        }
        transform.Translate(Mathf.Abs(posX) * Vector3.right * moveSpeed * Time.deltaTime);
    }

    private static void AttackEntity(EntityMonster enemy) {
        enemy.Attacked();
    }

    public void Attacked(string eid) {
        if (!isMe) return;
        long now = TimeManager.CurrentTimeMillis;
        if (now - beforeDamagedTimeInClient > 500) {
            //TODO: Attacked
            // TcpManager.instance.SocketSend(new JsonSetting().Add("entity", eid).Add("type", "PlayerDamagedByEntity").ToString());
            beforeDamagedTimeInClient = now;
        }
    }

    public float GetSpeed() {
        return moveSpeed;
    }

    public void SetIsMe(bool me) {
        isMe = me;
        
        if(me)
            InGameDataManager.instance.me = this;

        var vec = transform.position;
        vec.z = me ? -2 : -1;
        transform.position = vec;
        
        TransparentSetting();
    }
    public void Damaged() {
        damagedTimer = 0f;
        isDamaged = true;
    }

    private void DamagedShow()
    {
        if (!isDamaged) return;
        damagedTimer += Time.deltaTime;
        if (damagedTimer >= 1f) {
            isDamaged = false;
            damagedTimer = 0f;
            render.color = new Color(1, 1, 1);
        } else if (damagedTimer % 0.2 <= 0.1) {
            render.color = new Color(1, 1, 1);
        }else {
            render.color = new Color(1, 0.8f, 0.8f, 0.7f);
        }
        TransparentSetting();
    }

    private void TransparentSetting() {
        if (!isMe) {
            var color = render.color;
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
        if (!isMe) return;
        transform.position = new Vector3(0, -4, transform.position.z);
        Camera.main.backgroundColor = new Color(0, 0, 0, 0);
    }

    public void QuaternionChange(float x, float y, float z) {
        var q = transform.rotation;
        q.x = x;
        q.y = y;
        q.z = z;
        transform.rotation = q;
    }
}
