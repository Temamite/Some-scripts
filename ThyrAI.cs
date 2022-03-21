using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;



public class ThyrAI : MonoBehaviour, MMEventListener<MMDamageTakenEvent>
{

    protected float DistanceToEnemy = 0f;

    /// The layers the agent will try to shoot atD:\Kamina Dimension\Projects\Frozen Will\Project Frozen Will\Assets\MindSeize Scripts\Enemies and bosses\ThyrAI.cs
    public LayerMask TargetLayerMask;

    public GameObject HealDrone;

    protected DamageOnTouch _damage;

    float fightStartTime;
    protected CharacterHandleWeapon _characterShoot;
    protected RaycastHit2D _raycast;
    protected RaycastHit2D _raycastBehind;

    public Transform SwordAttack;
    public Transform TurnAxeAttack;

    [HideInInspector]
    public Character _character;
    //  [HideInInspector]
    // public Collider2D _overlapCircleHitMoreRange;
    [HideInInspector]
    public SpriteRenderer _renderer;

    [HideInInspector]
    public int randomizer = 1;
    
    [HideInInspector]
    public bool lookingright = true;

    AIWalk _walker;
    protected Animator _animator;
    public float MeleeDistance = 3;
    public bool ReadyToAct = false;
    
    
    RaycastHit2D angleray;

    public int numPoints;

    private Vector3[] ChainPos;

    float SaveInCaseOfStuckTimer = 0;

    public ParticleSystem ChainLightParticle;
    public ParticleSystem ChainLightPixel;
    public ParticleSystem ChainLightRoller;
    public ParticleSystem ChainLightStrangerGlowTausta;
    public ParticleSystem ChainLightStrangerGlowTaustaTall;
    public ParticleSystem ChainLightDroneGlowTausta;
    private ParticleSystem.EmitParams EmitTiedot = new ParticleSystem.EmitParams();
    private ParticleSystem.EmitParams PixelTiedot = new ParticleSystem.EmitParams();
    private ParticleSystem.EmitParams GlowTiedot = new ParticleSystem.EmitParams();
    private ParticleSystem.EmitParams StrangerGlowTiedot = new ParticleSystem.EmitParams();
    private ParticleSystem.EmitParams StrangerPixelTiedot = new ParticleSystem.EmitParams();


    private ParticleSystem.Particle[] chainparticles;
    private ParticleSystem.Particle[] rollerparticle;
    private ParticleSystem.MainModule ChainMain;

    private BoxCollider2D _collider;

    public Transform MiddlePointHolder;
    private Vector3 ColMin;
    private Vector3 ColMax;
    private Vector3 UnderHealDrone;

    private CharacterJump _jumper;

    private int ParticleCount;
    private int RollerCount;
    private int LightInt = 0;
    //PELAAJAPOS CURVETESTING
    private Transform PelaajaPos;
    public Transform HealDronePos;
    public Transform MiddlePoint;

    private int CurrentAnimInt = 0;

    IEnumerator Lightroll;
    IEnumerator _SmoothMove;

    IEnumerator _ChainHandler;

    public bool LightChainTest = false;
    private bool CoroChainSpawn;
    private bool SmoothMoving;


    public bool CanGuard = true;

    int GuardKerroin = 0;

    public void Guard()
    {
        

        if (jumping || !_character._controller.State.IsCollidingBelow || _character.MovementState.CurrentState == CharacterStates.MovementStates.Flip || !CanGuard)
            return;

        _character._controller.SetForce(Vector2.zero);
        SaveInCaseOfStuckTimer = 0;
        _walker.SetDirection(Vector2.zero);
        running = false;
        ReadyToAct = false;
        StopAllCoroutines();
        CancelInvoke();
        waiting = false;
        takinDamageTimer = 0;

        CanGuard = false;
        _animator.Play("ThyrGuard");
        _character._health.Invulnerable = true;
    }
    void PassiveGuard()
    {
      
        _walker.SetDirection(Vector2.zero);
        GuardKerroin++;
        CancelInvoke();
        waiting = false;
        StopAllCoroutines();
        ReadyToAct = false;
        CanGuard = false;
        _character._health.Invulnerable = true;


        if(Random.Range(1,4)==1 && GameManager.Instance.PlayerCharacter.MovementState.CurrentState!=CharacterStates.MovementStates.Crouching)
        PassGuard = true;

    }
    void CheckPassiveGuard()
    {
        
        if(CanPunish)
        {
            if (Random.Range(1,4) == 1 && DistanceToEnemy > 3.5f && GameManager.Instance.IsInFront(_character) && _character._health.CurrentHealth < 150)
            {
                SoundManager.Instance.PlaySound(IRONMAN, transform.position);
                _animator.SetTrigger("IronMan");
            }
            else
                StartCoroutine(CanActSoon(0.0f));
        }
        else
            StartCoroutine(CanActSoon(0.0f));

        _walker._direction = Vector2.zero;
        _character._health.Invulnerable = false;
        CanPunish = false;
        PassGuard = false;
        untilend = false;
        running = false;

    }
    bool CanPunish = false;
    bool PassGuard = false;

    void CanAxe()
    {
        _walker._direction = Vector2.zero;
        if (DistanceToEnemy <= 3.5 && GameManager.Instance.IsInFront(_character) && Random.Range(1, 10) <= 4)
        {

            _character._health.Invulnerable = false;
            _animator.SetTrigger("FrontAxe");
            _animator.ResetTrigger("IronMan");
        }
        else
            _animator.ResetTrigger("FrontAxe");
    }

    void CheckAxeDirection()
    {

        if (GameManager.Instance.PlayerCharacter.MovementState.CurrentState == CharacterStates.MovementStates.Dashing && ((!GameManager.Instance.PlayerCharacter.IsFacingRight && _character.IsFacingRight) || (GameManager.Instance.PlayerCharacter.IsFacingRight && !_character.IsFacingRight)))
        {
            _animator.ResetTrigger("IronMan");
            _animator.ResetTrigger("FrontAxe");
            print("punishaa!");

            _walker.ChangeDirection();
        }
        else if(!GameManager.Instance.IsInFront(_character))
        {
            _animator.ResetTrigger("IronMan");
            _animator.ResetTrigger("FrontAxe");
            print("punishaa!");
            _walker.ChangeDirection();
        }
        
    }

    void EndGuardInAttack()
    {
        _character._health.Invulnerable = false;
        _walker._direction = Vector2.zero;
        waiting = false;
        takinDamageTimer = 0;
        StopAllCoroutines(); CancelInvoke();
        ReadyToAct = false;
        GuardAttack++;
    }
    int GuardAttack = 0;

