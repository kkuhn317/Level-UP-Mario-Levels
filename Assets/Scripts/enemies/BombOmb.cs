using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BombOmb : EnemyAi
{

    public enum EnemyState {
        walking,
        primed
    }

    public EnemyState state = EnemyState.walking;

    public Vector2 kickForce;

    private Animator animator;

    private AudioSource audioSource;

    public float explodeTime = 4f;

    public float explodeRadius = 3f;
    public GameObject explosionObject;

    void Awake()
    {
        animator = GetComponent<Animator>();
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

    protected override void Update()
    {
        base.Update();
        if (state == EnemyState.primed) {
            // if y velocity is 0, then we are on the ground
            // so stop x velocity
            if (velocity.y == 0) {
                velocity.x = 0;
            }
        }
    }

    protected override void touchNonPlayer(GameObject other)
    {
        if (other.gameObject.tag == "Projectile") {
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
        
        velocity = new Vector2(0, velocity.y);
    }


    protected override void hitByStomp(GameObject player) {
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

    protected override void hitOnSide(GameObject player) {
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
        audioSource.PlayOneShot(knockAwaySound);
        objectState = ObjectState.falling;
        movingLeft = direction;
        velocity = new Vector2(kickForce.x, kickForce.y);
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
                    hitCollider.gameObject.GetComponent<EnemyAi>().KnockAway(transform.position.x > hitCollider.transform.position.x);
                    break;
            }

        }
        Instantiate(explosionObject, transform.position, Quaternion.identity);
        Destroy(gameObject);

    }

}
