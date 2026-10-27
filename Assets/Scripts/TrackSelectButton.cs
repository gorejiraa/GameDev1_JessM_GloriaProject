using UnityEngine;

public class TrackSelectButton : MonoBehaviour
{
    [SerializeField]
    private Track _track;
    [SerializeField] private bool _tutorial;

    public void StartTrack() 
    {
        GameManager.Instance.StartTrack(_track);
    }
}
