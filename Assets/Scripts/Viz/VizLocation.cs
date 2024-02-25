using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

[RequireComponent(typeof(SpriteRenderer))]
public class VizLocation : MonoBehaviour
{
    private Vector3 size;
    private SpriteRenderer spriteRenderer;
    public Gradient gradient;

    private Color _origColor;

    void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _origColor = spriteRenderer.color;
        Reset();
    }

    public void SetSize(Vector3 newSize) {
        size = newSize;
        transform.localScale = size;
    }

    public void Reset() {
        spriteRenderer.color = _origColor;
    }

    public void SetIntensity(double intensity) {
        spriteRenderer.color = gradient.Evaluate(Mathf.Clamp((float)intensity, 0f, 1f));
    }
}
