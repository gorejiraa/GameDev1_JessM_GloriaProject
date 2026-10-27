using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI proximityText;
    [SerializeField] private float proximityTextDisplayTime = 0.5f;
    private float proxTextTimer = 0;

    [SerializeField] private Slider playerHealthBar;
    [SerializeField] private Slider enemyHealthBar;
    [SerializeField] private GameObject LossPanel;
    [SerializeField] private GameObject WinPanel;
    [SerializeField] private GameObject MenuPanel;

    [SerializeField] private GameObject OffsetCalculationPanel;
    [SerializeField] private GameObject OffsetCalculationDetailsPanel;
    [SerializeField] private TextMeshProUGUI OffsetDisplayText;

    [SerializeField] private bool DebugMessages = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        proximityText.color = new Color(0, 0, 0, 0);

        Judge.OnValidateInput += SetProximityText;
        PlayerStatus.OnPlayerHealthChange += UpdatePlayerHealthBar;
        EnemyEncounter.OnEnemyHealthChange += UpdateEnemyHealthBar;
        GameManager.ResetGameState += ResetState;

        ResetState();
        ToggleGameplayUI(false);
    }

    // Update is called once per frame
    void Update()
    {
        proxTextTimer -= Time.deltaTime;

        Color proxTextColor = proximityText.color;

        proxTextColor.a = Mathf.Clamp01(proxTextTimer / (proximityTextDisplayTime / 2));

        proximityText.color = proxTextColor;
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

    private void SetProximityText(bool success) 
    {
        string text = "ERROR";
        Color color = Color.white;

        if (!success)
        {
            text = "Ouch!";
            color = Color.red;
        }
        else 
        {
            float proximity = GameManager.Instance.Metronome.GetProximityToNearestBeat();

            if (Mathf.Abs(proximity) < 10)
            {
                text = "Perfect!";
                color = Color.yellow;
            }
            else if (Mathf.Abs(proximity) < 45)
            {
                text = "Nice";
                color = Color.green;
            }
            else 
            {
                text = "Okay...";
                color = Color.blue;
            }
        }

        proximityText.text = text;
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
