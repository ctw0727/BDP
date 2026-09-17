using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sounds
{
    public string name;
    public AudioClip clip;

    public bool isPlaying;
}


public class SoundManager : MonoBehaviour
{
    public AudioSource[] bgm;
    public AudioSource[] effects;

    public string[] sourceName;


    [SerializeField]
    public Sounds[] bgmPL; //play list
    public Sounds[] effectPL;




    public void PlayE(string eName) //입력한 effect 재생
    {
        for(int i = 0; i < effectPL.Length; i++)
        {
            if(eName == effectPL[i].name)
            {
                for (int j = 0; j < effects.Length; j++)
                {
                    if (!effectPL[j].isPlaying )
                    {
                        sourceName[j] = effects[i].name;

                        effects[j].clip = effectPL[j].clip;
                        effects[j].Play();

                        return;
                    }
                }

                Debug.Log("++");
            }
        }

        Debug.Log(eName + "is non exist");
    }


    public void ShutDownEffect()
    {
        for(int i = 0; i<effects.Length; i++ )
        {
            effects[i].Stop();
        }
    }

    public void StopEffect(string ename) //해당 이펙트 정지.
    {
        for(int i = 0; i < effectPL.Length; i++)
        {
            if(sourceName[i] == ename)
            {
                effects[i].Stop();

                return;
            }
        }
        Debug.Log("cant stop?");
    }
}