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

    public float slopeD {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.dSpaceLocation - h1.dSpaceLocation;
            return diff.x == 0 ? 0f : diff.y / diff.x;
        }
    }
    public float yInterceptD {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.dSpaceLocation - h1.dSpaceLocation;
            if (diff.x == 0) { return 0f; }
            return h1.dSpaceLocation[0]/(h1.dSpaceLocation[1]*slopeD);
        }
    }
    public float orthoSlopeD {
        get {
            if (slopeD == 0) { return 0f; }
            return -1f / slopeD;
        }
    }

    public float slopeG {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.gSpaceLocation - h1.gSpaceLocation;
            return diff.x == 0 ? 0f : diff.y / diff.x;
        }
    }
    public float yInterceptG {
        get {
            if (h1 == null || h2 == null) { return 0f; }
            Vector2 diff = h2.gSpaceLocation - h1.gSpaceLocation;
            if (diff.x == 0) { return 0f; }
            return h1.gSpaceLocation[0]/(h1.gSpaceLocation[1]*slopeG);
        }
    }
    public float orthoSlopeG {
        get {
            if (slopeG == 0) { return 0f; }
            return -1f / slopeG;
        }
    }
    
    public float segmentLengthD {
        get { return (h2.dSpaceLocation - h1.dSpaceLocation).magnitude; }
    }
    public float segmentLengthG {
        get { return (h2.gSpaceLocation - h1.gSpaceLocation).magnitude; }
    }
    public Vector2 midpointD {
        get { return h1.dSpaceLocation+((h2.dSpaceLocation - h1.dSpaceLocation)/2); }
    }
    public Vector2 midpointG {
        get { return h1.gSpaceLocation+((h2.gSpaceLocation - h1.gSpaceLocation)/2); }
    }

    public LineHandle Other(LineHandle lh) {
        if (lh == h1) { return h2; }
        else if (lh == h2) { return h1; }
        return null;
    }
    public float SlopeToG(LineHandle endpoint) {
        if (endpoint == h1) { return -slopeG; }
        if (endpoint == h2) { return slopeG; }
        return 0f;
    }
    public float SlopeToD(LineHandle endpoint) {
        if (endpoint == h1) { return -slopeD; }
        if (endpoint == h2) { return slopeD; }
        return 0f;
    }
    public Vector2 DiffToG(LineHandle endpoint) {
        if (endpoint == h1) { return h1.gSpaceLocation - h2.gSpaceLocation; }
        if (endpoint == h2) { return h2.gSpaceLocation - h1.gSpaceLocation; }
        return Vector2.zero;
    }
    public Vector2 DiffToD(LineHandle endpoint) {
        if (endpoint == h1) { return h1.dSpaceLocation - h2.dSpaceLocation; }
        if (endpoint == h2) { return h2.dSpaceLocation - h1.dSpaceLocation; }
        return Vector2.zero;
    }

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
    }

    public void SetColor(Color c) {
        lineRenderer.startColor = c;
        lineRenderer.endColor = c;
    }

    public void HideLine() { lineRenderer.enabled = false; }
    public void ShowLine() { lineRenderer.enabled = true; }

    // Creates n samples uniformly spaced between the two endpoints (including endpoints)
    public Vector2[] SampleDSpace(int nSamples) {
        Vector2 startPoint = h1.dSpaceLocation;
        Vector2 endPoint = h2.dSpaceLocation;
        if (nSamples <= 2) { return new Vector2[2]{startPoint, endPoint}; }
        Vector2[] points = new Vector2[nSamples];
        float denom = (float)nSamples;
        for (int i = 0; i < nSamples; i++) {
            points[i] = new Vector2(
                Mathf.Lerp(startPoint.x, endPoint.x, i/(denom-1)),
                Mathf.Lerp(startPoint.y, endPoint.y, i/(denom-1))
            );
        }
        return points;
    }
    public Matrix<double> SampleDSpaceMatrix(int nSamples) {
        Vector2 startPoint = h1.dSpaceLocation;
        Vector2 endPoint = h2.dSpaceLocation;
        // Made in column-major order
        if (nSamples <= 2) { return Matrix<double>.Build.Dense(2, 2, new double[]{startPoint.x, endPoint.x, startPoint.y, endPoint.y}); }
        double[] pointData = new double[2*nSamples];
        float denom = (float)nSamples;
        // Fill first 'column' with x
        for (int i = 0; i < nSamples; i++) {
            pointData[i] = (double)Mathf.Lerp(startPoint.x, endPoint.x, i/(denom-1));
        }
        // Fill second 'column' with y
        for (int i = 0; i < nSamples; i++) {
            pointData[nSamples+i] = (double)Mathf.Lerp(startPoint.y, endPoint.y, i/(denom-1));
        }
        Matrix<double> points = Matrix<double>.Build.Dense(nSamples, 2, pointData);
        return points;
    }
    
    public void RedrawLine() {
        if (h1 == null || h2 == null) { return; }
        lineRenderer.SetPositions(new Vector3[]{
            h1.transform.position,
            h2.transform.position,
        });
    }
}
