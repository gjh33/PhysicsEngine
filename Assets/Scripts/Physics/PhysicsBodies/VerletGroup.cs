using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// WARNING: Any child objects without a VerletBody will be removed
public class VerletGroup : MonoBehaviour {
    public int constraintStrength = 10;

    public List<VerletBody> verlets = new List<VerletBody>(); // Our verlets contained within the body
    private List<Constraint> constraints = new List<Constraint>(); // List of constraints

	// Use this for initialization
	void Awake() {
        // Automatically grab all the verlets in the group
		foreach (Transform child in gameObject.transform)
        {
            VerletBody verlet = child.gameObject.GetComponent<VerletBody>();
            if (verlet)
            {
                verlets.Add(verlet);
            }
            else
            {
                child.SetParent(null);
            }
        }

        // Register with the physics system
        PhysicsSystem.register(gameObject);
	}

    // Resolves the constraints of the group
    public void resolveConstraints()
    {
        int i;
        for (i=0; i < constraintStrength; i++)
        {
            foreach(Constraint constraint in constraints)
            {
                constraint.solve();
            }
        }
    }

    // Adds a constraint
    public void addConstraint(Constraint constraint)
    {
        constraints.Add(constraint);
    }

    // Sets velocity of a subset of 5 points
    public void setVelocity(Vector3 vel)
    {
        int i;
        for (i=0; i<5; i++)
        {
            verlets[i].velocity = vel;
        }
    }
}
