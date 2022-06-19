using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepMaterialScale : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var max = Room.Length > Room.Height ? Room.Length : Room.Height;

        if (this.GetComponent<MeshRenderer>())
        {
            MeshRenderer _meshRenderer = this.GetComponent<MeshRenderer>();
            _meshRenderer.material.mainTextureScale = new Vector2(Room.Length, Room.Width);//Your desired scale
        }
    }
}
