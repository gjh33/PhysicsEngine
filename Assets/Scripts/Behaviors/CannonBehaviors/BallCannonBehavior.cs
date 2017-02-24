using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCannonBehavior : CannonBehavior {
    public GameObject cannonBall; // The cannonball used to fire

    public override void fire()
    {
        // Rotate the cannon to a random angle
        float cannonAngle = UnityEngine.Random.Range(minAngle, maxAngle);
        rotateBarrel(cannonAngle);

        GameObject spawnPoint = barrel.transform.Find("SpawnPoint").gameObject;
        GameObject ball = Instantiate(cannonBall, spawnPoint.transform.position, Quaternion.identity);
        RigidBody rb = ball.GetComponent<RigidBody>();
        rb.velocity = new Vector3(Mathf.Cos(cannonAngle * Mathf.Deg2Rad) * fireSpeed, Mathf.Sin(cannonAngle * Mathf.Deg2Rad) * fireSpeed, 0);
    }
}
