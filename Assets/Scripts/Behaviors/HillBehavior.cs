using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HillBehavior : MonoBehaviour {
    // Public variables
    public float height = 2.0f; // Height of the mountain
    public float platformWidth = 0.5f; // Width of the top platform
    public float baseWidth = 1.5f; // Width of the base of the platform
    public int detail; // How many levels of recursion
    public float maximumDeviation; // The max deviation from the original slope for any vertex


    // Mesh variables
    private Vector3[] verticies;
    private int[] triangles;
    private Mesh mesh;

	// Use this for initialization
	void Start () {
        //Get variables
        mesh = gameObject.GetComponent<MeshFilter>().mesh;

        //Initialization
        generateVerticies(); // Generate the verticies of our mountain
        generateTriangles(); // Generate the triangles of our mountain
        updateMesh();
	}

    void generateVerticies()
    {
        // Anchor point used to draw all the triangles to. Index 0 of verticies
        Vector3 anchorPoint = new Vector3(0, 0, 1);

        // Basic outline of trapezoid shape for mountain
        Vector3[] basicOutline =
        {
            new Vector3(baseWidth/2, 0, 1), // Bottom right
            new Vector3(-baseWidth/2, 0, 1), // Bottom left
            new Vector3(-platformWidth/2, height, 1), // Top left
            new Vector3(platformWidth/2, height, 1), // Top right
        };

        Vector3[] leftSlope = midpointBisection(detail, basicOutline[1], basicOutline[2], maximumDeviation);
        Vector3[] rightSlope = midpointBisection(detail, basicOutline[3], basicOutline[0], maximumDeviation);

        deviateVerticies(leftSlope, maximumDeviation);
        deviateVerticies(rightSlope, maximumDeviation);

        // Build our final list using leftSlope, rightSlope and the anchor point
        verticies = new Vector3[leftSlope.Length + rightSlope.Length + 1]; // +1 for the anchor point
        verticies[0] = anchorPoint;
        int i;
        for (i = 1; i < verticies.Length; i++) // i=1 because we already have the anchor point at index 0
        {
            if ((i-1) < leftSlope.Length) // i-1 because we're starting at index 1 we wanna copy from index-1
            {
                verticies[i] = leftSlope[i - 1];
            }
            else
            {
                verticies[i] = rightSlope[i - leftSlope.Length - 1]; // -1 because same reason as above
            }
        }
    }

    void generateTriangles()
    {
        // Init triangles
        triangles = new int[(verticies.Length-2) * 3];
        // Our triangle faces will take 2 perimiter vertices and the anchor point to connect to.
        // Anchor point is 0
        int i;
        for (i=1; i < verticies.Length-1; i++) // Start at 1 cause anchor is 0. -2 because we dont need any trianles that include anchor (i.e. anchor -> bottom left -> anchor)
        {
            int trianglesIndex = (i - 1) * 3; // -1 because we start at 1 because of anchor. *3 cause we do 3 at a time

            triangles[trianglesIndex] = i; // Perimeter 1
            triangles[trianglesIndex + 1] = i+1; // Perimeter 2
            triangles[trianglesIndex + 2] = 0; // Anchor point
        }
    }

    void updateMesh()
    {
        mesh.Clear();
        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    // Returns an array of numberOfVerticies verticies from midpoint bisections between a and b
    Vector3[] midpointBisection(int depth, Vector3 a, Vector3 b, float maxDeviation)
    {
        // Recursive case
        if (depth != 0)
        {
            Vector3 line = b - a; // Vector from a to b representing the line we want to bisect
            Vector3 lineNormal = new Vector3(line.y, -line.x, 1); // Normal to the line
            lineNormal.Normalize();

            // Calculate the midpoint
            Vector3 midpoint = a + (Random.Range(0.3f, 0.7f) * line);

            // Recurs on left and right lines
            Vector3[] leftSide = midpointBisection(depth - 1, a, midpoint, maxDeviation);
            Vector3[] rightSide = midpointBisection(depth - 1, midpoint, b, maxDeviation);

            // Combine
            Vector3[] verticiesOut = new Vector3[leftSide.Length + rightSide.Length - 1]; // -1 because both contain the midpoint
            int i;
            for (i=0; i < verticiesOut.Length; i++)
            {
                if (i < leftSide.Length)
                {
                    verticiesOut[i] = leftSide[i];
                }
                else
                {
                    verticiesOut[i] = rightSide[(i - leftSide.Length) + 1]; // +1 to ignore the first element since it duplicates the last
                }
            }

            // Return
            return verticiesOut;

        }
        // Base case
        else
        {
            Vector3[] verticiesOut = { a, b };
            return verticiesOut;
        }
    }

    // Deviates verticies between the first and second vertex by a random amount from the line
    void deviateVerticies(Vector3[] vertexList, float maxDeviation)
    {
        // Get our base line and normal to it
        Vector3 line = vertexList[0] - vertexList[vertexList.Length - 1]; // Vector from a to b representing the line we want to bisect
        Vector3 lineNormal = new Vector3(line.y, -line.x, 0); // Normal to the line
        lineNormal.Normalize();

        // For each vertex displace it a random amount from the line within max deviation
        // Excluding first and last
        int i;
        for (i=1; i < vertexList.Length - 1; i++)
        {
            vertexList[i] += lineNormal * Random.Range(-maxDeviation, maxDeviation);
        }
    }
}
