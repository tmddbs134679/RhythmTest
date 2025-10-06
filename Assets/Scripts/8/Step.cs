using UnityEngine;

public enum InputType { Tap, Swipe }
public enum Dir9 { Center, Up, Down, Left, Right, UpLeft, UpRight, DownLeft, DownRight }

[System.Serializable]
public struct Step
{
    public InputType type;
    public Dir9 dir;
    public int beat;            // 0~7 (���� �� ��Ʈ ��ġ)
}

[CreateAssetMenu(menuName = "Beatmap/Pattern")]
public class PatternSO : ScriptableObject
{
    public Step[] steps;        // �ִ� 8
}
