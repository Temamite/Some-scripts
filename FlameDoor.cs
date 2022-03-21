using MoreMountains.CorgiEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FlameDoor : MonoBehaviour {


    public enum DoorType
    {
        TentacleDoor,
        BurnableDoor
    }


    public DoorType Type;



    bool CanBeFlamed = true;
    public int opentime = 2;

    public bool open = false;
    public GameObject FlameBlockade;

    PolygonCollider2D thisCollider;
    Animator _animator;
    public GameObject piilo = null;

    private void Start()
    {
        thisCollider = GetComponent<PolygonCollider2D>();
        _animator = GetComponent<Animator>();

        if(GameManager.Instance.DestoyedForever.Contains(name))
        {
            Destroy(FlameBlockade);
            if (piilo)
                piilo.SetActive(false);
            thisCollider.enabled = false;
        }
    }

    float openTimer = 0;
	// Update is called once per frame
	void Update () {

        if(open && Type == DoorType.TentacleDoor)
        {
            openTimer += Time.deltaTime;

            if(openTimer>=opentime)
            {
                openTimer = 0;
                open = false;
                _animator.SetBool("Open", false);
                Invoke("CanSoonAgain",0.5f);
            }

        }
		
	}

    void CanSoonAgain()
    {
        CanBeFlamed = true;
        SoundManager.Instance.PlaySound(ReturnTentaclesClip, transform.position);
    }


    private void OnParticleCollision(GameObject other)
    {

        if(Type == DoorType.TentacleDoor)
        {
            if ((other.name.Contains("Flame") || other.name.Contains("Fire")) && !open)
            {
                SoundManager.Instance.PlaySound(BurnClip, transform.position);
                CanBeFlamed = false;
                open = true;
                _animator.SetBool("Open", true);
                if (piilo && piilo.activeInHierarchy)
                    StartCoroutine(PiiloMene());
            }
        }
        else if(Type == DoorType.BurnableDoor)
        {
            if ((other.name.Contains("Flame") || other.name.Contains("Fire")) && !open)
            {
                open = true;
                SoundManager.Instance.PlaySound(BurnClip, transform.position);
                CanBeFlamed = false;
                open = true;
                _animator.Play("BurnToTheGround");
                GameManager.Instance.DestoyedForever.Add(name);

            }
        }
       
    }

    public AudioClip BurnClip;
    public AudioClip ReturnTentaclesClip;
    void HereDies()
    {
        thisCollider.enabled = false;
        Destroy(FlameBlockade);

        if (piilo && piilo.activeInHierarchy)
            StartCoroutine(PiiloMene());
    }

    IEnumerator PiiloMene()
    {
        Tilemap sprait = piilo.GetComponent<Tilemap>();
        float laskur = sprait.color.a;
        while(sprait.color.a>0)
        {
            laskur -= 0.2f;
            yield return new WaitForEndOfFrame();
            sprait.color = new Color(sprait.color.r, sprait.color.g, sprait.color.b, laskur);
        }
        piilo.SetActive(false);
    }

}
