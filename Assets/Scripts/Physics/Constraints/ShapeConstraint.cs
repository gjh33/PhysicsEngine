using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class ShapeConstraint : Constraint {
    private List<LinkConstraint> links = new List<LinkConstraint>();

    public ShapeConstraint(List<VerletBody> verlets)
    {
        int i, j;
        for (i = 0; i < verlets.Count - 1; i++)
        {
            for (j = i + 1; j < verlets.Count; j++)
            {
                links.Add(new LinkConstraint(verlets[i], verlets[j]));
            }
        }
    }

    public override void solve()
    {
        foreach (LinkConstraint link in links)
        {
            link.solve();
        }
    }
}
