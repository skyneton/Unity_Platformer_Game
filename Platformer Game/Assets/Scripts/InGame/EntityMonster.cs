using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMonster : MonoBehaviour
{
    private bool isDamaged = false;
    private float damagedTimer = 0f;

    private Animator anim;

    private float beforeX;
    private float moveTimer = 0f;
    private bool beforeMove = false;
    public string entityID;
    private SoundManager soundManager;
    bool isDie = false;
    float dieTimer = 0f;

    private long beforeDamagedTimeInClient = 0;

    private static readonly int IsDie = Animator.StringToHash("isDie");
    private static readonly int IsRun = Animator.StringToHash("isRun");

    // Start is called before the first frame update
    private void Start() {
        anim = GetComponent<Animator>();
        soundManager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
    }

    // Update is called once per frame
    private void Update() {
        DieMotion();
        IsMoving();
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

    public void IsMoving() {
        var nowLoc = transform.position;

        if (Math.Abs(beforeX - nowLoc.x) > 0.04) {
            anim.SetBool(IsRun, true);
            beforeX = nowLoc.x;
            beforeMove = true;
            moveTimer = 0f;
        }

        if (!beforeMove) return;
        beforeMove = true;
        if (moveTimer >= 0.05f) anim.SetBool(IsRun, false);
    }

    public void Die() {
        isDie = true;
        anim.SetBool(IsDie, true);
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

    public void Attacked() {
        long now = TimeManager.CurrentTimeMillis;
        if (now - beforeDamagedTimeInClient > 300) {
            //TODO: Damaged
            // NetworkManager.instance.SocketSend(new JsonSetting().Add("entity", ENTITY_ID).Add("type", "EntityDamagedByPlayer").ToString());
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
        enemy.Attacked(entityID);
    }

    public void QuaternionChange(float x, float y, float z) {
        var q = transform.rotation;
        q.x = x;
        q.y = y;
        q.z = z;
        transform.rotation = q;
    }
}
