using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingNumberDisplay : MonoBehaviour
{

    public TMP_Text textbox;
    public int precision = 2;

    void Awake() {
        if (textbox == null) textbox = gameObject.GetComponentInChildren<TMP_Text>();
    }

    public void SetVolumeDisplay(float value) {
        textbox.text = value.ToString($"F{precision}");
    }
}