    void EndGuard()
    {
        //väliaikanen!

        
        _character._health.Invulnerable = false;
        
        if(Random.Range(1,11)<=4)
        {

            if (GameManager.Instance.IsInFront(_character) && GameManager.Instance.PlayerCharacter.MovementState.CurrentState != CharacterStates.MovementStates.Crouching && _character._health.CurrentHealth < 150)
            {
                SoundManager.Instance.PlaySound(IRONMAN, transform.position); _animator.SetTrigger("IronMan");
            }
            else
                StartCoroutine(CanActSoon(0f));
        }
        else
            StartCoroutine(CanActSoon(0f));

        _walker._direction = Vector2.zero;
    }

    private void OnEnable()
    {
        this.MMEventStartListening<MMDamageTakenEvent>();
    }
    private void OnDisable()
    {
        this.MMEventStopListening<MMDamageTakenEvent>();
    }

    GhostSprites _ghost;
    private void Start()
    {
        GameManager.Instance.ShowThyrOnMap.enabled = false;
        _ghost = GetComponent<GhostSprites>();
        GhostOff();
        droneStartPosition = HealDrone.transform.position;
        _character = GetComponent<Character>();
        _characterShoot = GetComponent<CharacterHandleWeapon>();
        _damage = GetComponent<DamageOnTouch>();
        _animator = GetComponent<Animator>();
        ReadyToAct = false;
        _walker = GetComponent<AIWalk>();
        _collider = GetComponent<BoxCollider2D>();
        _renderer = GetComponent<SpriteRenderer>();
        ChainPos = new Vector3[numPoints];
        _jumper = GetComponent<CharacterJump>();
        //   Invoke("ActSoon", 2f);

        if (_character.IsFacingRight)
        {
            lookingright = true;
        }

        else
        {
            lookingright = false;
        }

        HealDroneHealth = HealDrone.GetComponent<Health>();

        //GetComponent<HealthBar>().Activation();

        AxePox = TurnAxeAttack.transform.localPosition;

        PelaajaPos = GameObject.Find("Rectangle 1").transform;
        HealDronePos = HealDrone.transform;
       EmitTiedot.applyShapeToPosition = true;
        PixelTiedot.applyShapeToPosition = true;
        GlowTiedot.applyShapeToPosition = true;
        StrangerGlowTiedot.applyShapeToPosition = true;
       StrangerPixelTiedot.applyShapeToPosition = true;

     //   ChainLightParticle = GetComponentInChildren<ParticleSystem>();

        for (int i=0;i<transform.childCount;i++)
        {
            if (transform.GetChild(i).name == "ValoChainParticle")
            {
                ChainLightParticle = transform.GetChild(i).GetComponent<ParticleSystem>();
                //ChainLightPixel = ChainLightParticle.subEmitters.GetSubEmitterSystem(0);
            }
            else if (transform.GetChild(i).name == "ValoChainRoller")
            {
                ChainLightRoller = transform.GetChild(i).GetComponent<ParticleSystem>();
            } else if (transform.GetChild(i).name == "ValoChainStrangerGlowTausta")
            {
                ChainLightStrangerGlowTausta = transform.GetChild(i).GetComponent<ParticleSystem>();
            } else if (transform.GetChild(i).name == "ValoChainDroneGlowTausta")
            {
                ChainLightDroneGlowTausta = transform.GetChild(i).GetComponent<ParticleSystem>();
            } else if (transform.GetChild(i).name == "ValoChainStrangerGlowTaustaTall")
            {
                ChainLightStrangerGlowTaustaTall = transform.GetChild(i).GetComponent<ParticleSystem>();
            } else if (transform.GetChild(i).name == "MiddlePoint")
            {
                MiddlePoint = transform.GetChild(i).transform;
            } else if (transform.GetChild(i).name == "ValoChainPixel")
            {
                ChainLightPixel = transform.GetChild(i).GetComponent<ParticleSystem>();
            }
                
        }
       
        ChainMain = ChainLightParticle.main;
        ChainMain.maxParticles = numPoints * 2; // Kaksi grouppia, joita swapataan


       
    }

    Vector3 AxePox;

