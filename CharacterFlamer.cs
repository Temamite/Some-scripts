using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;


namespace MoreMountains.CorgiEngine
{

    [AddComponentMenu("Corgi Engine/Character/Abilities/Character Flamer")]
    public class CharacterFlamer : CharacterAbility
    {

        ParticleSystem.MinMaxGradient originalColor;
        public ParticleSystem.MinMaxGradient UpgradedGreener;
        public bool UpgradedFlamer;

        public GameObject Flame;
        public LayerMask lvl1groundflamer;
        public LayerMask lvl2groundflamer;
        public LayerMask lvl1enemyflamer;
        public LayerMask lvl2enemyflamer;
        
        [HideInInspector]
        public ParticleSystem FlameSpawner;
        [HideInInspector]
        public ParticleSystem FlameSpawner2;
      //  public float Pituus = 1.6f;
       
        public bool ShootingFlames { get; set; }
        public int PowerCost = 2;
        // Initialization
        protected override void Initialization()
        {
            base.Initialization();
            ShootingFlames = false;
            Setup();
        }

        Power power;

#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
        public void Initialize()
#pragma warning restore CS0114 // Member hides inherited member; missing override keyword
        {
            Initialization();
            Setup();
        }
        /// <summary>
        /// Grabs various components and inits stuff
        /// </summary>
        public virtual void Setup()
        {



            power = GetComponent<Power>();

            _character = gameObject.GetComponentNoAlloc<Character>();
            originalColor = Flame.GetComponent<ParticleSystem>().main.startColor;
        }
        int removelaskuri = 2;
        /// <summary>
        /// Every frame we check if it's needed to update the ammo display
        /// </summary>
        public override void ProcessAbility()
        {
            if(ShootingFlames)
            {
                if (power.CurrentPower <= 0 || (_character._drone._weaponhandlero.DroneInWater && !UpgradedFlamer) || _character._drone.Haxing||GameManager.Instance.ChangingLevel)
                {
                    if(!GameManager.Instance.ChangingLevel)
                    power.PlayNoPowerSfx();

                    ShootStop();
                    return;
                }
                   

                if (_inputManager.HealB.IsPressed)
                {
                    
                }
                else
                {
                    if (PowerCounter >= UseAtLeastThisMuchPower)
                    {
                        ShootStop();
                        return;
                    }
                        
                }

                if (removelaskuri >= 3)
                {
                    PowerCounter++;
                    power.RemovePower(PowerCost);
                    removelaskuri = 0;
                }
                else
                    removelaskuri++;

                

                
                if(_controller.State.IsGrounded)
                {
                    var iv = FlameSpawner.inheritVelocity;
                    var iv2 = FlameSpawner2.inheritVelocity;

                    iv2.enabled = true;
                    iv.enabled = true;
                }
                else
                {
                    var iv = FlameSpawner.inheritVelocity;
                    var iv2 = FlameSpawner2.inheritVelocity;

                    iv2.enabled = false;
                    iv.enabled = false;
                }
                

                if (_character.IsFacingRight)
                {
                   
                    FlameSpawner.transform.position = _character._drone.sprite.transform.position;


                    if(InputManager.Instance.Camerastick.IsPressed)
                    {
                       
                        if (InputManager.Instance.Camerastick.X < 0f)
                        {
                          //  ShootStop();
                         //   if (power.CurrentPower > 0)
                         //       ShootStart();
                            _character._drone.sprite.flipX = true;
                            FlameSpawner.transform.localRotation = Quaternion.Euler(-16, -90, 0);
                        }
                        else if (InputManager.Instance.Camerastick.X > 0f)
                        {
                          //  ShootStop();
                         //   if (power.CurrentPower > 0)
                         //       ShootStart();
                            _character._drone.sprite.flipX = false;
                            FlameSpawner.transform.localRotation = Quaternion.Euler(-16, 90, 0);
                        }
                    }
                       
                }
                else
                {
                    
                    FlameSpawner.transform.position = _character._drone.sprite.transform.position;
                    if (InputManager.Instance.Camerastick.IsPressed)
                    {
                        if (InputManager.Instance.Camerastick.X > 0f)
                        {
                       //     ShootStop();
                       //     if(power.CurrentPower > 0)
                       //     ShootStart();

                            _character._drone.sprite.flipX = false;
                            FlameSpawner.transform.localRotation = Quaternion.Euler(-16, 90, 0);
                        }
                        else if (InputManager.Instance.Camerastick.X < 0f)
                        {
                       //     ShootStop();
                         //   if (power.CurrentPower > 0)
                         //       ShootStart();
                              _character._drone.sprite.flipX = true;
                            FlameSpawner.transform.localRotation = Quaternion.Euler(-16, -90, 0);
                        }
                    }
                }
            }
               

            base.ProcessAbility();

        }

        public override void Flip()
        {
           /* if(ShootingFlames)
            {
                ShootStop();
                ShootStart();

            }
            */
        }

