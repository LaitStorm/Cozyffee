using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class ConfigMenuUI : MonoBehaviour
{
    [Header("Documentos UI")]
    [SerializeField] private UIDocument configDocument;
    [SerializeField] private UIDocument menuDocument;

    [Header("Audio (AudioMixer o AudioSources)")]
    [Tooltip("Opcional: Si usas AudioMixer, expón los parámetros 'AmbientVol' y 'SFXVol'.")]
    [SerializeField] private AudioMixer mainAudioMixer;
    [SerializeField] private string ambientMixerParam = "AmbientVol";
    [SerializeField] private string sfxMixerParam = "SFXVol";

    [Tooltip("Alternativa directa sin AudioMixer: arrastra aquí el AudioSource de música/ambiente.")]
    [SerializeField] private AudioSource ambientAudioSource;
    [Tooltip("Alternativa directa sin AudioMixer: arrastra aquí los AudioSources de efectos (cafetera, campana, etc.).")]
    [SerializeField] private List<AudioSource> sfxAudioSources = new List<AudioSource>();

    [Header("Control de NPCs")]
    [Tooltip("Reparte el máximo de clientes entre los generadores. Los NPCs se agregan o quitan al cerrar este panel.")]
    [SerializeField] private Game.Cozyffee.NPCPopulation npcPopulation;

    [Header("Locomoción (Giro)")]
    [SerializeField] private SnapTurnProvider snapTurnProvider;
    [SerializeField] private ContinuousTurnProvider continuousTurnProvider;

    // Nombres exactos en Config.uxml
    private const string SliderAmbienteName = "Slider_ambiente";
    private const string SliderSfxName = "Slider_sfx";
    private const string SliderNpcsName = "Slider_npcs";
    private const string LabelNpcsValorName = "Label_npcs_valor";
    private const string ToggleSnapTurnName = "Toggle_snap_turn";
    private const string BtnVolverName = "Boton_volver_menu";

    private Slider sliderAmbiente;
    private Slider sliderSfx;
    private SliderInt sliderNpcs;
    private Label labelNpcsValor;
    private Toggle toggleSnapTurn;
    private Button btnVolver;

    private void Awake()
    {
        if (configDocument == null)
            configDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        if (configDocument == null) return;

        VisualElement root = configDocument.rootVisualElement;

        sliderAmbiente = root.Q<Slider>(SliderAmbienteName);
        sliderSfx = root.Q<Slider>(SliderSfxName);
        sliderNpcs = root.Q<SliderInt>(SliderNpcsName);
        labelNpcsValor = root.Q<Label>(LabelNpcsValorName);
        toggleSnapTurn = root.Q<Toggle>(ToggleSnapTurnName);
        btnVolver = root.Q<Button>(BtnVolverName);

        // Cargar valores guardados previamente (PlayerPrefs) y sincronizar UI
        // Al iniciar el juego estos volúmenes los aplica AudioVolumeSettings (objeto Music), porque este panel empieza desactivado.
        float savedAmbient = AudioVolumeSettings.SavedAmbient;
        float savedSfx = AudioVolumeSettings.SavedSfx;
        int savedNpcs = npcPopulation != null ? npcPopulation.MaxCount : 0;
        bool savedSnapTurn = PlayerPrefs.GetInt("Config_SnapTurn", 0) == 1;

        if (sliderAmbiente != null)
        {
            sliderAmbiente.SetValueWithoutNotify(savedAmbient);
            sliderAmbiente.RegisterValueChangedCallback(OnAmbientChanged);
        }

        if (sliderSfx != null)
        {
            sliderSfx.SetValueWithoutNotify(savedSfx);
            sliderSfx.RegisterValueChangedCallback(OnSfxChanged);
        }

        if (sliderNpcs != null)
        {
            // Rango 0 a 20 (definido en COnfiguracion.uxml)
            sliderNpcs.SetValueWithoutNotify(Mathf.Clamp(savedNpcs, sliderNpcs.lowValue, sliderNpcs.highValue));
            sliderNpcs.RegisterValueChangedCallback(OnNpcsChanged);
            UpdateNpcLabel(sliderNpcs.value);
        }

        if (toggleSnapTurn != null)
        {
            toggleSnapTurn.SetValueWithoutNotify(savedSnapTurn);
            toggleSnapTurn.RegisterValueChangedCallback(OnSnapTurnChanged);
        }

        if (btnVolver != null)
        {
            btnVolver.clicked += OnVolverClicked;
        }

        // Aplicar los valores iniciales al abrir
        ApplyAmbientVolume(savedAmbient);
        ApplySfxVolume(savedSfx);
        ApplyTurnMode(savedSnapTurn);
    }

    private void OnDisable()
    {
        if (sliderAmbiente != null) sliderAmbiente.UnregisterValueChangedCallback(OnAmbientChanged);
        if (sliderSfx != null) sliderSfx.UnregisterValueChangedCallback(OnSfxChanged);
        if (sliderNpcs != null) sliderNpcs.UnregisterValueChangedCallback(OnNpcsChanged);
        if (toggleSnapTurn != null) toggleSnapTurn.UnregisterValueChangedCallback(OnSnapTurnChanged);
        if (btnVolver != null) btnVolver.clicked -= OnVolverClicked;

        PlayerPrefs.Save();

        // Panel cerrado (botón Volver o botón de menú del control): ahora sí se agregan o quitan NPCs
        if (npcPopulation != null)
            npcPopulation.ApplyPendingChanges();
    }

    private void OnAmbientChanged(ChangeEvent<float> evt)
    {
        ApplyAmbientVolume(evt.newValue);
        PlayerPrefs.SetFloat(AudioVolumeSettings.AmbientKey, evt.newValue);
    }

    private void OnSfxChanged(ChangeEvent<float> evt)
    {
        ApplySfxVolume(evt.newValue);
        PlayerPrefs.SetFloat(AudioVolumeSettings.SfxKey, evt.newValue);
    }

    private void OnNpcsChanged(ChangeEvent<int> evt)
    {
        // Solo se guarda: los NPCs se agregan o quitan al cerrar el panel (OnDisable)
        if (npcPopulation != null)
            npcPopulation.SetMaxCount(evt.newValue);

        UpdateNpcLabel(evt.newValue);
    }

    private void OnSnapTurnChanged(ChangeEvent<bool> evt)
    {
        ApplyTurnMode(evt.newValue);
        PlayerPrefs.SetInt("Config_SnapTurn", evt.newValue ? 1 : 0);
    }

    private void ApplyAmbientVolume(float value)
    {
        if (mainAudioMixer != null)
        {
            AudioVolumeSettings.Apply(mainAudioMixer, ambientMixerParam, value);
        }
        else if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = value;
        }
    }

    private void ApplySfxVolume(float value)
    {
        if (mainAudioMixer != null)
        {
            AudioVolumeSettings.Apply(mainAudioMixer, sfxMixerParam, value);
        }
        else
        {
            foreach (AudioSource src in sfxAudioSources)
            {
                if (src != null) src.volume = value;
            }
        }
    }

    private void UpdateNpcLabel(int count)
    {
        if (labelNpcsValor != null)
            labelNpcsValor.text = count.ToString();
    }

    private void ApplyTurnMode(bool useSnapTurn)
    {
        if (snapTurnProvider != null)
            snapTurnProvider.enabled = useSnapTurn;

        if (continuousTurnProvider != null)
            continuousTurnProvider.enabled = !useSnapTurn;
    }

    private void OnVolverClicked()
    {
        if (menuDocument != null)
            menuDocument.gameObject.SetActive(true);

        configDocument.gameObject.SetActive(false);
    }
}