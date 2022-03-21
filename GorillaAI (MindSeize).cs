using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
    
    [RequireComponent(typeof(CharacterHandleWeapon))]
    [AddComponentMenu("Enemies/GorillaAI")]
    public class GorillaAI : MonoBehaviour
    {
        [Header("Behaviour")]
        [Information("Add this component to a CorgiController2D and it will try to kill your player on sight. This component requires a CharacterShoot component, and will simply tell it to press the trigger whenever a Player crosses its sight.", MoreMountains.Tools.InformationAttribute.InformationType.Info, false)]
        /// maksimimatka ampumiselle
        public float ShootDistance = 10f;
        /// raycastin lähtöpiste
        public Vector2 RaycastOriginOffset = new Vector2(0, 0);
        /// mitä layereita vastaan toimitaan
        public LayerMask TargetLayerMask;
        public LayerMask PlayerLayerMask;
        public bool TurretInHiding = false;
        public BossGate EnemyGate = null;
        public float MeleeDistance = 10f;


        protected bool DuringMelee = false;

        // private stuff
        protected Vector2 _direction;

        protected CharacterHandleWeapon _characterShoot;
        protected Vector2 _raycastOrigin;
        protected RaycastHit2D _raycast;
        protected RaycastHit2D _raycastBehind;
        

        [HideInInspector]
        public Character _character;
        [HideInInspector]
        public Collider2D _overlapCircleHit;
        //  [HideInInspector]
        // public Collider2D _overlapCircleHitMoreRange;
        [HideInInspector]
        public Collider2D _overlapCircleMeleeRange;
      
        [HideInInspector]
        public int randomizer = 1;

        public bool animationlock = true;
        [HideInInspector]
        public GameObject playa = null;


        /// <summary>
        /// on start we get our components
        /// </summary>
        protected virtual void Start()
        {

            PlayerLayerMask = TargetLayerMask;
            PlayerLayerMask ^= (1 << 8);

            playa = GameObject.FindWithTag("Player");
            _character = GetComponent<Character>();
            _characterShoot = GetComponent<CharacterHandleWeapon>();


            //Tämä siksi, jos kuollaan flipin aikana
            if (_character.IsFacingRight)
                lookingright = true;
            else
                lookingright = false;
        }

        

        protected virtual void OnEnable()
        {
            playa = GameObject.FindWithTag("Player");
            randomizer = 1;
            animationlock = true;

 
        }

        //tässä muutetaan flipin keskivaiheessa lookingright/left.
        void FlipMidPoint()
        {
            if (!_character.IsFacingRight)
                lookingright = true;
            else
                lookingright = false;
        }

        bool suffering = false;
        //health-scriptissä kutsutaan tätä damagejuttua. Ei tuu animaatiota jos menossa kääntyminen tai hyökkäys
        public void Damage()
        {

            if (_character._controller._crossBelowSlopeAngle.z!=0.0f)
                NoMoreSticky();
           
                if (animationlock == true || (_character.MovementState.CurrentState == CharacterStates.MovementStates.Idle && !DuringMelee))
                {

                    suffering = true;
                    _character._animator.SetTrigger("VampireDamage");
                }
            
         }

        public bool lookingright = true;

       

        //flippaa hahmon tarvittaessa ympäri jos kuolee kääntymisanimaation aikana 
        void deadflip()
        {

            if (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead && _character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
            {
                if ((_character.IsFacingRight && lookingright) || (!_character.IsFacingRight && !lookingright))
                {

                }
                else
                    _character.Flip();
            }
        }
        /// <summary>
        /// Every frame, check for the player and try and kill it
        /// </summary>
        protected virtual void Update()
        {
            
            if (playa == null)
            {
                playa = GameObject.FindWithTag("Player");
            }

            if (playa && GameManager.Instance.PlayerCharacter.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
            {
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


                //Raycasteja

                    _overlapCircleHit = Physics2D.OverlapCircle(transform.position, ShootDistance, PlayerLayerMask);
      
                    _overlapCircleMeleeRange = Physics2D.OverlapCircle(transform.position - Vector3.down, MeleeDistance, PlayerLayerMask);

               
                    // raycastin suunta
                    _direction = (_character.IsFacingRight) ? transform.right : -transform.right;

                    // raycastilla etsitään pelaajaa
                    _raycastOrigin.x = _character.IsFacingRight ? transform.position.x + RaycastOriginOffset.x : transform.position.x - RaycastOriginOffset.x;
                    _raycastOrigin.y = transform.position.y + RaycastOriginOffset.y;
                    _raycast = MMDebug.RayCast(_raycastOrigin, _direction, ShootDistance, TargetLayerMask, Color.yellow, true);
                
   
                //Raycasti taakse ja raycasti pään korkeudelta
               
                   
                    _direction = (_character.IsFacingRight) ? -transform.right : transform.right;


                    _raycastOrigin.x = _character.IsFacingRight ? transform.position.x - RaycastOriginOffset.x : transform.position.x + RaycastOriginOffset.x;
                    _raycastOrigin.y = transform.position.y - 0.5f;
                    _raycastBehind = MMDebug.RayCast(_raycastOrigin, _direction, ShootDistance, TargetLayerMask, Color.yellow, true);


                    _direction = (_character.IsFacingRight) ? transform.right : -transform.right;


                    _raycastOrigin.x = _character.IsFacingRight ? transform.position.x + RaycastOriginOffset.x : transform.position.x - RaycastOriginOffset.x;
                    _raycastOrigin.y = transform.position.y + 0.5f;

            

            // jos osutaan, hyökätään
            if (((_raycast && _raycast.collider.gameObject.layer == 9) || _overlapCircleHit) && animationlock == true)
            {
                
                //Meleerangella
                if ((_raycast && _raycast.collider && _raycast.collider.gameObject.layer== 9 && _overlapCircleMeleeRange)&& _overlapCircleHit && !suffering)
                    {
                        
                       
                       if (animationlock == true && _overlapCircleMeleeRange && _character.MovementState.CurrentState!=CharacterStates.MovementStates.Flip)
                        {
                            
                            animationlock = false;
                       GetComponent<AIWalk>()._direction = Vector2.zero;
                        DuringMelee = true;
                        _character._animator.Play("gorilla_attack"); 
                        }


                    }
                    //takana
                    else if (_raycastBehind && _raycastBehind.collider.gameObject.layer == 9)
                    {
                        if (animationlock == true)
                        {
                        if(GetComponent<AIWalk>().CanTurnAgain)
                                 GetComponent<AIWalk>().ChangeDirection();
                        }
                    }

                   //rangella, muttei tulilinjalla
                   else if (_overlapCircleHit && animationlock == true && _character.MovementState.CurrentState == CharacterStates.MovementStates.Idle && (!_raycastBehind || (_raycastBehind &&_raycastBehind.collider.gameObject.layer != 9)))
                {
                        animationlock = false;
                        StartCoroutine(StartWalking());
                    }
            }
            else
            {
                //ei olla ollenkaan rangella 
                if (this.gameObject.GetComponent<Character>().MovementState.CurrentState == CharacterStates.MovementStates.Idle && animationlock == true)
                    {                
                        animationlock = false;
                        StartCoroutine(StartWalking());
                    }    
            }
        }

        //ANIMATION EVENTEILLE
        public void AnimationLock()
        {
            animationlock = false;
        }

        public void AnimationLockTrue()
        {
            animationlock = true;
            HitBoxBack();
            suffering = false;
        }

        //hitboksien koon muuttaminen hyökkäykseen
        void HitBoxSmaller()
        {
            
            if(_character._controller._crossBelowSlopeAngle.z != 0.0f)
            {
                Debug.Log(_character._controller._crossBelowSlopeAngle+" "+Time.timeSinceLevelLoad);
                if (rigidi)
                    rigidi.constraints = RigidbodyConstraints2D.FreezeAll;
                StartCoroutine(KeepGorillaFromFlying());
            }
            



            GetComponent<BoxCollider2D>().size = new Vector2(0.85f, 1.868977f);
            if(_character.IsFacingRight)
                GetComponent<BoxCollider2D>().offset = new Vector2(-0.4f, -0.4025207f);
            else
                GetComponent<BoxCollider2D>().offset = new Vector2(0.4f, -0.4025207f);
        }
        void HitBoxBack()
        {
           
            GetComponent<BoxCollider2D>().size = new Vector2(1.4f, 1.868977f);
            if (_character.IsFacingRight)
                GetComponent<BoxCollider2D>().offset = new Vector2(0, -0.4025207f);
            else
                GetComponent<BoxCollider2D>().offset = new Vector2(0, -0.4025207f);

            _character._animator.ResetTrigger("Damage");
            _character._animator.ResetTrigger("VampireDamage");
        }

        //ANIMATION EVENTEILLE
        public void ShootDem()
        {

            _characterShoot.CurrentWeapon.GetComponent<SimpleObjectPooler>().FillPool();
            _characterShoot.ShootStart();

            
            //laitetaan gorillan hitboksi normaaliks
            HitBoxBack();

            Invoke("ShakeAgain", 1f);
        }

        public Rigidbody2D rigidi;
        public void NoMoreSticky()
        {
            just = false;
            if (rigidi && rigidi.constraints == RigidbodyConstraints2D.FreezeAll)
                rigidi.constraints = RigidbodyConstraints2D.None;
        }

        void ShakeAgain()
        {
            GameManager.Instance.MainCameraController.Shake(new Vector3(0.3f, 0.3f, 0.3f));
        }

        void AAAPlayGorillaAttacksound()
        {
           SoundManager.Instance.PlaySound(_character._health.ExtraSoundSfx,transform.position);
        }
        bool just = true;
        IEnumerator KeepGorillaFromFlying()
        {
            just = true;

            while(just)
            {
                _character._controller.SetForce(Vector2.zero);
                yield return new WaitForEndOfFrame();
            }





        }

        public void AnimationRecovery()
        {
            _characterShoot.ShootStop();

           

            if (_character.MovementState.CurrentState == CharacterStates.MovementStates.Flip)
            {
                _character.HEREWEWILLFLIPFLIP_AI();
                animationlock = true;
            }
                
            else
            {
                DuringMelee = false;
                Invoke("AnimationLockTrue", 0.9f); //hyökkäyksen jälkeen vähän odottaa ennen seuraavaa
            }

            if (_character.IsFacingRight)
                lookingright = true;
            else
                lookingright = false;

            _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);

            _character._animator.ResetTrigger("Damage");


        }

        


        IEnumerator StartWalking()
        {
            
            yield return new WaitForSeconds(0.9f);
            _characterShoot.ShootStop();
            GetComponent<AIWalk>().Start();
            animationlock = true;
        }

        //Näitä ei gorilla kaipaa 

        /*
        IEnumerator StartMoveOnSight()
        {
            yield return new WaitForSeconds(0f);
            animationlock = true;
            this.gameObject.GetComponent<AIWalk>().WalkBehaviour = AIWalk.WalkBehaviours.MoveOnSight;
        }

        
        IEnumerator StartPatrol()
        {
            _characterShoot.ShootStop();
             yield return new WaitForSeconds(0.5f);
            _characterShoot.ShootStop();
            animationlock = true;
            GetComponent<CharacterHorizontalMovement>().MovementSpeed = 2;
            if (this.gameObject.GetComponent<Character>().MovementState.CurrentState != CharacterStates.MovementStates.Shooting)
            {

                this.gameObject.GetComponent<AIWalk>().WalkBehaviour = AIWalk.WalkBehaviours.Patrol;

                if (Random.Range(0f, 1f) > 0.5f)
                    this.gameObject.GetComponent<AIWalk>()._direction = Vector2.left;
                else
                    this.gameObject.GetComponent<AIWalk>()._direction = Vector2.right;

            }

        }

       
        
     */
        
     
    }

}