        /// <summary>
        /// Gets input and triggers methods based on what's been pressed
        /// </summary>
        protected override void HandleInput()
        {
            if(timerToFlameAgain>0)
            {
                timerToFlameAgain -= Time.deltaTime;

                if (power.CurrentPower > 1)
                    timerToFlameAgain = 0;
            }


            if (timerToFlameAgain>0 ||_movement.CurrentState == CharacterStates.MovementStates.Dashing || _character._drone.Haxing || _movement.CurrentState == CharacterStates.MovementStates.AirDashing)
            {
                //Demossa iteminapista 
                // if (((ModifyB.IsPressed && ShootB.WasPressed) || (_inputManager.ModifyButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed && _inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown ) )&& GetComponent<Power>().CurrentPower >= PowerCost-10)



            }
            else

            {
                if (_inputManager.ModifyB.IsPressed && _inputManager.HealB.WasPressed )
                {
                 

                    ShootStart();
                }

                if(ShootingFlames && (_inputManager.ModifyB.WasReleased || _inputManager.HealB.WasReleased))
                {

                    if(PowerCounter >= UseAtLeastThisMuchPower)
                        ShootStop();
                }
            }
        }
        public virtual void ShootStop()
        {
            var iv = FlameSpawner.inheritVelocity;
            var iv2 = FlameSpawner2.inheritVelocity;

            iv2.enabled = false;
            iv.enabled = false;
            ShootingFlames = false;
            FlameSpawner.Stop();
            FlameSpawner2.Stop();

            PlayAbilityStopSfx();
            StopAbilityUsedSfx();

            if (_character.IsFacingRight && _character._drone.sprite.flipX)
                _character._drone.sprite.flipX = false;
            else if (!_character.IsFacingRight && !_character._drone.sprite.flipX)
                _character._drone.sprite.flipX = true;

            _character._drone.sprite.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        /// <summary>
        /// Causes the character to start shooting
        /// </summary>
        public virtual void ShootStart()
        {
            // if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
            if (!AbilityPermitted

                || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
                || (_movement.CurrentState == CharacterStates.MovementStates.LadderClimbing)
                || _movement.CurrentState == CharacterStates.MovementStates.WallClinging
                || _movement.CurrentState == CharacterStates.MovementStates.Gripping
                || ShootingFlames
                || _movement.CurrentState == CharacterStates.MovementStates.WallJumping
                 )
            {
                
                return;
            }

           


            FlameSpawner = Instantiate(Flame, _character._drone.sprite.transform).GetComponent<ParticleSystem>();
            FlameSpawner2 = FlameSpawner.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>();
            var stoppi = FlameSpawner.main;
            stoppi.stopAction = ParticleSystemStopAction.Destroy;
            FlameSpawner.subEmitters.AddSubEmitter(GameManager.Instance.FlameSystems[0], ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritColor);
            FlameSpawner2.subEmitters.AddSubEmitter(GameManager.Instance.FlameSystems[1], ParticleSystemSubEmitterType.Collision, ParticleSystemSubEmitterProperties.InheritColor);

            if (UpgradedFlamer)
            {
                var main = FlameSpawner.main;
                var main2 = FlameSpawner2.main;
                var sub1 = GameManager.Instance.FlameSystems[0].main;
                var sub2 = GameManager.Instance.FlameSystems[1].main;

                var mainColl = FlameSpawner.collision;
                var main2Coll = FlameSpawner2.collision;

                mainColl.collidesWith = lvl2enemyflamer;
                main2Coll.collidesWith = lvl2groundflamer;

                main.startColor = UpgradedGreener;
                main2.startColor = UpgradedGreener;
                sub1.startColor = UpgradedGreener;
                sub2.startColor = UpgradedGreener;
            }
            else
            {
                var mainColl = FlameSpawner.collision;
                var main2Coll = FlameSpawner2.collision;

                mainColl.collidesWith = lvl1enemyflamer;
                main2Coll.collidesWith = lvl1groundflamer;
            }

           
            PlayAbilityStartSfx();
            PlayAbilityUsedSfx();




            if (_character.IsFacingRight)
            {
                FlameSpawner.transform.position = _character._drone.sprite.transform.position;
                FlameSpawner.transform.localRotation = Quaternion.Euler(-16, 90, 0);
            }
            else
            {
                FlameSpawner.transform.position = _character._drone.sprite.transform.position;
                FlameSpawner.transform.localRotation = Quaternion.Euler(-16, -90, 0);
            }


          
                
                
            ShootingFlames = true;
            PowerCounter = 0;
            
            var iv = FlameSpawner.inheritVelocity;
            var iv2 = FlameSpawner2.inheritVelocity;

            iv2.enabled = true;

            iv.enabled = true;


    
            _character._drone._animator.SetBool("Scanning", true);



            if (power.CurrentPower == 0)
            {
                FlameSpawner.Play();
               // FlameSpawner2.Play();

                power.PlayNoPowerSfx();
                power.CurrentPower = 1;
                timerToFlameAgain = 3;
            }
            else
            {
                FlameSpawner.Play();
                FlameSpawner2.Play();

            }


        }

        float timerToFlameAgain = 0;
        public int UseAtLeastThisMuchPower = 20;
        int PowerCounter = 0;

        IEnumerator AllowMovement()
        {
            yield return new WaitForEndOfFrame();


            if (_controller.State.IsGrounded)
                yield return new WaitForSeconds(0.3f);
            else
                yield return new WaitForSeconds(0.15f);

            if (_character.MovementState.PreviousState != CharacterStates.MovementStates.NanoHealing && _character.MovementState.PreviousState != CharacterStates.MovementStates.ShootingMines)
            {
                   _character.MovementState.RestorePreviousState();
            }
            else
            {
              _character.MovementState.ChangeState(CharacterStates.MovementStates.Idle);
            }
                

            

            _character.UnFreeze();

            yield return new WaitForSeconds(0.5f);

            ShootingFlames = false;
            
        }


        protected override void InitializeAnimatorParameters()
        {
            RegisterAnimatorParameter("ShootingFlames", AnimatorControllerParameterType.Bool);
        }

        /// <summary>
        /// At the end of the cycle, we update our animator's Dashing state 
        /// </summary>
        public override void UpdateAnimator()
        {

            
                MMAnimator.UpdateAnimatorBool(_animator, "ShootingFlames", (_movement.CurrentState == CharacterStates.MovementStates.ShootingFlames), _character._animatorParameters);
          
            
        }



        /// <summary>
        /// Adds required animator parameters to the animator parameters list if they exist
        /// </summary>

    }
}