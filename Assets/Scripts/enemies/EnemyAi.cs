using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyAi : MonoBehaviour
{

    public float gravity;
    public Vector2 velocity;
    public bool isWalkingLeft = true;

    public bool DontFallOffLedges = false;

    public bool checkEnemyCollision = true;

    public bool stayStill = false;

    public float enemyHeight;

    public LayerMask floorMask;
    public LayerMask wallMask;

    private bool firstframe = true;

    private enum EnemyState {
        
        walking,
        falling,
        knockedAway
    }

    private EnemyState state = EnemyState.falling;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;


        //Fall();
    }

    // Update is called once per frame
    void Update()
    {
        if ((!stayStill || state == EnemyState.knockedAway) && !firstframe)
            UpdateEnemyPosition ();
        firstframe = false;
    }

    void UpdateEnemyPosition () {

        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        if (state == EnemyState.falling || state == EnemyState.knockedAway) {
            
            pos.y += velocity.y * Time.deltaTime;

            velocity.y -= gravity * Time.deltaTime;
        }

        if (isWalkingLeft) {

            pos.x -= velocity.x * Time.deltaTime;

            scale.x = 1;

        } else {

            pos.x += velocity.x * Time.deltaTime;

            scale.x = -1;
        }

        if (state != EnemyState.knockedAway) {
            if (velocity.y <= 0)
                pos = CheckGround (pos);

            CheckWalls (pos, -scale.x);

            if (DontFallOffLedges && state == EnemyState.walking) 
                CheckLedges(pos);
        }

        transform.localPosition = pos;
        transform.localScale = scale;
    }

    Vector3 CheckGround (Vector3 pos) {

        float halfHeight = enemyHeight / 2;

        Vector2 originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - halfHeight);
        Vector2 originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - halfHeight);
        //print("Time.deltaTime is:" + (Time.deltaTime));
        //print("Velocity is:" + velocity);
        RaycastHit2D groundLeft = Physics2D.Raycast (originLeft, Vector2.down, -velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundMiddle = Physics2D.Raycast (originMiddle, Vector2.down, -velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundRight = Physics2D.Raycast (originRight, Vector2.down, -velocity.y * Time.deltaTime, floorMask);

        if (groundLeft.collider != null || groundMiddle.collider != null || groundRight.collider != null) {

            RaycastHit2D hitRay = groundLeft;

            if (groundLeft) {
                hitRay = groundLeft;
            } else if (groundMiddle) {
                hitRay = groundMiddle;
            } else if (groundRight) {
                hitRay = groundRight;
            }

            //pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + halfHeight;
            pos.y = hitRay.point.y + halfHeight;
            velocity.y = 0;

            state = EnemyState.walking;

        } else {

            if (state != EnemyState.falling) {

                Fall ();
            }
        }

        return pos;
    }

    void CheckLedges(Vector3 pos) {
        float halfHeight = enemyHeight / 2;

        Vector2 origin;

        if (isWalkingLeft) 
            origin = new Vector2 (pos.x - 0.5f, pos.y - halfHeight);
        else
            origin = new Vector2 (pos.x + 0.5f, pos.y - halfHeight);

        RaycastHit2D ground = Physics2D.Raycast (origin, Vector2.down, 0.5f, floorMask);

        if (!ground) {
            isWalkingLeft = !isWalkingLeft;
        }

    }

    void CheckWalls (Vector3 pos, float direction) {

        float halfHeight = enemyHeight / 2;

        Vector2 originTop = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.8f);
        Vector2 originMiddle = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + .5f);
        Vector2 originBottom = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.2f);

        RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {

            RaycastHit2D hitRay = wallTop;

            if (wallTop) {
                hitRay = wallTop;
            } else if (wallMiddle) {
                hitRay = wallMiddle;
            } else if (wallBottom) {
                hitRay = wallBottom;
            }

            if (hitRay.collider.gameObject.tag == "Enemy") {
                EnemyAi otherEnemyAi = hitRay.collider.gameObject.GetComponent<EnemyAi>();
                if (otherEnemyAi.isWalkingLeft == isWalkingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0)
                    // They are walking the same direction, so 
                    return;
            }

            if (!(hitRay.collider.gameObject.tag == "Enemy" && !checkEnemyCollision))
                isWalkingLeft = !isWalkingLeft;

        }
    }

    void OnBecameVisible() {
        
        enabled = true;
    }

    void Fall () {

        velocity.y = 0;

        state = EnemyState.falling;

    }

    public void setStill (bool still) {
        stayStill = still;
    }


    public void KnockAway(bool direction) {
        if (state != EnemyState.knockedAway) {
            GetComponent<SpriteRenderer>().flipY = true;
            state = EnemyState.knockedAway;
            velocity.y = 5;
            velocity.x = 5;
            isWalkingLeft = direction;
            GetComponent<Collider2D>().enabled = false;
            transform.rotation = Quaternion.identity;
        }
    }
}
