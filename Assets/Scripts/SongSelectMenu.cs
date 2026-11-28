using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SongSelectMenu : MonoBehaviour
{
    public List<Track> Songs;

    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DifficultyText;

    private int _trackIndex = 0;

    [SerializeField] private Color _easyColor = Color.green;
    [SerializeField] private Color _mediumColor = Color.orange;
    [SerializeField] private Color _hardColor = Color.red;
    [SerializeField] private Color _impossibleColor = Color.darkRed;

    private void OnEnable()
    {
        _trackIndex = 0;
        DisplaySongDetails();
    }

    public void OnUpButton()
    {
        _trackIndex--;

        if (_trackIndex < 0) _trackIndex = Songs.Count-1;

        DisplaySongDetails();
    }

    public void OnDownButton()
    {
        _trackIndex++;

        if (_trackIndex >= Songs.Count) _trackIndex = 0;

        DisplaySongDetails();
    }

    public void OnStart() 
    {
        GameManager.Instance.StartTrack(Songs[_trackIndex]);
    }

    public void OnBack() 
    {
        GameManager.Instance.ToggleSongSelect(false);
    }

    public void DisplaySongDetails() 
    {
        TitleText.text = Songs[_trackIndex].Name;
        DifficultyText.text = $"{Songs[_trackIndex].Difficulty.ToString()}\n{Songs[_trackIndex].BPM.ToString()} BPM";

        switch (Songs[_trackIndex].Difficulty) 
        {
            case SongDifficulty.Easy:
                DifficultyText.color = _easyColor;
                break;
            case SongDifficulty.Medium:
                DifficultyText.color = _mediumColor;
                break;
            case SongDifficulty.Hard:
                DifficultyText.color = _hardColor;
                break;
            case SongDifficulty.Impossible:
                DifficultyText.color = _impossibleColor;
                break;
        }
    }
}
