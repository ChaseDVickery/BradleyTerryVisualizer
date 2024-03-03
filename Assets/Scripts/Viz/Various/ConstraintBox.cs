using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ConstraintBox : MonoBehaviour
{
    private LineRenderer lineRenderer;
    public Vector2 bounds;

    void Awake() {
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void UpdateBounds(Vector2 newBounds) { bounds = newBounds; }
    public void UpdateBounds(float newBounds) { bounds = new Vector2(newBounds, newBounds); }
    public void UpdateXBounds(float newBounds) { bounds = new Vector2(newBounds, bounds.y); }
    public void UpdateYBounds(float newBounds) { bounds = new Vector2(bounds.x, newBounds); }
    
    public void CreateBox(Vector3[] points) {
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
    }
    public void CreateBox(Vector3 upLeft, Vector3 upRight, Vector3 botRight, Vector3 botLeft) {
        CreateBox(new Vector3[] { upLeft, upRight, botRight, botLeft});
    }
    public void CreateBox(Vector2 upLeft, Vector2 upRight, Vector2 botRight, Vector2 botLeft) {
        CreateBox((Vector3)upLeft, (Vector3)upRight, (Vector3)botRight, (Vector3)botLeft);
    }
    // Boxes centered at 0,0
    public void CreateBox(float xBounds, float yBounds) {

    }
    public void CreateBox(float boundSize) {
        CreateBox(boundSize, boundSize);
    }

    public void SetShowBox(bool toShow) {
        lineRenderer.enabled = toShow;
    }
    public void ShowBox() {
        lineRenderer.enabled = true;
    }
    public void HideBox() {
        lineRenderer.enabled = false;
    }
}
