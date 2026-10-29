using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomExtension.ArrayExtensions;


#region    "using" Script Clarification
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

public class EnemyCharacter : BasePlayer, IPunObservable
{
    #region    Variables
        #region    Controls 
            [Header(" Character Settings")]
                    [SerializeField] Animator _animator;
                    private Animator I_Anim_Character;
            [Space(10)]
            [Header(" Movement Settings")]
                // Input //
                    [SerializeField] float _movementSpeed;
                    private float I_Flt_MovementSpeed;
                // Runtime //
                    private int R_Int_LocationIndex = 2;
                    private Vector2 R_Vec2_StashedLocation;
                    private bool R_Bool_IsHiding;
                    private bool R_Bool_IsRelocating; 
                    private bool R_Bool_HasReachedLocation; 
                    [SerializeField] float _movementCoyoteTime;
                    float I_Flt_MovementCoyoteTime;
                    float R_Flt_MovementCoyoteCounter;
                    private bool R_Bool_WasMovementCTEnabled;
                // Hashtable Keys //
                    private const string IsHiding_KEY = "IsHiding";
                    private const string Location_KEY = "Location"; 
            [Space(10)]
            [Header(" Combat Settings")]
                // Runtime //
                    private bool R_Bool_IsShooting;
                    private bool R_Bool_CanShoot;
                    private bool R_Bool_IsUsingWindhorn;
                    private bool R_Bool_IsReloading;
                    private bool R_Bool_IsHealing; 

