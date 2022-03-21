using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
   
    //Audioplayeri, joka soittaa haluttuja ‰‰niklippej‰. 

    public void PlayEffect(AudioClip audio, float volume)
    {

        //Luodaan uusi ‰‰ni ja v‰‰nnet‰‰n nippelit oikeiksi
        GameObject newAudio = new GameObject(audio.name);
        newAudio.transform.position = transform.position;
        AudioSource audi = newAudio.AddComponent<AudioSource>();
        audi.volume = volume;
        audi.clip = audio;
        audi.Play();
        StartCoroutine(KillAudioClip(audi)); 
    }

    //Luotu audiosource tuhoutuu soittamisen j‰lkeen
    IEnumerator KillAudioClip(AudioSource AudioSourceke)
    {
        yield return new WaitForSeconds(AudioSourceke.clip.length);
        GameObject.Destroy(AudioSourceke.gameObject);
    }




}
