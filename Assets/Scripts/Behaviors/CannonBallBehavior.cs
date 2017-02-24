using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallBehavior : MonoBehaviour {
    // boundaries and stationary deletion
    private void Update()
    {
        if (gameObject.transform.position.x < -10 || gameObject.transform.position.x > 10 || gameObject.transform.position.y < -10)
        {
            Destroy(gameObject);
        }
        if (gameObject.GetComponent<RigidBody>().velocity.magnitude < 0.05)
        {
            Destroy(gameObject);
        }
    }

    // Collisions deletion
    public void OnCollisionDetected(GameObject other)
    {
        if (other.name == "Ground")
        {
            Destroy(gameObject);
        }
    }

    // Apply wind
    private void FixedUpdate()
    {
        float wind = GameObject.Find("PhysicsSystem").GetComponent<PhysicsSystem>().activeWind * Time.fixedDeltaTime;
        RigidBody rb = gameObject.GetComponent<RigidBody>();
        if (!(rb.velocity.x + wind < 0))
        {
            rb.velocity.x += wind;
        }
    }
}
