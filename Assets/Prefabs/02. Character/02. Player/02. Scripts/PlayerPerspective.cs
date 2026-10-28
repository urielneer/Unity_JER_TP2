using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UIElements;
using static CustomExtension.ArrayExtensions;

public class PlayerPerspective : BasePlayer, IPunObservable
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
            [Space(10)]
            [Header(" Combat Settings")]
                // Runtime //
                    private bool R_Bool_IsHiding;
                    private bool R_Bool_IsShooting;
                    private bool R_Bool_IsUsingWindhorn;
                    private bool R_Bool_IsReloading;
                    private bool R_Bool_IsHealing; 

                    private bool R_Bool_IsShootOutTime;
            [Space(10)]
            [Header(" State Settings")]
                // Runtime //
                    private bool R_Bool_WasDamaged;
                    private bool R_Bool_WasKilled;
                    private bool R_Bool_IsRelocating; 
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
                    I_Anim_Character = _animator;
            }
        #endregion Unity Methods
        #region    Override Methods
        #endregion Override Methods
        #region    Custom Methods
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
                            // Get New Location //
                                R_Int_LocationIndex = (Vec2_Input.x, Vec2_Input.y) switch 
                                                      {
                                                            (-1.00f, +0.00f) => 0,   // Bottom Left Corner  //
                                                            (-0.71f, +0.71f) => 1,   // Top Left Corner     //
                                                            (+0.00f, +1.00f) => 2,   // Top Middle Corner   //
                                                            (+0.71f, +0.71f) => 3,   // Top Right Corner    //
                                                            (+1.00f, +0.00f) => 4,   // Bottom Right Corner //
                                                            _                => 5,   // Default             //
                                                      };
                            // Is Not Hide? //
                                R_Bool_IsRelocating = (R_Int_LocationIndex != 5) ? 
                                                        true: 
                                                        false;
                            // Change Position //
                                if (R_Bool_IsHiding)
                                    R_Bool_IsHiding = false;
                                else
                                    R_Bool_IsHiding = true;
                            // Animation Update? //
                    }
                    public void OnStartedShooting() 
                    {
                        // Stop If Dead //
                            if (R_Bool_WasKilled) return;
                        // Allow If It's Shootout Time //
//                            if (!R_Bool_IsShootOutTime) return;
                        // Proceed //
//                            if (!R_Bool_CanShoot) return;   
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
        public void HideStart()
        {

        }
        public void HideOver()
        {
            if (R_Bool_IsRelocating)
            {
                R_Bool_IsRelocating = false;
                R_Bool_IsHiding = false;

                I_Anim_Character.SetBool("IsHidden", R_Bool_IsHiding);
                I_Anim_Character.SetInteger("Location", R_Int_LocationIndex);
            }
        }
        public void ShowStart()
        {

        }
        public void ShowOver()
        {

        }
        public void DamageOver()
        {

        }
    #endregion Animation
}
