using System;
using System.Collections.Generic;
using UnityEngine;

public enum Note
{
   Blank, 
   Up, 
   Down, 
   Left, 
   Right,
   Finish = 7
}

[Serializable]
public struct NoteActionDetails 
{
    public Note Note;
    public Sprite Sprite;
    public AudioClip Sound;
}

[Serializable]
public struct Pattern 
{
    public bool HalfBeats;
    public List<Note> Notes;
}
