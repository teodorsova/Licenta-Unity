using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    private GameObject room;
    void Start()
    {
        room = GameObject.Find("Room");
        if (Room.Length != 0 && Room.Width != 0)
        {
            if (Room.Height != 0)
            {
                room.gameObject.transform.localScale = new Vector3(Room.Length / 2f, Room.Height / 2f, Room.Width / 2f);
            } else
            {
                room.gameObject.transform.localScale = new Vector3(Room.Length / 2f, 1, Room.Width / 2f);
            }
           
        }

    }

}
