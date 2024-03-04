using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderSnapper : MonoBehaviour
{
    public float snapPrecision = 0.01f;
    private Slider slider;
    private bool changed = false;

    void Awake() {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(SnapValue);
    }

    public void SnapValue(float v) {
        if (changed) {return;}
        slider.value = Mathf.Round(v/snapPrecision)*snapPrecision;
        changed = true;
    }

    void Update() {
        changed = false;
    }
}
