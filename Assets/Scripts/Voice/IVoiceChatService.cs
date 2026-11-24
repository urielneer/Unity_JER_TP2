// Contrato del chat de voz - El juego (AudioManager, HUD) depende de esto, no de Photon Voice //
// Permite cambiar de proveedor (Vivox, Dissonance, etc.) sin tocar a quien lo usa //
public interface IVoiceChatService
{
    bool IsTransmitting { get; }   // El jugador local esta hablando (push-to-talk apretado y con audio) //
    bool IsRemoteSpeaking { get; } // Algun jugador remoto esta hablando (duelo 1v1: el rival) //

    void SetTransmitEnabled(bool Bool_IsEnabled);
}
