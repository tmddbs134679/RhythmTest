
using System.Collections.Generic;
using UnityEngine;

public class PatternRunner : MonoBehaviour
{
    public Conductor conductor;
    public InputJudge judge;                 // Inspector에서 연결
    [Header("임시 패턴 데이터")]
    public List<Step> steps = new List<Step>();

    [Header("Preview UI")]
    public UI_GameScene ui;                  // ★ 미리보기 표시 담당

    private enum Phase { Teach, Play }
    private Phase phase = Phase.Teach;


    void OnEnable()
    {
        conductor.OnBeat += HandleBeat;
        judge.OnPlayEnded += OnPlayEnded;    //  Judge로부터 종료 알림 받음
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
            // 해당 비트에 해당하는 Step들을 시각화
            for (int i = 0; i < steps.Count; i++)
            {
                var s = steps[i];
                if (s.beat == localBeat)
                {
                    ShowStep(s);
                    if (ui != null) ui.ShowStepVisual(s);  // ★ 추가: UI 표시
                }
            }

            // 0~7 끝나면 PLAY로 전환
            if (localBeat == 7)
            {
                phase = Phase.Play;
                judge.StartPlay(globalBeat, steps);  //  Judge에 PLAY 시작
            }
        }
        // PLAY 동안엔 Runner는 표시는 안 함 (입력 단계)
    }

    void OnPlayEnded()
    {
        phase = Phase.Teach; // 다음 라운드 시작 가능
    }

    void ShowStep(Step s)
    {
        Debug.Log($"[TEACH] Beat {s.beat + 1}: {s.type} {s.dir}");
    }
}
