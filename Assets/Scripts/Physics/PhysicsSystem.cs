using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Physics system. Numbers are handled in meters per second assuming level is 1km wide
public class PhysicsSystem : MonoBehaviour {
    // Conversion constants
    const double METERS_TO_UNITS = 18.0f / 1000.0f;
    const double UNITS_TO_METERS = 1000.0f / 18.0f;

    // Public variables for our physics system
    public float gravity = 9.8f; // Gravity in m/s^2
    public float wind = 5; // In m/s
    public bool windIndicator = true;
    [HideInInspector] public float activeWind = 0;

    // Tracks registered GameObjects in our physics system
    private static List<GameObject> registered = new List<GameObject>();
    // Tracks registered verlet groups in our physics system
    private static List<GameObject> verletGroups = new List<GameObject>();
    // Wind timer
    private float timeLeftToWindChange = 0.5f;

    // Conversion between unity coords and meters
    public static float toUnityUnits(float meters)
    {
        return (float)(meters * METERS_TO_UNITS);
    }
    public static double toUnityUnits(double meters)
    {
        return (meters * METERS_TO_UNITS);
    }
    public static Vector3 toUnityUnits(Vector3 meters)
    {
        return (meters * (float)METERS_TO_UNITS);
    }
    public static float toMeters(float units)
    {
        return (float)(units * UNITS_TO_METERS);
    }
    public static double toMeters(double units)
    {
        return (units * UNITS_TO_METERS);
    }
    public static Vector3 toMeters(Vector3 units)
    {
        return (units * (float)UNITS_TO_METERS);
    }

    // Registers a gameobject with the system
    public static void register(GameObject member)
    {
        if (member.GetComponent<PhysicsBody>())
            registered.Add(member);
        else if (member.GetComponent<VerletGroup>())
            verletGroups.Add(member);
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        UpdateRegisteredMembers(); // Remove any members that have been destroyed since the last update
        UpdateWind();
        ApplyEnvironmentalForces(); // Apply environmental forces
        UpdateRigidBodies(); // Update rigid body positions
        ResolveVerletConstraints(); // Resolve any constraints
        ResolveCollisions(); // Detect and resolve any collisions
	}

    // Scans the list of registered members and removes any that have been destroyed
    private void UpdateRegisteredMembers()
    {
        foreach (GameObject member in registered.ToArray())
        {
            if (member == null)
            {
                registered.Remove(member);
            }
        }
        foreach (GameObject member in verletGroups.ToArray())
        {
            if (member == null)
            {
                verletGroups.Remove(member);
            }
        }
    }