    void ThrowAxer()
    {
        if (!HealDrone.activeInHierarchy && _character._health.CurrentHealth <= 120 && DroneTimer <= 0)
            HealDroneHealme();

        StartCoroutine(AxeFlying());
        
        
    }
    GameObject FlyerAxe = null;
    IEnumerator AxeFlying()
    {

        if (_character.IsFacingRight)
        {
            FlyingAxePosition.transform.localPosition = new Vector2(1.25f,-0.25f);
        }
        else
            FlyingAxePosition.transform.localPosition = new Vector2(-1.25f, -0.25f);

        StartAxeLoop();

        FlyerAxe = Instantiate(FlyingAxe, FlyingAxePosition.transform.position, Quaternion.identity);
        
        if (!_character.IsFacingRight)
            FlyerAxe.GetComponent<SpriteRenderer>().flipX = true;


        float timer = 0;
        Vector2 randomPoint = new Vector2(transform.position.x+(_character.IsFacingRight?7+Random.Range(-4f, 1f):-5 + Random.Range(-1f, 4f)), transform.position.y+Random.Range(-1f,1f));

        while (Vector2.Distance(randomPoint, FlyerAxe.transform.position) > 0.5f )
        {
            timer += Time.deltaTime;
            FlyerAxe.transform.position = Vector2.MoveTowards(FlyerAxe.transform.position, randomPoint, 6f * Time.deltaTime);
            //  FlyingAxe.transform.eulerAngles = new Vector3(0, 0, thrownSpear.transform.eulerAngles.z + 11f);
            yield return new WaitForEndOfFrame();

           
        }
        while (Vector2.Distance(GameManager.Instance.CurrentPlayer.transform.position, FlyerAxe.transform.position) > 0.5f || GameManager.Instance.PlayerCharacter.MovementState.CurrentState != CharacterStates.MovementStates.AirDashing || GameManager.Instance.PlayerCharacter.MovementState.CurrentState != CharacterStates.MovementStates.Dashing)
        {
            timer += Time.deltaTime;
            FlyerAxe.transform.position = Vector2.MoveTowards(FlyerAxe.transform.position, GameManager.Instance.CurrentPlayer.transform.position, 6f * Time.deltaTime);
            //  FlyingAxe.transform.eulerAngles = new Vector3(0, 0, thrownSpear.transform.eulerAngles.z + 11f);
            yield return new WaitForEndOfFrame();

            if (timer >= 2.5f)
            {

                break;
            }
        }
        while (Vector2.Distance(transform.position, FlyerAxe.transform.position) > 0.5f)
        {
           
            FlyerAxe.transform.position = Vector2.MoveTowards(FlyerAxe.transform.position, transform.position, 10 * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            
        }

        SoundManager.Instance.StopLoopingSound(AxeSpinSource);

        PlayJetEndSound();

        Destroy(FlyerAxe.gameObject);
        FlyerAxe = null;
      //  Invoke("AnimationRecovery", 0.47f);
        _animator.SetTrigger("AxeThrow");
    }

    public void Chargester()
    {
        GhostOn();
        _character._controller.GravityActive(true);
    }

    public void Toimitaan()
    {
        fightStartTime = Time.time;
        BackgroundMusic.Instance.PlaySomething(-1, 101.1614f);
        //  ReadyToAct = true;
       


        if(LevelManager.Instance.BossRushScene)
        {
            _character._health.Invoke("ReadyNowFinally", 0.35f);
        }

            ReadyToAct = true;
     //   _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
        //Guard();

       
    }

    


    int jumpCombo = -1;

    int framecounter = 3;

    [HideInInspector]
    public bool neverdoanythingagain = false;

    bool jumpAttack = false;
    bool untilend = false;
    public GameObject landBoom;
    public Transform LevelMidPoint;
    public bool running = false;
    float droneTimeri = 0;
    // Update is called once per frame
    void Update()
    {
        if (neverdoanythingagain)
            return;

        if (waiting)
        {
            takinDamageTimer += Time.deltaTime;
            
        }

        if (DroneIsHealing)
        {

            droneTimeri += Time.deltaTime;

            if(droneTimeri>0.3f)
            {
                _character._health.GetHealth(1, HealDrone);
                droneTimeri = 0;

            }

            if (HealDroneHealth.CurrentHealth <= 0)
            {
                DroneTimer = 35f;
                HealDrone.GetComponent<HackingPoint>().NytMeni();
                DroneIsHealing = false;

                SoundManager.Instance.StopLoopingSound(healloop);
                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("ThyrUlti"))
                    CanDoBigassAttack = true;
            }
                
        }
        else if(DroneTimer>0)
        {
            DroneTimer -= Time.deltaTime;
        }



        if (!running)
            _walker.SetDirection(Vector2.zero);

        if (running)
            _animator.ResetTrigger("IronMan");

        if (GameManager.Instance.PlayerCharacter.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
        {
            
            GhostOff();
            if(FlyerAxe!=null)
                Destroy(FlyerAxe.gameObject);
            StopAllCoroutines();
            CancelInvoke();
            neverdoanythingagain = true;
            _character._controller.GravityActive(true);
            _animator.Play("ThyrVictory");
            _character._spriteRenderer.sortingLayerName = "Platforms";
            SoundManager.Instance.PlaySound(victory, transform.position);
            return;
        }
        if (GameManager.Instance.SoftLock)
            return;





        if ((_character == null)) { return; }

        if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
        {

            GhostOff();
            StopAllCoroutines(); CancelInvoke();
            neverdoanythingagain = true;

            if (GameObject.Find("FlyingAxe(Clone)"))
            {
                Destroy(GameObject.Find("FlyingAxe(Clone)"));
                SoundManager.Instance.StopLoopingSound(AxeSpinSource);
            }

            if(DroneIsHealing)
                SoundManager.Instance.StopLoopingSound(healloop);
            SwordAttack.gameObject.SetActive(false);
            InitialDeeSwordAttack.gameObject.SetActive(false);
            LastFramesSwordAttack.gameObject.SetActive(false);
            SwordAttack.gameObject.SetActive(false);
            TurnAxeAttack.gameObject.SetActive(false);


            _character._health.NPCInfo.StartLogRoutine();
            _animator.Play("ThyrLose");

            GameManager.Instance.DestoyedForever.Add("Thyr");
            GameManager.Instance.Steam.CheckAchievements();
            _character._health.NPCInfo.StartLogRoutine();

            IronBeam.SetActive(false);
           
            BackgroundMusic.Instance.FadeMusic();
            SoundManager.Instance.PlaySound(defeat, transform.position);
            HealDrone.GetComponent<HackingPoint>().NytMeni();

            _character._controller.GravityActive(true);
            if (!GameManager.Instance.IsInFront(_character))
                _character.HEREWEWILLFLIPFLIP_AI();

            if (GameManager.Instance.PlayerCharacter.IsFacingRight && transform.position.x < GameManager.Instance.CurrentPlayer.transform.position.x)
                GameManager.Instance.PlayerCharacter.Flip();
            else if (!GameManager.Instance.PlayerCharacter.IsFacingRight && transform.position.x > GameManager.Instance.CurrentPlayer.transform.position.x)
                GameManager.Instance.PlayerCharacter.Flip();

            GameManager.Instance.StopCharacter(true);
            StopLoops();

            if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Walking)
                _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            _character._characterHorizontal.RunStopSouind();
            if (LevelManager.Instance.BossRushScene)
            {
                GameManager.Instance.AllowContinue();
                GameObject.Find("SwordPoint").GetComponent<Animator>().enabled = true;
            }
            else
                StartCoroutine(KohtaJutellaan());
          //  GetComponent<ActionTrigger>().enabled = true;
            return;
        }


        if (CanGuard)
            _character._health.GuardianShieldsActive = true;
        else
            _character._health.GuardianShieldsActive = false;


       



        if (_character.IsFacingRight)
        {
            MiddlePointHolder.localScale = new Vector3(1, 1, 1);
            lookingright = true;

            TurnAxeAttack.transform.localPosition = new Vector2(AxePox.x, AxePox.y);

            if(_character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
                TurnAxeAttack.transform.localScale = new Vector3(1, 1, 1);
            else
                TurnAxeAttack.transform.localScale = new Vector3(-1, 1, 1);

            if (DroneIsHealing)
            {
                dronejoint.linearOffset = new Vector2(-0.85f, -3.25f);
                if ((jumping) && _character._controller.Speed.y >= 0)
                    dronejoint.linearOffset = new Vector2(-2f, 0f);

            }
               

        }
        else
        {
            lookingright = false;
            MiddlePointHolder.localScale = new Vector3(-1, 1, 1);
            
            TurnAxeAttack.transform.localPosition = new Vector2(-AxePox.x,AxePox.y);

            if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
                TurnAxeAttack.transform.localScale = new Vector3(-1, 1, 1);
            else
                TurnAxeAttack.transform.localScale = new Vector3(1, 1, 1);

            if (DroneIsHealing)
            {
                dronejoint.linearOffset = new Vector2(0.85f, -3.25f);
                if ((jumping) && _character._controller.Speed.y >= 0)
                    dronejoint.linearOffset = new Vector2(2f, 0f);
            }
              
        }

        
        
        _raycast = MMDebug.RayCast(transform.position, Vector2.up, 6, TargetLayerMask, Color.yellow, true);


        DistanceToEnemy = Vector2.Distance(new Vector3(transform.position.x,0), new Vector3(GameManager.Instance.CurrentPlayer.transform.position.x,0));
        _animator.SetFloat("Distance",DistanceToEnemy);

       

        if (jumping)
        {
            SaveInCaseOfStuckTimer = 0;


            if (_character._controller.Speed.y <= 0 && !jumpAttack)
            {
                jumpAttack = true;
                _character._controller.SetForce(Vector2.zero);
                _character._controller.GravityActive(false);
                DistanceToEnemy = Vector2.Distance(new Vector3(transform.position.x, 0), new Vector3(LevelMidPoint.position.x, 0));

                

                if (((DistanceToEnemy < 5 && Random.Range(1, 11) < 5) || Vector2.Distance(new Vector3(transform.position.x, 0), new Vector3(PelaajaPos.position.x, 0))> 13)&& jumpCombo<0)
                {
                    _animator.SetTrigger("AxeThrow");
                }
                else
                {
                    _animator.SetTrigger("GroundCharge");

                   if(_character._health.CurrentHealth<50)
                    {

                        if (Random.Range(1, 6) == 1)
                            jumpCombo = 0;
                    }
                }
            }
            else if(jumpAttack && _character._controller.Speed.y<0 && !_character._controller.State.IsCollidingBelow)
            {
                if (DistanceToEnemy < 1)
                    _character._controller.AddHorizontalForce(_character.IsFacingRight ? 0.25f : -0.25f);
                else if(DistanceToEnemy<2)
                    _character._controller.AddHorizontalForce(_character.IsFacingRight ? 1.5f : -1.5f);
                else if(DistanceToEnemy<5)
                    _character._controller.AddHorizontalForce(_character.IsFacingRight ? 2.5f : -2.5f);
                else 
                    _character._controller.AddHorizontalForce(_character.IsFacingRight ? 3.5f : -3.5f);
            }
            else if(jumpAttack && _character._controller.State.IsCollidingBelow)
            {

                jumping = false;
            }

        }

        

        if (!ReadyToAct && !running)
        {
            
            if(!jumping && !waiting && !_character._health.Invulnerable && _animator.GetCurrentAnimatorStateInfo(0).IsName("ThyrIdle"))
            {
                SaveInCaseOfStuckTimer += Time.deltaTime;

                if (SaveInCaseOfStuckTimer > 2f)
                {
                    StopAllCoroutines(); CancelInvoke();
                    waiting = false;
                    takinDamageTimer = 0;
                    print("pelastettu"+Time.timeSinceLevelLoad);
                    ReadyToAct = true;
                    SaveInCaseOfStuckTimer = 0;
                }
            }
            else
                SaveInCaseOfStuckTimer = 0;


            return;
        }
        else if(running && !untilend)
        {
            SaveInCaseOfStuckTimer = 0;
            if ((!GameManager.Instance.IsInFront(_character) && DistanceToEnemy>1f)|| _raycast)
            {
                randomizer = Random.Range(1, 3);
                
                if (randomizer == 1 || _raycast)
                {

                    ReadyToAct = false;
                    running = false;
                    _walker.ChangeDirection();
                }
                else if (!GameManager.Instance.IsInFront(_character) && DistanceToEnemy > 1f && randomizer == 2)
                {
                    StartJump();
                }
                else
                    untilend = true;
            }

            return;
        }
        else if(running && DistanceToEnemy>6 && !GameManager.Instance.IsInFront(_character))
        {
            ReadyToAct = false;
            running = false;
            _walker.ChangeDirection();
            untilend = false;

        }

        if(ReadyToAct)
        SaveInCaseOfStuckTimer = 0;

        if (untilend)
        {
            if (_character.MovementState.CurrentState != CharacterStates.MovementStates.Idle)
                return;
            else
            {
                print("unblock"); untilend = false;
            }
               

        }
            


       
        randomizer = Random.Range(1, 5);
        if(randomizer==1 && ReadyToAct)
        { 
            CanGuard = false;
            StartJump();
        }
        else if(GameManager.Instance.IsInFront(_character) && ReadyToAct)
        {
            CanGuard = false;
            if(GuardKerroin>1)
            {
                GuardKerroin = 0;
                Invoke("AllowGuard", 0.33f);
            }

            if (randomizer == 2)
                untilend = true;

            running = true;
            ReadyToAct = false;
            _animator.SetTrigger("StartRun");
            _walker.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);

            if (!HealDrone.activeInHierarchy && _character._health.CurrentHealth <= 120 && DroneTimer <= 0)
                HealDroneHealme();

        }
        else if (!GameManager.Instance.IsInFront(_character))
        {
            ReadyToAct = false;
            _walker.ChangeDirection();
        }


    }

