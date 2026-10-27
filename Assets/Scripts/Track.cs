using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum SongDifficulty 
{
    Easy,
    Medium,
    Hard,
    Impossible
}

[CreateAssetMenu(fileName = "Track", menuName = "Scriptable Objects/Track")]
public class Track : ScriptableObject
{
    [Header("Song Information")]
    public string Name;
    public SongDifficulty Difficulty;
    public AudioClip Song;
    public float BPM;

    [Header("Gameplay Numbers")]
    public float Offset;
    public int GameplayStartBeat = 0;
    public int BeatsInBar = 8;
    public bool HalfBeatsEnabled = false;

    [Header("Difficulty")]
    public int EnemyHealth = 10;
    public int PlayerHealth = 5;
    public int TimingWindow = 100;
    public bool Tutorial = false;

    [Header("Patterns")]
    public bool RandomPatterns = true;
    public List<Pattern> Patterns;

    [Header("Dialogue")]
    public List<TimedDialogue> Dialogue;
}
