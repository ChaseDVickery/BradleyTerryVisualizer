using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using TMPro;

// [RequireComponent(typeof(Collider2D))]
public class DeltaPoint : MonoBehaviour
{
    public Vector2 dSpaceLocation {
        get { return visualizer.GToDSpace(new Vector2(transform.position.x, transform.position.y)); }
    }
    public bool activeUpdating;
    public BTVisualizer visualizer;

    public TMP_Text detailText;

    public void UpdateDetails() {
        if (detailText == null) { return; }

        detailText.text = $"({dSpaceLocation.x.ToString("F3")},{dSpaceLocation.y.ToString("F3")})";
    }

    public void ShowDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(true); }
    }
    public void HideDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(false); }
    }
}