    IEnumerator KohtaJutellaan()
    {
        yield return new WaitForSeconds(2f);

        ConversationManager.Instance.StartRadioConversation("s5");
    }



    public GameObject FlyingAxe;
    public GameObject FlyingAxePosition;
    

    void Juttelu()
    {
        GameManager.Instance.StopCharacter();
        ConversationManager.Instance.StartRadioConversation("s6");

    }

    void BoomBoomBoom()
    {
        jumping = false;
        jumpAttack = false;
        GhostOff();
        if (!landBoom.activeInHierarchy)
            landBoom.SetActive(true);
        else
        {
            landBoom.transform.GetChild(0).gameObject.SetActive(true);
        }
            

        if (_character.IsFacingRight)
            landBoom.transform.localScale = new Vector3(1, 1, 1);
        else
            landBoom.transform.localScale = new Vector3(-1, 1, 1);

        
    }
    void NoMoreBoom()
    {
        landBoom.SetActive(false);
        landBoom.transform.GetChild(0).gameObject.SetActive(false);
    }

    public GameObject pomppuhomppu;
    void FacePlayer()
    {
        if (_character.IsFacingRight && PelaajaPos.position.x < transform.position.x)
            _character.HEREWEWILLFLIPFLIP_AI();
        else if (!_character.IsFacingRight && PelaajaPos.position.x > transform.position.x)
            _character.HEREWEWILLFLIPFLIP_AI();
    }

