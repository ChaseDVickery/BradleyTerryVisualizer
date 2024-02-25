using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class ScaleBar : MonoBehaviour {
    public Image barImage;
    public TMP_Text minText;
    public TMP_Text maxText;

    public bool normalized = false;

    public void UpdateScale(Gradient gradient, double min, double max) {
        if (normalized) { min = 0.0; max = 1.0; } 
        if (Mathf.Abs((float)min) >= 0.01 || min == 0.0) { minText.text = min.ToString("F3"); }
        else { minText.text = min.ToString("E2"); }
        if (Mathf.Abs((float)max) >= 0.01) { maxText.text = max.ToString("F3"); }
        else { maxText.text = max.ToString("E2");}
    }
}