using UnityEngine;
using System.Collections;

public class NexVideoObject : MonoBehaviour {

    public float speed = 10.0f;

    void Update () {
        this.transform.Rotate(speed * Time.deltaTime, 60 * Time.deltaTime, 0);
    }
}