    void StartJump()
    {
        running = false;
        ReadyToAct = false;
        if (_raycastBehind)
        {

            _walker.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
            _walker.ChangeDirection();
        }
        else
        {
            
            _walker.SetDirection(Vector2.zero);
            _character._animator.Play("ThyrJumpijump");
            SoundManager.Instance.PlaySound(JumpSound, transform.position);
        }


    }

    void AirChargeSound()
    {
        SoundManager.Instance.PlaySound(ParrySound, transform.position);
    }
    void AirMeteorSound()
    {
        SoundManager.Instance.PlaySound(Meteor, transform.position);
    }

    public bool jumping = false;
    void NOWJump()
    {

        jumpAttack = false;
        jumping = true;
        
        

        _jumper.JumpStart();


        Instantiate(pomppuhomppu, _character._controller.BoundsBottom, transform.rotation);

    }

    void AxeOn()
    {
        TurnAxeAttack.gameObject.SetActive(true);
    }
    void AxeOff()
    {
        TurnAxeAttack.gameObject.SetActive(false);
    }
    bool CanDoBigassAttack = true;
    public void BIGASSSWORDATTACK()
    {
        CanDoBigassAttack = false;
        running = false;
        ReadyToAct = false;
        _animator.Play("ThyrUlti");
        
    }

    public void ToggleHealEffect()
    {
        if (!DroneIsHealing)
        {
            StartCoroutine(HealdroneHeals());
        }
        else
        {
            DroneIsHealing = false;
        }
    }

    public bool waiting = false;
    float takinDamageTimer = 0;
    IEnumerator CanActSoon(float penalty)
    {
        if (takinDamageTimer != 0)
        {
            yield break;
        }


        waiting = true;
        //penalty = 5;
        takinDamageTimer = 0;
        while (takinDamageTimer < penalty)
        {
            yield return new WaitForEndOfFrame();

        }
        waiting = false;
        takinDamageTimer = 0;
        ReadyToAct = true;
    }

    public void CallSmoothMove(Vector3 TargetPos)
    {
        if (_SmoothMove != null)
            StopCoroutine(_SmoothMove);

        _SmoothMove = MiddlePointSmoothMove(TargetPos);
        StartCoroutine(_SmoothMove);
    }

    IEnumerator MiddlePointSmoothMove(Vector3 TargetPos)
    {
        SmoothMoving = true;
        float Distance = Vector2.Distance(MiddlePoint.localPosition, TargetPos);

        while (Distance >= 0.05f)
        {
            MiddlePoint.localPosition = Vector3.MoveTowards(MiddlePoint.localPosition, TargetPos,0.05f);
            //Debug.Log(transform.localPosition +", "+TargetPos);
            yield return new WaitForSeconds(0f);
        }

        MiddlePoint.localPosition = TargetPos;

        SmoothMoving = false;
        yield return null;
    }
    bool Stancessa = false;
    void Stanceen()
    {
        Stancessa = true;
    }
    void Stancesta()
    {
        Stancessa = false;
    }
    
    bool backdash = false;
    
    void AnimationRecovery()
    {

        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
        {
            _character.HEREWEWILLFLIPFLIP_AI();
            _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

            _walker.SetDirection(Vector2.zero);

            StartCoroutine(CanActSoon(1f));
            CanGuard = true;

            GuardKerroin++;
            _animator.ResetTrigger("IronMan");
        }
        else if(jumpCombo>=0)
        {
            if (jumpCombo < 3)
            {
                jumpCombo++;
                StartJump();
            }
            else
            {
                jumpCombo = -1;
                if (GameManager.Instance.IsInFront(_character) && Random.Range(1, 6) == 1  && _character._health.CurrentHealth < 150)
                {
                    StopAllCoroutines(); CancelInvoke();
                    waiting = false;
                    takinDamageTimer = 0;
                    _walker._direction = Vector2.zero;
                    _animator.SetTrigger("IronMan");
                    SoundManager.Instance.PlaySound(IRONMAN, transform.position);
                }
                else
                {
                    _walker._direction = Vector2.zero;
                    CanGuard = true;
                    GuardKerroin = 0;
                    StartCoroutine(CanActSoon(1.0f));
                }
            }


        }
        else
        {
            int randomi = Random.Range(1, 6);
            if (GameManager.Instance.IsInFront(_character) && randomi == 2 && _character._health.CurrentHealth < 150)
            {
                StopAllCoroutines(); CancelInvoke();
                waiting = false; takinDamageTimer = 0;
                _walker._direction = Vector2.zero;
                _animator.SetTrigger("IronMan"); SoundManager.Instance.PlaySound(IRONMAN, transform.position);
            }
            else if (GameManager.Instance.IsInFront(_character) && DistanceToEnemy<=5.5 && randomi >= 3 && _character._health.CurrentHealth < 110 && CanDoBigassAttack)
            {
                StopAllCoroutines(); CancelInvoke();
                waiting = false; takinDamageTimer = 0;
                _walker._direction = Vector2.zero;
                BIGASSSWORDATTACK();

            }
            else if(GameManager.Instance.IsInFront(_character))
            {
                _walker._direction = Vector2.zero;
                CanGuard = true;
                GuardKerroin = 0;


                StartCoroutine(CanActSoon(1.0f));
               
            }
            else
            {

                CanGuard = false;
                _walker.ChangeDirection();
            }
        
        }

       
        untilend = false;
        running = false;



    }
    Health HealDroneHealth;

    public GameObject InitialDeeSwordAttack;
    public GameObject LastFramesSwordAttack;
    void InitialSwordAttack()
    {
        SoundManager.Instance.PlaySound(ParryAndRankaise, transform.position);
        InitialDeeSwordAttack.gameObject.SetActive(true);
        if (_character.IsFacingRight)
            InitialDeeSwordAttack.transform.localScale = new Vector3(1, 1, 1);
        else
            InitialDeeSwordAttack.transform.localScale = new Vector3(-1, 1, 1);

    }
    void IronManBeam()
    {
      
        IronBeam.SetActive(true);
        if (_character.IsFacingRight)
        {
            IronBeam.transform.localPosition = new Vector3(9.71f, -0.968f, 0);
            IronBeam.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            IronBeam.transform.localScale = new Vector3(-1, 1, 1);
            IronBeam.transform.localPosition = new Vector3(-9.71f, -0.968f, 0);
        }
    }
    void IronManBeamOff()
    {
        IronBeam.SetActive(false);
    }
    public GameObject IronBeam;
    void SwordHIT()
    {
        InitialDeeSwordAttack.gameObject.SetActive(false);
        if (_character.IsFacingRight)
            SwordAttack.localScale = new Vector3(1,1,1);
        else
            SwordAttack.localScale = new Vector3(-1, 1, 1);

        SwordAttack.gameObject.SetActive(true);
    }
    void SwordCLOSE()
    {
        if (_character.IsFacingRight)
            LastFramesSwordAttack.transform.localScale = new Vector3(1, 1, 1);
        else
            LastFramesSwordAttack.transform.localScale = new Vector3(-1, 1, 1);
        LastFramesSwordAttack.SetActive(true);
        SwordAttack.gameObject.SetActive(false);
        
    }
    void SwordFinalClose()
    {
       
        LastFramesSwordAttack.SetActive(false);
    }
    float DroneTimer = 0;

