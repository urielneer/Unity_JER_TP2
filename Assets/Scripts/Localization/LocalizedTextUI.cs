using UnityEngine;
using TMPro;

public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _uiText;
    [SerializeField] private string _tableKey = "TextosMenu"; 
    [SerializeField] private string _entryKey;  

    private ILocalizationService _localizationService;

    private void Start()
    {
        // Instanciamos el servicio de localización que armamos
        _localizationService = new UnityLocalizationService();

        // Actualizamos el texto en pantalla al iniciar
        UpdateText();

        // Nos suscribimos por si el usuario cambia de idioma en caliente
        _localizationService.OnLanguageChanged += UpdateText;
    }

    private void OnDestroy()
    {
        if (_localizationService != null)
        {
            _localizationService.OnLanguageChanged -= UpdateText;
        }
    }

    private void UpdateText()
    {
        if (_uiText != null)
        {
            _uiText.text = _localizationService.GetLocalizedValue(_tableKey, _entryKey);
        }
    }
}