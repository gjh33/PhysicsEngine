using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Basic collider that provides polymorphism
[RequireComponent(typeof(PhysicsBody))]
public abstract class CustomCollider : MonoBehaviour {
    [Range(0.0f, 1.0f)] public float bounciness = 1.0f; // How much energy is conserved (1 = perfect bounce, 0 = no bounce)

    public PhysicsBody getPhysicsBody()
    {
        return gameObject.GetComponent<PhysicsBody>();
    }
}