                    private bool R_Bool_IsShootOutTime;
            [Space(10)]
            [Header(" State Settings")]
                // Runtime //
                    private bool R_Bool_WasDamaged;
                    private bool R_Bool_WasKilled;
        #endregion Controls 
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
            {
                if (stream.IsWriting) 
                {
//                        stream.SendNext(IO_Str_OwnerNickname);
//                        stream.SendNext(IO_Int_ID);
//                        stream.SendNext(IO_Flt_Health);
//                        stream.SendNext(R_E_PT_ClientType);
//                        stream.SendNext(O_Bool_IsMoving);
                }
                else
                {
//                    IO_Str_OwnerNickname = (string) stream.ReceiveNext();
//                    IO_Int_ID = (int) stream.ReceiveNext();
//                    IO_Flt_Health = (float) stream.ReceiveNext();
//                    R_E_PT_ClientType = (PlayerType) stream.ReceiveNext();
//                    O_Bool_IsMoving = (bool) stream.ReceiveNext();
                }
            }
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                // Variables //
                    I_Flt_MovementSpeed =_movementSpeed; 
                    I_Flt_MovementCoyoteTime = _movementCoyoteTime;
                    I_Anim_Character = _animator;
                    R_Bool_IsRelocating = false;
                    R_Bool_HasReachedLocation = true;
                    R_Bool_WasMovementCTEnabled = false;
                    R_Bool_CanShoot = true;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                // Reduce "Coyote" Counter //
                //Debug.Log("AAA");
                    if (R_Bool_WasMovementCTEnabled)
                        R_Flt_MovementCoyoteCounter -= Flt_DT;
                // Execute "Coyote" Stashed Movement //
                    if (R_Bool_WasMovementCTEnabled && (R_Flt_MovementCoyoteCounter > 0) && R_Bool_HasReachedLocation)
                         OnStartedMoving(R_Vec2_StashedLocation);
                    else
                        if (R_Flt_MovementCoyoteCounter < 0)
                            R_Bool_WasMovementCTEnabled = false;
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Network Methods
                #region    Regular
                #endregion Regular
                #region    RPC
                    [PunRPC]
                    private void RPC_NotifyMovementUpdate(Hashtable Hsh_Input)
                    {
                        I_Anim_Character.SetBool("IsHidden"   , (bool) Hsh_Input[IsHiding_KEY]);
                        I_Anim_Character.SetInteger("Location", (int)  Hsh_Input[Location_KEY]);   
            
                        I_Anim_Character.speed = 0f;

                        I_Anim_Character.Update(0f);

                        I_Anim_Character.speed = 1f;            
                    }
                #endregion RPC
            #endregion Network Methods
        #endregion Custom Methods
        #region Listener Methods
            #region    Callee Methods
                #region   OnStarted
                    public void OnStartedMoving(Vector2 Vec2_Input)
                    {
                        // Stop If Dead //
                            if (R_Bool_WasKilled) return;
                        // Stop If It's Shootout Time //
                            if (R_Bool_IsShootOutTime) return;
                        // Proceed //
                            if (R_Bool_HasReachedLocation)
                            {
                                // Get New Location //
                                    int Int_PotentialLocation = (Vec2_Input.x, Vec2_Input.y) switch 
                                                                {
                                                                    (-1.00f, +0.00f) => 0,   // Bottom Left Corner  //
                                                                    (-0.71f, +0.71f) => 1,   // Top Left Corner     //
                                                                    (+0.00f, +1.00f) => 2,   // Top Middle Corner   //
                                                                    (+0.71f, +0.71f) => 3,   // Top Right Corner    //
                                                                    (+1.00f, +0.00f) => 4,   // Bottom Right Corner //
                                                                    _                => 5,   // Default             //
                                                                };
                                // Is Moving to a direfent location ? //
                                    if (R_Int_LocationIndex != Int_PotentialLocation)
                                    {
                                        // Set New Location //
                                            R_Int_LocationIndex = Int_PotentialLocation;
                                            R_Vec2_StashedLocation = new Vector2();
                                            R_Bool_WasMovementCTEnabled = false;
                                            R_Flt_MovementCoyoteCounter = 0;
                                        // Is Not ONLY Hiding ? //
                                            R_Bool_IsRelocating = (R_Int_LocationIndex != 5) ? 
                                                                    true: 
                                                                    false;
                                        // Change Position //
                                            R_Bool_HasReachedLocation = false;
                                            if (R_Bool_IsHiding)
                                                R_Bool_IsHiding = false;
                                            else
                                                R_Bool_IsHiding = true;
                                        // Animation Update? //
                                            AnimatorStateInfo ASI_StateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                                            I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
                                            I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
            
                                            I_Anim_Character.speed = 0f;

                                            I_Anim_Character.Update(0f);

                                            I_Anim_Character.speed = 1f;
                                    }
                            }
                            else
                            {   
                                int Int_Temp = (Vec2_Input.x, Vec2_Input.y) switch 
                                               {
                                                    (-1.00f, +0.00f) => 0,   // Bottom Left Corner  //
                                                    (-0.71f, +0.71f) => 1,   // Top Left Corner     //
                                                    (+0.00f, +1.00f) => 2,   // Top Middle Corner   //
                                                    (+0.71f, +0.71f) => 3,   // Top Right Corner    //
                                                    (+1.00f, +0.00f) => 4,   // Bottom Right Corner //
                                                    _                => 5,   // Default             //
                                               };
                                if (Int_Temp != R_Int_LocationIndex)
                                {
                                    R_Bool_WasMovementCTEnabled = true;
                                    R_Flt_MovementCoyoteCounter = I_Flt_MovementCoyoteTime;
                                    R_Vec2_StashedLocation = Vec2_Input;
                                }
                            }
                    }
                    public void OnStartedShooting() 
                    {
                        // Stop If Dead //
                            if (R_Bool_WasKilled) return;
                        // Allow If It's Shootout Time //
                            if (!R_Bool_IsShootOutTime) return;
                        // Proceed //
                            if (!R_Bool_CanShoot) return;   
                        // Logic //

                    }
                    public void OnStartedReloading() 
                    {
                        // Stop If Dead //
                            if (R_Bool_WasKilled) return;
                        // Allow If It's Shootout Time //
//                            if (!R_Bool_IsShootOutTime) return;
                        // Proceed //
//                            if (!R_Bool_CanShoot) return;   
                        //  Logic //

                    }
                    public void OnStartedWindhorn() 
                    {
                        // Stop If Dead //
                            if (R_Bool_WasKilled) return;
                        // Allow If It's Shootout Time //
//                            if (!R_Bool_IsShootOutTime) return;
                        // Proceed //
//                            if (!R_Bool_CanShoot) return;   
                        // Logic //

                    }
                #endregion OnStarted
                #region    OnStopped
                    public void OnStoppedMoving(Vector2 Vec2_Input) 
                    {

                    }
                #endregion OnStopped
            #endregion Callee Methods
        #endregion Listener Methods
        #region    Coroutines Methods
        #endregion Coroutines Methods
    #endregion Methods    
    #region    Animation
        public void OtherHideStart()
        {

        }
        public void OtherHideOver()
        {
            if (R_Bool_IsRelocating)
            {
                R_Bool_IsRelocating = false;
                R_Bool_IsHiding = false;

                I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
                I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
            }
            else
            {
                if (!R_Bool_HasReachedLocation && (R_Int_LocationIndex == 5))
                    R_Bool_HasReachedLocation = true;
            }
        }
        public void OtherShowStart()
        {

        }
        public void OtherShowOver()
        {
            if (!R_Bool_HasReachedLocation)
                 R_Bool_HasReachedLocation = true;
        }
        public void OtherDamageOver()
        {
            R_Bool_HasReachedLocation = true;
        }
        public void OtherShootOver()
        {
        }
    #endregion Animation
}
