using UnityEngine;

public class Conductor : MonoBehaviour
{
    public AudioSource music;
    public float bpm = 120f;
    public double offset = 0.0;          // 초
    public double startDsp;
    double SecPerBeat => 60.0 / bpm;

    public System.Action<int> OnBeat;

    int lastBeat = -1;

    void Start()
    {
        startDsp = AudioSettings.dspTime + 0.10;
        music.PlayScheduled(startDsp); // 안정적 시작
    }

    void Update()
    {
        double songPos = AudioSettings.dspTime - startDsp - offset;
        if (songPos < 0)
            return;

        int beat = (int)System.Math.Floor(songPos / SecPerBeat);
        if (beat != lastBeat)
        {
            lastBeat = beat;
            OnBeat?.Invoke(beat);
        }
    }
}