    void HealDroneHealme()
    {
        
        StartCoroutine(HealdroneHeals());
    }
    void GhostOff()
    {
        _ghost.trailSize = 0;
    }
    void GhostOn()
    {
        _ghost.trailSize = 5;
    }
    public bool DroneIsHealing = false;

    RelativeJoint2D dronejoint;
    IEnumerator HealdroneHeals()
    {
        HealDroneHealth.CurrentHealth = HealDroneHealth.MaximumHealth;
        GameObject.Find("DroneSpawner").GetComponent<SpriteMask>().enabled = true;
        HealDrone.transform.position = droneStartPosition;
        dronejoint = HealDrone.GetComponent<RelativeJoint2D>();
        CoroChainSpawn = true;

        chainparticles = new ParticleSystem.Particle[ChainLightParticle.main.maxParticles];
        rollerparticle = new ParticleSystem.Particle[ChainLightRoller.main.maxParticles];
        ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        RollerCount = ChainLightRoller.GetParticles(rollerparticle);

        HealDrone.GetComponent<Rigidbody2D>().gravityScale = 0;
        dronejoint.enabled = true;
        if (_character.IsFacingRight)
        {
            dronejoint.linearOffset = new Vector2(-0.85f, -4.5f);
            //HealDrone.transform.localPosition = new Vector3(0.765f, 1.075f, 0);
        }
        else
        {
            dronejoint.linearOffset = new Vector2(0.85f, -4.5f);
          //  HealDrone.transform.localPosition = new Vector3(-0.765f, 1.075f, 0);
        }
        
        HealDrone.SetActive(true);
        HealDrone.GetComponent<Animator>().Play("Stranger_drone_heilunta");
        //ChainLightParticle.SetParticles(chainparticles, ParticleCount);
        yield return new WaitForSeconds(0.25f);
        HealDrone.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;

        HealDrone.transform.SetParent(null);
        
        DroneIsHealing = true;


        healloop = SoundManager.Instance.PlaySound(healLoopSound, transform.position,true);
        SoundManager.Instance.PlaySound(DroneHeal, transform.position);
        GameObject.Instantiate(GameManager.Instance.PlayerCharacter._health._nanobots.NanoEffect, transform.position, Quaternion.identity, transform);

        
        //Debug.Log("HealDrone childCount: " + HealDrone.transform.childCount);
        HealDronePos = HealDrone.gameObject.transform;
        ChainLightDroneGlowTausta.transform.SetParent(HealDronePos.transform);
        ChainLightDroneGlowTausta.transform.position = new Vector3(HealDronePos.transform.position.x,HealDronePos.transform.position.y-0.25f);

        ChainLightStrangerGlowTausta.transform.SetParent(MiddlePoint);
        ChainLightStrangerGlowTaustaTall.transform.SetParent(MiddlePoint);

        ChainLightStrangerGlowTausta.transform.localScale = Vector3.one;
        ChainLightStrangerGlowTaustaTall.transform.localScale = Vector3.one;
        
        ChainLightStrangerGlowTausta.transform.position = MiddlePoint.position;
        ChainLightStrangerGlowTaustaTall.transform.position = MiddlePoint.position;
        DrawQuadraticCurve();
        //float Timing = 1f;
        // StartCoroutine(ChainLight());
     //   StartCoroutine(StartChainLight());
       
       
      
    }

    public AudioClip AxeHitSound;
    public AudioClip AxeThrowSound;
    public AudioClip AxeSpinLoop;
    AudioSource AxeSpinSource;
    AudioSource JetSource;
    public AudioClip JetStart;
    public AudioClip JetEnd;
    public AudioClip JetLoop;
    public AudioClip ParrySound;
    public AudioClip ParryAndRankaise;
    public AudioClip IRONMAN;
    public AudioClip JumpSound;
    public AudioClip DroneHeal;
    public AudioClip Meteor;
    public AudioClip defeat;
    public AudioClip victory;
    public AudioClip UltiSound;

    AudioSource healloop;
    public AudioClip healLoopSound;

    void PlayJetStartSound()
    {
        SoundManager.Instance.PlaySound(JetStart, transform.position);
        JetSource = SoundManager.Instance.PlaySound(JetLoop, transform.position, true);
    }
    void PlayJetEndSound()
    {
        SoundManager.Instance.PlaySound(JetEnd, transform.position);
        SoundManager.Instance.StopLoopingSound(JetSource);
        
    }

    void PlayAxeThrow()
    {
        SoundManager.Instance.PlaySound(AxeThrowSound, transform.position);
    }
    void StartAxeLoop()
    {
        AxeSpinSource= SoundManager.Instance.PlaySound(AxeSpinLoop, transform.position, true);
    }

    public void PlayUltiSound()
    {
        SoundManager.Instance.PlaySound(UltiSound, transform.position);
    }
    public void PlayAxeSound()
    {
        SoundManager.Instance.PlaySound(AxeHitSound, transform.position);
    }


    public void StopLoops()
    {
        
        _character._characterHorizontal.RunStopSouind();
    }

