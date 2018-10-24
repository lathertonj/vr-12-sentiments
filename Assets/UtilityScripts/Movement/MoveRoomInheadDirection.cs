using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRoomInheadDirection : MonoBehaviour
{	
	public Transform room;
	public float speed = 5f;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
		room.position += Time.deltaTime * speed * transform.forward;
    }
}
