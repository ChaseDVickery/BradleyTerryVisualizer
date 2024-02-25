using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SettingNumberDisplay : MonoBehaviour
{

    public TMP_Text textbox;

    void Awake() {
        if (textbox == null) textbox = gameObject.GetComponentInChildren<TMP_Text>();
    }

    public void SetVolumeDisplay(float value) {
        textbox.text = value.ToString("F2");
    }
}
