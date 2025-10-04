using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputJudge : MonoBehaviour
{
    public Conductor conductor;

    [Header("이번 라운드(PLAY) 요구 스텝들 (0~7비트)")]
    public List<Step> steps;

    [Header("판정 윈도우 (beat 단위)")]
    public float beatWindow = 0.12f;   // ±0.12 beat

    // 라운드 상태
    bool playActive = false;
    int playRoundStartBeat = 0;

    // 미입력 Miss 체크
    bool[] expect = new bool[8];   // 해당 비트에 Center 요구?
    bool[] got = new bool[8];   // 해당 비트 성공 처리됨?

    bool justStartedPlay = false;   // 첫 틱 prev 검사 방지
    bool endScheduled = false;   // 종료 예약 중인지

    public Action OnPlayEnded;      // 라운드 종료 콜백 (PatternRunner가 구독)

    void OnEnable() { conductor.OnBeat += HandleBeat; }
    void OnDisable() { conductor.OnBeat -= HandleBeat; }

    // TEACH → PLAY 전환 시 PatternRunner가 호출
    public void StartPlay(int currentGlobalBeat, List<Step> playSteps)
    {
        steps = playSteps;

        Array.Clear(expect, 0, expect.Length);
        Array.Clear(got, 0, got.Length);

        // 지금은 Center Tap만 요구(프로토타입). 이후 Tap/Swipe/방향 확장 가능.
        foreach (var s in steps)
            if (s.type == InputType.Tap && s.dir == Dir9.Center && s.beat >= 0 && s.beat < 8)
                expect[s.beat] = true;

        playRoundStartBeat = currentGlobalBeat + 1; // 다음 비트부터 PLAY 시작
        playActive = true;
        justStartedPlay = true; // 첫 틱 prev 미스 체크 금지
        endScheduled = false;
    }

    void HandleBeat(int globalBeat)
    {
        if (!playActive) return;

        // 플레이 라운드의 현재 로컬 비트(0..7) 계산
        int localBeat = (globalBeat - playRoundStartBeat) % 8;
        if (localBeat < 0) return;

        // 첫 틱은 prev 미스 체크 skip
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

        //  라운드 종료를 '다음 비트 경계'에서 처리 (play 시작 + 8비트 = 글로벌 16, 24, ...)
        if (globalBeat >= playRoundStartBeat + 8) // (=로컬 0에 도달)
        {
            playActive = false;
            OnPlayEnded?.Invoke();
        }
    }


    void Update()
    {
        if (!playActive) return;

        // 스페이스바 = Center 입력
        if (Input.GetKeyDown(KeyCode.Space))
            JudgeCenter();
    }

    void JudgeCenter()
    {
        double spb = 60.0 / conductor.bpm;
        double t = AudioSettings.dspTime - conductor.startDsp - conductor.offset;
        double beatF = t / spb;

        // ▶ 플레이 라운드 시작(global 8)을 0으로 맞춘 '로컬 연속 비트'
        double beatFLocal = beatF - playRoundStartBeat; // 예: -0.02, 0.01, 0.98 ...
        int idx = Mathf.RoundToInt((float)beatFLocal); // 0~7 타겟 비트

        // PLAY 구간(8~15)에 해당하는 0~7만 인정
        if (idx < 0 || idx > 7) return;

        // 이 비트가 요구되는 비트인지
        if (!expect[idx])
        {
            Debug.Log($"[Judge] Wrong Input (Beat {idx})");
            return;
        }

        // ±beatWindow 대칭 창으로 판정
        float diffBeat = Mathf.Abs((float)(beatFLocal - idx));
        if (diffBeat <= beatWindow)
        {
            got[idx] = true;
            expect[idx] = false;
            Debug.Log($"Perfect (Beat {idx})");
        }
        else
        {
            // 여기 선택
            // 1) Late/Early도 '성공'으로 인정하고 Miss 방지하려면:
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
        OnPlayEnded?.Invoke();   //  PatternRunner에 라운드 종료 알림
    }
}
