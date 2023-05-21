using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class specialQuestionBlock : MonoBehaviour
{

    private bool activated = false;

    public GameObject pipe;

    public GameObject[] spinies;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!activated && GetComponent<QuestionBlock>().activated) {
            activated = true;
            pipe.GetComponent<Animator>().SetTrigger("goDown");
            SpriteRenderer[] sprites = pipe.GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sprite in sprites) {
                sprite.sortingLayerID = 0;
                sprite.sortingOrder = -1;
            }
            Invoke("moveSpinies", 1);
        }
    }

    void moveSpinies() {
        foreach(GameObject spiny in spinies) {
            spiny.GetComponent<EnemyAi>().movement = ObjectPhysics.ObjectMovement.sliding;
        }
    }
}
