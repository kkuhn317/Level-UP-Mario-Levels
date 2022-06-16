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
    public float enemyWidth = 1;

    public float enemyMidadjustment = 0f;

    public bool canBeFireballed = true;

    public LayerMask floorMask;
    public LayerMask wallMask;

    // THIS IS DISTANCE AWAY FROM SIDES
    public float floorRaycastSpacing = 0.2f;
    public float wallRaycastSpacing = 0.2f;

    private bool firstframe = true;

    private float adjDeltaTime;


    //private bool bounced = false;

    private Vector2 normalScale;

    public enum EnemyState {
        
        walking,
        falling,
        knockedAway
    }

    public EnemyState state = EnemyState.falling;

    // Start is called before the first frame update
    void Start()
    {
        enabled = false;
        normalScale = transform.localScale;

        //Fall();
    }

    // Update is called once per frame
    void Update()
    {
        adjDeltaTime = Time.deltaTime;

        if (adjDeltaTime > 0.1f) {
            adjDeltaTime = 0f;  // lag spike fix
            // TODO: apply lag spike fix to all other moving objects
            //print("lagging!");
        }

        if ((!stayStill || state == EnemyState.knockedAway) && !firstframe)
            UpdateEnemyPosition ();
        firstframe = false;

        //bounced = false;
    }

    void UpdateEnemyPosition () {

        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        // check walls first
        if (state != EnemyState.knockedAway) {
            if (velocity.x != 0) {
                pos = CheckWalls (pos, isWalkingLeft ? -1 : 1);
            }
        }

        //print("adjDeltaTime: " + adjDeltaTime);

        //if (!bounced) {

            if (state == EnemyState.falling || state == EnemyState.knockedAway) {
                
                pos.y += velocity.y * adjDeltaTime;

                velocity.y -= gravity * adjDeltaTime;
            }

            if (isWalkingLeft) {

                pos.x -= velocity.x * adjDeltaTime;

                scale.x = normalScale.x;

            } else {

                pos.x += velocity.x * adjDeltaTime;

                scale.x = -normalScale.x;
            }
        //}

        // fix bug where enemy has y velocity but walking
        // making it walk in the air
        if (state == EnemyState.walking) {
            velocity.y = 0;
        }

        if (state != EnemyState.knockedAway) {
            // convert to global position
            //if (transform.parent != null) {
            //    globalPos = transform.parent.TransformPoint(pos);
            //}


            if (velocity.y <= 0) {
                pos = CheckGround (pos);
            }


            //if (velocity.x != 0)
            //    pos = CheckWalls (pos, isWalkingLeft ? -1 : 1);



            if (DontFallOffLedges && state == EnemyState.walking) {
                //CheckLedges(pos);
                CheckLedges(pos);
            }

            // convert back to local position
            //if (transform.parent != null) {
            //    pos = transform.parent.InverseTransformPoint(globalPos);
            //} else {
            //    pos = globalPos;
            //}
        }

        transform.position = pos;
        transform.localScale = scale;
    }

    Vector3 CheckGround (Vector3 pos) {
        //if (velocity.y >= 0) {
        //    return pos;
        //}

        float halfHeight = enemyHeight / 2;
        float halfWidth = enemyWidth / 2;

        Vector2 originLeft = new Vector2 (pos.x - halfWidth + floorRaycastSpacing, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - (halfHeight + 0.01f));
        Vector2 originRight = new Vector2 (pos.x + halfWidth - floorRaycastSpacing, pos.y - halfHeight);
        //print("adjDeltaTime is:" + (adjDeltaTime));
        //print("Velocity is:" + velocity);
        RaycastHit2D groundLeft = Physics2D.Raycast (originLeft, Vector2.down, -velocity.y * adjDeltaTime, floorMask);
        RaycastHit2D groundMiddle = Physics2D.Raycast (originMiddle, Vector2.down, -velocity.y * adjDeltaTime, floorMask);
        RaycastHit2D groundRight = Physics2D.Raycast (originRight, Vector2.down, -velocity.y * adjDeltaTime, floorMask);

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


            //print("hit " + hitRay.collider.gameObject.name);

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

    RaycastHit2D RaycastWalls (Vector3 pos, float direction) {
        float halfHeight = enemyHeight / 2;
        float halfWidth = enemyWidth / 2;

        Vector2 originTop = new Vector2 (pos.x + halfWidth, pos.y + halfHeight - wallRaycastSpacing);
        Vector2 originMiddle = new Vector2 (pos.x + halfWidth, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + halfWidth, pos.y - halfHeight + wallRaycastSpacing);

        RaycastHit2D wallTop = Physics2D.Raycast (originTop, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast (originMiddle, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast (originBottom, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null) {

            RaycastHit2D hitRay = wallTop;

            if (wallTop) {
                hitRay = wallTop;
            } else if (wallMiddle) {
                hitRay = wallMiddle;
            } else if (wallBottom) {
                hitRay = wallBottom;
            }

            return hitRay;
        }
        return new RaycastHit2D ();
    }

    RaycastHit2D RaycastWallsBetter (Vector3 pos, float direction) {
        // use raycast all and don't count itself
        float halfHeight = enemyHeight / 2;
        float halfWidth = enemyWidth / 2;

        Vector2 originTop = new Vector2 (pos.x + direction * halfWidth, pos.y + halfHeight - wallRaycastSpacing);
        Vector2 originMiddle = new Vector2 (pos.x + direction * halfWidth, pos.y);
        Vector2 originBottom = new Vector2 (pos.x + direction * halfWidth, pos.y - halfHeight + wallRaycastSpacing);

        RaycastHit2D[] wallTop = Physics2D.RaycastAll (originTop, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallMiddle = Physics2D.RaycastAll (originMiddle, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallBottom = Physics2D.RaycastAll (originBottom, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);


        // get shortest distance
        float shortestDistance = float.MaxValue;
        RaycastHit2D shortestRay = new RaycastHit2D ();

        foreach (RaycastHit2D hitRay in wallTop) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                if (hitRay.distance < shortestDistance) {
                    shortestDistance = hitRay.distance;
                    shortestRay = hitRay;
                }
            }
        }

        foreach (RaycastHit2D hitRay in wallMiddle) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                if (hitRay.distance < shortestDistance) {
                    shortestDistance = hitRay.distance;
                    shortestRay = hitRay;
                }
            }
        }

        foreach (RaycastHit2D hitRay in wallBottom) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                if (hitRay.distance < shortestDistance) {
                    shortestDistance = hitRay.distance;
                    shortestRay = hitRay;
                }
            }
        }

        return shortestRay;
    }



    Vector3 CheckWalls (Vector3 pos, float direction) {

        //RaycastHit2D hitRay = RaycastWalls (pos, direction);
        RaycastHit2D hitRay = RaycastWallsBetter (pos, direction);

        if (hitRay.collider == null) {
            return pos;
        }

        //print(gameObject.name + " hit " + hitRay.collider.gameObject.name);

        if (hitRay.collider.gameObject.tag == "Enemy") {
            EnemyAi otherEnemyAi = hitRay.collider.gameObject.GetComponent<EnemyAi>();

            if (otherEnemyAi.isWalkingLeft == isWalkingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0) {
                // They are walking the same direction. do some more checks
                if (otherEnemyAi.checkOtherEnemies()) {
                    isWalkingLeft = !isWalkingLeft;
                    //bounced = true;
                    //pos.x = hitRay.collider.bounds.center.x + (hitRay.collider.bounds.size.x/2 + 0.5f) * -direction;
                    // TODO: make collisions between enemies look a bit better
                }
                return pos;
                
            }

            if (otherEnemyAi.state == EnemyState.knockedAway) {
                // They are knocked away, so don't push them
                return pos;
            }

            // test: set x position right next to the other enemy
            pos.x = hitRay.collider.bounds.center.x + (hitRay.collider.bounds.size.x/2 + enemyWidth / 2) * -direction;
            //print(pos.x);
        }

        if (!(hitRay.collider.gameObject.tag == "Enemy" && !checkEnemyCollision)) {
            isWalkingLeft = !isWalkingLeft;
            //bounced = true;
        }

        return pos;

    }

    // return true if need to bounce back
    bool checkOtherEnemies() {
        // check if the other enemy is about to hit something
        RaycastHit2D hit = RaycastWallsBetter (transform.position, isWalkingLeft ? -1 : 1);
        if (hit.collider == null) {
            // they are not about to hit anything
            return false;
        } else {
            if (hit.collider.transform.tag != "Enemy") {
                // they are about to hit something, but it is not an enemy
                isWalkingLeft = !isWalkingLeft;
                //bounced = true;
                return true;
            } else {
                // they are about to hit an enemy
                EnemyAi otherEnemyAi = hit.collider.gameObject.GetComponent<EnemyAi>();
                if (otherEnemyAi.state == EnemyState.knockedAway) {
                    // they are about to hit an enemy that is knocked away
                    return false;
                }
                if (otherEnemyAi.isWalkingLeft == isWalkingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0) {
                    // they are about to hit an enemy that is moving the same direction
                    // check this next enemy too
                    return otherEnemyAi.checkOtherEnemies();
                } else {
                    // they are about to hit an enemy that is moving the opposite direction
                    otherEnemyAi.isWalkingLeft = !otherEnemyAi.isWalkingLeft;
                    //otherEnemyAi.bounced = true;
                    isWalkingLeft = !isWalkingLeft;
                    //bounced = true;
                    //transform.position = new Vector2(hit.collider.bounds.center.x + (hit.collider.bounds.size.x/2 + enemyWidth / 2) * (isWalkingLeft ? 1 : -1), transform.position.y);
                    return true;
                }
            }
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

    public void KnockAwaySound(AudioClip sound, bool direction) {
        // play sound
        if (sound != null && state != EnemyState.knockedAway && GetComponent<AudioSource>() != null) {
            GetComponent<AudioSource>().PlayOneShot(sound);
            KnockAway(direction);
        }

    }

    private void OnBecameInvisible() {
        // once the knocked away enemy is off screen, destroy it
        if (state == EnemyState.knockedAway) {
            Destroy(gameObject);
        }
    }
}
