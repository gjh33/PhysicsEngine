using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidBody : PhysicsBody {
	// Update is called in a fixed time interval
	public override void PhysicsUpdate () {
        if (!isKinematic)
        {
            velocity += acceleration * Time.fixedDeltaTime;
            gameObject.transform.position += PhysicsSystem.toUnityUnits(velocity);
        }
        else
        {
            velocity = Vector3.zero;
            acceleration = Vector3.zero;
        }
	}
}
