using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class NoteData
{
    public float time;       // 초 단위 (곡 기준)
    public NoteType type;
    public float holdDur;    // Hold 전용, 초 단위 (기타 타입은 0)
    public NoteData(float t, NoteType ty, float dur = 0f) { time = t; type = ty; holdDur = dur; }
}
public class Define 
{
    public enum NoteType 
    {
        Left,
        Right,
        Push, 
        Flick,
        Hold 
    }
}