    public Vector3 droneStartPosition;
       IEnumerator StartChainLight()
    {
        int Loc = 0;
        //float Timer = 0;
        UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
        //HandleAnimPositioning();
        DrawQuadraticCurve();
        //ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        bool Handler = false;
        ChainLightDroneGlowTausta.Emit(GlowTiedot, 3);
        //StartCoroutine(ChainLightHandler());

        int RolPos = 0;

        
       EmitTiedot.position = ChainPos[0];
        ChainLightRoller.Emit(EmitTiedot, 1);
        RollerCount = ChainLightRoller.GetParticles(rollerparticle);

        while (RolPos < ChainPos.Length && RollerCount > 0)
        {
            RollerCount = ChainLightRoller.GetParticles(rollerparticle);


            rollerparticle[0].position = ChainPos[RolPos];

            RolPos += 2;

            ChainLightRoller.SetParticles(rollerparticle, RollerCount);

            yield return null;
        }
        rollerparticle[0].remainingLifetime = -1f;
        ChainLightRoller.SetParticles(rollerparticle, RollerCount);
        yield return null;
        
      
        EmitTiedot.position = ChainPos[0];
        ChainLightRoller.Emit(EmitTiedot, 1);
        RollerCount = ChainLightRoller.GetParticles(rollerparticle);
        while (Loc < numPoints)
        {
            //HandleAnimPositioning();
            UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
            DrawQuadraticCurve();
            SpawnChainParticle(ChainPos[Loc]);

            //if (Random.Range(0,2) == 0)
            //{
                PixelTiedot.position = ChainPos[Random.Range(0,Loc)];
                ChainLightPixel.Emit(PixelTiedot, 1);
            //}
            
            if (Loc < numPoints - 4)
            {
                SpawnChainParticle(ChainPos[Loc + 1]);
                SpawnChainParticle(ChainPos[Loc + 2]);
                SpawnChainParticle(ChainPos[Loc + 3]);
                SpawnChainParticle(ChainPos[Loc + 4]);
                Loc +=4;
            }
            
            
            Loc++;
            if (Loc<numPoints)
            rollerparticle[0].position = ChainPos[Loc];
            ChainLightRoller.SetParticles(rollerparticle, RollerCount);
            if (!Handler)
            {
                //Debug.LogWarning(ParticleCount);
                Handler = true;
                StartCoroutine(ChainLightHandler());
                
            }
            
            yield return null;
        }
        
        //StartCoroutine(ChainLightHandler());
        
        for (int i = 0;i<ParticleCount;i++)
        {
            chainparticles[i].remainingLifetime = 0.1f;
        }
        rollerparticle[0].remainingLifetime = -1f;

        ChainLightParticle.SetParticles(chainparticles, ParticleCount);
        ChainLightRoller.SetParticles(rollerparticle, RollerCount);
        /*while (Timer < 1.99f)
        {
            Timer += Time.deltaTime;
            yield return null;
        }*/

        StartCoroutine(ChainLight());
        yield return null;
    }

   /* IEnumerator EndChainLight()
    {
        ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        for (int i = 0; i < ParticleCount; i++)
        {
            chainparticles[i].remainingLifetime = 0.5f;
        }
        int Positioner = 0;
        
        //ChainLightParticle.SetParticles(chainparticles, ParticleCount);
        //ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        while (ParticleCount > 0 && Positioner < numPoints)
        {
            if (Positioner >= numPoints / 2)
            {
                Positioner = 0;
            }
            Debug.LogWarning("ENDCHAIN"+Positioner);
            ParticleCount = ChainLightParticle.GetParticles(chainparticles);
            DrawQuadraticCurve();
            UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
            HandleAnimPositioning();
            ParticleCount = ChainLightParticle.GetParticles(chainparticles);
            Debug.Log("Counter: " + Positioner + ", pCount: " + ParticleCount);
            chainparticles[Positioner].remainingLifetime = -1f;
            //chainparticles[Positioner].position = new Vector3()
            if (Positioner < numPoints-2 && Positioner < numPoints/2-2)
            {
                chainparticles[Positioner + 1].remainingLifetime = -1f;
                chainparticles[Positioner + 2].remainingLifetime = -1f;
                chainparticles[Positioner+1].position = ChainPos[Positioner+1];
                chainparticles[Positioner+1].position = ChainPos[Positioner+2];
                Positioner += 2;
            }
            
            Positioner++;
            //ParticleCount = ChainLightParticle.GetParticles(chainparticles);
            ChainLightParticle.SetParticles(chainparticles, ParticleCount);
            
            yield return null;
        }

        yield return null;
    }*/

    

    IEnumerator EndChainLight2()
    {

        RollerCount = ChainLightRoller.GetParticles(rollerparticle);

        if (RollerCount > 0)
        {
            rollerparticle[0].remainingLifetime = -1f;
            ChainLightRoller.SetParticles(rollerparticle, RollerCount);
        }

        ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        for (int i = 0; i < ParticleCount; i++)
        {
            chainparticles[i].remainingLifetime = 2f;
        }
       

        //ChainLightParticle.SetParticles(chainparticles, ParticleCount);
        //ParticleCount = ChainLightParticle.GetParticles(chainparticles);
        while (ParticleCount > 0)
        {
            ParticleCount = ChainLightParticle.GetParticles(chainparticles);
            DrawQuadraticCurve();
            UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
            //HandleAnimPositioning();
            for (int i=0;i<4;i++)
            {
                int Chosen = Random.Range(0, ParticleCount);
                chainparticles[Chosen].remainingLifetime = -1f;
                PixelTiedot.position = ChainPos[Chosen];
                ChainLightPixel.Emit(PixelTiedot, 1);

            }
            ChainLightParticle.SetParticles(chainparticles, ParticleCount);

            yield return null;
        }

        yield return null;
    }

    IEnumerator ChainLight()
    {
        //ChainLightRoller.Emit(PixelTiedot, 1);
        float Timing = 2f;
     //   int i = 0;
        UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
        ChainLightDroneGlowTausta.Emit(GlowTiedot, Random.Range(3, 5));
        DrawQuadraticCurve();
        //StartCoroutine(ChainLightHandler());
        while (DroneIsHealing)
        {
            
            Timing += Time.deltaTime;
            DrawQuadraticCurve();
            UnderHealDrone = new Vector3(HealDrone.transform.position.x, HealDrone.transform.position.y - 2f);
            //HandleAnimPositioning();

            if (Timing>=1.99f)
            {
                CoroChainSpawn = true;
                Timing = 0f;
                //GlowTiedot.position = HealDronePos.position;
                ChainLightDroneGlowTausta.Emit(GlowTiedot, Random.Range(3,5));

                if (Lightroll != null && ParticleCount > 0)
                {
                    int rolCount = ChainLightRoller.GetParticles(rollerparticle);
                    if (rolCount > 0)
                    {
                        rollerparticle[0].remainingLifetime = -1f;
                        ChainLightRoller.SetParticles(rollerparticle, rolCount);
                    }
                    StopCoroutine(Lightroll);
                }

                Lightroll = LightRoller();
                StartCoroutine(LightRoller());
                
            } else
            {
                CoroChainSpawn = false;
            }
            
            
            ColMin = _collider.bounds.min;
            ColMax = _collider.bounds.max;

            StrangerPixelTiedot.position = new Vector3(Random.Range(MiddlePoint.position.x - 0.4f, MiddlePoint.position.x + 0.4f), Random.Range(MiddlePoint.position.y - 0.9f, MiddlePoint.position.y + 1.1f));
            StrangerGlowTiedot.position = new Vector3(Random.Range(-0.6f,0.6f),Random.Range(-1f,1.2f));

                ChainLightStrangerGlowTausta.Emit(StrangerGlowTiedot, 1);
                if (Random.Range(0,3)==0)
                ChainLightStrangerGlowTaustaTall.Emit(StrangerGlowTiedot, 1);


            //DrawQuadraticCurve(CoroChainSpawn); // VANHA

            //HandleQuadraticCall(CoroChainSpawn, 10);

            


            if (Random.Range(0,2) == 0)
            {
                PixelTiedot.position = ChainPos[Random.Range(0, ChainPos.Length)];
                ChainLightPixel.Emit(PixelTiedot, 1);
                ChainLightPixel.Emit(StrangerPixelTiedot, 1);
            }

            yield return null;
        }

        //StartCoroutine(EndChainLight());
        StartCoroutine(EndChainLight2());

        yield return null;
    }

