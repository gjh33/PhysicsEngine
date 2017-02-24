using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkConstraint : Constraint {
    // The verlets we are linking together
    private VerletBody verletA;
    private VerletBody verletB;
    // The distance we are locking them at
    private float distanceLock;

    public LinkConstraint(VerletBody a, VerletBody b)
    {
        verletA = a;
        verletB = b;
        distanceLock = Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position);
    }

    public LinkConstraint(VerletBody a, VerletBody b, float distance)
    {
        verletA = a;
        verletB = b;
        distanceLock = distance;
    }

    public override void solve()
    {
        // Getting positions to shorten references to position
        Vector3 aLocation = verletA.gameObject.transform.position;
        Vector3 bLocation = verletB.gameObject.transform.position;
        // Compute difference
        Vector3 difference = aLocation - bLocation;
        // Compute the multiple for the difference correction
        float correctionLength = (distanceLock - difference.magnitude) / difference.magnitude;
        // Compute the actual correction
        Vector3 correction = difference * 0.5f * correctionLength;
        // Apply the corrections
        verletA.gameObject.transform.position += correction;
        verletB.gameObject.transform.position -= correction;
    }
}
