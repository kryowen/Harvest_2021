using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerachArea : MonoBehaviour
{
    // Objects
    public CircleCollider2D area;
    public GameObject Enemy;

    // Scripts
    private Melee melee;
    private Range range;

    // Status
    public float areaRange;

    void Start() {
        if(Enemy.tag.Equals("MeeleEnemy")) {  // 종류구분
            melee = Enemy.GetComponent<Melee> ();
        }
        else if (Enemy.tag.Equals("RangeEnemy")) {
            range = Enemy.GetComponent<Range> ();
        }

        area = gameObject.GetComponent<CircleCollider2D>();
        area.radius = areaRange;
    }

    void Update() {
       
    }

    // Colliders
    private void OnTriggerStay2D(Collider2D other) { 
        if(other.gameObject.tag.Equals("Player") && this.tag.Equals("MeeleEnemy")) {
            Debug.Log("플레이어가 추적 범위에 들어옴");
            melee.setTrace(true);
        }
        else if (other.gameObject.tag.Equals("Player") && this.tag.Equals("RangeEnemy")) {
            // 발사
        }

        if (this.tag.Equals("MeeleEnemy") && (other.gameObject.tag.Equals("MeeleEnemy") || other.gameObject.tag.Equals("MeeleEnemy"))) { 
            if(other.gameObject.GetComponent<Melee>().getHitted().Equals(true)) { // || other.gameObject.GetComponent<Range>().getHitted().Equals(true)) {
                melee.setTrace(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.tag.Equals("Player") && this.tag.Equals("MeeleEnemy")) {
            Debug.Log("플레이어가 추적 범위에 들어옴");
            melee.setTrace(false);
        }
        else if (other.gameObject.tag.Equals("Player") && this.tag.Equals("RangeEnemy")) { 
           // 발사
        }
    }
}
