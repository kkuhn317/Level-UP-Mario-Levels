using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Castle : MonoBehaviour
{

    void OnTriggerEnter2D (Collider2D other) {

        if (other.tag == "Player") {

            SceneManager.LoadScene("Win");
        }
    }
}
