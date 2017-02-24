using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatVerletBehavior : MonoBehaviour {
    VerletBody vb;

    public void Start()
    {
        vb = gameObject.GetComponent<VerletBody>();
    }

    public void OnCollisionDetected(GameObject other)
    {
        if (other.name == "Hill")
        {
            vb.isKinematic = true;
            vb.getVerletGroup().addConstraint(new PinConstraint(vb));
        }
        else if (other.name == "Ground")
        {
            Destroy(vb.getVerletGroup().gameObject);
        }
        else if (other.GetComponent<CannonBallBehavior>())
        {
            vb.velocity += other.GetComponent<RigidBody>().velocity;
            Destroy(other);
        }
    }

    // Apply wind forces
    public void FixedUpdate()
    {
        float wind = GameObject.Find("PhysicsSystem").GetComponent<PhysicsSystem>().activeWind * Time.fixedDeltaTime;
        VerletBody vb = gameObject.GetComponent<VerletBody>();
        if (!(vb.velocity.x + wind > 0))
        {
            vb.velocity.x += wind;
        }
    }
}
