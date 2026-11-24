using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Aplica al iniciar el juego los volúmenes guardados desde el menú de configuración (música/ambiente y SFX).
/// Debe ir en un objeto que esté activo desde el inicio (ej: Music): el panel de configuración empieza desactivado
/// y solo aplica los volúmenes cuando el jugador lo abre.
/// </summary>
public class AudioVolumeSettings : MonoBehaviour
{
    public const string AmbientKey = "Config_Ambient";
    public const string SfxKey = "Config_SFX";
    public const float DefaultAmbient = 0.8f;
    public const float DefaultSfx = 1f;

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string ambientMixerParam = "AmbientVol";
    [SerializeField] private string sfxMixerParam = "SFXVol";

    public static float SavedAmbient => PlayerPrefs.GetFloat(AmbientKey, DefaultAmbient);
    public static float SavedSfx => PlayerPrefs.GetFloat(SfxKey, DefaultSfx);

    // En Start y no en Awake/OnEnable: AudioMixer.SetFloat no tiene efecto si se llama mientras carga la escena.
    private void Start()
    {
        Apply(mixer, ambientMixerParam, SavedAmbient);
        Apply(mixer, sfxMixerParam, SavedSfx);
    }

    /// <summary>Pone el volumen de un parámetro expuesto del mixer a partir de un valor de slider (0 a 1).</summary>
    public static void Apply(AudioMixer mixer, string parameter, float value)
    {
        if (mixer == null || string.IsNullOrEmpty(parameter))
            return;

        if (!mixer.SetFloat(parameter, ToDecibels(value)))
            Debug.LogWarning($"AudioVolumeSettings: el mixer '{mixer.name}' no tiene el parámetro expuesto '{parameter}'.", mixer);
    }

    /// <summary>Slider (0 a 1) a decibeles: 1 = 0 dB, 0 = -80 dB (silencio).</summary>
    public static float ToDecibels(float value) => Mathf.Log10(Mathf.Max(0.0001f, value)) * 20f;
}
