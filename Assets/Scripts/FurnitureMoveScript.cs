using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureMoveSCript : MonoBehaviour
{
    private Vector3 screenPoint;
    private Vector3 offset;
    private float speed = 50f;

    private void Start()
    {

    }
    void OnMouseDown()
    {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
        if (SelectedFurniture.SelectedGameObject != null)
        {
            //SelectedFurniture.SelectedGameObject.gameObject.GetComponent<MeshRenderer>().material.color += new Color(0f, 0f, 0f, 100f);
        }
        SelectedFurniture.SelectedGameObject = this.gameObject;
        
        //SelectedFurniture.SelectedGameObject.gameObject.GetComponent<MeshRenderer>().material.color -= new Color(0f, 0f, 0f, 100f);

        //var prefabButton = Resources.Load<GameObject>("Arrow/Arrow");
        //prefabButton.transform.position = offset;
        //Instantiate(prefabButton);
    }

    void OnMouseDrag()
    {
        Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + offset;
        transform.position = cursorPosition;
    }

    private void OnMouseUp()
    {
        
    }

    void Update()
    {
        if (this.gameObject == SelectedFurniture.SelectedGameObject)
        {
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up * speed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(-Vector3.up * speed * Time.deltaTime);
            }
        }
    }
}
