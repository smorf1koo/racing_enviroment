using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public void LoadSelectedTrack(string trackSceneName)
    {
        SceneManager.LoadScene(trackSceneName); 
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }
    // Start is called before the first frame update
    private void Start()
    {
        // optionsScreen.SetActive(false);
        // optionsButton.gameObject.SetActive(true); 
        
        // optionsButton.onClick.AddListener(ShowOptions); 
        // backButton.onClick.AddListener(OnBackPressed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
