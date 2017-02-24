using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinConstraint : Constraint {
    private VerletBody verlet; // The verlet
    private Vector3 pinPosition; // The position

    public PinConstraint(VerletBody verletBody)
    {
        verlet = verletBody;
        pinPosition = verletBody.gameObject.transform.position;
    }

    public PinConstraint(VerletBody verletBody, Vector3 position)
    {
        verlet = verletBody;
        pinPosition = position;
    }

    public override void solve()
    {
        verlet.gameObject.transform.position = pinPosition;
    }
}
