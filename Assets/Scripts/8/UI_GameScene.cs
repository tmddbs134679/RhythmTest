using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_GameScene : MonoBehaviour
{
    [Header("Cells (3x3, index: 0..8  [0 1 2 / 3 4 5 / 6 7 8])")]
    public Image[] cellImages = new Image[9];     // 각 칸의 Image (깜빡임 대상)
    public Transform[] cellAnchors = new Transform[9]; // 각 칸의 위치(별 이동용)

    [Header("Star Prefab (Swipe 전용)")]
    public GameObject starPrefab;                 // 스와이프에서만 사용
    public float swipePreviewTime = 0.45f;        // 별 이동 시간

    [Header("Tap Blink")]
    public Color tapBlinkColor = new Color(0f, 1f, 0f, 1.0f); // 투명도 깜빡 or 강조색
    public float tapBlinkOnTime = 0.08f;
    public float tapBlinkOffTime = 0.08f;
    public int tapBlinkRepeat = 1;

    private GameObject star;
    private Color[] originalColors;

    void Awake()
    {
        // 원본 컬러 저장
        originalColors = new Color[cellImages.Length];
        for (int i = 0; i < cellImages.Length; i++)
        {
            if (cellImages[i] != null)
                originalColors[i] = cellImages[i].color;
        }
    }

    void Start()
    {
        // 스와이프 프리뷰 전용 별 프리팹 1개 생성
        if (starPrefab != null && cellAnchors != null && cellAnchors.Length >= 5 && cellAnchors[4] != null)
        {
            star = Instantiate(starPrefab, cellAnchors[4].position, Quaternion.identity);
            star.SetActive(false);
        }
    }

    // PatternRunner가 TEACH 단계에서 호출
    public void ShowStepVisual(Step s)
    {
        StopAllCoroutines();

        if (s.type == InputType.Tap)
        {
            int cellIdx = Dir9ToIndex(s.dir);
            if (cellIdx < 0 || cellIdx >= cellImages.Length || cellImages[cellIdx] == null)
                return;

            //  Tap: 해당 셀 UI 자체 깜빡임
            StartCoroutine(BlinkCell(cellIdx, tapBlinkRepeat, tapBlinkOnTime, tapBlinkOffTime, tapBlinkColor));
        }
        else if (s.type == InputType.Swipe)
        {
            //  Swipe: 별 프리팹 이동(센터→타겟)
            int startIdx = 4; // 기본: 센터에서 시작
            int targetIdx = Dir9ToIndex(s.dir);
            if (star == null || cellAnchors[startIdx] == null || cellAnchors[targetIdx] == null) return;

            StartCoroutine(MoveStar(cellAnchors[startIdx].position, cellAnchors[targetIdx].position, swipePreviewTime));
        }
    }

    // === 외부(판정 성공 시 등)에서 즉시 깜빡이기 위해 공개 ===
    public void FlashTapCell(int cellIdx, int repeat = 1)
    {
        if (cellIdx < 0 || cellIdx >= cellImages.Length || cellImages[cellIdx] == null) 
            return;

        StartCoroutine(BlinkCell(cellIdx, repeat, tapBlinkOnTime, tapBlinkOffTime, tapBlinkColor));
    }

    // === Blink ===
    IEnumerator BlinkCell(int cellIdx, int repeat, float onTime, float offTime, Color blinkColor)
    {
        var img = cellImages[cellIdx];
        var orig = originalColors[cellIdx];

        for (int r = 0; r < repeat; r++)
        {
            img.color = blinkColor;  // on
            yield return new WaitForSeconds(onTime);
            img.color = orig;        // off
            yield return new WaitForSeconds(offTime);
        }
    }

    // === Swipe preview star move ===
    IEnumerator MoveStar(Vector3 startPos, Vector3 endPos, float dur)
    {
        star.SetActive(true);
        star.transform.position = startPos;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            star.transform.position = Vector3.Lerp(startPos, endPos, t / dur);
            yield return null;
        }
        star.SetActive(false);
    }

    // Dir9 → 3x3 인덱스 매핑 (row0=상단)
    int Dir9ToIndex(Dir9 d)
    {
        switch (d)
        {
            case Dir9.UpLeft: return 0;
            case Dir9.Up: return 1;
            case Dir9.UpRight: return 2;
            case Dir9.Left: return 3;
            case Dir9.Center: return 4;
            case Dir9.Right: return 5;
            case Dir9.DownLeft: return 6;
            case Dir9.Down: return 7;
            case Dir9.DownRight: return 8;
            default: return 4;
        }
    }
}
