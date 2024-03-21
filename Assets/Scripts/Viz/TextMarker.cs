using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Distributions;

using TMPro;

public class TextMarker : Marker
{
    public TMP_InputField textInput;

    public override void Setup() {
        textInput.onDeselect.AddListener(OnDeselect);
    }
    void OnDeselect(string text) {
        visualizer.Focus();
        StartCoroutine(DisableInput());
    }
    IEnumerator DisableInput() {
        yield return new WaitForEndOfFrame();
        textInput.interactable = false;
    }
    
    public override void UpdateDetails() {
        return;
    }
    public override void ShowDetails() {
        if (textInput != null) { textInput.gameObject.SetActive(true); }
    }
    public override void HideDetails() {
        // if (textInput != null) { textInput.gameObject.SetActive(false); }
    }

    public void Focus() {
        textInput.interactable = true;
        textInput.Select();
        // textInput.ActivateInputField();
    }
}
