using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker : MonoBehaviour {
    public Vector2 dSpaceLocation {
        get { return visualizer.GToDSpace(new Vector2(transform.position.x, transform.position.y)); }
    }
    public Vector2 gSpaceLocation {
        get { return new Vector2(transform.position.x, transform.position.y); }
    }
    public BTVisualizer visualizer;

    public virtual void UpdateDetails() { return; }
    public virtual void ShowDetails() { return; }
    public virtual void HideDetails() { return; }
    public virtual void Setup() { return; }
}