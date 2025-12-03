using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public string battleSelectScene;
    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instansce.PlayMenuMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        SceneManager.LoadScene(battleSelectScene);

        AudioManager.instansce.PlaySFX(0);
    }

    public void QuitGame()
    {
        Application.Quit();

        Debug.Log("Quitting Game");

        AudioManager.instansce.PlaySFX(0);
    }
}
