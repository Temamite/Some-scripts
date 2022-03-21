using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{

    //Scripti joka mahdollistaa pelaajan seuraamisen kameralle

    public float speed = 5;
    public Camera maincamera;
    public Transform player;
    public float pixPerUnit = 16;

    public ZombiSkills zombie;
    

    //Perusvalmisteluja
    void Start()
    {
        maincamera = Camera.main;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        zombie = player.gameObject.GetComponent<ZombiSkills>();
    }


    //Kamera seuraa updatessa pelaajaa joka framella tietyllä vauhdilla.
    private void Update()
    {
        Vector3 begin = transform.position;
        Vector3 end;


        //Kiintopiste riippuu siitä, onko pelihahmo kerännyt jalat vai ei
        if (!zombie.legs)
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 2.5f, -10);
        else
            transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 2.5f, -10);

        if (!zombie.legs)
            end = new Vector3(player.transform.position.x, player.transform.position.y - 2.5f, -10);
        else
            end = new Vector3(player.transform.position.x, player.transform.position.y - 2.5f, -10);

       
        transform.position = Vector3.MoveTowards(begin, end, speed * Time.deltaTime);
    }

   
       //Pikselintarkka positio
        void LateUpdate()
        {
            transform.position = new Vector3(
                /*Mathf.Round(transform.position.x * pixPerUnit) / pixPerUnit*/transform.position.x,
                Mathf.Round(transform.position.y * pixPerUnit) / pixPerUnit,
                transform.position.z);
        }
    
   
}
