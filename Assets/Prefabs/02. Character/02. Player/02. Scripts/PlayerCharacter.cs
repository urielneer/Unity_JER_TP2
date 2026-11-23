using System;
using Photon.Pun;
using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static CustomExtension.ArrayExtensions;


#region    "using" Script Clarification
using Hashtable = ExitGames.Client.Photon.Hashtable;
#endregion "using" Script Clarification

public class PlayerCharacter : BasePlayer, IPunObservable
{
    #region    Variables
        #region    Controls 
            [Header(" Character Settings")]
                    [SerializeField] private Animator _animator;
                    private Animator I_Anim_Character;
            [Space(10)]
            [Header(" Movement Settings")]
                // Input //
                    [SerializeField] private float _movementSpeed;
                    private float I_Flt_MovementSpeed;
                // Runtime //
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
                    [SerializeField] private int _bulletCount;
                    private int I_Int_BulletCount;
                    [SerializeField] private Aim _aim;
                    Aim I_Aim_Own;
                    private bool O_Bool_CanShoot;
                    public bool CanShoot => O_Bool_CanShoot;
                    private bool R_Bool_IsShooting;
                    private bool R_Bool_IsReloading;
                    private bool R_Bool_IsHealing;
                    private bool R_Bool_IsDamaged;
                    private bool R_Bool_IsWindhorning;
                    private bool R_Bool_IsFullReloading;

                    private bool R_Bool_IsShootOutTime;
            [Space(10)]
            [Header(" State Settings")]
                // Runtime //
                    private bool R_Bool_WasDamaged;
                    private bool R_Bool_IsStunned;
                    private bool R_Bool_WasKilled;
            [Space(10)]
            [Header(" GUI Settings")]
                // Runtime //
                    [SerializeField] private UnityEngine.UI.Image _cillinder;
                    private UnityEngine.UI.Image I_Image_Cillinder;
                    [SerializeField] private Sprite[] _bullets;
                    private Sprite[] I_SpriteA1_Bullets;
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
                    I_Int_BulletCount = _bulletCount;
                    I_SpriteA1_Bullets = _bullets;
                    I_Image_Cillinder = _cillinder;
                    I_Image_Cillinder.sprite = I_SpriteA1_Bullets[I_Int_BulletCount];

                    R_Int_LocationIndex = 2;

                    I_Flt_MovementSpeed =_movementSpeed; 
                    I_Flt_MovementCoyoteTime = _movementCoyoteTime;
                    I_Anim_Character = _animator;
                    I_Aim_Own = _aim;
        
                    R_Bool_IsHiding = false;
                    R_Bool_IsRelocating = false; 
                    R_Bool_HasReachedLocation = true; 
                    R_Bool_WasMovementCTEnabled = false; 
        
                    R_Bool_IsDamaged = false;
                    R_Bool_IsWindhorning = false;
                    R_Bool_IsFullReloading = false;
                    
                    O_Bool_CanShoot = true;
                    R_Bool_IsShooting = false;
                    R_Bool_IsReloading = false;
                    R_Bool_IsHealing = false; 
                    R_Bool_IsShootOutTime = true;
//                    R_Bool_IsShootOutTime = false;
            
