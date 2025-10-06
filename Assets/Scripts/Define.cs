using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[System.Serializable]
public class NoteData
{
    public float time;       // �� ���� (�� ����)
    public NoteType type;
    public float holdDur;    // Hold ����, �� ���� (��Ÿ Ÿ���� 0)
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
