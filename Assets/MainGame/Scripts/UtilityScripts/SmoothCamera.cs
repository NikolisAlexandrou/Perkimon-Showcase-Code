using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothCamera : MonoBehaviour
{

    [SerializeField] GameObject player;
    [SerializeField] float helpCameraFollowFaster = 10;

    void FixedUpdate()
    {
        this.transform.position = new Vector3(Mathf.Lerp(this.transform.position.x, player.transform.position.x, Time.deltaTime * helpCameraFollowFaster),
                                               Mathf.Lerp(this.transform.position.y, player.transform.position.y, Time.deltaTime * helpCameraFollowFaster), -10);
    }
}
