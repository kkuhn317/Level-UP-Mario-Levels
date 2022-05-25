using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class deadMario : MonoBehaviour
{

    public float gravity;
    public Vector2 velocity;

    // Start is called before the first frame update
    void Start()
    {
        globalVariables.lives -= 1;
        globalVariables.levelscene = SceneManager.GetActiveScene().buildIndex;
        Invoke("ToLivesScene", 1);
    }



    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.localPosition;
        Vector3 scale = transform.localScale;

        pos.y += velocity.y * Time.deltaTime;

        velocity.y -= gravity * Time.deltaTime;

        transform.localPosition = pos;
    }

    void ToLivesScene() {
        SceneManager.LoadScene("Lives");
    }
}
