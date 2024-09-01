using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource source;
    public AudioClip happy, sad, jump;

    public void PlayHappy()
    {
        source.clip = happy;
        source.Play();
    }

    public void PlaySad()
    {
        source.Stop();
        source.clip = sad;
        source.Play();
    }
}
