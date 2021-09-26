using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Range : MonoBehaviour
{

    // Status
    public int full_hp;
    int curr_hp;
    public float movePower;
    float chasePower;
    public float maxShotDelay;
    float curShotDelay;
    public float maxShotReadyDelay;
    float curShotReadyDelay;
    public float maxWaveDelay;
    float curWaveDelay;
    float range;
    float shootTimer;
    float rotateDg; // 발사각

    // Status Boolean
    bool isMoving;
    bool isTracing; // 적 추적
    bool runAway; // 거리조절
    bool targeting; // 조준
    bool bowCharging;
    bool canShoot;
    bool bowShoot;
    bool isHitted;
    bool isStunned;
    bool isDead;

    // Components
    Animator animator;
    Rigidbody2D rigidbody;
    AudioSource audio;

    Vector3 soundPos;
    Vector3 targetPos;

    // Movements
    int movementFlag; // 0 : Idle, 1 : Left, 2 : Right, 3 : Up, 4 : Down

    // Objects
    
    // public SearchnShoot search;
    public GameObject enemyBullet;
    public GameObject soundWave;
    // public Player player;

    // Audios
    public AudioClip bowCharge;
    public AudioClip shoot;
    public AudioClip hit;
    public AudioClip shieldBashHit;
    public AudioClip dead;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rigidbody = GetComponent<Rigidbody2D>();
        audio = GetComponent<AudioSource>();

        // player = GameObject.Find("Player").GetComponent<Player>();

        soundPos = new Vector3(0, 0, 50);
    }

    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            range = Vector3.Distance(targetPos, rigidbody.transform.position);
        }

        // ShowSoundWave();
        // Delay();
    }

    void FixedUpdate()
    {
        Move();
    }

    // Functions
    void Init()
    {
        curr_hp = full_hp;
        chasePower = movePower * 4f;

        isMoving = false;
        isDead = false;
        isHitted = false;
        isStunned = false;
        isTracing = false;
        canShoot = false;
        targeting = false;
        bowCharging = false;
        bowShoot = false;
        runAway = false;
    }

    // Status Functions (외부참조)
    public void setTrace(bool parameter)
    {
        this.isTracing = parameter;
    }

    public void setRun(bool parameter)
    {
        this.runAway = parameter;
    }

    public void setDead(bool parameter)
    {
        this.isDead = parameter;
    }

    public void setHitted(bool parameter)
    {
        this.isHitted = parameter;
    }

    public void setStunned(bool parameter)
    {
        this.isStunned = parameter;
    }

    public void setTargeting(bool parameter)
    {
        this.targeting = parameter;
    }

    // Functions
    void Move()
    {
        Vector3 moveVelocity = Vector3.zero;

        /*
        if (player.gameObject.layer == 18 || isDead) {
            return;
        } */

        if (isTracing)
        {
            Vector3 PlayerPos = targetPos;

            // 거리에 따른 패턴
            if (range < 4.0f && GameObject.FindGameObjectWithTag("Player") != null && !bowCharging)
            { //근거리 접근 + 화살 장전하지 않을 시 후퇴
                animator.SetBool("isMoving", true);

                setTargeting(false);
                setRun(true);
            }

            else if ((range >= 4.0f && range < 13.0f && isTracing == true && GameObject.FindGameObjectWithTag("Player") != null) || bowCharging)
            { //중거리 + 화살 장전 시 발사 준비 
                animator.SetBool("isMoving", false);

                setTargeting(true);
                setRun(false);

                StartCoroutine("Shot");
            }

            else if (range >= 13.0f && GameObject.FindGameObjectWithTag("Player") != null)
            {
                animator.SetBool("isMoving", true);

                setTargeting(false);
                setRun(false);
            }

            // 애니메이션
            if (Mathf.Abs(PlayerPos.x - this.transform.position.x) > Mathf.Abs(PlayerPos.y - this.transform.position.y))
            {
                if (PlayerPos.x > transform.position.x && PlayerPos.x != transform.position.x)
                {
                    //Debug.Log("우측");
                    animator.SetInteger("Direction", 2);
                }

                else if (PlayerPos.x < transform.position.x && PlayerPos.x != transform.position.x)
                {
                    //Debug.Log("좌측");
                    animator.SetInteger("Direction", 1);
                }
            }

            else
            {
                if (PlayerPos.y > transform.position.y && PlayerPos.y != transform.position.y)
                {
                    //Debug.Log("위");
                    animator.SetInteger("Direction", 3);
                }

                else if (PlayerPos.y < transform.position.y && PlayerPos.x != transform.position.y)
                {
                    //Debug.Log("아래");
                    animator.SetInteger("Direction", 4);
                }
            }

            // 속도 조절
            if (runAway)
            {
                chasePower = movePower * 4f;
            }
            else if (targeting)
            {
                chasePower = 0f; // 조준 시 속도를 멈춘다.
            }
            else
            {
                chasePower = movePower * 4f;
            }

            // 이동
            if (!runAway)
            {
                transform.position = Vector3.MoveTowards(transform.position, PlayerPos, (chasePower * Time.fixedDeltaTime));
            }
            else if (runAway)
            {
                transform.position = Vector3.MoveTowards(transform.position, PlayerPos, -(chasePower * Time.fixedDeltaTime));
            }
        }
    }

    void ShowSoundWave() {
        if (isDead) {
            return;
        }

        if (bowCharging == true && curWaveDelay > maxWaveDelay) {
            Debug.Log("사운드웨이브");
            GameObject Wave = Instantiate(soundWave, this.transform.position + soundPos, Quaternion.Euler(-90, 0, 0));
            curWaveDelay = 0;
            Destroy(Wave, 3);
        }

        else {
            //볼륨 줄이기
            audio.volume = 0.05f;
            return;
        }
    }

    void Delay() {
        if (isDead)
            return;

        curWaveDelay += Time.deltaTime;
        curShotReadyDelay += Time.deltaTime;

        if (curShotReadyDelay >= maxShotReadyDelay)
            canShoot = true;
        else if (curShotReadyDelay < maxShotReadyDelay)
            canShoot = false;

        if (bowCharging)
        {
            curShotDelay += Time.deltaTime;
        }
        else if (!bowCharging)
        {
            curShotDelay = 0;
        }
    }

    void Stun()
    {
        setStunned(true);
        // player.cur_Stamina += 30;

        Invoke("StunOut", 1f);
    }

    void StunOut()
    {
        setStunned(false);
    }

    void PlaySound(string name)
    {
        switch (name)
        {
            case "BCharge":
                audio.clip = bowCharge;
                break;
            case "Shoot":
                audio.clip = shoot;
                break;
            case "Hit":
                audio.clip = hit;
                break;
            case "ShieldBashHit":
                audio.clip = shieldBashHit;
                break;
            case "Dead":
                audio.clip = dead;
                break;
        }

        audio.Play();
    }

    // Colliders
    /*
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (isDead)
            return;

        if (other.gameObject.tag == "PlayerBullet")
        {
            Debug.Log("적 원거리 피격당함!");
            PlaySound("Hit");
            setHitted(true);
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            StartCoroutine(OnDamage(bullet.damage));
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "PlayerSwing")
        {
            Debug.Log("적 근접 피격당함!");
            PlaySound("Hit");
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            StartCoroutine(OnDamage(bullet.damage));
        }
        else if (other.gameObject.tag == "PlayerShieldBash")
        {
            Debug.Log("적 쉴드 배시 당함!");
            PlaySound("ShieldBashHit");
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            Stun();
            StartCoroutine(OnDamage(bullet.damage));
        }
    } */

    // Coroutines
    IEnumerator Shot()
    {
        float dy = targetPos.y - rigidbody.transform.position.y;
        float dx = targetPos.x - rigidbody.transform.position.x;
        rotateDg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;

        Vector2 dir = (targetPos - rigidbody.transform.position);

        if (canShoot)
        {
            bowCharging = true;

            if (curShotDelay == 0)
                PlaySound("Charge");

            if (curShotDelay >= maxShotDelay)
            {
                bowShoot = true;
                PlaySound("Shoot");

                GameObject eBullet = Instantiate(enemyBullet, transform.position, Quaternion.Euler(0, 0, rotateDg));
                Rigidbody2D rigid = eBullet.GetComponent<Rigidbody2D>();
                rigid.AddForce(dir.normalized * 30, ForceMode2D.Impulse);
                curShotReadyDelay = 0f;

                bowCharging = false;
                bowShoot = false;
            }
        }

        yield return null;
    }
}