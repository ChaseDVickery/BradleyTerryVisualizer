using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class DeltaPoint : MonoBehaviour
{
    public Vector2 dSpaceLocation {
        get { return visualizer.GToDSpace(new Vector2(transform.position.x, transform.position.y)); }
    }
    public bool activeUpdating;
    public BTVisualizer visualizer;

    public TMP_Text detailText;
    private LineRenderer lineRenderer;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateDetails() {
        if (detailText == null) { return; }

        detailText.text = $"({dSpaceLocation.x.ToString("F3")},{dSpaceLocation.y.ToString("F3")})";

        // Get line perpendicular to this location (from origin)
        Vector2 slope = Vector2.Perpendicular(dSpaceLocation).normalized;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[]{
            new Vector3(10*slope.x, 10*slope.y*(visualizer.xRange/visualizer.yRange)),
            new Vector3(-10*slope.x, -10*slope.y*(visualizer.xRange/visualizer.yRange))
        });
    }

    public void ShowDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(true); }
        lineRenderer.enabled = true;
    }
    public void HideDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(false); }
        lineRenderer.enabled = false;
    }
}
