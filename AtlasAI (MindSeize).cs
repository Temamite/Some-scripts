using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;



public class AtlasAI : MonoBehaviour
{

    protected float DistanceToEnemy = 0f;

    /// The layers the agent will try to shoot at
    public LayerMask TargetLayerMask;
    public LayerMask PlatformLayerMask;

    public GameObject Shockwaves;

    protected DamageOnTouch _damage;


    protected CharacterHandleWeapon _characterShoot;
    protected AIWalk _walk;
    protected RaycastHit2D _raycast;
    protected RaycastHit2D _raycastBehind;

    public Transform DukePunch;

    [HideInInspector]
    public Character _character;
    [HideInInspector]
    public Collider2D _meleeCircleHit;
    //  [HideInInspector]
    // public Collider2D _overlapCircleHitMoreRange;


    [HideInInspector]
    public int randomizer = 1;
   
    [HideInInspector]
    public GameObject playa = null;
    [HideInInspector]
    public bool lookingright = true;

    public float MeleeDistance = 3;
    protected CharacterJump _jumpster;
    public bool running = false;
    public bool jumping = false;
    protected int runCounter = 0;
    public bool ReadyToAct = false;

    public AudioClip MeteorSound;
    public AudioClip GrabSound;
    public AudioClip PunchSound;
    public AudioClip GettingStartedSound;
    public AudioClip ThrowSound;
    public AudioClip JumpSound;
    public AudioClip GrenadeSound;

    private void Start()
    {
        playa = GameObject.FindWithTag("Player");
        _jumpster = GetComponent<CharacterJump>();
        _character = GetComponent<Character>();
        _characterShoot = GetComponent<CharacterHandleWeapon>();
        _walk = GetComponent<AIWalk>();
        _damage = GetComponent<DamageOnTouch>();
        ReadyToAct = false;
        grenadeInt = 0;
        

        if (_character.IsFacingRight)
        {
            lookingright = true;
            DukePunch.localScale = new Vector3(1, 1, 1);
        }

        else
        {
            lookingright = false;
            DukePunch.localScale = new Vector3(-1, 1, 1);
        }
    }

    public AudioClip secondaryStart;
    void PlayJumpSound()
    {
        SoundManager.Instance.PlaySound(JumpSound, transform.position);
    }
    void PlaySecondayPunch()
    {
        SoundManager.Instance.PlaySound(secondaryStart, transform.position);
    }
    void PlayMeteorSound()
    {
        SoundManager.Instance.PlaySound(MeteorSound, transform.position);
    }
    void PlayGrabSound()
    {
        SoundManager.Instance.PlaySound(GrabSound, transform.position);
    }
    void PlayGettingStarted()
    {
        SoundManager.Instance.PlaySound(GettingStartedSound, transform.position);
    }
    void PlayExtraWaveSound()
    {
        SoundManager.Instance.PlaySound(_character._health.ExtraSoundSfx,transform.position);
    }


    public void NaturalDirectionChange()
    {
        
        runCounter = 0;
        grenadeInt = 0;
        jumping = false;
        _walk.ChangeDirection();
        AfterJumpTurn = false;
        if(!waiting)
        StartCoroutine(CanActSoon(1.5f));
    }


    public int phase = 1;

    float takinDamageTimer = 0;


    int threshold = 0;

    public void DamageStun(int damage)
    {   
            
        if(damage>threshold ||damage>10)
        {
            
            threshold = 20;
            takinDamageTimer -= damage / 7;
           
        }
       

       
        
    }

