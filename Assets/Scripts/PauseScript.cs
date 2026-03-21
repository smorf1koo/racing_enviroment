using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseScript : MonoBehaviour
{
    public GameObject pauseMenu;
    private bool stopped = false;

    [SerializeField] private List<Animator> menuAnimators;
    public void Continue()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        stopped = false;
    }
    public void Stop()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        stopped = true;
    }
    public void Quit()
    {
        
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
        #endif
    }
    public void StartMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
    // Start is called before the first frame update
    void Start()
    {
        if (menuAnimators != null && menuAnimators.Count > 0)
        {
            foreach (var animator in menuAnimators)
            {
                if (animator != null)
                {
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (stopped) Continue(); else Stop();
        }
    }
}
