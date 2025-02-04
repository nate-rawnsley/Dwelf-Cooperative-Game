using UnityEngine;
using UnityEngine.UI;

//Controls the black overlay on the screen, that changes opacity based on the brightness setting.

public class BrightnessOverlay : MonoBehaviour {
    private Image image;

    private void Awake() {
        image = GetComponent<Image>();
        BrightnessUpdate();
    }

    public void BrightnessUpdate() {
        Color tempColour = GetComponent<Image>().color;
        tempColour.a = 1 - PlayerPrefs.GetFloat("brightness", 1);
        GetComponent<Image>().color = tempColour;
    }
}