                    R_Bool_WasDamaged = false;
                    R_Bool_IsStunned = false;
                    R_Bool_WasKilled = false;
                // Children StartUp //
                    I_Aim_Own.OnStartUp();
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
            }
        #endregion Override Methods
        #region    Custom Methods
            #region    Bool-Resseters
                public void ResetCanshootFlag()
                {
                    O_Bool_CanShoot = true;
                }
            #endregion Bool-Resseters
            #region    Network Methods
                #region    Regular
                #endregion Regular
                #region    RPC
                    [PunRPC]
                    private void RPC_NotifyMovementUpdate(Hashtable Hsh_Input)
                    {
                        ((EnemyCharacter) (MasterManager.Instance.CharacterManager.EnemyPlayer)).NotifyMovementUpdate((Vector2) Hsh_Input[Location_KEY]);
                    }
                    [PunRPC]
                    private void RPC_NotifyShootUpdate(int Int_Input)
                    {
                        ((EnemyCharacter) (MasterManager.Instance.CharacterManager.EnemyPlayer)).ForceShootAnimation(R_Int_LocationIndex);
                    }
                    [PunRPC]
                    private void RPC_NotifyReloadUpdate(int Int_Input)
                    {
                        ((EnemyCharacter) (MasterManager.Instance.CharacterManager.EnemyPlayer)).ForceReloadAnimation(R_Int_LocationIndex);
                    }
                    [PunRPC]
                    private void RPC_NotifyWindhornUpdate(int Int_Input)
                    {
                        // Mirror Animation //
                            ((EnemyCharacter) (MasterManager.Instance.CharacterManager.EnemyPlayer)).ForceWindhornAnimation(R_Int_LocationIndex);
                        // Stun Enemy //
                    }
                    [PunRPC]
                    private void RPC_ForceLocationUpdate(Vector2 Vec2_Input)
                    {
                        ((EnemyCharacter) (MasterManager.Instance.CharacterManager.EnemyPlayer)).ForceUpdateLocation(Vec2_Input);
                    }
                #endregion RPC
            #endregion Network Methods
            #region    Mirror Methods
                #region    Combat
                    public void NotifyDamageUpdate(Hashtable Hsh_Input)
                    {
                        // Clear Values //
                            R_Vec2_StashedLocation = new Vector2();
                            R_Bool_WasMovementCTEnabled = false;
                            R_Flt_MovementCoyoteCounter = 0;
                            R_Bool_IsRelocating = false;
                        // Update Anim Values //
                            ResetAnimBooleans(true, -1); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                            ForciblyResetLocation();
                        // Overwrite Values //
                            R_Bool_IsHiding = (bool) Hsh_Input[IsHiding_KEY];
                            R_Bool_WasDamaged = (bool)Hsh_Input[IsDamaged_KEY];
                            R_Bool_IsShooting = (bool)Hsh_Input[IsShooting_KEY];
                        // Animator //
                            UpdateAnimator();
                    }
                    public void NotifyShootingUpdate()
                    {
                        ResetAnimBooleans(true, -1); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                        ForciblyResetLocation();
                        UpdateAnimator();
                    }
                    public void NotifyWindhornUpdate()
                    {
                        ResetAnimBooleans(true, -1); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                        ForciblyResetLocation();
                        UpdateAnimator();
                    }
                    public void NotifyReloadingUpdate(int Int_Input)
                    {
                        I_Int_BulletCount = (I_Int_BulletCount + Int_Input <= 6) ?
                                                I_Int_BulletCount + Int_Input : 
                                                6;
                        I_Image_Cillinder.sprite = I_SpriteA1_Bullets[I_Int_BulletCount];
                    }
                #endregion Combat
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
                                    R_Bool_IsStunned  || // Stop If Afected By A Windhorn //
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
                                        // Notify Opponent //
                                            Hashtable Hsh_Keys = new Hashtable
                                            {
                                                { 
                                                    Location_KEY, 
                                                    (Vector2) 
                                                    (
                                                        (Int_PotentialLocation) switch
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
                                            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyMovementUpdate),RpcTarget.Others, Hsh_Keys);
                                    }
                            }
                            else
                            {   
                                // Coyote Relocation //
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
                                    R_Bool_IsStunned       || // Stop If Afected By A Windhorn //
                                    R_Bool_IsReloading     || // Stop If Reloading //
                                    !O_Bool_CanShoot       || // Stop If Not Allowed To Shoot //
                                    I_Int_BulletCount < 1     // Stop If Cillinder Is Empty //
                                )
                                return;
                            #endregion Should Proceed [?] 
                        // Logic //
                            // Animation //
                                ResetAnimBooleans(true, 2); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                                ForciblyResetLocation();
                                UpdateAnimator();
                            // Spawn Bullet //
                                I_Aim_Own.SpawnBullet();
                                O_Bool_CanShoot = false;
                            // Reduce Bullets //
                                I_Int_BulletCount--;
                                I_Image_Cillinder.sprite = I_SpriteA1_Bullets[I_Int_BulletCount];
                            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyShootUpdate),RpcTarget.Others, R_Int_LocationIndex);
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
                                    R_Bool_IsStunned       || // Stop If Afected By A Windhorn //
                                    R_Bool_IsShooting      || // Stop If Shooting //
                                    I_Int_BulletCount > 5     // Stop If Cillinder Is Full //
                                )
                                return;
                            #endregion Should Proceed [?]
                        //  Logic //
                            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyReloadUpdate),RpcTarget.Others, R_Int_LocationIndex);

                    }
                    public void OnStartedWindhorn() 
                    {
                        // Valdidation //
                            #region    Should Proceed [?]
                                if 
                                (
                                    R_Bool_WasKilled       || // Stop If Dead //
                                    R_Bool_WasDamaged      || // Stop If Damaged //
                                    R_Bool_IsStunned      // Stop If Afected By A Windhorn //
                                )
                                return;
                            #endregion Should Proceed [?]
                        // Logic //
                            GetComponent<PhotonView>().RPC(nameof(RPC_NotifyWindhornUpdate),RpcTarget.Others, R_Int_LocationIndex);
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
        #region    Events
            public void OwnHideStart()
            {

            }
            public void OwnHideOver()
            {
                if (R_Bool_IsRelocating)
                {
                    R_Bool_IsRelocating = false;
                    ResetAnimBooleans(true, -1); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                    UpdateAnimator();
                }
                else
                {
                    if (!R_Bool_HasReachedLocation && (R_Int_LocationIndex == 5))
                        R_Bool_HasReachedLocation = true;
                }
            }
            public void OwnShowStart()
            {

            }
            public void OwnShowOver()
            {
                if (!R_Bool_HasReachedLocation)
                     R_Bool_HasReachedLocation = true;
            }
            public void OwnDamageOver()
            {
                ResetAnimBooleans(true, -1); // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
                ForciblyResetLocation();
                UpdateAnimator();
            }
            public void OwnShootOver()
            {
            }
        #endregion Events
        #region    Reusables
            public void ResetAnimBooleans(bool? B_Input, int Int_Input) // -1: None / 0: IsHidden? / 1: IsDamaged? / 2: IsShooting? / 3: IsReloading? / 4: IsWindhorning? / 5: IsStunned?  / 6: Healing PU / 7: FullReload //
            {
                if (B_Input != null)
                    R_Bool_HasReachedLocation = (bool) B_Input;
        
                R_Bool_IsHiding        = (Int_Input == 0) ? true : false;
                R_Bool_IsDamaged       = (Int_Input == 1) ? true : false;
                R_Bool_IsShooting      = (Int_Input == 2) ? true : false;
                R_Bool_IsReloading     = ((Int_Input == 3) || (Int_Input == 7)) ? true : false;
                R_Bool_IsWindhorning   = (Int_Input == 4) ? true : false;
                R_Bool_IsStunned       = (Int_Input == 5) ? true : false;
        
                R_Bool_IsHealing       = (Int_Input == 6) ? true : false;
                R_Bool_IsFullReloading = (Int_Input == 7) ? true : false;
            }
            public void UpdateAnimator()
            {
                // Values //
                    I_Anim_Character.SetBool("IsHidden"     , R_Bool_IsHiding);
                    I_Anim_Character.SetBool("IsDamaged"    , R_Bool_IsDamaged);
                    I_Anim_Character.SetBool("IsShooting"   , R_Bool_IsShooting);
                    I_Anim_Character.SetBool("IsReloading"  , R_Bool_IsReloading);
                    I_Anim_Character.SetBool("IsWindhorning", R_Bool_IsWindhorning);
                    I_Anim_Character.SetBool("IsStunned"    , R_Bool_IsStunned);
                    I_Anim_Character.SetInteger("Location"  , R_Int_LocationIndex);
                    I_Anim_Character.SetInteger("PowerUp"  ,  (R_Bool_IsHealing) ?
                                                                 1 :
                                                                 (R_Bool_IsFullReloading) ?
                                                                    2 :
                                                                    0
                                               );
                // Proceed //
                    I_Anim_Character.speed = 0f;
                    I_Anim_Character.Update(0f);
                    I_Anim_Character.speed = 1f;
            }
            public void ForciblyResetLocation()
            {
                string Str_Temp = (I_Anim_Character.GetCurrentAnimatorClipInfo(0))[0].clip.name;
                R_Int_LocationIndex = (Str_Temp.Substring(Math.Max(0, Str_Temp.Length - 2))) switch
                {
                    "LL" => 0,   // Bottom Left Corner  //
                    "TL" => 1,   // Top Left Corner     //
                    "TM" => 2,   // Top Middle Corner   //
                    "TR" => 3,   // Top Right Corner    //
                    "LR" => 4,   // Bottom Right Corner //
                    _ => 5,      // Bottom (Hide)       //
                };
                R_Vec2_StashedLocation = new Vector2();
            }
        #endregion Reusables
    #endregion Animation
}