    IEnumerator ChainLightHandler()
    {
       
        while (DroneIsHealing)
        {
            ParticleCount = ChainLightParticle.GetParticles(chainparticles);

            if (ParticleCount == 0)
            {
                SpawnChainLight();
                ParticleCount = ChainLightParticle.GetParticles(chainparticles);
            }
            int Counter = -1;
            for (int i = 0; i < ParticleCount; i++) // numPoints
            {
                
                if (Counter < ChainPos.Length || Counter < ParticleCount)
                    Counter++;
                else
                    Counter = 0;
                    
                //Debug.Log("Counter: " + Counter + ", pCount: "+ParticleCount);
                
                    chainparticles[i].position = ChainPos[Counter];
                    
                
            }
            ChainLightParticle.SetParticles(chainparticles, ParticleCount);
            /*for (int i = 0; i < ChainPos.Length; i++) // numPoints
            {

                chainparticles[i].position = ChainPos[i];
                ChainLightParticle.SetParticles(chainparticles, ParticleCount);

            }*/
            yield return null;
        }
      

            yield return null;
    }

    public void SpawnChainLight()
    {
        for (int i = 0; i < numPoints; i++)
        {
            SpawnChainParticle(ChainPos[i]);
        }
        ParticleCount = ChainLightParticle.GetParticles(chainparticles);
    }



    IEnumerator LightRoller()
    {
        int RolPos = 0;
        EmitTiedot.position = ChainPos[0];
        ChainLightRoller.Emit(EmitTiedot, 1);
        RollerCount = ChainLightRoller.GetParticles(rollerparticle);

        while (RolPos < ChainPos.Length && RollerCount > 0)
        {
            RollerCount = ChainLightRoller.GetParticles(rollerparticle);
            

            rollerparticle[0].position = ChainPos[RolPos];

            RolPos += 2;

            ChainLightRoller.SetParticles(rollerparticle, RollerCount);

            yield return null;
        }
        RollerCount = ChainLightRoller.GetParticles(rollerparticle);
        if (RollerCount >0)
        {
            rollerparticle[0].remainingLifetime = -1f;
            ChainLightRoller.SetParticles(rollerparticle, RollerCount);
        }
        
        yield return null;
    }

    void HandleQuadraticCall(bool SpawnLoop, int Divisions) // Jaa 5
    {
        //DivisionSize: 
        int Size = numPoints / Divisions;
        int StartIndex = new int();
        for (int i=0;i<Divisions;i++)
        {
            /*if (i == 0 || i == Divisions-1)
            {
                StartIndex = i * Size;
            }else
            {
                StartIndex = i * Size + 1;
            }*/
            StartIndex = i * Size;
            
            int EndIndex = StartIndex + Size;

            DrawQuadraticSegment(SpawnLoop, StartIndex, EndIndex);
        }


    }

    void DrawQuadraticSegment(bool SpawnLoop, int StartIndex, int EndIndex)
    {
        ParticleCount = ChainLightParticle.GetParticles(chainparticles);

        if (ParticleCount == 0)
        {
            SpawnLoop = true;
        }

        for (int i = StartIndex; i<EndIndex;i++)
        {
            float t = i / (float)numPoints;
            ChainPos[i] = CalculateQuadraticBezierPoint(t, HealDronePos.position, UnderHealDrone, MiddlePoint.position);

            if (SpawnLoop)
            {
                SpawnChainParticle(ChainPos[i]);
                ParticleCount = ChainLightParticle.GetParticles(chainparticles);
                //chainparticles[i].position = ChainPos[i]; // ?
                //ChainLightParticle.SetParticles(chainparticles, ParticleCount); // ?
                Debug.LogWarning("SpawnCHAIN");
            }

            //else
            //{
                chainparticles[i].position = ChainPos[i];
                ChainLightParticle.SetParticles(chainparticles, ParticleCount);
            //}
        }

    }
    void DrawQuadraticCurve()   // Laskee reitin spawnattaville efekteille strangerin ja healdronen välillä
    {
        for (int i = 0; i < numPoints; i++)
        {
            float t = i / (float)numPoints;
            ChainPos[i] = CalculateQuadraticBezierPoint(t, HealDronePos.position,UnderHealDrone, MiddlePoint.position);
        }
        //Debug.LogWarning("numPoints:"+numPoints);
       
    }
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float t2 = t * t;
        float u2 = u * u;
        Vector3 point = u2 * p0;
        point += 2 * u * t * p1;
        point += t2 * p2;
        return point;
    }
    public void SpawnChainParticle(Vector3 pos)
    {
        EmitTiedot.position = pos;
        ChainLightParticle.Emit(EmitTiedot, 1);
    }

    private Vector3 GetCross(Vector3 a, Vector3 b, Vector3 c)
    {
        if (!lookingright)
        {
            a.x -= 2*a.x;
        }
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;

        return Vector3.Cross(side1, side2).normalized;
    }

    void AllowGuard()
    {
        CanGuard = true;
    }
    public virtual void OnMMEvent(MMDamageTakenEvent damaget)
    {
        

        if (damaget.AffectedCharacter == null)
            return;
        if (damaget.AffectedCharacter.name != name)
            return;

        if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            return;


     
        if (jumping || !_character._controller.State.IsCollidingBelow)
            return;



        if (PassGuard)
        {
            PassGuard = false;
            CanPunish = true;
        }
            
    }



}
