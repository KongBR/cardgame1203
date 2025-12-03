using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleSelectMenu : MonoBehaviour
{

    public string levelToLoad;

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.instansce.PlayBattleSelectMusic();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectBattle()
    {
        SceneManager.LoadScene(levelToLoad);



        AudioManager.instansce.PlaySFX(0);
    }
}
