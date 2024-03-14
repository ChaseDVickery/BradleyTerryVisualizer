using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Axis : MonoBehaviour {
    public TMP_Text minText;
    public TMP_Text maxText;
    public bool allowExpPrecision = true;
    public int fPrecision = 3;
    public int ePrecision = 2;

    public void UpdateAxisRange(float min, float max) {
        if (Mathf.Abs(min) >= 0.01 || !allowExpPrecision) { minText.text = min.ToString("F"+fPrecision); }
        else { minText.text = min.ToString("E"+ePrecision); }
        if (Mathf.Abs(max) >= 0.01 || !allowExpPrecision) { maxText.text = max.ToString("F"+fPrecision); }
        else { maxText.text = max.ToString("E"+ePrecision);}
    }

    public void UpdateAxisRange(Vector2 range) {
        UpdateAxisRange(range.x, range.y);
    }
}