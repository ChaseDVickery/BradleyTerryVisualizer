using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using TMPro;

public class LineHandle : Marker
{
    public TMP_Text detailText;
    public Line line;
    
    public override void UpdateDetails() {
         if (detailText == null) { return; }
        detailText.text = $"({dSpaceLocation.x.ToString("F3")},{dSpaceLocation.y.ToString("F3")})";
        line.RedrawLine();
    }
    public override void ShowDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(true); }
        line.ShowLine();
    }
    public override void HideDetails() {
        if (detailText != null) { detailText.gameObject.SetActive(false); }
        // line.HideLine();
    }
}
