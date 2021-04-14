using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMonster : MonoBehaviour
{
    private bool isDamaged = false;
    private float damagedTimer = 0f;
    public bool locationSend = false;

    private Animator anim;

    private Vector3 beforeLoc;
    private float moveTimer = 0f;
    private bool beforeMove = false;
    private string ENTITY_ID;
    private float attackTimer = 0f;
    private SoundManager soundManager;
    bool isDie = false;
    float dieTimer = 0f;

    private long beforeDamagedTimeInClient = 0;

    private float sendLocationTimer = 0f;
    private Vector3 position;
    private float rY;

    private bool canLeft = true, canRight = true;
    public bool isGround;
    private int betweenLength = 0;
    private float betweenTimer = 0f;
    public LayerMask groundLayer;

    public Transform bottomChecker;
    public Transform frontChecker;
    public Transform topChecker;
    public Transform bottomFrontChecker;
    public Transform behindFarChecker;
    public Transform topFrontChecker;
    public Transform topBehindChecker;
    public Transform frontFarChecker;

    public GameObject target;

    private float moveSpeed = 4.0f;
    private float jumpPower = 8.8f;

    private Rigidbody2D rigid;
    // Start is called before the first frame update
    void Start() {
        anim = GetComponent<Animator>();
        rigid = GetComponent<Rigidbody2D>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    // Update is called once per frame
    void Update() {
        DieMotion();
        if (!isDie) {
            DamagedShow();
            isMoving();
            attackTimer += Time.deltaTime;
            if (attackTimer >= 0.4f) {
                AttackAround();
                attackTimer = 0f;
            }

            if (locationSend) {
                IsOnGround();
            }
        }
    }

    void FixedUpdate() {
        if(transform.position.z != -1) {
            Vector3 vec = transform.position;
            vec.z = -1;
            transform.position = vec;
        }
        if (locationSend) {
            MoveManager();
            SendLocationPacket();
        }
    }

    void IsOnGround() {
        isGround = Physics2D.OverlapCircle(bottomChecker.position, 0.08f, groundLayer);
        if(!isGround) {
            canLeft = true;
            canRight = true;
            betweenTimer = 0;
            betweenLength = 0;
        }
    }

    public void SendLocationPacket() {
        if (InGameDataManager.instance.me != null) {
            sendLocationTimer += Time.deltaTime;
            float beforeX = Mathf.Round(position.x * 1000) / 1000;
            float beforeY = Mathf.Round(position.y * 1000) / 1000;
            float nowX = Mathf.Round(transform.position.x * 1000) / 1000;
            float nowY = Mathf.Round(transform.position.y * 1000) / 1000;
            float nowRY = Mathf.Round(transform.rotation.y * 1000) / 1000;
            if (sendLocationTimer >= 0.04f && (position == null || beforeX != nowX || beforeY != nowY || rY != nowRY)) {
                JsonSetting json = new JsonSetting();
                sendLocationTimer = 0f;
                position = transform.position;
                rY = nowRY;
                TcpManager.instance.SocketSend(json.Add("id", ENTITY_ID).Add("x", nowX).Add("y", nowY).Add("rY", nowRY).Add("type", "MonsterLocation").ToString());
            }
        }
    }

    public void DieMotion() {
        if (isDie) {
            dieTimer += Time.deltaTime;
            if (dieTimer >= 1f) {
                soundManager.PlayHitSound();
                Destroy(gameObject);
            }
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
            if (moveTimer >= 0.05f) anim.SetBool("isRun", false);
        }
    }

    public void Die() {
        isDie = true;
        anim.SetBool("isDie", true);
    }
    public void Damaged() {
        damagedTimer = 0f;
        isDamaged = true;
        soundManager.PlayHitSound();
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
            } else if(damagedTimer >= 0.7f) {
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
        }
    }

    public void SetEntityID(string id) {
        ENTITY_ID = id;
    }

    public void Attacked() {
        long now = TimeUtils.CurrentTimeInMillis();
        if (now - beforeDamagedTimeInClient > 300) {
            TcpManager.instance.SocketSend(new JsonSetting().Add("entity", ENTITY_ID).Add("type", "EntityDamagedByPlayer").ToString());
            beforeDamagedTimeInClient = now;
        }
    }

    private void AttackAround() {
        Collider2D col = Physics2D.OverlapCircle(transform.position, 1.1f, (1 << LayerMask.NameToLayer("EntityPlayer")));

        if (col != null) {
            AttackEntity(col.gameObject.GetComponent<EntityPlayer>());
        }
    }

    private void AttackEntity(EntityPlayer enemy) {
        enemy.Attacked(ENTITY_ID);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (target == null && other.gameObject.tag.Equals("Player")) {
            target = other.gameObject;
        }
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (target == null && other.gameObject.tag.Equals("Player")) {
            target = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (target == other.gameObject) {
            target = null;
        }
    }

    private void MoveManager() {
        if (target == null) MoveForward();
        else MoveTargeting();
    }

    private void MoveForward() {
        bool hasForward = Physics2D.OverlapCircle(frontChecker.position, 0.13f, groundLayer);
        bool hasForwardBottom = Physics2D.OverlapCircle(bottomFrontChecker.position, 0.4f, groundLayer);
        if (hasForward || !hasForwardBottom) {
            if (transform.rotation.y != 0) {
                if(hasForward) canRight = false;
                QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
            }
            else {
                if (hasForward) canLeft = false;
                QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
            }
        }

        transform.Translate(-Vector3.right * moveSpeed * Time.deltaTime);
    }

    private void MoveTargeting() {
        bool hasForward = Physics2D.OverlapCircle(frontChecker.position, 0.13f, groundLayer);
        bool hasForwardBottom = Physics2D.OverlapCircle(bottomFrontChecker.position, 0.4f, groundLayer);

        int posX = 0;
        if (target.transform.position.y >= transform.position.y - 0.1f && target.transform.position.y <= transform.position.y + 1.2f) {
            if(target.transform.position.x < transform.position.x) {
                posX = 1;
                if (transform.rotation.y != 0) {
                    QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
                }
            }
            else if (target.transform.position.x > transform.position.x) {
                if (transform.rotation.y == 0) {
                    QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
                }
                posX = 1;
            }
        }
        else if(target.transform.position.y > transform.position.y) {
            bool cantUp = Physics2D.OverlapCircle(topChecker.position, 0.1f, groundLayer);
            bool hasTopForward = Physics2D.OverlapCircle(topFrontChecker.position, 0.3f, groundLayer);
            bool hasTopBehind = Physics2D.OverlapCircle(topBehindChecker.position, 0.3f, groundLayer);
            bool hasTopBehindFar = Physics2D.OverlapCircle(behindFarChecker.position, 0.3f, groundLayer);
            bool hasTorwardFar = Physics2D.OverlapCircle(frontFarChecker.position, 0.1f, groundLayer);
            posX = 1;
            if (!cantUp && (hasTopBehind && !hasTopBehindFar || hasTopForward && !hasTorwardFar) && isGround) {
                if (hasTopBehind && !hasTopBehindFar && !hasTopForward) {
                    if (transform.rotation.y == 0) {
                        if (target.transform.position.x > transform.position.x)
                            QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
                        else Jump();
                    }
                    else {
                        if (target.transform.position.x < transform.position.x)
                            QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
                        else Jump();
                    }
                }
                Jump();
            }
            else if(!hasForwardBottom && isGround) {
                if (transform.rotation.y == 0) {
                    if (target.transform.position.x > transform.position.x)
                        QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
                    else Jump();
                }else {
                    if (target.transform.position.x < transform.position.x)
                        QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
                    else Jump();
                }
            }
        }else {
            posX = 1;
            if (betweenLength > 0) {
                betweenTimer += Time.deltaTime;
                if (betweenTimer >= 0.3f) {
                    betweenTimer--;
                }
            }
            if (canLeft && !canRight || canLeft && target.transform.position.x < transform.position.x) {
                if (transform.rotation.y != 0 && betweenLength <= 5 && isGround) {
                    QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
                    betweenLength++;
                }
            }
            else if (canRight && transform.rotation.y == 0 && betweenLength <= 5 && isGround) {
                QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
                betweenLength++;
            }
            else if(!canLeft && !canRight) {
                canLeft = true;
                canRight = true;
                betweenTimer = 0;
                betweenLength = 0;
            }
        }

        transform.Translate(posX * -Vector3.right * moveSpeed * Time.deltaTime);

        if (hasForward && isGround) {
            if (transform.rotation.y != 0) {
                if (hasForward) canRight = false;
                QuaternionChange(transform.rotation.x, 0, transform.rotation.z);
            }
            else {
                if (hasForward) canLeft = false;
                QuaternionChange(transform.rotation.x, 180, transform.rotation.z);
            }
        }
    }

    public void Jump() {
        if(isGround) {
            Vector2 velocity = rigid.velocity;
            velocity.y = 0;
            rigid.velocity = velocity;
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
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
