using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VerletGroup))]
[RequireComponent(typeof(LineRenderer))]
// Behavior class for a goat
public class GoatBehavior : MonoBehaviour {
    VerletGroup vg;

    // Use this for initialization
    void Start () {
        vg = gameObject.GetComponent<VerletGroup>();
        ShapeConstraint sc = new ShapeConstraint(vg.verlets);
        vg.addConstraint(sc);
	}

    // Destroy object if out of bounds and draw the goat
    public void Update()
    {
        if (gameObject.transform.position.x < -10 || gameObject.transform.position.x > 10 || gameObject.transform.position.y < -10)
        {
            Destroy(gameObject);
        }
        draw();
    }

    // For each verlet add a point to the line renderer
    private void draw()
    {
        // Enumerate the verlet positions
        List<Vector3> points = new List<Vector3>();
        foreach (VerletBody vb in vg.verlets)
        {
            points.Add(vb.gameObject.transform.position);
        }
        // Remove the eye which is the last one
        points.Remove(points[points.Count - 1]);
        // Add the first point as the last spot
        points.Add(vg.verlets[0].gameObject.transform.position);
        // Draw
        gameObject.GetComponent<LineRenderer>().numPositions = points.Count;
        gameObject.GetComponent<LineRenderer>().SetPositions(points.ToArray());
    }
}
