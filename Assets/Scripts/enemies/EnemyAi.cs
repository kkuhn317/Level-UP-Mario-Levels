﻿using System.Collections;
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

    public bool canBeFireballed = true;

    public LayerMask floorMask;
    public LayerMask wallMask;

    private bool firstframe = true;

    private float adjDeltaTime;

    private bool bouncedBack = false;

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

        bouncedBack = false;
    }

    void UpdateEnemyPosition () {

        Vector3 pos = transform.position;
        Vector3 scale = transform.localScale;

        // check walls first
        if (state != EnemyState.knockedAway) {
            if (velocity.x != 0)
                    pos = CheckWalls (pos, -scale.x);
        }

        //print("adjDeltaTime: " + adjDeltaTime);

        if (state == EnemyState.falling || state == EnemyState.knockedAway) {
            
            pos.y += velocity.y * adjDeltaTime;

            velocity.y -= gravity * adjDeltaTime;
        }

        if (isWalkingLeft) {

            pos.x -= velocity.x * adjDeltaTime;

            scale.x = 1;

        } else {

            pos.x += velocity.x * adjDeltaTime;

            scale.x = -1;
        }

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
            //    pos = CheckWalls (pos, -scale.x);


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

        Vector2 originLeft = new Vector2 (pos.x - 0.5f + 0.2f, pos.y - halfHeight);
        Vector2 originMiddle = new Vector2 (pos.x, pos.y - (halfHeight + 0.01f));
        Vector2 originRight = new Vector2 (pos.x + 0.5f - 0.2f, pos.y - halfHeight);
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

        Vector2 originTop = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.8f);
        Vector2 originMiddle = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + .5f);
        Vector2 originBottom = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.2f);

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

        Vector2 originTop = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.8f);
        Vector2 originMiddle = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + .5f);
        Vector2 originBottom = new Vector2 (pos.x + direction * 0.5f, pos.y - halfHeight + 0.2f);

        RaycastHit2D[] wallTop = Physics2D.RaycastAll (originTop, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallMiddle = Physics2D.RaycastAll (originMiddle, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);
        RaycastHit2D[] wallBottom = Physics2D.RaycastAll (originBottom, new Vector2 (direction, 0), velocity.x * adjDeltaTime, wallMask);

        // remove self from list
        List<RaycastHit2D> hitRays = new List<RaycastHit2D> ();

        foreach (RaycastHit2D hitRay in wallTop) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                hitRays.Add (hitRay);
            }
        }

        foreach (RaycastHit2D hitRay in wallMiddle) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                hitRays.Add (hitRay);
            }
        }

        foreach (RaycastHit2D hitRay in wallBottom) {
            if (hitRay.collider != GetComponent<Collider2D> ()) {
                hitRays.Add (hitRay);
            }
        }

        if (hitRays.Count > 0) {
            return hitRays[0];
        }

        return new RaycastHit2D ();
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
            if (otherEnemyAi.isWalkingLeft == isWalkingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0)
                // They are walking the same direction, so 
                return pos;

            if (otherEnemyAi.state == EnemyState.knockedAway) {
                // They are knocked away, so don't push them
                return pos;
            }
            // test: set x position right next to the other enemy
            pos.x = hitRay.collider.bounds.center.x + (hitRay.collider.bounds.size.x/2 + 0.5f) * -direction;
            //print(pos.x);
        }

        if (!(hitRay.collider.gameObject.tag == "Enemy" && !checkEnemyCollision)) {
            isWalkingLeft = !isWalkingLeft;
            if (!bouncedBack)
                BounceEnemyBack(pos, -direction);
        }

        return pos;

    }

    // Fixes multiple enemies being right next to each other
    void BounceEnemyBack(Vector3 pos, float direction) {
        bouncedBack = true;

        //RaycastHit2D hitRay = RaycastWalls (pos, direction);
        RaycastHit2D hitRay = RaycastWallsBetter (pos, direction);

        if (hitRay.collider == null) {
            return;
        }

        //print (gameObject.name + " bounce back " + hitRay.collider.gameObject.name);

        if (hitRay.collider.gameObject.tag == "Enemy") {
            
            EnemyAi otherEnemyAi = hitRay.collider.gameObject.GetComponent<EnemyAi>();
            if (otherEnemyAi.isWalkingLeft == isWalkingLeft && !otherEnemyAi.stayStill && otherEnemyAi.velocity.x != 0)
                // They are walking the same direction, so 
                return;

            // not next to each other, so bounce back
            otherEnemyAi.BounceEnemyBack(otherEnemyAi.transform.position, direction);  // check the previous enemy too
            otherEnemyAi.isWalkingLeft = !otherEnemyAi.isWalkingLeft;
            // test: set x position of the other enemy right next to this one
            otherEnemyAi.transform.position = new Vector3(pos.x + (hitRay.collider.bounds.size.x/2 + 0.5f) * direction, otherEnemyAi.transform.position.y, otherEnemyAi.transform.position.z);
            
            //print(pos.x);
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
