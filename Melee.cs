using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    // Status
    public int full_hp;
    int curr_hp; // 현재 체력 → 시작 시, 현제체력 = 전체체력 
    public float movePower; // 평상시 의 이동속도
    float chasePower; // 추적상태의 이동속도 (평상시의 속도 * 1.8)
    public float attackDelay = 1.5f; // 공격속도 지연
    float attackTimer;
    float range; // 객체와 플레이어사이의 거리

    // Status Boolean
    bool isTrace; // 추적상태
    bool isMoving;
    bool isAttack;
    bool isStunned;
    bool isHit;
    bool isDead;

    // Components
    Animator animator;
    Rigidbody2D rigidbody2D;
    AudioSource audioSource;
    Vector3 soundPos;
    Vector3 targetPos;

    // Movements;
    int movementFlag = 0; // 0 : Idle, 1 : Left, 2 : Right, 3 : Up, 4 : Down

    // Objects
    public GameObject area;

    // Audios
    public AudioClip shieldBashHit;
    public AudioClip hit;
    public AudioClip dead;

    void Start() {
        init(); // 객체 초기화

        rigidbody2D = gameObject.GetComponent<Rigidbody2D> ();
        animator = gameObject.GetComponentInChildren<Animator> ();

        audioSource = gameObject.GetComponent<AudioSource> ();
        soundPos = new Vector3(0, 0, 50);

        if (GameObject.FindGameObjectWithTag("Player") != null) { // 플레이어가 없으면, 초기화하지 않는다.
            targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            range = Vector3.Distance(transform.position, targetPos);
        }

        StartCoroutine("MovementChange");
    }


    void Update() {
        if (GameObject.FindGameObjectWithTag("Player") != null)  { // 초기위치 뿐만 아니라 지속적인 추적이 가능하게끔 Update() 에도 넣는다.
            targetPos = GameObject.FindGameObjectWithTag("Player").transform.position;
            range = Vector3.Distance(transform.position, targetPos);
        }

        Delay();
        //ShowSoundWave();
    }

    void FixedUpdate()
    {
        move();
    }

    // Colliders
    /*
    void OnTriggerEnter2D(Collider2D other) {

        if (isDead) { // 객체가 죽었을 경우
            return;
        }

        if (other.gameObject.tag.Equals("PlayerBullet")) { // 원거리 피격
            Debug.Log("적 원거리 피격당함!");
            PlaySound("Hit");
            setHitted(true);
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            StartCoroutine(OnDamage(bullet.damage));
            Destroy(other.gameObject);
        }

        else if (other.gameObject.tag.Equals("PlayerSwing")) { // 근거리 피격
            Debug.Log("적 근접 피격당함!");
            PlaySound("Hit");
            setHitted(true);
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            StartCoroutine(OnDamage(bullet.damage));
        }

        else if (other.gameObject.tag.Equals("PlayerShieldBash")) { // 패링당할 경우
            Debug.Log("적 쉴드 배시 당함!");
            PlaySound("shieldBashHit");
            Bullet bullet = other.gameObject.GetComponent<Bullet>();
            Stun();
            StartCoroutine(OnDamage(bullet.damage));
        }
    } */

    // initialization Function
    void init() {
        // Status initializing
        curr_hp = full_hp;
        chasePower = (movePower * 1.8f);
        attackTimer = 0;

        // Status Boolean initializing
        isTrace = false; // 추적상태
        isMoving = false;
        isAttack = false;
        isStunned = false;
        isHit = false;
        isDead = false;
    }

    // Status Functions
    public void setTrace(bool parameter) {
        this.isTrace = parameter;
    }

    public void setMoving(bool parameter) {
        this.isMoving = parameter;
    }

    public void setAttack(bool parameter) {
        this.isAttack = parameter;
    }

    public void setStunned(bool parameter) {
        this.isStunned = parameter;
    }

    public void setHitted(bool parameter) {
        this.isHit = parameter;
    }

    public void setDead(bool parameter) {
        this.isDead = parameter;
    }

    public bool getHitted() { // 맞았는지 확인
        return isHit;
    }

    void Stun() {
        setStunned(true);
        // 플레이어의 스테미너를 채워줌.
        Invoke("StunOut", 1f);
    }
    void StunOut() {
        setStunned(false);
    }

    void Delay()
    {
        if (isDead) {
            return;
        }

        attackTimer += Time.deltaTime;
        // curWaveDelay += Time.deltaTime;
    }

    // Behavior Functions
    private void move() {
        Vector3 moveVelocity = Vector3.zero; // 방향 초기화

        if(isDead) {
            animator.SetBool("isAttack", false);
            return;
        }

        // 두 객체사이의 거리가 2.5이상 이하일때, 공격를 함.
        if (range < 2.5f && GameObject.FindGameObjectWithTag("Player") != null && !isStunned && !isAttack)
        {
            animator.SetBool("isRanged", false);
            animator.SetBool("isMoving", false);

            setAttack(true);
            StartCoroutine("MeleeAttack");
        }

        else if (range >= 2.5f && isTrace == true && GameObject.FindGameObjectWithTag("Player") != null && !isAttack)
        {
            animator.SetBool("isAttack", false);
            animator.SetBool("isRanged", true);
            animator.SetBool("isMoving", true);
        }


        // 공격 시, 이동속도를 조정하는 부분
        float tmpMovePower = 0;
        if (isAttack) {
            tmpMovePower = chasePower;
            chasePower = 0f;
        }
        else if (!isAttack) {
            chasePower = tmpMovePower;
        }

        if (isTrace) { // 추적상태일 경우

            if (Mathf.Abs(targetPos.x - this.transform.position.x) > Mathf.Abs(targetPos.y - this.transform.position.y)) {
                if (targetPos.x > transform.position.x && targetPos.x != transform.position.x) {
                    //Debug.Log("우측");
                    animator.SetInteger("Directions", 2);
                }

                else if (targetPos.x < transform.position.x && targetPos.x != transform.position.x) {
                    //Debug.Log("좌측");
                    animator.SetInteger("Directions", 1);
                }
            }

            else {
                if (targetPos.y > transform.position.y && targetPos.y != transform.position.y) {
                    //Debug.Log("위");
                    animator.SetInteger("Directions", 3);
                }

                else if (targetPos.y < transform.position.y && targetPos.x != transform.position.y) {
                    //Debug.Log("아래");
                    animator.SetInteger("Directions", 4);
                }
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, (chasePower * Time.fixedDeltaTime));
        }

        else { // 비 추적상태일 경우
            if (movementFlag == 1) {
                moveVelocity = Vector3.left;
                animator.SetInteger("Directions", 1);
            }

            else if (movementFlag == 2) {
                moveVelocity = Vector3.right;
                animator.SetInteger("Directions", 2);
            }

            else if (movementFlag == 3) {
                moveVelocity = Vector3.up;
                animator.SetInteger("Directions", 3);
            }

            else if (movementFlag == 4) {
                moveVelocity = Vector3.down;
                animator.SetInteger("Directions", 4);
            }

            transform.position += moveVelocity * movePower * Time.fixedDeltaTime;
        }
    }
    // VF Functions
    /*
    void ShowSoundWave()
    {
        if (isDead) {
            return;
        }

        if (player.earsLost == false) {

            if (isTracing == true && curWaveDelay > maxWaveDelay) {
                Debug.Log("사운드웨이브 발생");
                GameObject Wave = Instantiate(soundWave, this.transform.position + soundPos, Quaternion.Euler(-90, 0, 0));
                curWaveDelay = 0;
                Destroy(Wave, 4);
            }
        }
        else {
            audioSource.volume = 0.05f; // 볼륨을 줄인다.
            return;
        }
    }*/

    void PlaySound(string name) {
        switch (name) {
            case "Hit":
                audioSource.clip = hit;
                break;

            case "ShieldBashHit":
                audioSource.clip = shieldBashHit;
                break;

            case "Dead":
                audioSource.clip = dead;
                break;
        }

        audioSource.Play();
    }

    // Coroutines
    IEnumerator ChangeMovement() {
        movementFlag = 0;
        // movementFlag = Random.Range(0,5); // 0 ~ 4 까지 방향을 조절

        if (movementFlag == 0) {
            animator.SetBool("isMoving", false);
        }
        else {
            animator.SetBool("isMoving", true);
        }

        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");
    }

    IEnumerator MeleeAttack() {
        float dy = targetPos.y - transform.position.y;
        float dx = targetPos.x - transform.position.x;

        float rotateDg = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        Vector2 Dir = targetPos - transform.position;

        if (isAttack) {
            if (attackTimer >= attackDelay) {
                animator.SetBool("isAttack", true);

                yield return new WaitForSeconds(0.3f);
                
                /* if (count == 0) {
                    GameObject swordWay = Instantiate(enemySwordWay, transform.position, Quaternion.Euler(0, 0, rotateDg));
                    Rigidbody2D rigid = swordWay.GetComponent<Rigidbody2D>();
                    rigid.AddForce(Dir.normalized * 20, ForceMode2D.Impulse);
                    count++;
                } */

                attackTimer = 0f;
            }

            else if (attackTimer < attackDelay) {
                animator.SetBool("isAttack", false);
            }
        }
        yield return new WaitForSeconds(0.4f);
        setAttack(false);
    }

    IEnumerator OnDamage(int damage)
    {
        if (curr_hp > 0) {
            curr_hp -= damage;
        }
           
        if (curr_hp <= 0) {
            curr_hp = 0;
            PlaySound("Dead");
            StopAllCoroutines();

            setDead(true);
            animator.SetBool("isDead", true);

            gameObject.layer = 19;
        }

        yield return null;
    }
}
