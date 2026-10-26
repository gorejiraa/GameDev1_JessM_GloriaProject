using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Track", menuName = "Scriptable Objects/Track")]
public class Track : ScriptableObject
{
    public AudioClip Song;
    public float BPM;
    public float Offset;
    public int GameplayStartBeat = 0;
    public int BeatsInBar = 8;
    public bool HalfBeatsEnabled = false;

    public bool RandomPatterns = true;
    public List<Pattern> Patterns;

    public List<TimedDialogue> Dialogue;
}
