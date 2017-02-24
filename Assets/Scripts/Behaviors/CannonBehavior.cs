using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CannonBehavior : MonoBehaviour {
    // Public variables
    public bool active = false;
    public float fireSpeed = 15.0f; // The speed in m/s that the ball is fired at
    [Range(0, 180)] public float minAngle = 5; // minAngle the cannon can rotate
    [Range(0, 180)] public float maxAngle = 80; // maxAngle the cannon can rotate

    // Protected variables
    protected GameObject barrel;

	// Use this for initialization
	void Start () {
        // The gameobject for the barrel of the cannon
        barrel = gameObject.transform.FindChild("Barrel").gameObject;

        // Set the barel to a default angle
        rotateBarrel(minAngle + ((maxAngle - minAngle) / 2));
	}
	
	// Update is called once per frame
	void Update () {
        handleInput();
	}

    // Rotates the cannon barrel to a given angle
    protected void rotateBarrel(float deg)
    {
        barrel.transform.eulerAngles = new Vector3(0, 0, deg);
    }

    // Routes keyboard input to corresponding functions
    void handleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) && active)
        {
            fire(); // FIRE THE CANNON!!!!!!!!!!!!
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            active = !active; // Toggle active
        }
    }

    // Fires the cannon
    public abstract void fire();
}
