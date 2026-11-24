using System;
using System.Collections.Generic;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using UnityEngine;

// Chat de voz con Photon Voice 2 //
// - Un unico PunVoiceClient + un unico Recorder (microfono local), creados por codigo y persistentes. //
//   El PunVoiceClient "sigue" a PUN: cuando PUN entra a la sala X, Voice entra a la sala "X_voice_". //
// - Los personajes son objetos de escena (no PhotonNetwork.Instantiate), asi que NO se usa PhotonVoiceView //
//   (dependia de IsMine, que en objetos de escena solo es true para el MasterClient). //
//   Cada voz remota recibe un Speaker clonado de una plantilla: camino "userData == null" de PunVoiceClient. //
public sealed class PunVoiceChatService : IVoiceChatService, IDisposable
{
    #region    Constants
        private const string ROOT_NAME          = "VoiceChat";
        private const string SPEAKER_TEMPLATE   = "SpeakerTemplate";
        private const int    EXPECTED_SPEAKERS  = 2; // Duelo 1v1: con 2 alcanza y sobra //
    #endregion Constants
    #region    Variables
        private readonly GameObject I_GObj_Root;
        private readonly PunVoiceClient I_PVC_Client;
        private readonly Recorder I_Rec_Microphone;
        private readonly List<Speaker> R_Lst_Speakers = new List<Speaker>(EXPECTED_SPEAKERS);
    #endregion Variables
    #region    Properties
        public bool IsTransmitting => I_Rec_Microphone != null && I_Rec_Microphone.IsCurrentlyTransmitting;
        public bool IsRemoteSpeaking
        {
            get
            {
                // Recorrido sin LINQ ni allocations: se consulta todos los frames desde el AudioManager //
                for (int Int_Index = R_Lst_Speakers.Count - 1; Int_Index >= 0; Int_Index--)
                {
                    Speaker Spk_Remote = R_Lst_Speakers[Int_Index];
                    if (Spk_Remote == null) { R_Lst_Speakers.RemoveAt(Int_Index); continue; } // Destruido al irse la voz remota //
                    if (Spk_Remote.IsPlaying) return true;
                }
                return false;
            }
        }
    #endregion Properties
    #region    Methods
        #region    Constructor / Dispose
            public PunVoiceChatService()
            {
                // Raiz persistente - Sobrevive a los cambios de escena igual que los managers //
                    I_GObj_Root = new GameObject(ROOT_NAME);
                    UnityEngine.Object.DontDestroyOnLoad(I_GObj_Root);
                // Microfono - Graba mientras estemos en la sala de voz, pero transmite SOLO con push-to-talk //
                    I_Rec_Microphone = I_GObj_Root.AddComponent<Recorder>();
                    I_Rec_Microphone.RecordWhenJoined = true;
                    I_Rec_Microphone.TransmitEnabled = false;
                    I_Rec_Microphone.VoiceDetection = false;
                // Plantilla de Speaker - Audio 2D: la camara es fija, no tiene sentido la espacializacion //
                    GameObject GObj_Template = new GameObject(SPEAKER_TEMPLATE, typeof(AudioSource));
                    GObj_Template.transform.SetParent(I_GObj_Root.transform, false);
                    AudioSource Src_Template = GObj_Template.GetComponent<AudioSource>();
                    Src_Template.playOnAwake = false;
                    Src_Template.spatialBlend = 0f;
                    GObj_Template.AddComponent<Speaker>();
                // Cliente de voz - Toma el AppIdVoice y la region de PhotonServerSettings //
                    I_PVC_Client = I_GObj_Root.AddComponent<PunVoiceClient>();
                    I_PVC_Client.UsePunAppSettings = true;
                    I_PVC_Client.UsePunAuthValues = true;
                    I_PVC_Client.SpeakerPrefab = GObj_Template;
                    I_PVC_Client.PrimaryRecorder = I_Rec_Microphone;
                    I_PVC_Client.SpeakerLinked += OnSpeakerLinked;
                // "Use Primary Recorder" solo se puede tildar desde el Inspector: se registra el Recorder a mano //
                    if (!I_PVC_Client.AddRecorder(I_Rec_Microphone))
                        Debug.LogWarning("[Voice] No se pudo registrar el microfono en el PunVoiceClient");
                    Debug.Log("[Voice] Chat de voz inicializado (push-to-talk)");
            }
            public void Dispose()
            {
                if (I_PVC_Client != null) I_PVC_Client.SpeakerLinked -= OnSpeakerLinked;
                if (I_GObj_Root != null) UnityEngine.Object.Destroy(I_GObj_Root);
                R_Lst_Speakers.Clear();
            }
        #endregion Constructor / Dispose
        #region    API
            public void SetTransmitEnabled(bool Bool_IsEnabled)
            {
                if (I_Rec_Microphone == null || I_Rec_Microphone.TransmitEnabled == Bool_IsEnabled) return;
                I_Rec_Microphone.TransmitEnabled = Bool_IsEnabled;
            }
        #endregion API
        #region    Photon Voice Callbacks
            private void OnSpeakerLinked(Speaker Spk_New)
            {
                if (Spk_New == null || R_Lst_Speakers.Contains(Spk_New)) return;
                R_Lst_Speakers.Add(Spk_New);
                Debug.Log("[Voice] Voz remota conectada: " + Spk_New.name);
            }
        #endregion Photon Voice Callbacks
    #endregion Methods
}
