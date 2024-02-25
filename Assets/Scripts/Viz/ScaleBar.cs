using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class ScaleBar : MonoBehaviour {
    public Image barImage;
    public TMP_Text minText;
    public TMP_Text maxText;

    public void UpdateScale(Gradient gradient, double min, double max) {
        if (min >= 0.01) { minText.text = min.ToString("F3"); }
        else { minText.text = min.ToString("E2"); }
        if (max >= 0.01) { maxText.text = max.ToString("F3"); }
        else { maxText.text = max.ToString("E2");}
    }
}