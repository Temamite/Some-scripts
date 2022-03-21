using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ZombiSkills : MonoBehaviour
{

    //Yksinkertainen pelihahmon kykyj‰ kontrolloiva systeemi

    public PlayerMovement move; //liikescripti
    public CharacterController2D controller; //hahmokontrolleri

    public int hp = 1; //Hahmon t‰m‰nhetkinen HP


    //Ker‰tt‰v‰t abilityt, onko jo lˆydetty?
    public bool eye = false;
    public bool arms = false;
    public bool sword = false;
    public bool legs = false;
    public bool heart = false;

    


    //Eri abilityjen ‰‰niklipit
    public AudioClip dig;
    public AudioClip jump;
    public AudioClip dead;
    public AudioClip breakblock;
    public AudioClip land;
    public AudioClip swordattack;
    public AudioClip pickup;
    public AudioClip enemydies;
    public AudioClip dmg;
    public AudioClip win;


    public GameObject text1;
    public GameObject text2;
    public GameObject text3;
    public GameObject text4;
    public GameObject text5lose;
    public GameObject text6win;



    //N‰ytt‰‰ pelaajalle teksti‰ aina tarvittaessa, kun kyky ker‰t‰‰n 
    public void StartSomething(int numer)
    {
        StartCoroutine(showtexti(numer));
    }

    public IEnumerator showtexti(int num)
    {
        if(num == 1)
        {
            text1.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text1.SetActive(false);

        }
        else if (num == 2)
        {
            text2.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text2.SetActive(false);
        }
        else if(num == 3)
        {
            text3.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text3.SetActive(false);
        }
        else if (num == 4)
        {
            text4.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text4.SetActive(false);
        }
        else if (num == 5)
        {
            text5lose.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text5lose.SetActive(false);
        }
        else if (num == 6)
        {
            text6win.SetActive(true);
            yield return new WaitForSecondsRealtime(3f);
            text6win.SetActive(false);
        }

           
        if(hp<=0)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
