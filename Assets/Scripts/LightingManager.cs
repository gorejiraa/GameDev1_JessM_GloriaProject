using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightingManager : MonoBehaviour
{
    public Light2D GlobalLight;
    public GameObject EnemyLight;
    public GameObject PlayerLight;

    public float BattleDimness = 0.8f;
    public int BeatsToDim = 8;
    public int BeatsToBeDimBeforeStart = 8;
    private int StartBattleBeat = 999;

    private bool SpotlightsEnabled = true;

    [SerializeField] private bool DebugMessages = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalLight.intensity = 1f;

        Choreographer.OnSwitchToDemoState += EnemyLighting;
        Choreographer.OnSwitchToListenState += PlayerLighting;
        GameManager.ResetGameState += ResetState;
        EnemyEncounter.OnEnemyHealthEmpty += DisableSpotlights;
        PlayerStatus.OnPlayerHealthEmpty += DisableSpotlights;
    }

    // Update is called once per frame
    void Update()
    {
        double currentBeat = GameManager.Instance.Metronome.GetCurrentBeat();
        int dimStart = StartBattleBeat - (BeatsToDim + BeatsToBeDimBeforeStart);
        int dimEnd = StartBattleBeat - BeatsToBeDimBeforeStart;

        //Debug.Log($"Current beat: {currentBeat} (Waiting to dim at beat {dimStart})");
        if (currentBeat < dimStart) 
        {
            GlobalLight.intensity = 1f;
        }
        else if (currentBeat > dimStart && currentBeat < dimEnd)
        {
            float progress = Mathf.InverseLerp(dimStart, dimEnd, (float)currentBeat);

            GlobalLight.intensity = Mathf.Lerp(1f, BattleDimness, progress);

            //Debug.Log($"Dimming lights to {GlobalLight.intensity}");
        }
        else if (currentBeat > dimEnd)
        {
            GlobalLight.intensity = BattleDimness;
        }
    }

    public void SetStartBattleBeat(int beat) 
    {
        StartBattleBeat = beat;
        if (DebugMessages) Debug.Log($"Set start battle beat to: {beat} (In LightingManager)");
    }

    private void EnemyLighting() 
    {
        if (!SpotlightsEnabled) return;
        if (DebugMessages) Debug.Log("Swapping to enemy light");

        PlayerLight.SetActive(false);
        EnemyLight.SetActive(true);
    }

    private void PlayerLighting()
    {
        if (!SpotlightsEnabled) return;
        if (DebugMessages) Debug.Log("Swapping to player light");

        PlayerLight.SetActive(true);
        EnemyLight.SetActive(false);
    }

    private void ResetState() 
    {
        if (DebugMessages) Debug.Log("Resetting lights");

        PlayerLight.SetActive(false);
        EnemyLight.SetActive(false);
        SpotlightsEnabled = true;
    }

    private void DisableSpotlights() 
    {
        ResetState();
        SpotlightsEnabled = false;
    }
}
