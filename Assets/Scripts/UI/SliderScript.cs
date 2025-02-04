using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

//Reuseable script on sliders, resetting their values to the saved preferences on start.

public class SliderScript : MonoBehaviour {
    public enum Type { Music, SFX, Brightness };
    [SerializeField] private Type type;
    public UnityEvent<float> startValueSet;

    private void Start() {
        float value = 0;
        switch (type) { 
            case Type.Music:
                value = PlayerPrefs.GetFloat("musicVol", 0.5f);
                break;
            case Type.SFX:
                value = PlayerPrefs.GetFloat("sfxVol", 0.5f);
                break;
            case Type.Brightness:
                value = PlayerPrefs.GetFloat("brightness", 1);
                break;
        }
        GetComponent<Slider>().value = value;
        SliderText valueScript = transform.Find("Value Text").GetComponent<SliderText>();
        valueScript.ChangeText(value);
        startValueSet.Invoke(value);
    }
}
