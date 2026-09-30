using System.Linq;
using System;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static CustomExtension.ArrayExtensions;
public class Menu_ChatElement : BaseMenuElement
{
    #region    Variables
        [Header("Chat Settings")]
            [SerializeField] MenuListContainerChat _chat;
            MenuListContainerChat I_Ctm_MLCC_Chat;
            [SerializeField] BaseMenuInputField _chatElement;
            BaseMenuInputField IO_Ctm_BMIF_ChatElement;
            public BaseMenuInputField Element => IO_Ctm_BMIF_ChatElement;
            [SerializeField] int _chatLength;
            int I_Int_ChatLength;
            private string[] I_Str_A1_ChatLog = new string[1];
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Resume Base //
                    base.Awake();
                // Variables //
                    I_Ctm_MLCC_Chat = _chat;
                    I_Int_ChatLength = _chatLength;
                     IO_Ctm_BMIF_ChatElement = _chatElement;
                    I_Str_A1_ChatLog = new string[0];
            }
            protected override void OnValidate()
            {
                // Resume Base //
                    base.OnValidate();
            }
        #endregion Unity Methods
        #region    Custom Methods
            // Game Manager //
                public void LeftRoom() 
                { 
                    // Update //
                        I_Ctm_MLCC_Chat.CleanList();
                }
                public void JoinRoom()
                {
                    // Listeners //    
                }
                public void GameStart() 
                {
                    I_Str_A1_ChatLog = new string[0];
                    I_Ctm_MLCC_Chat.CleanList();
                    AddPlayerControllerListener();
                    DisableChatUsage();
                    IO_Ctm_BMIF_ChatElement.gameObject.SetActive(false); 
                }
                public void GameEnd()
                {
                    RemovePlayerControllerListener();
                }
            // Element //
                public void UpdateChat()
                {
                    // Update Own Room Prefab //
                        I_Ctm_MLCC_Chat.SetList(I_Str_A1_ChatLog.OfType<object>().ToList());  
                    LayoutRebuilder.ForceRebuildLayoutImmediate(this.gameObject.GetComponent<RectTransform>()); 
                }
                public void SendChat(string Str_Player, string Str_Message)
                {                    
                    string Str_Full = Str_Player + ": " + Str_Message;
                    GetComponent<PhotonView>().RPC(nameof(RPC_SendMessageToMaster),RpcTarget.All, Str_Full);
                }
                public void EnableChatUsage()
                {
                    if (IO_Ctm_BMIF_ChatElement.InputField.isFocused) return;
                    IO_Ctm_BMIF_ChatElement.gameObject.SetActive(true);
                    IO_Ctm_BMIF_ChatElement.InputField.Select();
                    IO_Ctm_BMIF_ChatElement.InputField.interactable = true;
                    EventSystem.current.SetSelectedGameObject(IO_Ctm_BMIF_ChatElement.InputField.gameObject);
                    IO_Ctm_BMIF_ChatElement.InputField.ActivateInputField();      
                }
                public void DisableChatUsage()
                {
                    if (!IO_Ctm_BMIF_ChatElement.InputField.isFocused) return;
                    IO_Ctm_BMIF_ChatElement.gameObject.SetActive(false);
                    IO_Ctm_BMIF_ChatElement.InputField.interactable = false;
                    EventSystem.current.SetSelectedGameObject(null);
                    IO_Ctm_BMIF_ChatElement.InputField.DeactivateInputField();     
                }
        #endregion Custom Methods
        #region    Listener Methods
            #region    Caller Methods
                // Message //
                // Input Manager //
                    private void AddPlayerControllerListener()
                    {
                        IO_Ctm_BMIF_ChatElement.InputField.onSelect.RemoveListener(MasterManager.Instance.InputManager.OnActivateControllerInput);
                        IO_Ctm_BMIF_ChatElement.InputField.onSelect.AddListener(MasterManager.Instance.InputManager.OnActivateControllerInput);
        
                        IO_Ctm_BMIF_ChatElement.InputField.onDeselect.RemoveListener(MasterManager.Instance.InputManager.OnDeactivateControllerInput);
                        IO_Ctm_BMIF_ChatElement.InputField.onDeselect.AddListener(MasterManager.Instance.InputManager.OnDeactivateControllerInput);
                    }
                    private void RemovePlayerControllerListener()
                    {
                        IO_Ctm_BMIF_ChatElement.InputField.onSelect.RemoveListener(MasterManager.Instance.InputManager.OnActivateControllerInput);
        
                        IO_Ctm_BMIF_ChatElement.InputField.onDeselect.RemoveListener(MasterManager.Instance.InputManager.OnDeactivateControllerInput);
                    }
            #endregion Caller Methods
            #region    Callee Methods
            #endregion Callee Methods
        #endregion Listener Methods
        #region    PUN
            #region    RPC
                [PunRPC]
                private void RPC_SendMessageToMaster(string Str_Log)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Update //
                            // Is Full? //
                                if (I_Str_A1_ChatLog.Length == I_Int_ChatLength)
                                {
                                    RemoveByIndexFromArray(ref I_Str_A1_ChatLog, 0);
                                }
                            // Proceed //
                                AddNewToArray(ref I_Str_A1_ChatLog, Str_Log);
                        // Force Update Other Client's Scoreboard Prefab //
                            GetComponent<PhotonView>().RPC(nameof(RPC_ForceUpdateListToClients),RpcTarget.All, new object[] { I_Str_A1_ChatLog }); // ¿Por que object? PORQUE NO PARABA DE DAR FAIL Y ME VOLVIO LOCO!!!1! <3 //
                    }
                }
                [PunRPC]
                private void RPC_ForceUpdateListToClients(string[] Str_A1_Logs)
                {
                    // If Empty //
                        if (Str_A1_Logs.Length == 0) return;
                    // Update //
                        I_Str_A1_ChatLog = Str_A1_Logs;
                        UpdateChat();
                }
            #endregion RPC
        #endregion PUN
    #endregion Methods
}
