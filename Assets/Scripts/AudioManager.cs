using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instansce;

    private void Awake()
    {
        if (instansce == null)
        {

            instansce = this;

            DontDestroyOnLoad(gameObject);
        }else if(instansce != this)
        
        {
            Destroy(gameObject);
        }
        
    }

    public AudioSource menuMusic;
    public AudioSource battleSelectMusic;
    public AudioSource[] bgm;
    private int currentBGM;
    private bool playingBGM;

    public AudioSource[] sfx;

    public AudioSource bossBGM;

    public void PlayBossBGM()
    {
        StopMusic();

        if (bossBGM != null)
        {
            bossBGM.Play();
            playingBGM = false;   // 보스전은 루프가 필요하면 bossBGM.loop = true 로 해결
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playingBGM)
        {
            if(bgm[currentBGM].isPlaying == false)
            {
                currentBGM++;
                if(currentBGM >= bgm.Length)
                {
                    currentBGM = 0;
                }

                bgm[currentBGM].Play();
            }
        }
    }

    public void StopMusic()
    {
        menuMusic.Stop();
        battleSelectMusic.Stop();
        foreach(AudioSource track in bgm)
        {
            track.Stop();
        }

        playingBGM = false;
    }

    public void PlayMenuMusic()
    {
        StopMusic();
        menuMusic.Play();
    }

    public void PlayBattleSelectMusic()
    {
        if(!battleSelectMusic.isPlaying == false)
        {
            StopMusic();
            battleSelectMusic.Play();

        }
        

    }

    public void PlayBGM()
    {
        StopMusic();

        currentBGM = Random.Range(0, bgm.Length);

        bgm[currentBGM].Play();
        playingBGM = true;
    }

    public void PlaySFX(int sfxToPlay)
    {
        sfx[sfxToPlay].Stop();
        sfx[sfxToPlay].Play();
    }
}
