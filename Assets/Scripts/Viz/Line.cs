using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using TMPro;

[RequireComponent(typeof(LineRenderer))]
public class Line : MonoBehaviour
{
    public LineHandle _h1;
    public LineHandle h1 {
        get { return _h1; }
        set {
            _h1 = value;
            _h1.line = this;
        }
    }
    public LineHandle _h2;
    public LineHandle h2 {
        get { return _h2; }
        set {
            _h2 = value;
            _h2.line = this;
        }
    }

    private LineRenderer lineRenderer;

    public float slope {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.dSpaceLocation - h1.dSpaceLocation;
            return diff.x == 0 ? 0f : diff.y / diff.x;
        }
    }
    public float yIntercept {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.dSpaceLocation - h1.dSpaceLocation;
            if (diff.x == 0) { return 0f; }
            return h1.dSpaceLocation[0]/(h1.dSpaceLocation[1]*slope);
        }
    }

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    public void HideLine() { lineRenderer.enabled = false; }
    public void ShowLine() { lineRenderer.enabled = true; }
    
    public void RedrawLine() {
        if (h1 == null || h2 == null) { return; }
        lineRenderer.SetPositions(new Vector3[]{
            h1.transform.position,
            h2.transform.position,
        });
    }
}
