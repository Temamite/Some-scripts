using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class mene : MonoBehaviour
{
    //Yksinkertainen scripti, joka starttaa pelin alkukuvasta

    void Start()
    {
      
        StartCoroutine(soon());

    }


    IEnumerator soon()
    {
        yield return new WaitForSeconds(5);
        SceneManager.LoadScene("Game");
    }
    
}