    // Update the wind if 0.5s has passed since last update
    private void UpdateWind()
    {
        timeLeftToWindChange -= Time.deltaTime;
        if (timeLeftToWindChange < 0)
        {
            activeWind = Random.Range(-wind, wind);
            timeLeftToWindChange = 0.5f;
            GameObject windIndicatorGO = gameObject.transform.Find("WindIndicator").gameObject;
            if (windIndicator)
            {
                // MAGIC NUMBER: 2 is the max arrow length
                float arrowLength = 2 * (Mathf.Abs(activeWind) / wind); // Max length of 2 times the % of max wind
                // Draw the arrow
                LineRenderer lr = windIndicatorGO.GetComponent<LineRenderer>();
                Vector3[] pos =
                {
                    Vector3.zero,
                    new Vector3(arrowLength, 0, 0),
                    new Vector3(arrowLength, -0.4f, 0),
                    new Vector3(arrowLength + 0.5f, 0, 0),
                    new Vector3(arrowLength, 0.4f, 0)
                };
                lr.SetPositions(pos);
                if (activeWind < 0)
                {
                    windIndicatorGO.transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                {
                    windIndicatorGO.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }

    // Applies environment forces like gravity and air resistance
    // Wind is not applied because of assignment specs. Specific objects shouldn't be blown backward
    // because of this BS I have to write the code on the individual objects
    private void ApplyEnvironmentalForces()
    {
        foreach (GameObject member in registered)
        {
            PhysicsBody rb;
            if (rb = member.GetComponent<PhysicsBody>())
            {
                // Calculate and apply air resistance
                // F = -kv^2
                // Assuming unit mass -> delta V = F s
                // We use an assistVector to preserve direction when squaring. This is just the absolute of velocty
                Vector3 assistVector = new Vector3(Mathf.Abs(rb.velocity.x), Mathf.Abs(rb.velocity.y), Mathf.Abs(rb.velocity.z));
                Vector3 airResist = -rb.dragCoefficient * Vector3.Scale(rb.velocity, assistVector);
                Vector3 airResistVector = airResist * Time.fixedDeltaTime;
                // Apply gravity
                Vector3 gravityVector = new Vector3(0, -gravity * Time.fixedDeltaTime, 0);
                rb.velocity += gravityVector + airResistVector;
            }
        }
    }

    // Calls the PhysicsUpdate method of all registered members
    private void UpdateRigidBodies()
    {
        foreach(GameObject member in registered)
        {
            PhysicsBody rb;
            if (rb = member.GetComponent<PhysicsBody>())
            {
                rb.PhysicsUpdate();
            }
        }
    }

    // Resolves all the verlet constraints
    private void ResolveVerletConstraints()
    {
        foreach(GameObject member in verletGroups)
        {
            member.GetComponent<VerletGroup>().resolveConstraints();
        }
    }

    // Check each unique pair for collisions
    private void ResolveCollisions()
    {
        int i, j;
        for (i = 0; i < registered.Count - 1; i++)
        {
            for (j = i + 1; j < registered.Count; j++)
            {
                testForCollision(registered[i], registered[j]);
            }
        }
    }

    // Check if 2 objects have a collider and if yes dispatch to the correct collision method
    private void testForCollision(GameObject a, GameObject b)
    {
        CustomCollider aCollider = a.GetComponent<CustomCollider>();
        CustomCollider bCollider = b.GetComponent<CustomCollider>();

        if(aCollider && bCollider)
        {
            if (aCollider is CustomBoxCollider && bCollider is CustomCircleCollider)
            {
                Circ2BoxTest(bCollider as CustomCircleCollider, aCollider as CustomBoxCollider);
            }
            else if (aCollider is CustomCircleCollider && bCollider is CustomBoxCollider)
            {
                Circ2BoxTest(aCollider as CustomCircleCollider, bCollider as CustomBoxCollider);
            }
            else if (aCollider is CustomCircleCollider && bCollider is FlatMeshCollider)
            {
                Circ2Mesh(aCollider as CustomCircleCollider, bCollider as FlatMeshCollider);
            }
            else if (aCollider is FlatMeshCollider && bCollider is CustomCircleCollider)
            {
                Circ2Mesh(bCollider as CustomCircleCollider, aCollider as FlatMeshCollider);
            }
            else if (aCollider is CustomCircleCollider && bCollider is CustomCircleCollider)
            {
                Circ2Circ(aCollider as CustomCircleCollider, bCollider as CustomCircleCollider);
            }
        }
    }

    // Collisions handling for circle to box collider
    // NOTE: this is semi-naive. There is 1 very rare case where it is not perfect but close to perfect
    // If you read this code, the case is a shallow angle moving slightly upwards hitting the bottom left side
    // at a speed such that mvt is for the bottom edge even though we hit the left edge first.
    private void Circ2BoxTest(CustomCircleCollider a, CustomBoxCollider b) {
        // NOTE: It's simple to add rotation by changing circle's x,y but it's not needed for this assign so I'm skipping it
        //Test for the collision
        Vector2 closestPoint = new Vector2(0, 0); // Closest point on the box
        Vector3 circlePos = a.gameObject.transform.position; // Position of the circle

        // Get the max and min values of the box
        float bTop = b.gameObject.transform.position.y + (b.getHeight() / 2);
        float bBottom = b.gameObject.transform.position.y - (b.getHeight() / 2);
        float bRight = b.gameObject.transform.position.x + (b.getWidth() / 2);
        float bLeft = b.gameObject.transform.position.x - (b.getWidth() / 2);

        // Calculate the closest point on the box to the circle pos
        if (circlePos.x < bLeft)
        {
            closestPoint.x = bLeft;
        }
        else if (circlePos.x > bRight)
        {
            closestPoint.x = bRight;
        }
        else
        {
            closestPoint.x = circlePos.x;
        }
        if (circlePos.y < bBottom)
        {
            closestPoint.y = bBottom;
        }
        else if (circlePos.y > bTop)
        {
            closestPoint.y = bTop;
        }
        else
        {
            closestPoint.y = circlePos.y;
        }
        // Calculate the distance between the circle and closest point
        float centerDistance = Vector2.Distance(closestPoint, circlePos);

        // If distance < radius then collision!!!
        if (centerDistance < a.radius)
        {
            // Minimum translation vector
            Vector3 mtv = new Vector3(0, 0, 0);
            // NOTE: mtv will always be x or y not a combination of both. Just check in opposite direction of motion
            float yDistance; // Distance to translate the ball out vertically
            float xDistance; // Distance to translate the ball out horizontally
            Vector3 velRelToB = a.getPhysicsBody().velocity - b.getPhysicsBody().velocity; // Velocity of the circle relative to square
            // NOTE: we choose relative to the square because if we were doing OBB we rotated the circle relative to square being at the origin
            // If relative y velocity is negative then the circle is coming down onto the square, we should look to resolve with bTop
            if (velRelToB.y <= 0)
            {
                yDistance = (bTop - circlePos.y) + a.radius; // Pop the circle up radius + penetration
            }
            // Otherwise resolve with bBottom
            else
            {
                yDistance = (bBottom - circlePos.y) - a.radius; // Pop the circle down radius + penetration
            }
            // Same idea but for x
            if (velRelToB.x <= 0)
            {
                xDistance = (bRight - circlePos.x) + a.radius; // Pop the circle right radius + penetration
            }
            else
            {
                xDistance = (bLeft - circlePos.x) - a.radius;
            }

            // Compare which is smaller and set mtv accordingly
            if (Mathf.Abs(yDistance) <= Mathf.Abs(xDistance))
            {
                mtv.y = yDistance; // yDistance is already the correct sign from before
            }
            else
            {
                mtv.x = xDistance; // xDistance is already the correct sign from before
            }

            float aPortion; // How much of the correction a is responsible for
            float bPortion; // How much of the correction b is responsible for
            // If a is kinematic but b is not, b gets all the correction
            if (a.getPhysicsBody().isKinematic && !b.getPhysicsBody().isKinematic)
            {
                aPortion = 0;
                bPortion = 1;
            }
            // If a is not kinematic but b is, a gets all the corrections
            else if (!a.getPhysicsBody().isKinematic && b.getPhysicsBody().isKinematic)
            {
                aPortion = 1;
                bPortion = 0;
            }
            // If neither are kinematic then they share based on mass
            // NOTE: masses are 1 in this simulation so they each take 1/2
            else if (!a.getPhysicsBody().isKinematic && !b.getPhysicsBody().isKinematic)
            {
                aPortion = 0.5f;
                bPortion = 0.5f;
            }
            // Otherwise they are both kinematic and do not have any correction
            else
            {
                aPortion = 0;
                bPortion = 0;
            }

            // Move the object double the min penetration vector (where it WOULD have been)
            // For each object only move it's portion
            a.gameObject.transform.position += (mtv + (mtv * (a.bounciness * b.bounciness))) * aPortion;
            b.gameObject.transform.position += -(mtv + (mtv * (a.bounciness * b.bounciness))) * bPortion;

            Vector3 collisionNormal = mtv.normalized; // In this case the normal is the normalized form of mtv (if mtv is up we hit the top face first etc)
            // Imagine the view from the square (ball takes responsibility)
            // We can find a perfect bounce by reflecting over the collision normal and multiply the collision normal component by bounciness
            // This is what would happen if the ball took full responsibility
            // We can then extract the change in velocity by subtracting the old and reflected
            // Now we distribute this change according to our previous proportions instead of JUST the ball
            // We also multiply the change by bounciness (0 = energy not conserved, 1 = energy conserved perfect bounce)
            Vector3 perfectBounce = Vector3.Reflect(a.getPhysicsBody().velocity, collisionNormal); // The perfect bounce
            // This line takes some explaining
            // First we want to get the component of our perfect bounce in the direction of the normal. I.e. projection
            // Then we want to scale that portion of the bounce to the correct size with bounciness
            // Then we subtract 1-correct from the original to get the correct components
            perfectBounce = perfectBounce - (Vector3.Project(perfectBounce, collisionNormal) * (1 - (a.bounciness * b.bounciness)));
            Vector3 difference = perfectBounce - a.getPhysicsBody().velocity; // The difference between this and the old velocity
            a.getPhysicsBody().velocity += difference * aPortion; // Adjust according to it's portion
            b.getPhysicsBody().velocity -= difference * bPortion; // Adjust opposite according to it's portion

            // Call OnCollisionDetected(GameObject other) on any scripts that have it
            MonoBehaviour[] aScripts = a.gameObject.GetComponents<MonoBehaviour>();
            MonoBehaviour[] bScripts = b.gameObject.GetComponents<MonoBehaviour>();
            foreach(MonoBehaviour script in aScripts)
            {
                if (script.GetType().GetMethod("OnCollisionDetected") != null)
                {
                    script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { b.gameObject });
                }
            }
            foreach (MonoBehaviour script in bScripts)
            {
                if (script.GetType().GetMethod("OnCollisionDetected") != null)
                {
                    script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { a.gameObject });
                }
            }
        }
    }

    // Collision handling for a circle and mesh collider
    private void Circ2Mesh(CustomCircleCollider a, FlatMeshCollider b)
    {
        // The strategy here is a naive one-sided circle to line test for each line
        // Side is determined using cross product with z (right hand rule)
        // This is naive because if more than one collision exists we are simply solving in arbitrary order
        // The smart thing would be to find out which line was hit first and solve in that order
        // However in discrete physics this is very very very difficult to do
        // NOTE: If you're marking this and know how to do it, I'd be very interested in knowing

        Mesh bMesh = b.gameObject.GetComponent<MeshFilter>().mesh; // Mesh on b
        Vector3 circlePos = a.gameObject.transform.position;
        circlePos.z = 0; // Ignore depth

        // NOTE: We assume the verticies form a hull. If doing a general case for meshes we should use triangles instead of verticies to get our lines
        // and use a hashset to store each line so there's no duplicates
        int i;
        for(i=0; i < bMesh.vertices.Length; i++)
        {
            // Get the next two endpoints
            Vector3 vertA = bMesh.vertices[i];
            Vector3 vertB = bMesh.vertices[(i + 1) % bMesh.vertices.Length];
            // Translate into world space
            vertA = b.gameObject.transform.TransformPoint(vertA);
            vertB = b.gameObject.transform.TransformPoint(vertB);

            // Ignore depth
            vertA.z = 0;
            vertB.z = 0;

            Vector3 lineDir = vertB - vertA; // The vector representing the direction of the line
            Vector3 aToCircle = circlePos - vertA; // The vector from A to the circle
            float projectionLength = Vector3.Dot(aToCircle, lineDir/lineDir.magnitude); // Project onto the line then take the size. This is to help with edge cases
            Vector3 closest; // The closest point on the line to the circle
            // If we are left of the line use point A
            if (projectionLength < 0)
            {
                closest = vertA;
            }
            // If we are right of the line use point B
            else if (projectionLength > lineDir.magnitude)
            {
                closest = vertB;
            }
            // Otherwise find the point based on the projection length
            else
            {
                closest = vertA + (projectionLength * lineDir.normalized);
            }
            Vector3 point2Circ = circlePos - closest;
            Vector3 surfaceNormal = Vector3.Cross(Vector3.forward, lineDir).normalized; // Cross product since we are doing 1 direction line test
            Vector3 relativeVelocity = a.getPhysicsBody().velocity - b.getPhysicsBody().velocity;

            // If the distance is less than radius we have a collision
            // If the velocity of the object is towards the normal, we have a collision from the direction we want
            if (point2Circ.magnitude < a.radius && (Vector3.Dot(relativeVelocity, surfaceNormal) < 0))
            {
                Vector3 collisionNormal = surfaceNormal; // In this case they are the same
                // Min translation vector. Move center to closest point then correct radius
                Vector3 mtv = (collisionNormal * a.radius) - point2Circ;

                float aPortion; // How much of the correction a is responsible for
                float bPortion; // How much of the correction b is responsible for

                // If a is kinematic but b is not, b gets all the correction
                if (a.getPhysicsBody().isKinematic && !b.getPhysicsBody().isKinematic)
                {
                    aPortion = 0;
                    bPortion = 1;
                }
                // If a is not kinematic but b is, a gets all the corrections
                else if (!a.getPhysicsBody().isKinematic && b.getPhysicsBody().isKinematic)
                {
                    aPortion = 1;
                    bPortion = 0;
                }
                // If neither are kinematic then they share based on mass
                // NOTE: masses are 1 in this simulation so they each take 1/2
                else if (!a.getPhysicsBody().isKinematic && !b.getPhysicsBody().isKinematic)
                {
                    aPortion = 0.5f;
                    bPortion = 0.5f;
                }
                // Otherwise they are both kinematic and do not have any correction
                else
                {
                    aPortion = 0;
                    bPortion = 0;
                }

                // Move the object double the min penetration vector (where it WOULD have been)
                // For each object only move it's portion
                // Also factor in bounciness
                a.gameObject.transform.position += (mtv + (mtv * (a.bounciness * b.bounciness))) * aPortion;
                b.gameObject.transform.position += -(mtv + (mtv * (a.bounciness * b.bounciness))) * bPortion;

                // Imagine the view from the line (ball takes responsibility)
                // We can find a perfect bounce by reflecting over the collision normal and multiply the collision normal component by bounciness
                // This is what would happen if the ball took full responsibility
                // We can then extract the change in velocity by subtracting the old and reflected
                // Now we distribute this change according to our previous proportions instead of JUST the ball
                // We also multiply the change by bounciness (0 = energy not conserved, 1 = energy conserved perfect bounce)
                Vector3 perfectBounce = Vector3.Reflect(a.getPhysicsBody().velocity, collisionNormal); // The perfect bounce
                // This line takes some explaining
                // First we want to get the component of our perfect bounce in the direction of the normal. I.e. projection
                // Then we want to scale that portion of the bounce to the correct size with bounciness
                // Then we subtract 1-correct from the original to get the correct components
                perfectBounce = perfectBounce - (Vector3.Project(perfectBounce, collisionNormal) * (1 - (a.bounciness * b.bounciness)));
                Vector3 difference = perfectBounce - a.getPhysicsBody().velocity; // The difference between this and the old velocity
                a.getPhysicsBody().velocity += difference * aPortion; // Adjust according to it's portion
                b.getPhysicsBody().velocity -= difference * bPortion; // Adjust opposite according to it's portion

                // Call OnCollisionDetected(GameObject other) on any scripts that have it
                MonoBehaviour[] aScripts = a.gameObject.GetComponents<MonoBehaviour>();
                MonoBehaviour[] bScripts = b.gameObject.GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour script in aScripts)
                {
                    if (script.GetType().GetMethod("OnCollisionDetected") != null)
                    {
                        script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { b.gameObject });
                    }
                }
                foreach (MonoBehaviour script in bScripts)
                {
                    if (script.GetType().GetMethod("OnCollisionDetected") != null)
                    {
                        script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { a.gameObject });
                    }
                }
            }
        }
    }

    // This is for verlet -> circle hit
    // Not going to resolve collision because then cannonballs will also collide
    // SO technically this is incomplete but I would need a refactor to allow ignoring of collisions
    // I know how but it's not worth the time
    // I just wanna be done
    // Let me off mr.bones wild ride
    private void Circ2Circ(CustomCircleCollider a, CustomCircleCollider b)
    {
        Vector3 distance = a.gameObject.transform.position - b.gameObject.transform.position;
        // Ignore z
        distance.z = 0;

        // Check if distance between them is less than the sum of their radii
        if (distance.magnitude < (a.radius + b.radius))
        {
            // Call OnCollisionDetected(GameObject other) on any scripts that have it
            MonoBehaviour[] aScripts = a.gameObject.GetComponents<MonoBehaviour>();
            MonoBehaviour[] bScripts = b.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in aScripts)
            {
                if (script.GetType().GetMethod("OnCollisionDetected") != null)
                {
                    script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { b.gameObject });
                }
            }
            foreach (MonoBehaviour script in bScripts)
            {
                if (script.GetType().GetMethod("OnCollisionDetected") != null)
                {
                    script.GetType().GetMethod("OnCollisionDetected").Invoke(script, new object[] { a.gameObject });
                }
            }
        }
    }
}
