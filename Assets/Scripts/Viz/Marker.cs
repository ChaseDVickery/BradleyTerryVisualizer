using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour {

    public SpriteRenderer spriteRenderer;
    private Color origColor;

    public Vector2 dSpaceLocation {
        get { return visualizer.GToDSpace(new Vector2(transform.position.x, transform.position.y)); }
    }
    public Vector2 gSpaceLocation {
        get { return new Vector2(transform.position.x, transform.position.y); }
    }
    public BTVisualizer visualizer;

    public Color currColor { get{return spriteRenderer.color;} }

    public virtual void UpdateDetails() { return; }
    public virtual void ShowDetails() { return; }
    public virtual void HideDetails() { return; }
    public virtual void Setup() { return; }

    protected virtual void Awake() {
        if (spriteRenderer != null) {
            origColor = spriteRenderer.color;
        }
    }

    public virtual void SetColor(Color newColor) {
        if (spriteRenderer == null) { return; }
        spriteRenderer.color = newColor;
    }
}