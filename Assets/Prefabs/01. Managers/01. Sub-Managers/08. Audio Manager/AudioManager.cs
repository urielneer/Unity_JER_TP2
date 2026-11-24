using System;
using UnityEngine;
using Photon.Pun;

// Audio y voz del juego (GDD: "Audio y voz -> AudioManager") //
public class AudioManager : BaseManager<AudioManager> // Antes heredaba BaseManager<CharacterManager>: compartia el singleton y podia autodestruirse //
{
    #region    Variables
        #region    Voice Chat
            private IVoiceChatService R_Ctm_IVCS_VoiceChat;
            public IVoiceChatService VoiceChat => R_Ctm_IVCS_VoiceChat;
            // Ultimo estado mostrado - El indicador solo se toca cuando cambia //
            private bool R_Bool_LastLocalSpeaking;
            private bool R_Bool_LastRemoteSpeaking;
        #endregion Voice Chat
    #endregion Variables
    #region    Methods
        #region    Unity Methods
            protected override void Awake()
            {
                // Singleton Operations //
                    base.Awake();
                    if (_instance != this) return;
                // Voice Chat - Una sola vez: el servicio vive toda la sesion //
                    R_Ctm_IVCS_VoiceChat = new PunVoiceChatService();
            }
            private void OnDestroy()
            {
                (R_Ctm_IVCS_VoiceChat as IDisposable)?.Dispose();
                R_Ctm_IVCS_VoiceChat = null;
            }
        #endregion Unity Methods
        #region    Override Methods
            #pragma warning disable CS1998
            public async override void OnStartUp()
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnStartUp();
                // Cada escena arranca sin transmitir //
                    SetPushToTalk(false);
            }
            #pragma warning disable CS1998
            public async override void OnUpdate(float Flt_DT)
            #pragma warning restore CS1998
            {
                // Resume Base //
                    base.OnUpdate(Flt_DT);
                // Voice Chat //
                    RefreshVoiceIndicator();
            }
        #endregion Override Methods
        #region    Voice Chat Methods
            // Lo llama el CustomInputManager (tecla V, mantener apretada) //
            public void SetPushToTalk(bool Bool_IsHeld)
            {
                if (R_Ctm_IVCS_VoiceChat == null) return;
                R_Ctm_IVCS_VoiceChat.SetTransmitEnabled(Bool_IsHeld && PhotonNetwork.InRoom);
            }
            private void RefreshVoiceIndicator()
            {
                if (R_Ctm_IVCS_VoiceChat == null) return;
                bool Bool_Local  = R_Ctm_IVCS_VoiceChat.IsTransmitting;
                bool Bool_Remote = R_Ctm_IVCS_VoiceChat.IsRemoteSpeaking;
                if (Bool_Local == R_Bool_LastLocalSpeaking && Bool_Remote == R_Bool_LastRemoteSpeaking) return;
                R_Bool_LastLocalSpeaking  = Bool_Local;
                R_Bool_LastRemoteSpeaking = Bool_Remote;
                VoiceActivityIndicator.SetState(Bool_Local, Bool_Remote);
            }
        #endregion Voice Chat Methods
    #endregion Methods
}
