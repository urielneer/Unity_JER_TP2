using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using static CustomExtension.ArrayExtensions;
using System;

public class CustomInputManager : BaseManager<CharacterManager>
{
    #region    Variables
        private PlayerInputActions O_IAct_PIA_Controller;
        public PlayerInputActions ControllerInput => O_IAct_PIA_Controller;
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Variables //
                    O_IAct_PIA_Controller = new PlayerInputActions();
                // Listeners //
                    AddKeyListeners();
            }
        #endregion Override Methods
        #region    Custom Methods
            public void EnableInputPlayerActions()
            {
                O_IAct_PIA_Controller.Player.Enable();
                O_IAct_PIA_Controller.Chat.Disable();
                MasterManager.Instance.GameManager.HideCursor();
            }
            public void DisableInputPlayerActions()
            {
                O_IAct_PIA_Controller.Player.Disable();
                O_IAct_PIA_Controller.Chat.Enable();
                MasterManager.Instance.GameManager.ShowCursor();
            }
        #endregion Custom Methods
        #region    Listener Methods
            #region    Caller Methods
                // Typing //
                    private void AddKeyListeners()
                    {
                        // Variables //
                            PlayerInputActions.PlayerActions IAA_Player = O_IAct_PIA_Controller.Player;
                            PlayerInputActions.ChatActions IAA_Chat = O_IAct_PIA_Controller.Chat;
                        // Player //
                            // Move //
                                // Regular //
                                    IAA_Player.MoveRegular.performed -= OnMoveKeyPressed;
                                    IAA_Player.MoveRegular.performed += OnMoveKeyPressed;
                                    IAA_Player.MoveRegular.canceled -= OnMoveKeyCancelled;
                                    IAA_Player.MoveRegular.canceled += OnMoveKeyCancelled;
                                // Diagonal //
                                    IAA_Player.MoveDiagonal.performed -= OnMoveKeyPressed;
                                    IAA_Player.MoveDiagonal.performed += OnMoveKeyPressed;
                                    IAA_Player.MoveDiagonal.canceled -= OnMoveKeyCancelled;
                                    IAA_Player.MoveDiagonal.canceled += OnMoveKeyCancelled;
                            // Shoot //
                                IAA_Player.Shoot.performed -= OnShootKeyPressed;
                                IAA_Player.Shoot.performed += OnShootKeyPressed;
                            // Pause //
                                IAA_Player.Pause.performed -= OnPauseKeyPressed;
                                IAA_Player.Pause.performed += OnPauseKeyPressed;
                            // Chat //
                                IAA_Player.Chat.performed -= OnChatKeyPressed;
                                IAA_Player.Chat.performed += OnChatKeyPressed;
                            // Debug //
                                IAA_Player.Debug.performed -= OnDebugKeyPressed;
                                IAA_Player.Debug.performed += OnDebugKeyPressed;
                        // Chat //
                            // Submit //
                                IAA_Chat.Submit.performed -= OnSubmitChatMessage;
                                IAA_Chat.Submit.performed += OnSubmitChatMessage;
                    }
            #endregion Caller Methods
            #region    Callee Methods
                #region    Player Methods
                    public void OnDeactivateControllerInput(string Str_Input) 
                    { DisableInputPlayerActions(); }
                    public void OnActivateControllerInput(string Str_Input) 
                    { EnableInputPlayerActions(); }
                    // Pressed //
                        public void OnMoveKeyPressed(InputAction.CallbackContext IACbC_Context) 
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
                                Vector2 Vec2_Input = IACbC_Context.ReadValue<Vector2>();
                                switch (IACbC_Context.action.name)
                                {
                                    case "MoveRegular":
                                        Vec2_Input = (Vec2_Input.x, Vec2_Input.y) switch 
                                                     {
                                                        (-1.00f, +0.00f) => new Vector2(-1.00f, +0.00f),   // Bottom Left Corner  //
                                                        (-0.71f, +0.71f) => new Vector2(-1.00f, +0.00f),   // Bottom Left Corner  //
                                                        (+0.00f, +1.00f) => new Vector2(+0.00f, +1.00f),   // Top Middle Corner   //
                                                        (+0.71f, +0.71f) => new Vector2(+1.00f, +0.00f),   // Bottom Right Corner //
                                                        (+1.00f, +0.00f) => new Vector2(+1.00f, +0.00f),   // Bottom Right Corner //
                                                        _                => new Vector2(+0.00f, -1.00f),   // Default             //
                                                     };
                                    break;
                                    case "MoveDiagonal":
                                        Vec2_Input = (Vec2_Input.x, Vec2_Input.y) switch 
                                                     {
                                                        (-1.00f, +0.00f) => new Vector2(-0.71f, +0.71f),   // Top Left Corner     //
                                                        (+1.00f, +0.00f) => new Vector2(+0.71f, +0.71f),   // Bottom Right Corner //
                                                        _                => new Vector2(+0.00f, -1.00f),   // Default             //
                                                     };
                                    break;
                                }
                                Debug.Log(Vec2_Input);
                                ((EnemyCharacter) MasterManager.Instance.CharacterManager.OwnPlayer).OnStartedMoving(Vec2_Input);
                                //((PlayerPerspective) MasterManager.Instance.CharacterManager.OwnPlayer).OnStartedMoving(Vec2_Input);
                        }
                        public void OnMouseMoveKeyPressed(InputAction.CallbackContext IACbC_Context) 
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                                MasterManager.Instance.CharacterManager.OwnPlayer.OnStartedMouseMoving(IACbC_Context.ReadValue<Vector2>());
                        }
                        public void OnRotateKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                                MasterManager.Instance.CharacterManager.OwnPlayer.OnStartedRotating(IACbC_Context.ReadValue<Vector2>());
                        } 
                        public void OnJumpKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStartedJumping(); 
                        } 
                        public void OnShootKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
                                MasterManager.Instance.GameManager.ShootDummy();
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStartedStraightShooting(); 
                        }
                        public void OnCurvedShootKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStartedCurvedShooting(); 
                        }
                        public void OnPauseKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
                                if (MasterManager.Instance.MenuManager.CurrentMenu.MenuName == "PauseMenu")
                                {
                                    // Variables //
                                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                                    // Action //
                                        Ctm_MTH_Reference.ChatElement.DisableChatUsage();
                                        MasterManager.Instance.MenuManager.OpenMenu("HUD");
                                        EnableInputPlayerActions();
                                }
                                else
                                {
                                    // Variables //
                                        MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                                    // Action //
                                        Ctm_MTH_Reference.ChatElement.DisableChatUsage();
                                        MasterManager.Instance.MenuManager.OpenMenu("PauseMenu");
                                        DisableInputPlayerActions();
                                }
                            // Corroutine //
                                StartCoroutine(ClearExtraCharNextFrame());
                           
                        }
                        public void OnScoreBoardKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
                        
                            // Corroutine //
                                StartCoroutine(ClearExtraCharNextFrame());
                           
                        } 
                        public void OnChatKeyPressed(InputAction.CallbackContext IACbC_Context)
                        { 
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Variables //
                                MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                            // Action //
                                Ctm_MTH_Reference.ChatElement.EnableChatUsage();
                                DisableInputPlayerActions();
                            // Corroutine //
                                StartCoroutine(ClearExtraCharNextFrame());
                        }
                        public void OnDebugKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.DamageCharacter(MasterManager.Instance.CharacterManager.OwnPlayer.ID, 200f);
                        }   
                        public void OnDestroyItemKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.DestroyFromInventory();                            
                        }  
                        public void OnToggleSkillKeyPressed(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.ToggleSkill();                            
                        }   
                    // Cancelled //
                        public void OnMoveKeyCancelled(InputAction.CallbackContext IACbC_Context) 
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.ToggleSkill();                            
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStoppedMoving(IACbC_Context.ReadValue<Vector2>());
                        }
                        public void OnMouseMoveKeyCancelled(InputAction.CallbackContext IACbC_Context) 
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                                MasterManager.Instance.CharacterManager.OwnPlayer.OnStoppedMouseMoving(IACbC_Context.ReadValue<Vector2>());
                        }
                        public void OnRotateKeyCancelled(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStoppedRotating(IACbC_Context.ReadValue<Vector2>());
                        } 
                        public void OnJumpKeyCancelled(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
//-//                               MasterManager.Instance.CharacterManager.OwnPlayer.OnStoppedJumping(); 
                        } 
                        public void OnScoreBoardKeyCancelled(InputAction.CallbackContext IACbC_Context)
                        {
                            // Is In Proper Scene? //
                                if (SceneManager.GetActiveScene().buildIndex != 1) return;
                            // Action //
                        
                            // Corroutine //
                                StartCoroutine(ClearExtraCharNextFrame());
                           
                        } 
                #endregion Player Methods
                #region    Chat Methods
                    public void OnSubmitChatMessage(InputAction.CallbackContext IACbC_Context)
                    {
                        // Variables //
                            MenuTypeHUD Ctm_MTH_Reference = (MasterManager.Instance.MenuManager.GetMenuReference("HUD")).gameObject.GetComponent<MenuTypeHUD>();
                            BaseMenuInputField Cym_BMIF_Submit = Ctm_MTH_Reference.ChatElement.Element;
                            string Str_Submit = Ctm_MTH_Reference.ChatElement.Element.InputField.text;
                        // Submit //
                            Ctm_MTH_Reference.ChatElement.DisableChatUsage();
                            EnableInputPlayerActions();
                            if (Str_Submit.Length > 0) Ctm_MTH_Reference.ChatElement.SendChat(PhotonNetwork.LocalPlayer.NickName, Str_Submit);
                            Cym_BMIF_Submit.Overwrite("");
                        // Corroutine //
                            StartCoroutine(ClearExtraCharNextFrame());
                    }
                #endregion Chat Methods
            #endregion Callee Methods
        #endregion Listener Methods
        #region    Coroutines Methods
            private System.Collections.IEnumerator ClearExtraCharNextFrame()
            {
                // Used to avoid using the input key twice by accident //
                yield return null;
            }
        #endregion Coroutines Methods
    #endregion Methods
}
