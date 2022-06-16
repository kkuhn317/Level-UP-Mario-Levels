using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goomba : MonoBehaviour
{

    public enum EnemyState {
        walking,
        crushed
    }

    public EnemyState state = EnemyState.walking;

    public bool stompable = true;

    private bool shouldDie = false;
    private float deathTimer = 0;
    public float timeBeforeDestroy = 1.0f;

    public AudioClip kickSound;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update() {
        CheckCrushed();
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Playerbetter playerscript = other.gameObject.GetComponent<Playerbetter>();
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

            if (playerscript.starPower) {
                GetComponent<AudioSource>().PlayOneShot(kickSound);
                GetComponent<EnemyAi>().KnockAway(other.transform.position.x > transform.position.x);
                return;
            }

            if (playerscript.powerupState == Playerbetter.PowerupState.small) {
                if (rb.position.y - 0.2 > transform.position.y + 0.2) {
                    playerscript.Jump();
                    Crush();
                } else {
                    playerscript.damageMario();
                }
            } else {
                if (rb.position.y - 0.7 > transform.position.y + 0.2) {
                    playerscript.Jump();
                    Crush();
                } else {
                    playerscript.damageMario();
                }
            }
        }
    }

    public void Crush () {

        GetComponent<AudioSource>().Play();
        
        if (!stompable) {
            return;
        }

        state = EnemyState.crushed;

        EnemyAi enemyAi = GetComponent<EnemyAi>();

        //enemyAi.velocity = new Vector2(0, enemyAi.velocity.y);
        enemyAi.stayStill = true;

        GetComponent<Animator>().SetBool("isCrushed", true);

        GetComponent<Collider2D>().enabled = false;

        shouldDie = true;

        
    }

    void CheckCrushed () {

        if (shouldDie) {
            
            if (deathTimer <= timeBeforeDestroy) {

                deathTimer += Time.deltaTime;

            } else {
                
                shouldDie = false;

                Destroy(this.gameObject);
            }
        }
    }

}
