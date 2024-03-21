using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(RectTransform))]
public class LinePlotter : MonoBehaviour
{
    public int samples = 100;
    public bool normalizedScale = false;
    public bool rescale = false;
    public float sizeRescaleRatio = 1f;
    public Vector2 minSize = Vector2.one;
    public Vector2 offset = Vector2.zero;
    public Axes axes;

    private float scaleUpX {
        get { return (gSize.x * 1/rt.localScale.x); }
    }
    private float scaleUpY {
        get { return (gSize.y * 1/rt.localScale.y); }
    }
    public Vector2 gSize {
        get { return new Vector2(rt.rect.width, rt.rect.height); }
    }
    private float minX = -5;
    private float maxX = 5;

    private LineRenderer lineRenderer;
    public RectTransform rt;

    public float width { get => rt.rect.width; }
    public float height { get => rt.rect.height; }

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
        rt = GetComponent<RectTransform>();
        HideDetails();
    }

    public void SetNormalizedScale(bool toThis) {
        normalizedScale = toThis;
    }
    public void SetRescale(bool toThis) {
        rescale = toThis;
    }
    public void SetRescaleRatio(float toThis) {
        sizeRescaleRatio = toThis;
    }
    public void SetSamples(float toThis) {
        SetSamples((int)Mathf.Round(toThis));
    }
    public void SetSamples(int toThis) {
        samples = toThis;
    }

    public void ShowDetails() {
        axes.gameObject.SetActive(true);
        lineRenderer.enabled = true;
    }

    public void HideDetails() {
        axes.gameObject.SetActive(false);
        lineRenderer.enabled = false;
    }

    public void Plot(float[] values) {
        Vector3[] positions = new Vector3[values.Length];
        for (int i = 0; i < values.Length; i++) {
            positions[i] = new Vector3(
                scaleUpX*Mathf.Lerp(-gSize.x/(2*width), gSize.x/(2*width), ((float)i)/(values.Length-1)),
                scaleUpY*(values[i] -gSize.y/(2*height)),
                0f
            );
        }
        Plot(positions);
    }

    public void Plot(Vector3[] positions) {
        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }

    public void PlotZeroes() {
        Plot(new Vector3[] {
            new Vector3(minX, 0f, 0f),
            new Vector3(maxX, 0f, 0f)
        });
    }
}
