using System.Collections;
using System.Collections.Generic;
using BS.Player;
using UnityEngine;

using UnityEngine.Audio;

public class BgmManager : MonoBehaviour
{
    public GameObject player;

    public AudioMixer audioMixer;
    public AudioMixerSnapshot nor;
    public AudioMixerSnapshot charghing;

    public AudioClip[] playList;
    private AudioSource Bgm;

    void Start()
    {
        Bgm = GetComponent<AudioSource>();

        Play(0);
    }
    //0 : lastremote

    void Update()
    {
        BgmDistortion();
    }

    public void Play(int bgmNum)
    {
        Bgm.clip = playList[bgmNum];
    }

    public void Stop(int bgmNum)
    {
        Bgm.Stop();
    }



    /// <summary>
    /// /////////
    /// </summary>
    
    public void BgmDistortion()
    {
        if (Input.GetMouseButton(0) && player.GetComponent<PlayerController>().OnAir() )
        {
            charghing.TransitionTo(.5f);
        }

        else
        {
            nor.TransitionTo(.1f);
        }
    }

    public void SetLPF(float frequency) //set Low Pass Frequency
    {
        audioMixer.SetFloat("Freq", frequency);
    }

    public void PitchDistortion(float pitch) //distort pitch
    {
        audioMixer.SetFloat("pitch", pitch);
    }

    public void ClearDistordion()
    {
        audioMixer.ClearFloat("");
    }
}