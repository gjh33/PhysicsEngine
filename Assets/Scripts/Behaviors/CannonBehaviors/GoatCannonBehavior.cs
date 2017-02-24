using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoatCannonBehavior : CannonBehavior {
    public GameObject goat; // The goat to fire

    public override void fire()
    {
        // Rotate the cannon to a random angle
        float cannonAngle = UnityEngine.Random.Range(minAngle, maxAngle);
        rotateBarrel(cannonAngle);

        GameObject spawnPoint = barrel.transform.Find("SpawnPoint").gameObject;
        GameObject mrGoat = Instantiate(goat, spawnPoint.transform.position, Quaternion.identity);

        VerletGroup vg = mrGoat.GetComponent<VerletGroup>();
        mrGoat.transform.eulerAngles = new Vector3(0, 0, cannonAngle - 180);
        vg.setVelocity(new Vector3(Mathf.Cos(cannonAngle * Mathf.Deg2Rad) * fireSpeed, Mathf.Sin(cannonAngle * Mathf.Deg2Rad) * fireSpeed, 0));
    }
}
