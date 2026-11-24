using UnityEngine;
using Photon.Voice.Unity;
using Photon.Pun;

public class PhotonVoiceChatService : MonoBehaviour, IVoiceChatService
{
    [SerializeField] private Recorder _voiceRecorder;
    [SerializeField] private PhotonView _photonView;

    private void Awake()
    {
        if (_voiceRecorder == null) _voiceRecorder = GetComponent<Recorder>();
        if (_photonView == null) _photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        // Si este prefab pertenece al jugador local, activamos su micrófono. Si es remoto, lo apagamos.
        if (_photonView != null && _photonView.IsMine)
        {
            SetTransmitEnabled(true);
        }
        else
        {
            SetTransmitEnabled(false);
        }
    }

    public void SetTransmitEnabled(bool isEnabled)
    {
        if (_voiceRecorder != null)
        {
            _voiceRecorder.TransmitEnabled = isEnabled;
            Debug.Log($"[VoiceService] Transmisión de voz establecida a: {isEnabled}");
        }
    }

    public bool IsTransmitting()
    {
        return _voiceRecorder != null && _voiceRecorder.TransmitEnabled;
    }
}