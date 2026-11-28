using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI _proximityText;
    [SerializeField] private float _proximityTextDisplayTime = 0.5f;
    private float _proxTextTimer = 0;
    [SerializeField] private Slider _playerHealthBar;
    [SerializeField] private Slider _enemyHealthBar;

    [Header("Panels")]
    [SerializeField] private GameObject _lossPanel;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _offsetCalculationPanel;
    [SerializeField] private GameObject _offsetCalculationDetailsPanel;
    [SerializeField] private TextMeshProUGUI _offsetDisplayText;

    [Header("Audio")]
    public AudioClip DefaultClickSound;
    private AudioSource _audioSource;

    [Header("Proximity Text")]
    [SerializeField] private Color _missColor;
    [SerializeField] private string _missText;
    [SerializeField] private Color _perfectColor;
    [SerializeField] private string _perfectText;
    [SerializeField] private Color _closeColor;
    [SerializeField] private string _closeText;
    [SerializeField] private Color _nearMissColor;
    [SerializeField] private string _nearMissText;

    [Header("Debug")]
    [SerializeField] private bool _debugMessages = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        _proximityText.color = new Color(0, 0, 0, 0);

        Judge.OnJudgementRating += SetProximityText;
        PlayerStatus.OnPlayerHealthChange += UpdatePlayerHealthBar;
        EnemyEncounter.OnEnemyHealthChange += UpdateEnemyHealthBar;
        GameManager.ResetGameState += ResetState;

        ResetState();
        ToggleGameplayUI(false);
        AddButtonSoundListeners();
    }

    // Update is called once per frame
    void Update()
    {
        _proxTextTimer -= Time.deltaTime;

        Color proxTextColor = _proximityText.color;

        proxTextColor.a = Mathf.Clamp01(_proxTextTimer / (_proximityTextDisplayTime / 2));

        _proximityText.color = proxTextColor;
    }

    public void ClickSound(AudioClip clip) 
    {
        if (clip == null) return;

        _audioSource.PlayOneShot(clip);
    }

    private void AddButtonSoundListeners() 
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            // Check if this specific button has a custom sound attached
            if (btn.TryGetComponent<CustomButtonSound>(out CustomButtonSound customSound) && customSound.Clip != null)
            {
                btn.onClick.AddListener(() => ClickSound(customSound.Clip));
            }
            else
            {
                btn.onClick.AddListener(() => ClickSound(DefaultClickSound));
            }
        }
    }

    private void UpdateEnemyHealthBar(Health health)
    {
        if (_debugMessages) Debug.Log($"Enemy health bar value updated to {(float)health.CurrentHealth / health.MaxHealth}");
        _enemyHealthBar.value = (float)health.CurrentHealth / health.MaxHealth;
    }

    private void UpdatePlayerHealthBar(Health health) 
    {
        if (_debugMessages) Debug.Log($"Player health bar value updated to {(float)health.CurrentHealth / health.MaxHealth}");
        _playerHealthBar.value = (float)health.CurrentHealth / health.MaxHealth;
    }

    private void SetProximityText(Judgement judgement, double proximity) 
    {
        string text = "ERROR";
        Color color = Color.white;

        switch (judgement) 
        {
            case Judgement.Perfect:
                text = _perfectText;
                color = _perfectColor;
                break;

            case Judgement.Great:
                text = _closeText;
                color = _closeColor;
                break;

            case Judgement.OK:
                text = _nearMissText;
                color = _nearMissColor;
                break;

            case Judgement.Miss:
                text = _missText;
                color = _missColor;
                break;
        }

        _proximityText.text = text + " (" + Math.Round(proximity) + ")";
        _proximityText.color = color;
        _proxTextTimer = _proximityTextDisplayTime;
    }

    public void ShowWinPanel() 
    {
        _winPanel.SetActive(true);
    }

    public void ShowLossPanel()
    {
        _lossPanel.SetActive(true);
    }

    public void ToggleOffsetCalculationPanel(bool toggle) 
    {
        _offsetCalculationPanel.SetActive(toggle);
    }

    public void ToggleDisplayOffsetCalculation(bool toggle) 
    {
        _offsetCalculationDetailsPanel.SetActive(toggle);
        _offsetDisplayText.text = GameManager.Instance.OffsetCalculator.GetOffsetAverageString();
    }

    private void ResetState() 
    {
        _lossPanel.SetActive(false);
        _winPanel.SetActive(false);
        ToggleGameplayUI(false);
        ToggleOffsetCalculationPanel(false);
        ToggleMainMenu(true);
    }

    public void ToggleGameplayUI(bool toggle) 
    {
        _playerHealthBar.gameObject.SetActive(toggle);
        _enemyHealthBar.gameObject.SetActive(toggle);
    }

    public void ToggleMainMenu(bool toggle) 
    {
        _menuPanel.SetActive(toggle);
    }
}
