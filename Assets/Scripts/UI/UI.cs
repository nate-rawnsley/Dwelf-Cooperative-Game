using UnityEngine;
using UnityEngine.SceneManagement;

//Controls the behaviour of buttons and sliders in the UI.

public class UI : MonoBehaviour {
    [SerializeField] private bool menu;
    private GameObject deathScreen;

    private void Awake() {
        if (!menu) {
            deathScreen = transform.Find("Death Menu").gameObject;
        }
    }

    public void ChangeLevel(int level) {
        Time.timeScale = 1;
        if (level > PlayerPrefs.GetInt("level", 1)) {
            PlayerPrefs.SetInt("level", level);
        }
        SceneManager.LoadScene(level);
    }

    public void DeathMenu() {
        deathScreen.SetActive(true);
    }

    public void Resume(GameObject target) {
        Time.timeScale = 1;
        target.SetActive(false);
    }

    public void ResetLevel() {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MusicVolume(float value) {
        PlayerPrefs.SetFloat("musicVol", value);
    }

    public void SFXVolume(float value) {
        PlayerPrefs.SetFloat("sfxVol", value);
    }

    public void Brightness(float value) {
        PlayerPrefs.SetFloat("brightness", value);
    }

    public void ResetProgress() {
        PlayerPrefs.SetInt("level", 1);
    }

    public void Exit() {
        Application.Quit();
    }
}
