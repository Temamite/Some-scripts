using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.CorgiEngine
{
    /// <summary>
    /// Add this component to a CorgiController2D and it will try to kill your player on sight.
    /// </summary>
    [AddComponentMenu("Enemies/AlarmBotAI")]
    public class AlarmBotAI : MonoBehaviour, MMEventListener<MMDamageTakenEvent>
    {
        [Header("Behaviour")]
        [Information("Add this component to a CorgiController2D and it will try to kill your player on sight. This component requires a CharacterShoot component, and will simply tell it to press the trigger whenever a Player crosses its sight.", MoreMountains.Tools.InformationAttribute.InformationType.Info, false)]
        
        protected RaycastHit2D _raycast;

        public HackingPoint hackkohta;
        public List<GameObject> SummonedEnemies;

        public AlarmTeleporter[] teleportit;

        [HideInInspector]
        public Character _character;
        
        [HideInInspector]
        public bool animationlock = true;
       
        [HideInInspector]
        public bool aggrottu = false;

        protected LineRenderer _line;

        public bool MovingAlarmBot = false;
        CharacterHorizontalMovement walkster = null;

        /// the origin of the raycast used to detect obstacles
        public Vector3 RaycastOriginOffset;
        /// the origin of the visible laser
        public Vector3 LaserOriginOffset;
        /// the maximum distance to which we should draw the laser
        public float LaserMaxDistance = 50;
        /// the collision mask containing all layers that should stop the laser
        public LayerMask LaserCollisionMask;
        /// the width of the laser
        public Vector2 LaserWidth = new Vector2(0.05f, 0.05f);
        /// the material used to render the laser
        public Material LaserMaterial;
        public RaycastHit2D _hit;
        public GameObject _weapon;
        Animator _weaponAnimator;
        protected Vector3 _direction;

        protected Vector3 _origin;
        protected Vector3 _destination;

        public int PatternMode = 0;
        protected virtual void OnEnable()
        {
         

            this.MMEventStartListening<MMDamageTakenEvent>();
        }
        protected virtual void OnDisable()
        {
           
            this.MMEventStopListening<MMDamageTakenEvent>();

        }
        public virtual void OnMMEvent(MMDamageTakenEvent damaget)
        {

            if (damaget.AffectedCharacter == null)
                return;
            if (damaget.AffectedCharacter.name != name)
                return;

            if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
                return;

            if(GameManager.Instance.PlayerCharacter._drone.Haxing)
            hackkohta.ForceEyeDroneQuit();


            if (LookingForPlayer)
            {
                PlayerHit();
            }

        }


        /// <summary>
        /// on start we get our components
        /// </summary>
        protected virtual void Start()
        {

          

            _character = GetComponent<Character>();
            _line = gameObject.AddComponent<LineRenderer>();
            _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _line.receiveShadows = true;
            _line.startWidth = LaserWidth.x;
            _line.endWidth = LaserWidth.y;
            _line.material = LaserMaterial;


            if (MovingAlarmBot)
                walkster = GetComponent<CharacterHorizontalMovement>();

            _weaponAnimator = _weapon.GetComponent<Animator>();

            _weaponAnimator.SetInteger("PatternMode", PatternMode);
            if (DifferentAlarmBot)
                _weaponAnimator.SetInteger("PatternMode",4);
        }
        
        


        /// <summary>
        /// Every frame, check for the player and try and kill it
        /// </summary>
        protected virtual void Update()
        {
            /*  if(this.gameObject.name == "PunkbotBlob")
              {
                  if(_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
                  {
                      StartCoroutine("UnfreezeThis");
                  }
              } */
           



            if (GameManager.Instance.PlayerCharacter.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            {
                if (MovingAlarmBot)
                    walkster.MovementSpeedMultiplier = 0;

                

                return;
            }

            if (GameManager.Instance.SoftLock)
                return;

            if ((_character == null)) { return; }

            if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
                || (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen))
            {
                hackkohta.hackicon.SetActive(false);
                hackkohta.enabled = false;
                return;
            }


            if(HackTimer>0)
            {
                if (MovingAlarmBot)
                    walkster.MovementSpeedMultiplier = 0;
                HackTimer -= Time.deltaTime;

                if(HackTimer<=0)
                {
                    stun = false;
                    _character._animator.SetBool("Hacked", false);
                    StartLooking();
                    hackkohta.Invoke("Reset", 1f);
                    VVIcon.SetActive(false);
                }
                return;
            }
            

            if(LookingForPlayer)
                LaserOn();

            if (Alerting)
            {
                if (MovingAlarmBot)
                    walkster.MovementSpeedMultiplier = 0;


                if (Timer > 0)
                    Timer -= Time.deltaTime;
                else
                {
                    if(Time.frameCount%CheckCounter == 0)
                    {
                        int laske = 0;
                        foreach(GameObject obj in SummonedEnemies)
                        {
                            if (!obj.activeInHierarchy)
                                laske++;
                        }


                        if (laske >= SummonedEnemies.Count)
                        {
                           Alerting = false;
                          _character._animator.SetBool("Alarm", false);
                          SoundManager.Instance.StopLoopingSound(alarmu);
                            SummonedEnemies.Clear();
                            SummonedEnemies = new List<GameObject>();
                        }
                        else
                            laske = 0;
                    }

                   
                }
                    

                
            }


            


        }

        int CheckCounter = 5;

        void StartLooking()
        {
            PlaySound(3);
            Invoke("StartNow", 1f);
        }

        void StartNow()
        {
            _line.enabled = true;
            LookingForPlayer = true;
            hackkohta.enabled = true;
            hackkohta.hackicon.SetActive(true);

        }

        public AudioClip sound1;
        public AudioClip sound2;
        public AudioClip sound3;

        public bool DifferentAlarmBot = false;
        bool LookingForPlayer = true;
        bool Alerting = false;
        protected virtual void LaserOn()
        {
            if (MovingAlarmBot)
                walkster.MovementSpeedMultiplier = 1;
            






            // our laser will be shot from the weapon's laser origin
            _origin = _weapon.transform.position;
            _direction = Vector2.down;

            // we cast a ray in front of the weapon to detect an obstacle
            _hit = MMDebug.RayCast(_origin, _weapon.transform.rotation * _direction, LaserMaxDistance, LaserCollisionMask, Color.cyan, true);

            // if we've hit something, our destination is the raycast hit
            if (_hit)
            {
                
                _destination = _hit.point;

                if(_hit.collider.gameObject && _hit.collider.gameObject.layer == 27)
                {
                    if(!NoLonger)
                    {
                        NoLonger = true;
                        Invoke("PlayerHit", 0.2f);
                    }
                }
            }
            // otherwise we just draw our laser in front of our weapon 
            else
            {
                _destination = _origin +  _direction * LaserMaxDistance;
                
              
            }

            // we set our laser's line's start and end coordinates
            _line.SetPosition(0, _origin);
            _line.SetPosition(1, _destination);
        }

        bool NoLonger = false;
        float Timer = 0;
        void PlayerHit()
        {

            
           
               
           
            NoLonger = false;
            
            hackkohta.enabled = false;
            hackkohta.hackicon.SetActive(false);
            VVIcon.SetActive(false);
            Alerting = true;
            LookingForPlayer = false;
            PlaySound(1);
            alarmu = SoundManager.Instance.PlaySound(sound2, transform.position, true, gameObject);
            _line.enabled = false;
            _character._animator.SetBool("Alarm", true);

            foreach(AlarmTeleporter tele in teleportit)
            {
                tele.TelePortEnemiesToHereSoon();
            }

            Timer = 2.5f;

        }
        AudioSource alarmu;
        void PlaySound(int s)
        {
            if(s==1)
             SoundManager.Instance.PlaySound(sound1, transform.position);
            else
                SoundManager.Instance.PlaySound(sound3, transform.position);
        }
        
        public bool stun = false;


        public void Vampired()
        {

            if (!stun)
            {
                if (LookingForPlayer)
                {
                    hackkohta.enabled = false;
                    _character._animator.SetTrigger("VampireMini");
                    LookingForPlayer = false;
                    _line.enabled = false;
                    Invoke("StartLooking", 1f);
                    return;
                }
                else
                    return;
               
            }

            hackkohta.enabled = false;
            hackkohta.hackicon.SetActive(false);
            VVIcon.SetActive(false);
            SoundManager.Instance.StopLoopingSound(alarmu);
            _character._animator.SetTrigger("Vampire");
            _character._controller.GravityActive(true);
            _character._controller.Parameters.Gravity = -30f;

            LookingForPlayer = false;
            StartCoroutine(WaitForGround());
            
        }

        public GameObject VVIcon;
        public void Hacked()
        {
            VVIcon.SetActive(true);
            stun = true;
            SoundManager.Instance.StopLoopingSound(alarmu);
            _character._animator.SetBool("Hacked", true);
            HackTimer = 6;
            LookingForPlayer = false;
            _line.enabled = false;
        }

        IEnumerator WaitForGround()
        {
            while(!_character._controller.State.IsCollidingBelow)
            {
                yield return new WaitForEndOfFrame();
            }
            _character._health.Damage(999, GameManager.Instance.CurrentPlayer, 0.5f, 0.5f);
        }

        float HackTimer = 0;


        

    }

}