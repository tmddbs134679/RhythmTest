using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public Conductor conductor;

    [Header("�̹� ����(PLAY) �䱸 ���ܵ� (0~7��Ʈ)")]
    public List<Step> steps;

    [Header("���� ������ (beat ����)")]
    public float beatWindow = 0.12f;   // ��0.12 beat

    // ���� ����
    bool playActive = false;
    int playRoundStartBeat = 0;

    // ���Է� Miss üũ
    bool[] expect = new bool[8];   // �ش� ��Ʈ�� Center �䱸?
    bool[] got = new bool[8];   // �ش� ��Ʈ ���� ó����?

    bool justStartedPlay = false;   // ù ƽ prev �˻� ����
    bool endScheduled = false;   // ���� ���� ������

    public Action OnPlayEnded;      // ���� ���� �ݹ� (PatternRunner�� ����)

    void OnEnable() { conductor.OnBeat += HandleBeat; }
    void OnDisable() { conductor.OnBeat -= HandleBeat; }

    // TEACH �� PLAY ��ȯ �� PatternRunner�� ȣ��
    public void StartPlay(int currentGlobalBeat, List<Step> playSteps)
    {
        steps = playSteps;

        Array.Clear(expect, 0, expect.Length);
        Array.Clear(got, 0, got.Length);

        // ������ Center Tap�� �䱸(������Ÿ��). ���� Tap/Swipe/���� Ȯ�� ����.
        foreach (var s in steps)
            if (s.type == InputType.Tap && s.dir == Dir9.Center && s.beat >= 0 && s.beat < 8)
                expect[s.beat] = true;

        playRoundStartBeat = currentGlobalBeat + 1; // ���� ��Ʈ���� PLAY ����
        playActive = true;
        justStartedPlay = true; // ù ƽ prev �̽� üũ ����
        endScheduled = false;
    }

    void HandleBeat(int globalBeat)
    {
        if (!playActive) return;

        // �÷��� ������ ���� ���� ��Ʈ(0..7) ���
        int localBeat = (globalBeat - playRoundStartBeat) % 8;
        if (localBeat < 0) return;

        // ù ƽ�� prev �̽� üũ skip
        if (!justStartedPlay)
        {
            int prev = (localBeat + 7) % 8;
            if (expect[prev] && !got[prev])
            {
              //  Debug.Log($"Miss (No Input) on Beat {prev}");
                expect[prev] = false;
            }
        }
        else justStartedPlay = false;

        //  ���� ���Ḧ '���� ��Ʈ ���'���� ó�� (play ���� + 8��Ʈ = �۷ι� 16, 24, ...)
        if (globalBeat >= playRoundStartBeat + 8) // (=���� 0�� ����)
        {
            playActive = false;
            OnPlayEnded?.Invoke();
        }
    }


    void Update()
    {
        if (!playActive) return;

        // �����̽��� = Center �Է�
        if (Input.GetKeyDown(KeyCode.Space))
            JudgeCenter();
    }

    void JudgeCenter()
    {
        double spb = 60.0 / conductor.bpm;
        double t = AudioSettings.dspTime - conductor.startDsp - conductor.offset;
        double beatF = t / spb;

        // �� �÷��� ���� ����(global 8)�� 0���� ���� '���� ���� ��Ʈ'
        double beatFLocal = beatF - playRoundStartBeat; // ��: -0.02, 0.01, 0.98 ...
        int idx = Mathf.RoundToInt((float)beatFLocal); // 0~7 Ÿ�� ��Ʈ

        // PLAY ����(8~15)�� �ش��ϴ� 0~7�� ����
        if (idx < 0 || idx > 7) return;

        // �� ��Ʈ�� �䱸�Ǵ� ��Ʈ����
        if (!expect[idx])
        {
            Debug.Log($"[Judge] Wrong Input (Beat {idx})");
            return;
        }

        // ��beatWindow ��Ī â���� ����
        float diffBeat = Mathf.Abs((float)(beatFLocal - idx));
        if (diffBeat <= beatWindow)
        {
            got[idx] = true;
            expect[idx] = false;
            Debug.Log($"Perfect (Beat {idx})");
        }
        else
        {
            // ���� ����
            // 1) Late/Early�� '����'���� �����ϰ� Miss �����Ϸ���:
            got[idx] = true;
            expect[idx] = false;

            Debug.Log($"Late/Early (Beat {idx})");
        }
    }


    IEnumerator EndRoundAfterWindow()
    {
        double secPerBeat = 60.0 / conductor.bpm;
        yield return new WaitForSeconds((float)(beatWindow * secPerBeat) + 0.01f);

        int lastBeat = 7;
        if (expect[lastBeat] && !got[lastBeat])
        {
            Debug.Log($"Miss (No Input) on Beat {lastBeat}");
            expect[lastBeat] = false;
        }

        playActive = false;
        endScheduled = false;
        OnPlayEnded?.Invoke();   //  PatternRunner�� ���� ���� �˸�
    }
}
