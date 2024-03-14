using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class Axes : MonoBehaviour {

    private Vector2 baseRatio;
    private Vector3 baseScale;
    private Vector2 baseShape;
    private RectTransform rt;

    public Axis xAxis;
    public Axis yAxis;

    void Awake() {
        baseScale = transform.localScale;
        rt = GetComponent<RectTransform>();
        baseShape = rt.sizeDelta;
        baseRatio = new Vector2(baseShape.x / baseScale.x, baseShape.y / baseScale.y);
    }

    // Bounds is just the x and y range in global space
    public void SetToBounds(Vector2 bounds) {
        // // We know how many units this spans with width*scalex
        // Vector2 newShape = new Vector2( baseRatio.x * bounds.x, baseRatio.y * bounds.y);
        // // Debug.Log($"{newShape}");
        // rt.sizeDelta = newShape;
        rt.sizeDelta = new Vector2(bounds.x * 1/rt.lossyScale.x, bounds.y * 1/rt.lossyScale.y);
    }

    public void UpdateXAxis(Vector2 range) { xAxis.UpdateAxisRange(range); }
    public void UpdateYAxis(Vector2 range) { yAxis.UpdateAxisRange(range); }
    public void UpdateXAxis(float min, float max) { xAxis.UpdateAxisRange(min, max); }
    public void UpdateYAxis(float min, float max) { yAxis.UpdateAxisRange(min, max); }
}