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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GlobalLight.intensity = 1f;

        Choreographer.OnBeatDemo += EnemyLighting;
        Choreographer.OnBeatListen += PlayerLighting;
    }

    // Update is called once per frame
    void Update()
    {
        float currentBeat = GameManager.Instance.Metronome.GetCurrentBeat();
        int dimStart = StartBattleBeat - (BeatsToDim + BeatsToBeDimBeforeStart);
        int dimEnd = StartBattleBeat - BeatsToBeDimBeforeStart;

        //Debug.Log($"Current beat: {currentBeat} (Waiting to dim at beat {dimStart})");

        if (currentBeat > dimStart && currentBeat < dimEnd)
        {
            float progress = Mathf.InverseLerp(dimStart, dimEnd, currentBeat);

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
        Debug.Log($"Set start battle beat to: {beat} (In LightingManager)");
    }

    private void EnemyLighting(int i) 
    {
        PlayerLight.SetActive(false);
        EnemyLight.SetActive(true);
    }

    private void PlayerLighting(int i)
    {
        PlayerLight.SetActive(true);
        EnemyLight.SetActive(false);
    }
}
