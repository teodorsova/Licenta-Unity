using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomCamera : MonoBehaviour
{

    public int mouseSensitivity = 500;
    public Transform playerBody;
    private float X;
    private float Y;

    private void Start()
    {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
    mouseSensitivity = 100;
#endif
    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            transform.Rotate(new Vector3(Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime, -Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 0));
            X = transform.rotation.eulerAngles.x;
            Y = transform.rotation.eulerAngles.y;
            transform.rotation = Quaternion.Euler(X, Y, 0);
        }
    }
}
