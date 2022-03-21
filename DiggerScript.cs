using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DiggerScript : MonoBehaviour
{
    // Scripti pelimaailman palikoiden hajottamiseen "kaivamalla"
    public GameObject murennus;
    public GameObject enemyposahus;
    public ZombiSkills skillssit;


    
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Mik‰li hosutaan kivi‰ tai vihollisia kohti
        if (collision.gameObject.tag != "Ground" || transform.tag != "Digger" || collision.gameObject.name.Contains("ROCK"))
        {
            //Jos on ker‰tty miekka, voidaan vahingoittaa vihollisia
            if(skillssit.sword && collision.gameObject.tag=="Enemy")
            {
                
                Instantiate(enemyposahus, collision.gameObject.transform.position, Quaternion.identity); //Luodaan kuolinefekti vihulle
                skillssit.move.audios.PlayEffect(skillssit.move.zombie.enemydies, 1); //ƒ‰net
                Destroy(collision.gameObject); //Poistetaan vihu
                return;
            }
            else
            return;
        }


        //Jos p‰‰st‰‰n t‰nne asti, on kyseess‰ kaivettavaksi kelpaavaa maastoa
        Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        Vector3 hitPosition = Vector3.zero;

        //Selvitet‰‰n, mihin kohtaan tilemappia kaivuriobjekti osuu, mik‰ tile kaivetaan rikki ja poistetaan tilemapista?
        foreach (ContactPoint2D hit in collision.contacts)
        {

            hitPosition.x = hit.point.x;
            hitPosition.y = hit.point.y;
            Vector3Int cell = new Vector3Int((int)hitPosition.x, (int)hitPosition.y, 0);

            tilemap.SetTile(tilemap.WorldToCell(hitPosition), null);
            skillssit.move.StopAttack(); //Kaivurit pois p‰‰lt‰

            //Efektit
            Instantiate(murennus, hitPosition, Quaternion.identity); 
            skillssit.move.audios.PlayEffect(skillssit.move.zombie.breakblock, 1);

        }
    }
}
