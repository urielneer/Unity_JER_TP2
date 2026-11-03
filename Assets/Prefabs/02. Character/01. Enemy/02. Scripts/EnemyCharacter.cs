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
                    [SerializeField] private float _movementSpeed;
                    private float I_Flt_MovementSpeed;
                // Runtime //
                    private Vector2 R_Vec2_OldLocation = new Vector2(+0.00f, +1.00f);
                    private int R_Int_LocationIndex = 2;
                    private Vector2 R_Vec2_StashedLocation;
                    private bool R_Bool_IsHiding;
                    private bool R_Bool_IsRelocating; 
                    private bool R_Bool_HasReachedLocation; 
                    [SerializeField] private float _movementCoyoteTime;
                    float I_Flt_MovementCoyoteTime;
                    float R_Flt_MovementCoyoteCounter;
                    private bool R_Bool_WasMovementCTEnabled; 
                // Hashtable Keys //
                    private const string IsHiding_KEY = "IsHiding";
                    private const string IsDamaged_KEY = "IsDamaged";
                    private const string IsShooting_KEY = "IsShooting";
                    private const string Location_KEY = "Location";
            [Space(10)]
            [Header(" Combat Settings")]
                // Runtime //
                    [SerializeField] private Aim _aim;
                    Aim I_Aim_Own;
                    private bool R_Bool_IsShooting;
                    private bool O_Bool_CanShoot;
                    private bool R_Bool_IsReloading;
                    private bool R_Bool_IsHealing; 

                    private bool R_Bool_IsShootOutTime;
            [Space(10)]
            [Header(" State Settings")]
                // Runtime //
                    private bool R_Bool_WasDamaged;
                    private bool R_Bool_WasWindHorned;
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
                base.Awake(); 
                OnStartUp();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async void OnStartUp()
            #pragma warning restore CS1998
            {
                // Variables //
                    I_Flt_MovementSpeed =_movementSpeed; 
                    I_Flt_MovementCoyoteTime = _movementCoyoteTime;
                    I_Anim_Character = _animator;
        
                    R_Vec2_OldLocation = new Vector2(+0.00f, +1.00f);
                    R_Int_LocationIndex = 2;

                    R_Bool_IsHiding = false;
                    R_Bool_IsRelocating = false; 
                    R_Bool_HasReachedLocation = true; 
                    R_Bool_WasMovementCTEnabled = false; 

                    R_Bool_IsShooting = false;
                    O_Bool_CanShoot = true;
                    R_Bool_IsReloading = false;
                    R_Bool_IsHealing = false; 
                    R_Bool_IsShootOutTime = true;
//                    R_Bool_IsShootOutTime = false;
            
                    R_Bool_WasDamaged = false;
                    R_Bool_WasWindHorned = false;
                    R_Bool_WasKilled = false;

            }
            #pragma warning disable CS1998
            public async void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                #region    Movement
                    // Reduce "Coyote" Counter //
                        if (R_Bool_WasMovementCTEnabled)
                            R_Flt_MovementCoyoteCounter -= Flt_DT;
                    // Execute "Coyote" Stashed Movement //
                        if (R_Bool_WasMovementCTEnabled && (R_Flt_MovementCoyoteCounter > 0) && R_Bool_HasReachedLocation)
                             OnStartedMoving(R_Vec2_StashedLocation);
                        else
                            if (R_Flt_MovementCoyoteCounter < 0)
                                R_Bool_WasMovementCTEnabled = false;
                #endregion Movement
                #region    IncreaseWindhorn
                #endregion IncreaseWindhorn
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    ForceUpdate-Variables
                public void ForceUpdateLocation(Vector2 Vec2_Input)
                {
                    R_Vec2_OldLocation = Vec2_Input;
                    R_Int_LocationIndex = (R_Vec2_OldLocation.x, R_Vec2_OldLocation.y) switch 
                                          {
                                              (-1.00f, +0.00f) => 0,   // Bottom Left Corner  //
                                              (-0.71f, +0.71f) => 1,   // Top Left Corner     //
                                              (+0.00f, +1.00f) => 2,   // Top Middle Corner   //
                                              (+0.71f, +0.71f) => 3,   // Top Right Corner    //
                                              (+1.00f, +0.00f) => 4,   // Bottom Right Corner //
                                              _                => 5,   // Bottom (Hide)       //
                                          };
                    I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
                }
                public void ForceShootAnimation(int Int_Input)
                {
                    // Valdidation //
                        #region    Should Proceed [?]
                            if 
                            (
                                R_Bool_WasDamaged // Stop If Damaged ///
                            )
                            return;
                        #endregion Should Proceed [?] 
                    // Logic //
                        R_Int_LocationIndex = Int_Input;

                        R_Bool_IsHiding = false;
                        R_Bool_IsRelocating = false;
                        R_Bool_IsShooting = true;
                        R_Bool_WasDamaged = false;

                        I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
                        I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
                        I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
                        I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
                }
            #endregion ForceUpdate-Variables
            #region    Combat
                public void Damage()
                {
                    // Valdidation //
                        #region    Should Proceed [?]
                            if 
                            (
                                R_Bool_WasDamaged // Stop If Already Damaged ///
                            )
                            return;
                        #endregion Should Proceed [?] 
                    // Logic //
                        #region    Health
                        #endregion Health
                        #region    Animation
                            R_Vec2_StashedLocation = new Vector2();
                            R_Bool_WasMovementCTEnabled = false;
                            R_Flt_MovementCoyoteCounter = 0;
                            R_Bool_HasReachedLocation = true;
                            R_Bool_IsHiding = false;
                            R_Bool_IsRelocating = false;
                            R_Bool_IsShooting = false;
                            R_Bool_WasDamaged = true;
                            R_Int_LocationIndex = (R_Vec2_OldLocation.x, R_Vec2_OldLocation.y) switch 
                                                  {
                                                      (-1.00f, +0.00f) => 0,   // Bottom Left Corner  //
                                                      (-0.71f, +0.71f) => 1,   // Top Left Corner     //
                                                      (+0.00f, +1.00f) => 2,   // Top Middle Corner   //
                                                      (+0.71f, +0.71f) => 3,   // Top Right Corner    //
                                                      (+1.00f, +0.00f) => 4,   // Bottom Right Corner //
                                                      _                => 5,   // Bottom (Hide)       //
                                                  };

                            I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
                            I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
                            I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
                            I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
                        #endregion Animation
                        #region    Network
                            Hashtable Hsh_Keys = new Hashtable
                            {
                                {
                                    IsHiding_KEY, 
                                    R_Bool_IsHiding
                                },
                                {
                                    IsDamaged_KEY,
                                    R_Bool_WasDamaged
                                },
                                {
                                    IsShooting_KEY,
                                    R_Bool_IsShooting
                                },
                                { 
                                    Location_KEY, 
                                    (Vector2) 
                                    (
                                        (R_Int_LocationIndex) switch
                                        {
                                            4 => new Vector2 (-1.00f, +0.00f),   // Bottom Left Corner  //
                                            3 => new Vector2 (-0.71f, +0.71f),   // Top Left Corner     //
                                            2 => new Vector2 (+0.00f, +1.00f),   // Top Middle Corner   //
                                            1 => new Vector2 (+0.71f, +0.71f),   // Top Right Corner    //
                                            0 => new Vector2 (+1.00f, +0.00f),   // Bottom Right Corner //
                                            _ => new Vector2 (+0.00f, -1.00f),   // Bottom (Hide)       //
                                        } 
                                    )
                                },
                            }; 
                            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyDamageUpdate),RpcTarget.Others, Hsh_Keys);
                        #endregion Network
                }
            #endregion Combat
            #region    Network Methods
                #region    Regular
                #endregion Regular
                #region    RPC
                    [PunRPC]
                    private void RPC_NotifyDamageUpdate(Hashtable Hsh_Input)
                    {
                        ((PlayerCharacter) (MasterManager.Instance.CharacterManager.OwnPlayer)).NotifyDamageUpdate(Hsh_Input);
                    }
                    [PunRPC]
                    private void RPC_NotifyWindhornUpdate(Hashtable Hsh_Input)
                    {
                        ((PlayerCharacter) (MasterManager.Instance.CharacterManager.OwnPlayer)).NotifyWindhornUpdate((Vector2) Hsh_Input[Location_KEY]);
                    }
                    [PunRPC]
                    private void RPC_NotifyShootUpdate()
                    {
                        ((PlayerCharacter) (MasterManager.Instance.CharacterManager.OwnPlayer)).NotifyShootingUpdate();
                    }
                #endregion RPC
            #endregion Network Methods
            #region    Mirror Methods
                #region    Movement
                    public void NotifyMovementUpdate(Vector2 Vec2_Input)
                    {
                        OnStartedMoving(Vec2_Input);  
                    }
                #endregion Movement
            #endregion Mirror Methods
        #endregion Custom Methods
        #region Listener Methods
            #region    Callee Methods
                #region   OnStarted
                    public void OnStartedMoving(Vector2 Vec2_Input)
                    {
                        // Valdidation //
                            #region    Should Proceed [?]
                                if 
                                (
//                                    R_Bool_IsShootOutTime || // Stop If Shootout Time //
                                    R_Bool_WasKilled      || // Stop If Dead //
                                    R_Bool_WasDamaged     || // Stop If Damaged //
                                    R_Bool_WasWindHorned  || // Stop If Afected By A Windhorn //
                                    R_Bool_IsReloading    || // Stop If Reloading //
                                    R_Bool_IsShooting        // Stop If Shooting //
                                )
                                return;
                            #endregion Should Proceed [?] 
                        // Logic //
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
                                                                    _                => 5,   // Bottom (Hide)       //
                                                                };
                                // Is Moving to a direfent location ? //
                                    if (R_Int_LocationIndex != Int_PotentialLocation)
                                    {
                                        // Set New Location //
                                            R_Vec2_OldLocation = Vec2_Input;
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
                                            I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
                                            I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
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
                                                    _                => 5,   // Bottom (Hide)       //
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
                        // Valdidation //
                            #region    Should Proceed [?]
                                if 
                                (
                                    !R_Bool_IsShootOutTime || // Stop If Outside Of Shootout Time //
                                    R_Bool_IsHiding        || // Stop If Hiding //
                                    R_Bool_WasKilled       || // Stop If Dead //
                                    R_Bool_WasDamaged      || // Stop If Damaged //
                                    R_Bool_WasWindHorned   || // Stop If Afected By A Windhorn //
                                    R_Bool_IsReloading        // Stop If Reloading //
                                )
                                return;
                            #endregion Should Proceed [?] 
                        // Logic //

                    }
                    public void OnStartedReloading() 
                    {
                        // Valdidation //
                            #region    Should Proceed [?]
                                if 
                                (
                                    !R_Bool_IsShootOutTime || // Stop If Outside Of Shootout Time //
                                    R_Bool_WasKilled       || // Stop If Dead //
                                    R_Bool_WasDamaged      || // Stop If Damaged //
                                    R_Bool_WasWindHorned   || // Stop If Afected By A Windhorn //
                                    R_Bool_IsShooting         // Stop If Shooting //
                                )
                                return;
                            #endregion Should Proceed [?]
                        //  Logic //
                    }
                    public void OnStartedWindhorn() 
                    {
                        // Valdidation //
                            #region    Should Proceed [?]
                                if 
                                (
                                    R_Bool_WasKilled       || // Stop If Dead //
                                    R_Bool_WasDamaged      || // Stop If Damaged //
                                    R_Bool_WasWindHorned      // Stop If Afected By A Windhorn //
                                )
                                return;
                            #endregion Should Proceed [?]
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
                I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
                I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
                I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);

                I_Anim_Character.speed = 0f;

                I_Anim_Character.Update(0f);

                I_Anim_Character.speed = 1f;
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
            R_Bool_IsHiding = false;
            R_Bool_WasDamaged = false;
            R_Bool_IsShooting = false;
            string Str_Temp = (I_Anim_Character.GetCurrentAnimatorClipInfo(0))[0].clip.name;
            R_Int_LocationIndex = (Str_Temp.Substring(Math.Max(0, Str_Temp.Length - 2))) switch 
                                  {
                                      "LL" => 0,   // Bottom Left Corner  //
                                      "TL" => 1,   // Top Left Corner     //
                                      "TM" => 2,   // Top Middle Corner   //
                                      "TR" => 3,   // Top Right Corner    //
                                      "LR" => 4,   // Bottom Right Corner //
                                      _    => 5,   // Bottom (Hide)       //
                                  };
            R_Vec2_StashedLocation = new Vector2();

            I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
            I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
            I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
            I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
        }
        public void OtherShootOver()
        {
            R_Bool_HasReachedLocation = true;
            R_Bool_IsHiding = false;
            R_Bool_IsShooting = false;
            string Str_Temp = (I_Anim_Character.GetCurrentAnimatorClipInfo(0))[0].clip.name;
            R_Int_LocationIndex = (Str_Temp.Substring(Math.Max(0, Str_Temp.Length - 2))) switch 
                                  {
                                      "LL" => 0,   // Bottom Left Corner  //
                                      "TL" => 1,   // Top Left Corner     //
                                      "TM" => 2,   // Top Middle Corner   //
                                      "TR" => 3,   // Top Right Corner    //
                                      "LR" => 4,   // Bottom Right Corner //
                                      _    => 5,   // Bottom (Hide)       //
                                  };
            R_Vec2_StashedLocation = new Vector2();

            I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
            I_Anim_Character.SetBool("IsDamaged", R_Bool_WasDamaged);
            I_Anim_Character.SetBool("IsShooting", R_Bool_IsShooting);
            I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);

            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyShootUpdate),RpcTarget.Others);
        }
    #endregion Animation
}
