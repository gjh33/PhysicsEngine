using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoxCollider : CustomCollider {
    // Width and height in local values (1 -> 1*gameObject.height)
    public float widthScale = 1;
    public float heightScale = 1;

    public float getWidth()
    {
        return gameObject.GetComponent<Renderer>().bounds.extents.x * 2;
    }
    public float getHeight()
    {
        return gameObject.GetComponent<Renderer>().bounds.extents.y * 2;
    }
}
