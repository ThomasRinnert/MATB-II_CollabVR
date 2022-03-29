using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    AudioSource audio_source;
    [SerializeField] AudioClip audio_clip;
    // Start is called before the first frame update
    void Start()
    {
        audio_source = transform.GetComponent<AudioSource>();
    }

    [ContextMenu("DEBUG")]
    public void DEBUG()
    {
        audio_source.mute = false;
        audio_source.clip = audio_clip;
        audio_source.Play();
    }
}
