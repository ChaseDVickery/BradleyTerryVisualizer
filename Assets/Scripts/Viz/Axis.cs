using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class Axis : MonoBehaviour {
    public TMP_Text minText;
    public TMP_Text maxText;

    public void UpdateAxisRange(float min, float max) {
        if (Mathf.Abs(min) >= 0.01) { minText.text = min.ToString("F3"); }
        else { minText.text = min.ToString("E2"); }
        if (Mathf.Abs(max) >= 0.01) { maxText.text = max.ToString("F3"); }
        else { maxText.text = max.ToString("E2");}
    }

    public void UpdateAxisRange(Vector2 range) {
        UpdateAxisRange(range.x, range.y);
    }
}