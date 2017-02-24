using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Verlet body uses verlet integration for it's movement
// These are often used within a VerletGroup to simulate a shape

// Unlike normal verlet we will also be storing old velocity
// This is so the physics engine can act on this like any other object
// We will store the velocity in the object at the end of our update so
// The physics engine can read it. We will allow the physics engine to act
// Upon it. We will then subtract the old velocity so any changes are taken
// Into account. We then recalculate the current vel using verlet integration
// Then add whatever changes the engine made BACK to our value.
public class VerletBody : PhysicsBody {
    private Vector3 oldVelocity = Vector3.zero;
    private Vector3 oldPosition = Vector3.zero;

    // Initialization called at the same time as Start()
    protected override void init()
    {
        oldVelocity = Vector3.zero;
        oldPosition = gameObject.transform.position;
    }

    // Physics engine calls this when it's time to update our physics
    public override void PhysicsUpdate()
    {
        if (!isKinematic)
        {
            // Calculate change in velocity implemented by the physics system
            Vector3 deltaV = velocity - oldVelocity;
            // Calculate the intertia on the object
            Vector3 inertia = gameObject.transform.position - oldPosition;
            inertia = PhysicsSystem.toMeters(inertia);
            // Correct the velocity to match verlet integration
            velocity = inertia + deltaV;
            // Store velocity and position
            oldVelocity = velocity;
            oldPosition = gameObject.transform.position;
            // Update position
            gameObject.transform.position += PhysicsSystem.toUnityUnits(velocity);
        }
        else
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
        }
    }

    public VerletGroup getVerletGroup()
    {
        return gameObject.transform.parent.gameObject.GetComponent<VerletGroup>();
    }
}
