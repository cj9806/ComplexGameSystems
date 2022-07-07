using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{
    public RoomGenerator room;
    // Start is called before the first frame update
    void Start()
    {
        transform.position = room.startRoomPos.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
