using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SongSelectMenu : MonoBehaviour
{
    public List<Track> Songs;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DifficultyText;

    private int TrackIndex = 0;

    [SerializeField] private Color easyColor = Color.green;
    [SerializeField] private Color mediumColor = Color.orange;
    [SerializeField] private Color hardColor = Color.red;
    [SerializeField] private Color impossibleColor = Color.darkRed;

    private void OnEnable()
    {
        TrackIndex = 0;
        DisplaySongDetails();
    }

    public void OnUpButton()
    {
        TrackIndex--;

        if (TrackIndex < 0) TrackIndex = Songs.Count-1;

        DisplaySongDetails();
    }

    public void OnDownButton()
    {
        TrackIndex++;

        if (TrackIndex >= Songs.Count) TrackIndex = 0;

        DisplaySongDetails();
    }

    public void OnStart() 
    {
        GameManager.Instance.StartTrack(Songs[TrackIndex]);
    }

    public void OnBack() 
    {
        GameManager.Instance.ToggleSongSelect(false);
    }

    public void DisplaySongDetails() 
    {
        TitleText.text = Songs[TrackIndex].Name;
        DifficultyText.text = Songs[TrackIndex].Difficulty.ToString();

        switch (Songs[TrackIndex].Difficulty) 
        {
            case SongDifficulty.Easy:
                DifficultyText.color = easyColor;
                break;
            case SongDifficulty.Medium:
                DifficultyText.color = mediumColor;
                break;
            case SongDifficulty.Hard:
                DifficultyText.color = hardColor;
                break;
            case SongDifficulty.Impossible:
                DifficultyText.color = impossibleColor;
                break;
        }
    }
}
