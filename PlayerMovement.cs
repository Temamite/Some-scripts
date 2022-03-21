using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{

    //Yleinen pelaajan liikkeestä vastuussa oleva manageri

    public CharacterController2D controller;
    public AudioPlayer audios;
    public float rolleroSpeed = 10f;
    float horizontalMove = 0f;
    public ZombiSkills zombie;
    public bool dig = false;
    public bool jumping = false;

    public bool attacking = false;

    bool restrictMovement = false;
    public GameObject darkness;
    public SpriteRenderer alkudarkness;

    [HideInInspector]
    public bool eisaaliikkuu = true;

    private void Awake()
    {
        
        darkness.SetActive(true);
        StartCoroutine(StartDarknessRemove());
    }


    //Tämä vain poistaa alussa olevan mustan ruudun. Periaatteessa voisi olla eri scriptissä, mutta on nyt tässä.
    IEnumerator StartDarknessRemove()
    {
        float juu = 1;
        SpriteRenderer dankness = alkudarkness;
       

        while (dankness.color.a > 0)
        {
            dankness.color = new Color(1, 1, 1, juu);
            yield return new WaitForEndOfFrame();
            juu = juu - 0.01f;
        }
        Destroy(alkudarkness.gameObject);
        eisaaliikkuu = false;
        zombie.StartSomething(1);
    }


    //Hoidetaan törmäyksiä vihollisten ja pelihahmon kesken
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        if(collision.gameObject.tag=="Enemy")
        {

            //Jos ollaan lyömässä itse miekalla, ei satu 
            if(attacking && zombie.sword)
            { }
            else
            {

                //Otetaan vahinkoa ja vihollinen lyö
                collision.gameObject.GetComponent<EnemySkribuli>().Attack();

                zombie.hp--;
            }

            //Jos zombin HP menee nollaan, kuollaan ja pysäytetään hommat
            if (zombie.hp == 0)
            {
                Debug.Log("YOU'RE DEAD!");
                controller._animator.SetBool("Dead",true);
                audios.PlayEffect(zombie.dead, 1);
                eisaaliikkuu = true;
                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionX;
                collision.gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                zombie.StartSomething(5);
            }
            else
            {
                Debug.Log(zombie.hp);
            }
        }
    }

    //Mitä tapahtuu toimintanappeja painaessa
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        //Mikäli liike estetty, ei jatketa eteenpäin
        if (eisaaliikkuu)
            return;


        //Liike
        if (!restrictMovement)
            horizontalMove = Input.GetAxisRaw("Horizontal") * rolleroSpeed;
        else
            horizontalMove = Input.GetAxisRaw("Horizontal") * (rolleroSpeed/4);


        //Päivitetään animaattoria
        if (horizontalMove != 0)
            controller._animator.SetBool("Walking", true);
        else
            controller._animator.SetBool("Walking", false);


        //Jos on jalat, voidaan hypätä
        if (zombie.legs && Input.GetButtonDown("Jump") && !jumping)
        {
            startedjump = 0;
            jumping = true;

            audios.PlayEffect(zombie.jump, 1);
        }


        //Jos on miekka, voidaan lyödä sillä
        if (zombie.sword && Input.GetButtonDown("Fire1") && !attacking)
        {
            
            Debug.Log("Sword attack!");
            if (rightdigger.transform.localPosition.x > 0)
                rightdigger.transform.localPosition = new Vector3(0.6f, 0, 0);
            else
                rightdigger.transform.localPosition = new Vector3(-0.6f, 0, 0);

            float horizon = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 direction = new Vector2(horizon, vertical);
            audios.PlayEffect(zombie.dig, 1);
            Dig(direction);
        }
        //Jos ei ole miekkaa, mutta on edes kädet, voidaan kaivaa
        else if (zombie.arms && Input.GetButtonDown("Fire1") && !attacking)
        {
            
            float horizon = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 direction = new Vector2(horizon, vertical);
            audios.PlayEffect(zombie.dig, 1);
            Dig(direction); //Kaivetaan
        }
        else if (Input.GetButtonDown("Fire1"))
            Debug.Log("cant!");

        
    }

    public GameObject updigger;
    public GameObject downdigger;
    public GameObject rightdigger;

    //Kaivetaan palikoita
    private void Dig(Vector2 direction)
    {
        attacking = true;
        restrictMovement = true;
        StartCoroutine(diggerointi(direction));

        controller._animator.SetBool("Digging", true);

    }

    
    
    IEnumerator diggerointi(Vector2 direction)
    {
        //Mihin suuntaan kaivamme? Laitetaan haluttu kaivuri päälle.
        if (direction.y > 0)
            updigger.SetActive(true);
        else if (direction.y < 0)
            downdigger.SetActive(true);
        else
            rightdigger.SetActive(true);


        yield return new WaitForSeconds(0.2f);
        restrictMovement = false; controller._animator.SetBool("Digging", false);
        yield return new WaitForSeconds(0.2f);

        updigger.SetActive(false);
        downdigger.SetActive(false);
        rightdigger.SetActive(false);
        attacking = false;
        
    }

    //Lopettaa kaivamisen tarvittaessa
    public void StopAttack()
    {
        updigger.SetActive(false);
        downdigger.SetActive(false);
        rightdigger.SetActive(false);
    }

    Vector2 lastFramePosition;

    //Täällä syötetään liikettä kontrollerille
    private void FixedUpdate()
    {
        controller.Move(horizontalMove*Time.fixedDeltaTime, jumping);
        if (startedjump <= 5)
            startedjump++;



        if (!controller.m_Grounded && lastFramePosition.y < transform.position.y)
            VerticalMovement = 1;
        else if (!controller.m_Grounded && lastFramePosition.y > transform.position.y)
            VerticalMovement = -1;
        else
            VerticalMovement = 0;

        
        //Viime framen positio. Tämän pohjalta katsotaan mm. liikesuunta
            lastFramePosition = transform.position;
    }

    public int startedjump = 10;

    //Hypyn laskeutumisen pölyefekti
    public void Landing()
    {
        if(startedjump>5)
        {
            jumping = false;
            startedjump = 10;
            //Debug.Log("Spawnaa"+Time.timeSinceLevelLoad);
            Instantiate(landaus, transform.position, Quaternion.identity);
        }
    }
    public  GameObject landaus;

    public int VerticalMovement = 0;
}
