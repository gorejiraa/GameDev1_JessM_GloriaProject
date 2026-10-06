using System;
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
