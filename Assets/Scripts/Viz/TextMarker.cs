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
    private bool focused = false;

    public override void Setup() {
        textInput.onDeselect.AddListener(OnDeselect);
    }
    void OnDeselect(string text) {
        visualizer.Focus();
        StartCoroutine(DisableInput());
    }
    IEnumerator DisableInput() {
        yield return new WaitForEndOfFrame();
        focused = false;
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
        focused = true;
        textInput.interactable = true;
        textInput.Select();
        // textInput.ActivateInputField();
    }

    private void ChangeFontSize(int amount) {
        textInput.pointSize = Mathf.Max(8, textInput.pointSize+amount);
    }
    void Update() {
        // Accept controls if focused
        if (focused) {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) {
                if (Input.GetKeyDown(KeyCode.Equals)) { ChangeFontSize((5+((int)textInput.pointSize/10))); }
                else if (Input.GetKeyDown(KeyCode.Minus)) { ChangeFontSize(-(5+((int)textInput.pointSize/10))); }
            }
        }
    }
}
