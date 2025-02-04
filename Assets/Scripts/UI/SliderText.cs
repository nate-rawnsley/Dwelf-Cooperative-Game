using UnityEngine;
using TMPro;

//Reuseable script on text associated with slider values.
//Called by events, changes the text to the value of the slider when updated.

public class SliderText : MonoBehaviour {
    [SerializeField] private bool usePercent;
    [SerializeField] private float divideBy;
    [SerializeField] private int decimalPlaces;
    private TextMeshProUGUI textMesh;

    private void Awake() {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void ChangeText(float value) {
        float displayVal = value / divideBy;
        string displayString = displayVal.ToString("n" + decimalPlaces.ToString());
        displayString += usePercent ? "%" : "";
        textMesh.text = displayString;
    }
}