    bool throwstarted = false;
    Vector2 grabposi;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag != "Player")
            return;

        
            

        if (throwstarted || GameManager.Instance.PlayerCharacter.MovementState.CurrentState == CharacterStates.MovementStates.WallClinging|| GameManager.Instance.PlayerCharacter.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead || !_character._controller.State.IsGrounded)
            return;

        GameManager.Instance.PlayerCharacter.GetComponent<CharacterLevelBounds>().Bottom = CharacterLevelBounds.BoundsBehavior.Nothing;

        if(_character.IsFacingRight)
            grabposi = new Vector2(transform.position.x + 1.41f, transform.position.y - 1f);
        else
            grabposi = new Vector2(transform.position.x - 1.41f, transform.position.y - 1f);

        if ((_character._controller.Speed.x > 1 || _character._controller.Speed.x < -1) && _character.MovementState.CurrentState == CharacterStates.MovementStates.Walking && _character.MovementState.CurrentState != CharacterStates.MovementStates.Flip && collision.gameObject.tag=="Player" && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)) )
        {

            if (GameManager.Instance.PlayerCharacter.MovementState.CurrentState == CharacterStates.MovementStates.AirDashing)
            {
                GameManager.Instance.PlayerCharacter.MovementState.ChangeState(CharacterStates.MovementStates.Falling);
                GameManager.Instance.CurrentPlayer.GetComponent<CharacterAirDash>().StopDash();
            }
            else if (GameManager.Instance.PlayerCharacter.MovementState.CurrentState == CharacterStates.MovementStates.Dashing)
                GameManager.Instance.CurrentPlayer.GetComponent<CharacterDash>().StopDashing();

            GameManager.Instance.CurrentPlayer.GetComponent<CharacterHandleWeapon>().ShootStop();
            GameManager.Instance.CurrentPlayer.GetComponent<CharacterHandleWeaponMelee>().DamageTakeForceExit();



            GameManager.Instance.PlayerCharacter.flickering = false;
            GameManager.Instance.PlayerCharacter._health.DamageDisabled();
            throwstarted = true;
            _walk._direction = Vector2.zero;
            _character._animator.Play("Hulk_grab");

            if(GameManager.Instance.PlayerCharacter._health._shield.CurrentShield>0)
            GameManager.Instance.PlayerCharacter._health.Damage(GameManager.Instance.PlayerCharacter._health._shield.CurrentShield/10, this.gameObject, 0, 0);


            GameManager.Instance.PlayerCharacter.Freeze();
            GameManager.Instance.CurrentPlayer.GetComponent<Animator>().SetBool("Suffering", true);
            GameManager.Instance.CurrentPlayer.transform.SetParent(transform);
          //   StartCoroutine(MoveQuicklyToPosition());
            GameManager.Instance.CurrentPlayer.transform.position = grabposi;

        }
        else
        {

            _damage.enabled = true;
        }
    }


    public GameObject pommi;


    IEnumerator MoveQuicklyToPosition()
    {
        
        while (Vector2.Distance(playa.transform.position,grabposi)>0.1f)
        {
            GameManager.Instance.CurrentPlayer.GetComponent<Animator>().SetBool("Suffering", true);
            playa.transform.position = Vector2.MoveTowards(playa.transform.position, grabposi, 15*Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }
        
        GameManager.Instance.PlayerCharacter._controller.SetForce(Vector2.zero);

    }
    void DukeHideBody()
    {
        playa.GetComponent<SpriteRenderer>().enabled = false;
        if (_character.IsFacingRight)
        {
            if (GameManager.Instance.PlayerCharacter.IsFacingRight)
                GameManager.Instance.PlayerCharacter.Flip();
            playa.transform.localPosition = new Vector2(-0.177f, 0.58f);
        }
            
        else
        {
            if (!GameManager.Instance.PlayerCharacter.IsFacingRight)
                GameManager.Instance.PlayerCharacter.Flip();
            playa.transform.localPosition = new Vector2(0.177f, 0.58f);

            
        }
       

    }

    void StartBodyturningforRectangleEliplayerikiehkieh()
    {
        GameManager.Instance.PlayerCharacter._animator.SetBool("Thrown", true);
        SoundManager.Instance.PlaySound(ThrowSound, transform.position);
    }

    void DukeGrabAndThrow()
    {
        GameManager.Instance.PlayerCharacter._health.DamageEnabled();
        GameManager.Instance.PlayerCharacter.GetComponent<CharacterLevelBounds>().Bottom = CharacterLevelBounds.BoundsBehavior.Kill;
        GameManager.Instance.PlayerCharacter.BeingThrown = true;
        playa.GetComponent<SpriteRenderer>().enabled = true;
        playa.transform.SetParent(null);
        GameManager.Instance.PlayerCharacter.UnFreeze();
        GameManager.Instance.PlayerCharacter.flickering = true;

        
        
        if(_character.IsFacingRight)
            GameManager.Instance.CurrentController.SetForce(new Vector2(-23, 15));
        else
            GameManager.Instance.CurrentController.SetForce(new Vector2(23, 15));
    }

    void StopThrowStarted()
    {
        throwstarted = false;
        _character.HEREWEWILLFLIPFLIP_AI();
        if (grenadeInt > 0)
            grenadeInt--;

        if(phase == 1)
        StartCoroutine(CanActSoon(0.5f));
        else
            StartCoroutine(CanActSoon(0.1f));

        jumping = false;
    }



    void RunStopTurn()
    {
        _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
        runCounter++;
        _walk.ChangeDirection();
        
    }

    bool Stop = false;
    int framecounter = 3;
    bool Dontbreakagain = false;

    void Die()
    {
        StopPunch();
        GameManager.Instance.DestoyedForever.Add(gameObject.name);
        pommi.SetActive(true);
        _character._animator.SetBool("Dead", true);
        GetComponent<HealthBar>()._animator.Play("bosshpdisappears");


        SoundManager.Instance.StopLoopingSound(GameObject.Find("BossEntrance").GetComponent<BossEntrance>().cheers);
        GameObject.Find("IRON CURTAIN").GetComponent<Animator>().SetTrigger("DOWN");
        SoundManager.Instance.PlaySound(IronCurtainCloseSound, transform.position);

        _character._health.NPCInfo.StartLogRoutine();

    }

    public AudioClip IronCurtainSound;
    public AudioClip IronCurtainCloseSound;
    // Update is called once per frame
    void Update()
    {

        if (throwstarted)
        {

            return;
        }
        if (_character.MovementState.CurrentState != CharacterStates.MovementStates.Flip && ( _character._controller.Speed.x>2 || _character._controller.Speed.x<-2) && _character._controller.State.IsGrounded)
        {
            _damage.enabled = false;
        }
        else
            _damage.enabled = true;


        if (waiting)
        {

            takinDamageTimer += Time.deltaTime;
            

        }

        

        if (playa == null)
        {
                playa = GameObject.FindWithTag("Player");
            
        }

        if (playa && GameManager.Instance.PlayerCharacter.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead && _character._controller.State.IsCollidingBelow)
        {
            if(!Stop)
            {
                _character.Freeze();
                Stop = true; _character._animator.Play("Hulk_voimailu");
            }
        

            return;
        }

        if (GameManager.Instance.SoftLock)
            return;

        if ((_character == null) || (_characterShoot == null)) { return; }

        if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
        {
            _characterShoot.ShootStop();
            return;
        }
        if (_character.IsFacingRight)
        {
            lookingright = true;
            DukePunch.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            lookingright = false;
            DukePunch.localScale = new Vector3(-1, 1, 1);
        }



        _raycast = MMDebug.RayCast(transform.position + Vector3.down*1.2f, (_character.IsFacingRight) ? transform.right : -transform.right, 2.5f, PlatformLayerMask, Color.cyan, true);

        _raycastBehind = MMDebug.RayCast(transform.position + Vector3.down * 1.2f, (_character.IsFacingRight) ? -transform.right : transform.right, 12, TargetLayerMask, Color.yellow, true);


            _meleeCircleHit = Physics2D.OverlapCircle(transform.position, MeleeDistance, TargetLayerMask);





        if (_raycast && !ReadyToAct && _character.MovementState.CurrentState==CharacterStates.MovementStates.Walking && !jumping && !Dontbreakagain)
        {
            if (_walk._characterHorizontalMovement.MovementSpeedMultiplier > 0.8f)
            {
                ReadyToAct = false;
                _walk._characterHorizontalMovement.MovementSpeedMultiplier = 0.49f;
                _character._animator.SetTrigger("RunEnd");
            }
        }
        else
        {
            
            _character._animator.ResetTrigger("RunEnd");
        }
            



        if (phase == 1 && !jumping)
        {
            if (ReadyToAct)
            {
                _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
                DistanceToEnemy = Vector2.Distance(transform.position, playa.transform.position);
                    randomizer = Random.Range(1, 11);

                   
                        if (DistanceToEnemy >= 7.5f && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                        {
                            if (randomizer == 9)
                            {
                                StartJump();
                            }
                            else if(randomizer == 10)
                            {
                                StartRun();
                             }
                            else
                            {
                                
                                if (grenadeInt < 2)
                                    StartGrenade();
                                else
                                {
                                if (randomizer <= 2)
                                    StartRun();
                                else
                                    StartJump();

                                }
                                   

                            }

                        }
                        else if (DistanceToEnemy < 7.5f && !_meleeCircleHit && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                        {

                            if (randomizer == 10)
                            {
                                 StartJump();
                            }
                            else if (randomizer >= 7)
                            {
                                 StartRun();
                            }
                            else
                            {
                                if (grenadeInt < 2)
                                    StartGrenade();
                                else if (randomizer >= 3)
                                {
                                    StartRun();
                                }
                                else
                                    StartJump();
                            }



                        }
                        else if (_meleeCircleHit)
                        {
                            if (grenadeInt < 2)
                            {
                                if (randomizer < 8 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                                    StartFist();
                                else
                                    StartJump();

                            }
                            else
                            {
                                    if (_character._controller.BackAgainstWall)
                                    {
                            if (randomizer > 8)
                                StartJump();
                            else if (randomizer > 4)
                                StartFist();
                            else
                                StartRun();
                                    }
                           
                                    else if (randomizer > 5 && _raycastBehind)
                                    {
                                        ReadyToAct = false;
                                        _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
                                        _walk.ChangeDirection();
                                    }
                                    else
                                        StartJump();
                    }
                        }
                        else if(_raycastBehind)
                {
                    _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
                    ReadyToAct = false;
                    _walk.ChangeDirection();
                }
                             



            }


        }
        else if (phase == 2 && !jumping)
        {
            if (ReadyToAct)
            {
                _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
                DistanceToEnemy = Vector2.Distance(transform.position, playa.transform.position);
                randomizer = Random.Range(1, 11);


                if (DistanceToEnemy >= 7.5f && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    if (randomizer == 8)
                    {
                        StartJump();
                    }
                    else if (randomizer >=9)
                    {
                        StartRun();
                    }
                    else
                    {

                        if(grenadeInt < 3)
                        {
                            StartGrenade();
                        }
                        else
                        {
                            if (randomizer <= 2)
                                StartRun();
                            else
                                StartJump();

                        }


                    }

                }
                else if (DistanceToEnemy < 7.5f && !_meleeCircleHit && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    if (randomizer == 10)
                    {
                        StartJump();
                    }
                    else if (randomizer >= 7)
                    {
                        StartRun();
                    }
                    else 
                    {
                        if (grenadeInt < 3)
                        {
                            
                            StartGrenade();
                        }
                        else if (randomizer >= 3)
                        {
                            StartRun();
                        }
                        else
                            StartJump();
                    }
                   



                }
                else if (_meleeCircleHit)
                {
                    if (grenadeInt < 3)
                    {
                        if (randomizer < 8 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                            StartFist();
                        else
                            StartJump();

                    }
                    else
                    {
                        if (_character._controller.BackAgainstWall)
                        {
                            if (randomizer > 7)
                                StartJump();
                            else if (randomizer > 4)
                            {
                                StartFist();
                            }
                            else
                                StartRun();
                        }

                        else if (randomizer > 7 && _raycastBehind)
                        {
                            ReadyToAct = false;
                            _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
                            _walk.ChangeDirection();
                        }
                        else if (randomizer > 4 && GameManager.Instance.IsInFront(_character))
                            StartFist();
                        else
                            StartJump();
                    }
                }
                else if (_raycastBehind)
                {
                    ReadyToAct = false; _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
                    _walk.ChangeDirection();
                }
                    





            }




        }



        




        if ((_raycastBehind || ((transform.position.x < playa.transform.position.x && !_character.IsFacingRight) || (transform.position.x > playa.transform.position.x && _character.IsFacingRight))) && runCounter <= 3 && !jumping && !AfterJumpTurn && _character.MovementState.CurrentState == CharacterStates.MovementStates.Walking && (_character._controller.Speed.x > 2 || _character._controller.Speed.x < -2))
        {
            if (_walk._characterHorizontalMovement.MovementSpeedMultiplier > 0.8f)
            {
                ReadyToAct = false;
                _walk._characterHorizontalMovement.MovementSpeedMultiplier = 0.3f;
                _character._animator.SetTrigger("RunStop");
            }


        }


        /*
        if (!ReadyToAct)
        {
            SaveInCaseOfStuckTimer += Time.deltaTime;

            if ((SaveInCaseOfStuckTimer > 2.5f && phase == 2) || (SaveInCaseOfStuckTimer>3 && phase ==1))
            {
                SaveInCaseOfStuckTimer = 0;
                ReadyToAct = true;
            }
        }
        else
            SaveInCaseOfStuckTimer = 0;

    */
        if (jumping)
        {
            if (_character._controller.Speed.y < 1f )
            {
                _jumpster.JumpStop();
                // _walk._direction = Vector2.zero;
                if ((transform.position.x < playa.transform.position.x && !_character.IsFacingRight) || (transform.position.x > playa.transform.position.x && _character.IsFacingRight))
                {
                    if(_walk._characterHorizontalMovement.MovementSpeedMultiplier > 0.1f)
                        _walk._characterHorizontalMovement.MovementSpeedMultiplier -= 0.065f;
                }

                _damage.DamageCaused = 20;
                _damage.MuuttumatonMaxDamage = 20;
            }

            if (_character._controller.State.IsGrounded && !_character._controller.State.WasGroundedLastFrame)
            {
                GameManager.Instance.MainCameraController.Shake(new Vector3(0.8f, 0.5f, 1f));
                _walk._characterHorizontalMovement.MovementSpeedMultiplier = 0f;
                jumping = false;
                AfterJumpTurn = true;

                _walk.SetDirection(Vector2.zero);
                _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                _character._animator.SetBool("Smash", true);
                    Invoke("AfterJumpFun", 0.8f);


                _damage.DamageCaused = 10;
                _damage.MuuttumatonMaxDamage = 10;
                Instantiate(Shockwaves, transform.position, Quaternion.identity);
                
            }
                
               
        }


      

    }


    bool JumiChecking = false;
    void EmergencyJumiCheck()
    {
        if(!JumiChecking)
        {
            JumiChecking = true;
            StartCoroutine(FixJumi());
        }
    }

    IEnumerator FixJumi()
    {
        yield return new WaitForSeconds(0.1f);
        float jumitimer = 0;
        while(jumitimer<0.5f)
        {
            yield return new WaitForEndOfFrame();
            jumitimer += Time.deltaTime;

            if (!_character._animator.GetCurrentAnimatorStateInfo(0).IsName("Hulk_idle"))
            {
                JumiChecking = false;
                yield break;
            }
        }
        if (jumping)
            jumping = false;

        Debug.Log("SAVED THE SITUATION!");
        StartCoroutine(CanActSoon(0.1f));
        yield return new WaitForSeconds(0.1f);
        JumiChecking = false;

    }



    float SaveInCaseOfStuckTimer = 0;
    public bool AfterJumpTurn = false;
    void AfterJumpFun()
    {
        _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
        _character._animator.SetBool("Smash", false);
        if(phase==1)
        {

            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;

                    if ((transform.position.x < playa.transform.position.x && !_character.IsFacingRight) || (transform.position.x > playa.transform.position.x && _character.IsFacingRight))
                    { 
                        _walk.ChangeDirection();

                    }

        }
        else
        {


            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
            if ((transform.position.x < playa.transform.position.x && !_character.IsFacingRight) || (transform.position.x > playa.transform.position.x && _character.IsFacingRight))
            {
                _walk.ChangeDirection();

            }
        }

        

    }

    void StartFist()
    {
        
        ReadyToAct = false;
        AfterJumpTurn = false;

        SoundManager.Instance.PlaySound(PunchSound, transform.position);
        _character._animator.Play("Hulk_punch");
    }

    void StartJump()
    {
        ReadyToAct = false;
        _raycast = MMDebug.RayCast(transform.position + Vector3.down, (_character.IsFacingRight) ? transform.right : -transform.right, 5, PlatformLayerMask, Color.red, true);
        if (_raycast && _raycastBehind)
        {

            _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
            _walk.ChangeDirection();
        }
        else
        {

            _walk.SetDirection(Vector2.zero);
            _character._animator.Play("Hulk_jump_start");
        }
           

    }

    void NOWJump()
    {

        


        

            DistanceToEnemy = Vector2.Distance(transform.position, playa.transform.position);



            if (DistanceToEnemy < 5)
            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 0.5f;
            else if (DistanceToEnemy < 8)
            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
            else
            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1.2f;


        AfterJumpTurn = false;
            
            _jumpster.JumpStart();
            _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);
            jumping = true;



        
    }
   

    void StartRun()
    {
        AfterJumpTurn = false;
        ReadyToAct = false;
        _walk.SetDirection(_character.IsFacingRight ? Vector2.right : Vector2.left);

    }

    void StartGrenade()
    {
        AfterJumpTurn = false;
        ReadyToAct = false;




        if (phase == 1 && grenadeInt == 0 && PhaseOneHidastuskranu && _character._health.CurrentHealth < 125)
        {
            _character._animator.Play("Duke_dukerang");
        }
        else if (phase == 2 && grenadeInt == 0 && _character._health.CurrentHealth < 125)
        {
            _character._animator.Play("Duke_dukerang");
        }
        else if(grenadeInt == 0)
        {
            _character._animator.Play("Hulk_first_grenade");
        }
        else
        {
            _character._animator.Play("Hulk_grenade");
        }


    }
    bool PhaseOneHidastuskranu = true;

    public GameObject BouncyMine;
    void HulkThrowGrenade()
    {
        SoundManager.Instance.PlaySound(GrenadeSound, transform.position);


        if(phase == 1 && grenadeInt == 0 && PhaseOneHidastuskranu && _character._health.CurrentHealth <125)
        {
            grenadeInt++;
            if (_character.IsFacingRight)
            {
                BouncyMine.GetComponent<ThrownObject>().Direction = (new Vector3(4, 1, 0));
                Instantiate(BouncyMine, transform.position + new Vector3(1.52f, 0.065f), Quaternion.identity);
                PhaseOneHidastuskranu = false;
            }
            else
            {
                BouncyMine.GetComponent<ThrownObject>().Direction = (new Vector3(-4, 1, 0));
                Instantiate(BouncyMine, transform.position + new Vector3(-1.52f, 0.065f), Quaternion.identity);
                PhaseOneHidastuskranu = false;
            }

        }
        else if(phase == 2 && grenadeInt == 0 && _character._health.CurrentHealth < 125 )
        {
            grenadeInt++;
            if (_character.IsFacingRight)
            {
                BouncyMine.GetComponent<ThrownObject>().Direction = (new Vector3(Random.Range(3f,5f), 1, 0));
                Instantiate(BouncyMine, transform.position + new Vector3(1.52f, 0.065f), Quaternion.identity);
            }
            else
            {
                
                BouncyMine.GetComponent<ThrownObject>().Direction = (new Vector3(Random.Range(-3f, -5f), 1, 0));
                Instantiate(BouncyMine, transform.position + new Vector3(-1.52f, 0.065f), Quaternion.identity);
            }
        }
        else
        {
            grenadeInt++;
            DistanceToEnemy = Vector2.Distance(transform.position, playa.transform.position);
            if (_characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().GetPooledGameObject())
            {


                if (50 * DistanceToEnemy > 260)
                    _characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().GetPooledGameObject().GetComponent<ThrownObject>()._initialSpeed = 50 * DistanceToEnemy;
                else
                    _characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().GetPooledGameObject().GetComponent<ThrownObject>()._initialSpeed = 260;




                if (_character.IsFacingRight)
                {
                    _characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().GetPooledGameObject().GetComponent<ThrownObject>().Direction = new Vector3(1.35f, 1.6f);
                }
                else
                {
                    _characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().GetPooledGameObject().GetComponent<ThrownObject>().Direction = new Vector3(-1.35f, 1.6f);
                }

            }



            _characterShoot.ShootStart();
        }
       
    }

    void HulkPunchInFace()
    {
        DukePunch.gameObject.SetActive(true);
    }
    void StopPunch()
    {
        DukePunch.gameObject.SetActive(false);
    }


    int grenadeInt = 0;
    void AnimationRecovery()
    {
        Dontbreakagain = false;
        _character._animator.ResetTrigger("RunStop");
        if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
        {
            _walk._characterHorizontalMovement.MovementSpeedMultiplier = 1f;
            _character.HEREWEWILLFLIPFLIP_AI();
            DukePunch.gameObject.SetActive(false);
            _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            if (runCounter == 0 && !AfterJumpTurn)
                _walk._direction = Vector2.zero;
            else if ((phase == 1 && !AfterJumpTurn))
            {
                
                
                if(runCounter==1&& ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    randomizer = Random.Range(1, 11);
                    if(randomizer>8)
                    {
                        if (_meleeCircleHit)
                        {
                            grenadeInt = 1;
                            _walk._direction = Vector2.zero;
                            StartFist();
                        }
                        else if (DistanceToEnemy > 4)
                        {
                            grenadeInt = 1;
                            _walk._direction = Vector2.zero;
                            StartGrenade();
                        }
                        else if(_walk._direction == Vector2.zero)
                        {
                            StartCoroutine(CanActSoon(0.25f));
                        }

                    }
                    else if (_walk._direction == Vector2.zero)
                    {
                        StartCoroutine(CanActSoon(0.25f));
                    }


                }
                else if(runCounter>1 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                { randomizer = Random.Range(1, 11);
                    if (randomizer > 5)
                    {
                        if (_meleeCircleHit && grenadeInt < 2)
                        {
                            grenadeInt = 1;
                            _walk._direction = Vector2.zero;
                            StartFist();
                        }
                        else if (DistanceToEnemy > 4)
                        {
                            grenadeInt = 1;
                            _walk._direction = Vector2.zero;
                            StartGrenade();
                        }
                        else if (_walk._direction == Vector2.zero)
                        {
                            StartCoroutine(CanActSoon(0.25f));
                        }

                    }
                    else if (_walk._direction == Vector2.zero)
                    {
                        StartCoroutine(CanActSoon(0.25f));
                    }
                }
                else
                {
                    _walk._direction = Vector2.zero;
                    StartCoroutine(CanActSoon(0.25f));
                }
                   

            }
            else if (phase == 2 && !AfterJumpTurn)
            {

                if (runCounter == 1 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    randomizer = Random.Range(1, 11);
                    if (randomizer > 7)
                    {
                        if (_meleeCircleHit)
                        {
                            grenadeInt = 2;
                            _walk._direction = Vector2.zero;
                            StartFist();
                        }
                        else if (DistanceToEnemy > 3.5f)
                        {
                            grenadeInt = 2;
                            _walk._direction = Vector2.zero;
                            StartGrenade();
                        }
                        else if (_walk._direction == Vector2.zero)
                        {
                            StartCoroutine(CanActSoon(0.25f));
                        }


                    }
                    else if (_walk._direction == Vector2.zero)
                    {
                        StartCoroutine(CanActSoon(0.25f));
                    }

                }
                else if (runCounter > 1 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    randomizer = Random.Range(1, 11);
                    if (randomizer > 5)
                    {
                        if (_meleeCircleHit && grenadeInt < 3)
                        {
                            grenadeInt = 2;
                            _walk._direction = Vector2.zero;
                            StartFist();
                        }
                        else if (DistanceToEnemy > 3.5f)
                        {
                            grenadeInt = 2;
                            _walk._direction = Vector2.zero;
                            StartGrenade();
                        }
                        else if (_walk._direction == Vector2.zero)
                        {
                            StartCoroutine(CanActSoon(0.25f));
                        }

                    }
                    else if (_walk._direction == Vector2.zero)
                    {
                        StartCoroutine(CanActSoon(0.25f));
                    }
                }
                else
                {
                    _walk._direction = Vector2.zero;
                    StartCoroutine(CanActSoon(0.25f));
                }


            }
            else if(phase == 2 && AfterJumpTurn && runCounter == 0)
            {
               
                    grenadeInt = 2;
                    runCounter = 3;
                    _walk._direction = Vector2.zero;
                 //   StartGrenade();

                StartCoroutine(CanActSoon(0.15f));

            }
            else if (phase == 1 && AfterJumpTurn && runCounter == 0)
            {

                grenadeInt = 1;
                runCounter = 3;
                _walk._direction = Vector2.zero;
                //   StartGrenade();

                StartCoroutine(CanActSoon(0.25f));

            }


        }
       
        else
        {
            if(phase == 1)
            {
                DukePunch.gameObject.SetActive(false);
                _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                if (_meleeCircleHit && grenadeInt < 2 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    StartFist();
                }
                else if (!_meleeCircleHit && grenadeInt < 2 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    StartGrenade();


                }
                else if (grenadeInt == 2 && !waiting)
                {
                    StartCoroutine(CanActSoon(0.5f));
                }
                else
                {
                    StartCoroutine(CanActSoon(0.25f));
                }

            }
            else if (phase == 2)
            {
                DukePunch.gameObject.SetActive(false);
                _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
                if (_meleeCircleHit && grenadeInt < 3 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    StartFist();
                }
                else if (!_meleeCircleHit && grenadeInt < 3 && ((transform.position.x < playa.transform.position.x && _character.IsFacingRight) || (transform.position.x > playa.transform.position.x && !_character.IsFacingRight)))
                {
                    StartGrenade();


                }
                else if (grenadeInt == 3 && !waiting)
                {
                    StartCoroutine(CanActSoon(0.4f));
                }
                else
                {
                    StartCoroutine(CanActSoon(0.2f));
                }

            }




        }


    }


    public void MOREPOWER()
    {

        phase = 2; 
        GameObject.Find("Spikesystem").GetComponent<Animator>().enabled = true;

        StartCoroutine(HealthFull());
    }
    
    IEnumerator HealthFull()
    {
        _walk.SetDirection(Vector2.zero);
        ReadyToAct = false;
        
        while(_character._health.CurrentHealth<_character._health.MaximumHealth)
        {
            _character._health.GetHealth(3, this.gameObject);
            yield return new WaitForEndOfFrame();
        }
        Destroy(transform.Find("HealthBar").gameObject);
      //  GameManager.Instance.currentBossBar = GetComponent<HealthBar>();
        StartCoroutine(CanActSoon(0.3f));
        GameManager.Instance.AllowContinue();
        
    }


    bool waiting = false;
    

    IEnumerator CanActSoon(float penalty)
    {


          

        waiting = true;
        //penalty = 5;
        takinDamageTimer = 0;
        while (takinDamageTimer < penalty)
        {
            yield return new WaitForEndOfFrame();

        }
        waiting = false;
        threshold = 0;
        takinDamageTimer = 0;
        ReadyToAct = true;
    }

    
}
