using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Gameplay UI")]
    [SerializeField] private TextMeshProUGUI proximityText;
    [SerializeField] private float proximityTextDisplayTime = 0.5f;
    private float proxTextTimer = 0;
    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Slider enemyHealthBar;

    [Header("Panels")]
    [SerializeField] private GameObject LossPanel;
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject OffsetCalculationPanel;
    [SerializeField] private GameObject OffsetCalculationDetailsPanel;
    [SerializeField] private TextMeshProUGUI OffsetDisplayText;

    [Header("Audio")]
    public AudioClip DefaultClickSound;
    private AudioSource _audioSource;

    [Header("Proximity Text")]
    [SerializeField] private Color missColor;
    [SerializeField] private string missText;
    [SerializeField] private Color perfectColor;
    [SerializeField] private string perfectText;
    [SerializeField] private Color closeColor;
    [SerializeField] private string closeText;
    [SerializeField] private Color nearMissColor;
    [SerializeField] private string nearMissText;

    [Header("Debug")]
    [SerializeField] private bool DebugMessages = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        proximityText.color = new Color(0, 0, 0, 0);

        Judge.OnValidateInputWithProximity += SetProximityText;
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
        proxTextTimer -= Time.deltaTime;

        Color proxTextColor = proximityText.color;

        proxTextColor.a = Mathf.Clamp01(proxTextTimer / (proximityTextDisplayTime / 2));

        proximityText.color = proxTextColor;
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
        if (DebugMessages) Debug.Log($"Enemy health bar value updated to {(float)health.CurrentHealth / health.MaxHealth}");
        enemyHealthBar.value = (float)health.CurrentHealth / health.MaxHealth;
    }

    private void UpdatePlayerHealthBar(Health health) 
    {
        if (DebugMessages) Debug.Log($"Player health bar value updated to {(float)health.CurrentHealth / health.MaxHealth}");
        playerHealthBar.value = (float)health.CurrentHealth / health.MaxHealth;
    }

    private void SetProximityText(bool success, double proximity) 
    {
        string text = "ERROR";
        Color color = Color.white;

        if (!success)
        {
            text = missText;
            color = missColor;
        }
        else 
        {
            if (Math.Abs(proximity) < 10)
            {
                text = perfectText;
                color = perfectColor;
            }
            else if (Math.Abs(proximity) < 45)
            {
                text = closeText;
                color = closeColor;
            }
            else 
            {
                text = nearMissText;
                color = nearMissColor;
            }
        }

        proximityText.text = text + " (" + Math.Round(proximity) + ")";
        proximityText.color = color;
        proxTextTimer = proximityTextDisplayTime;
    }

    public void ShowWinPanel() 
    {
        WinPanel.SetActive(true);
    }

    public void ShowLossPanel()
    {
        LossPanel.SetActive(true);
    }

    public void ToggleOffsetCalculationPanel(bool toggle) 
    {
        OffsetCalculationPanel.SetActive(toggle);
    }

    public void ToggleDisplayOffsetCalculation(bool toggle) 
    {
        OffsetCalculationDetailsPanel.SetActive(toggle);
        OffsetDisplayText.text = GameManager.Instance.OffsetCalculator.GetOffsetAverageString();
    }

    private void ResetState() 
    {
        LossPanel.SetActive(false);
        WinPanel.SetActive(false);
        ToggleGameplayUI(false);
        ToggleOffsetCalculationPanel(false);
        ToggleMainMenu(true);
    }

    public void ToggleGameplayUI(bool toggle) 
    {
        playerHealthBar.gameObject.SetActive(toggle);
        enemyHealthBar.gameObject.SetActive(toggle);
    }

    public void ToggleMainMenu(bool toggle) 
    {
        MenuPanel.SetActive(toggle);
    }
}
