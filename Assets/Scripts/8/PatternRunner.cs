
using System.Collections.Generic;
using UnityEngine;

public class PatternRunner : MonoBehaviour
{
    public Conductor conductor;
    public InputJudge judge;                 // Inspector���� ����
    [Header("�ӽ� ���� ������")]
    public List<Step> steps = new List<Step>();

    [Header("Preview UI")]
    public UI_GameScene ui;                  // �� �̸����� ǥ�� ���

    private enum Phase { Teach, Play }
    private Phase phase = Phase.Teach;


    void OnEnable()
    {
        conductor.OnBeat += HandleBeat;
        judge.OnPlayEnded += OnPlayEnded;    //  Judge�κ��� ���� �˸� ����
    }

    void OnDisable()
    {
        conductor.OnBeat -= HandleBeat;
        judge.OnPlayEnded -= OnPlayEnded;
    }

    void HandleBeat(int globalBeat)
    {
        int localBeat = globalBeat % 8;

        if (phase == Phase.Teach)
        {
            // �ش� ��Ʈ�� �ش��ϴ� Step���� �ð�ȭ
            for (int i = 0; i < steps.Count; i++)
            {
                var s = steps[i];
                if (s.beat == localBeat)
                {
                    ShowStep(s);
                    if (ui != null) ui.ShowStepVisual(s);  // �� �߰�: UI ǥ��
                }
            }

            // 0~7 ������ PLAY�� ��ȯ
            if (localBeat == 7)
            {
                phase = Phase.Play;
                judge.StartPlay(globalBeat, steps);  //  Judge�� PLAY ����
            }
        }
        // PLAY ���ȿ� Runner�� ǥ�ô� �� �� (�Է� �ܰ�)
    }

    void OnPlayEnded()
    {
        phase = Phase.Teach; // ���� ���� ���� ����
    }

    void ShowStep(Step s)
    {
        Debug.Log($"[TEACH] Beat {s.beat + 1}: {s.type} {s.dir}");
    }
}
