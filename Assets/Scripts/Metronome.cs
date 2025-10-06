using System;
using UnityEngine;

/// <summary>
/// 정확한 DSP 스케줄(PlayScheduled) 기반 메트로놈.
/// - 소스 풀(Accent/Normal)로 틱 누락 방지
/// - 절대 시작 시각(StartBaseDsp)과 subdivision 간격(SpSub) 공개
/// - OnTick(tickIndex, bar, pos) 이벤트 제공 (beatsPerBar * subdivision = ticksPerBar)
/// - 실시간 BPM/분할 변경 반영
/// </summary>
public class Metronome : MonoBehaviour
{
    [Header("Clock")]
    [Tooltip("분당 박자(BPM). 실시간 변경 가능")]
    public double bpm = 120.0;
    [Tooltip("마디당 박자 수(예: 8로 두면 8틱/마디)")]
    public int beatsPerBar = 8;
    [Tooltip("세부 분할: 1=4분, 2=8분, 4=16분 ...")]
    public int subdivision = 1;

    [Header("Sound (Click Synthesis)")]
    public float accentHz = 1200f;     // 강박(마디 첫 틱) 톤
    public float normalHz = 800f;      // 나머지 틱 톤
    [Range(1f, 60f)] public float clickMs = 20f;   // 클릭 길이(ms)
    [Range(0f, 1f)] public float clickGain = 0.5f; // 클릭 볼륨(0~1)

    [Header("Scheduling")]
    [Tooltip("시작을 약간 미래로 예약(안정)")]
    public double startDelaySec = 0.10;
    [Tooltip("오디오 스케줄 룩어헤드(초)")]
    public double lookaheadSec = 0.20;
    [Tooltip("PlayScheduled 누락 방지를 위한 소스 풀 크기")]
    public int poolSize = 4;
    public bool autoStart = true;

    [Header("Optional Sync")]
    [Tooltip("있으면 같은 startDsp를 공유")]
    public Conductor conductor;

    /// <summary>틱 이벤트: (전역틱 index, bar, pos)</summary>
    public event Action<int, int, int> OnTick;

    // ====== Public readonly properties ======
    /// <summary>0번째 틱의 절대 DSP 시간(초)</summary>
    public double StartBaseDsp { get; private set; }
    /// <summary>Subdivision 간격(초) = 60/bpm/subdivision</summary>
    public double SpSub => 60.0 / Math.Max(1e-6, bpm) / Math.Max(1, subdivision);

    // ====== internals ======
    AudioClip _clipAccent, _clipNormal;
    AudioSource[] _accentPool, _normalPool;
    int _accentPtr = 0, _normalPtr = 0;

    int _sampleRate;
    double _nextTickTime;
    int _tickIndex;
    bool _running;

    // for runtime change detection
    double _lastBpm;
    int _lastSubdivision;

    void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;

        _clipAccent = MakeClickClip(accentHz, clickMs, clickGain);
        _clipNormal = MakeClickClip(normalHz, clickMs, clickGain);

        // 소스 풀 생성(클립 사전 고정)
        _accentPool = new AudioSource[poolSize];
        _normalPool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            _accentPool[i] = gameObject.AddComponent<AudioSource>();
            _accentPool[i].playOnAwake = false;
            _accentPool[i].clip = _clipAccent;

            _normalPool[i] = gameObject.AddComponent<AudioSource>();
            _normalPool[i].playOnAwake = false;
            _normalPool[i].clip = _clipNormal;
        }

        _lastBpm = bpm;
        _lastSubdivision = subdivision;
    }

    void Start()
    {
        // 기준 시작 시각 계산(Conductor가 있으면 동일 startDsp 사용)
        double baseDsp = (conductor != null && conductor.startDsp > 0.0)
            ? conductor.startDsp
            : AudioSettings.dspTime;

        StartBaseDsp = baseDsp + startDelaySec;
        _nextTickTime = StartBaseDsp;
        _tickIndex = 0;

        if (autoStart) _running = true;
    }

    void Update()
    {
        // 런타임 변경 반영(다음 예약분부터 새로운 간격으로 진행)
        if (!Mathf.Approximately((float)bpm, (float)_lastBpm) ||
            _lastSubdivision != subdivision)
        {
            // 다음 틱부터 새 SpSub로 진행 (절대 시각 유지)
            _lastBpm = bpm;
            _lastSubdivision = subdivision;
        }

        if (!_running) return;

        double now = AudioSettings.dspTime;
        double end = now + lookaheadSec;
        double spSub = SpSub;

        // 룩어헤드 내 틱 모두 예약
        while (_nextTickTime <= end)
        {
            bool isAccent = IsAccentTick(_tickIndex);
            var src = isAccent ? _accentPool[_accentPtr] : _normalPool[_normalPtr];
            src.PlayScheduled(_nextTickTime); // 클립은 풀에서 이미 고정

            if (isAccent)
                _accentPtr = (_accentPtr + 1) % poolSize;
            else
                _normalPtr = (_normalPtr + 1) % poolSize;

            // 이벤트(예약 시점에 알려줌) — 절대시간이 필요하면 StartBaseDsp + _tickIndex*SpSub 계산
            int bar, pos;
            TickToBarPos(_tickIndex, out bar, out pos);
            OnTick?.Invoke(_tickIndex, bar, pos);

            _tickIndex++;
            _nextTickTime += spSub;
        }

        // 테스트 토글(선택)
        if (Input.GetKeyDown(KeyCode.Space))
            _running = !_running;
    }

    // ====== helpers ======
    bool IsAccentTick(int t)
    {
        int ticksPerBar = Math.Max(1, beatsPerBar) * Math.Max(1, subdivision);
        int pos = t % ticksPerBar;
        return pos == 0;
    }

    void TickToBarPos(int t, out int bar, out int pos)
    {
        int ticksPerBar = Math.Max(1, beatsPerBar) * Math.Max(1, subdivision);
        bar = t / ticksPerBar;
        pos = t % ticksPerBar;
    }

    AudioClip MakeClickClip(float hz, float ms, float gain)
    {
        int len = Mathf.Max(1, Mathf.CeilToInt(_sampleRate * ms * 0.001f));
        var data = new float[len];

        // 간단한 사인버스트 + 지수 감쇠
        float w = 2f * Mathf.PI * hz / _sampleRate;
        float env = 1f;
        float decay = Mathf.Exp(-6.0f / len); // 대략 6dB/len 샘플 감쇠
        float amp = Mathf.Clamp01(gain);

        for (int i = 0; i < len; i++)
        {
            data[i] = Mathf.Sin(w * i) * env * amp;
            env *= decay;
        }

        var clip = AudioClip.Create($"click_{hz}Hz", len, 1, _sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ====== public controls ======
    public void SetBpm(double newBpm) => bpm = Math.Max(1e-6, newBpm);
    public void SetSubdivision(int sub) => subdivision = Math.Max(1, sub);

    /// <summary>시작(필요시 특정 DSP 시간 기준)</summary>
    public void StartClock(double? atDsp = null)
    {
        if (_running) return;
        double baseDsp = atDsp ?? AudioSettings.dspTime;
        StartBaseDsp = baseDsp + startDelaySec;
        _nextTickTime = StartBaseDsp;
        _tickIndex = 0;
        _running = true;
    }

    public void StopClock() => _running = false;
}
