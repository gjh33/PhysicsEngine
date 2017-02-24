using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsBody : MonoBehaviour
{
    public bool isKinematic = false; // If true this object is not affected by outside forces
    public float dragCoefficient = 0.0f; // The coefficient used for air resistance calculations (coeff) * v^2
    [HideInInspector]
    public Vector3 velocity = Vector3.zero;
    [HideInInspector]
    public Vector3 acceleration = Vector3.zero;

    // Use this for initialization
    void Start()
    {
        PhysicsSystem.register(gameObject); // Register with the physics system
        init();
    }

    protected virtual void init() { } //Used for initialization in derived class (replaces start())

    // Update is called in a fixed time interval
    // This is where you are responsible for updating the position of your body based on information within the class
    public abstract void PhysicsUpdate();
}
