using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BombOmb : MonoBehaviour
{

    public enum EnemyState {
        walking,
        primed
    }

    public EnemyState state = EnemyState.walking;

    public Vector2 kickForce;

    private Animator animator;
    private EnemyAi enemyAi;

    private AudioSource audioSource;

    public AudioClip kickSound;

    public float explodeTime = 4f;

    public float explodeRadius = 3f;
    public GameObject explosionObject;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAi = GetComponent<EnemyAi>();
        audioSource = GetComponent<AudioSource>();

        switch(state) {
            case EnemyState.walking:
                ToWalking();
                break;
            case EnemyState.primed:
                ToPrimed();
                break;
        }
    }

    void Update()
    {
        if (state == EnemyState.primed) {
            // if y velocity is 0, then we are on the ground
            // so stop x velocity
            if (enemyAi.velocity.y == 0) {
                enemyAi.velocity.x = 0;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.tag == "Player") {
            Playerbetter playerscript = other.gameObject.GetComponent<Playerbetter>();
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

            if (playerscript.starPower) {
                audioSource.PlayOneShot(kickSound);
                GetComponent<EnemyAi>().KnockAway(other.transform.position.x > transform.position.x);
                return;
            }

            if (playerscript.powerupState == Playerbetter.PowerupState.small) {
                if (rb.position.y - 0.2 > transform.position.y + 0.2) {
                    onStomped(other.gameObject);
                } else {
                   onhitPlayer(other.gameObject);
                }
            } else {
                if (rb.position.y - 0.7 > transform.position.y + 0.2) {
                    onStomped(other.gameObject);
                } else {
                    onhitPlayer(other.gameObject);
                }
            }
        } if (other.gameObject.tag == "Projectile") {
            GameObject ball = other.gameObject;
            if (ball.GetComponent<fireball>()) {
                switch (state) {
                    case EnemyState.walking:
                        ToPrimed();
                        kickBomb(ball.GetComponent<ObjectPhysics>().movingLeft);
                        break;
                    case EnemyState.primed:
                        kickBomb(ball.GetComponent<ObjectPhysics>().movingLeft);
                        break;
                }
            }
        }
    }

    private void ToWalking() {
        state = EnemyState.walking;
    }

    private void ToPrimed() {
        if (state != EnemyState.primed) {
            state = EnemyState.primed;
            animator.SetTrigger("prime");
            Invoke("explode", explodeTime);
        }
        
        enemyAi.velocity = new Vector2(0, enemyAi.velocity.y);
    }


    private void onStomped(GameObject player) {
        Playerbetter playerScript = player.GetComponent<Playerbetter>();
        switch (state) {
            case EnemyState.walking:
                playerScript.Jump();
                audioSource.Play();
                ToPrimed();
                break;
            case EnemyState.primed:
                kickBomb(player.transform.position.x > transform.position.x);
                break;
        }
    }

    private void onhitPlayer(GameObject player) {
        Playerbetter playerScript = player.GetComponent<Playerbetter>();
        switch (state) {
            case EnemyState.walking:
                playerScript.damageMario();
                break;
            case EnemyState.primed:
                kickBomb(player.transform.position.x > transform.position.x);
                break;
        }
    }

    private void kickBomb(bool direction) {
        audioSource.PlayOneShot(kickSound);
        enemyAi.state = EnemyAi.EnemyState.falling;
        enemyAi.isWalkingLeft = direction;
        enemyAi.velocity = new Vector2(kickForce.x, kickForce.y);
    }

    private void explode() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        foreach (var hitCollider in hitColliders)
        {
            // just a reminder that sendmessage exists
            //hitCollider.SendMessage("AddDamage");

            if (hitCollider.gameObject.GetComponent<breakableBlock>()) {
                hitCollider.gameObject.GetComponent<breakableBlock>().breakBlock();
                continue;
            }

            switch (hitCollider.gameObject.tag) {
                case "Player":
                    hitCollider.gameObject.GetComponent<Playerbetter>().damageMario();
                    break;
                case "Enemy":
                    hitCollider.gameObject.GetComponent<EnemyAi>().KnockAwaySound(kickSound, transform.position.x > hitCollider.transform.position.x);
                    break;
            }

        }
        Instantiate(explosionObject, transform.position, Quaternion.identity);
        Destroy(gameObject);

    }

}
