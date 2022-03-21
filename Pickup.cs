using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{

    //Scripti erilaisille ker�tt�ville.


    //Ker�tt�v�n tyyppi
    public enum WhichType {eye, arms, sword, legs, heart }

    public WhichType type;

    Transform player;
    GameObject mask;
    private void Update()
    {

        //Mik�li ker�tt�v� on silm�, pelaajan n�k�kentt� suurenee ker�tt�ess�
        if(type==WhichType.eye && !nomore)
        {
            if (player == null)
            {
                mask = transform.GetChild(0).gameObject;
                player = GameObject.FindGameObjectWithTag("Player").transform;
            }

            float distancetoplayer = Vector2.Distance(transform.position, player.position);

            if (distancetoplayer < 4f && mask.activeInHierarchy)
                mask.SetActive(false);
            else if(distancetoplayer>=4f && !mask.activeInHierarchy)
                mask.SetActive(true);



        }
    }


    //T�rm�tt�ess� ker�tt�v��n
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            if(type!=WhichType.legs && type!=WhichType.eye && type != WhichType.heart)
            Destroy(gameObject);

            if(type == WhichType.eye)
            {
                PlayerMovement player;
                player = collision.transform.root.GetComponent<PlayerMovement>();

                player.audios.PlayEffect(player.zombie.pickup, 1);
                player.zombie.eye = true;
                player.controller._animator.SetTrigger("Eye");
                StartCoroutine(removeDarkness());
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<CircleCollider2D>().enabled = false;
            }
            else if (type == WhichType.arms)
            {
                PlayerMovement player;
                player = collision.transform.root.GetComponent<PlayerMovement>();

                player.audios.PlayEffect(player.zombie.pickup, 1);
                player.zombie.arms = true;
                player.controller._animator.SetTrigger("Arms");
                player.zombie.StartSomething(2);
            }
            else if (type == WhichType.sword)
            {
                PlayerMovement player;
                player = collision.transform.root.GetComponent<PlayerMovement>();

                player.audios.PlayEffect(player.zombie.pickup, 1);
                player.zombie.sword = true;
                player.controller._animator.SetTrigger("Sword");
                player.zombie.StartSomething(3);
            }
            else if (type == WhichType.legs)
            {
                PlayerMovement player;
                player = collision.transform.root.GetComponent<PlayerMovement>();

                player.audios.PlayEffect(player.zombie.pickup, 1);
                player.zombie.legs = true;
                player.controller._animator.SetTrigger("Legs");
                player.GetComponent<BoxCollider2D>().enabled = true;
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<CircleCollider2D>().enabled = false;
                player.zombie.StartSomething(4);
                StartCoroutine(transportCamera());
            }
            else if (type == WhichType.heart)
            {
                PlayerMovement player;
                player = collision.transform.root.GetComponent<PlayerMovement>();

                player.audios.PlayEffect(player.zombie.win, 1);
                player.zombie.heart = true;
                player.controller._animator.SetTrigger("Heart");
                GetComponent<Animator>().SetTrigger("getto");
                Debug.Log("VICTORY!!!");
                player.eisaaliikkuu = true;
                player.GetComponent<SpriteRenderer>().enabled = false;
                player.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                player.zombie.StartSomething(6);
                player.GetComponent<CircleCollider2D>().enabled = false;
                player.GetComponent<BoxCollider2D>().enabled = false;

            }

        }
    }
    bool nomore = false;

    //Ker�tt�ess� jalat kameran kiintopiste muuttuu hieman, koska pelaaja pystyy nyt hyppim��n ja matka jatkuu yl�sp�in
    IEnumerator transportCamera()
    {
        nomore = true;
        float juu = -2;
        while(Camera.main.transform.localPosition.y<2.5f)
        {
            Camera.main.transform.localPosition = new Vector3(0, juu, -10);
            yield return new WaitForEndOfFrame();
            juu = juu + 0.05f;
        }
        Camera.main.transform.localPosition = new Vector3(0, 2.5f, -10);
        Destroy(gameObject);
    }


    //Pimeys katoaa ymp�rilt� kun ker�t��n silm�
    IEnumerator removeDarkness()
    {
        nomore = true;
        float juu = 1;
        SpriteRenderer dankness;
        dankness = player.GetComponent<PlayerMovement>().darkness.GetComponent<SpriteRenderer>();
        Destroy(transform.GetChild(0).gameObject);

        while (dankness.color.a > 0)
        {
            dankness.color = new Color(1, 1, 1, juu);
            yield return new WaitForEndOfFrame();
            juu = juu - 0.01f;
        }
        Destroy(dankness.gameObject);
        Destroy(gameObject);
    }

}
