//using UnityEngine;
//using System.Collections;

//public class UI_GameScene : MonoBehaviour
//{
//    public Transform center;         // �߾� ������Ʈ (�� GameObject ����)
//    public Transform target;         // ��ǥ ������Ʈ
//    public GameObject starPrefab;    // ��(��, ��ƼŬ, ��������Ʈ ��) ������

//    public float moveTime = 0.5f;    // �̵� �ð�

//    private GameObject star;







//    void Start()
//    {
//        // ������ ���� (������ �ϳ��� �����)
//        star = Instantiate(starPrefab, center.position, Quaternion.identity);
//        star.SetActive(false);

//        // �׽�Ʈ ����
//        StartCoroutine(MoveStar());
//    }

//    IEnumerator MoveStar()
//    {
//        star.SetActive(true);

//        Vector3 startPos = center.position;
//        Vector3 endPos = target.position;

//        float elapsed = 0f;

//        while (elapsed < moveTime)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / moveTime;
//            star.transform.position = Vector3.Lerp(startPos, endPos, t);
//            yield return null;
//        }

//        star.SetActive(false);
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_GameScene : MonoBehaviour
{
    [Header("Cells (3x3, index: 0..8  [0 1 2 / 3 4 5 / 6 7 8])")]
    public Image[] cellImages = new Image[9];     // �� ĭ�� Image (������ ���)
    public Transform[] cellAnchors = new Transform[9]; // �� ĭ�� ��ġ(�� �̵���)

    [Header("Star Prefab (Swipe ����)")]
    public GameObject starPrefab;                 // �������������� ���
    public float swipePreviewTime = 0.45f;        // �� �̵� �ð�

    [Header("Tap Blink")]
    public Color tapBlinkColor = new Color(1f, 1f, 1f, 0.0f); // ���� ���� or ������
    public float tapBlinkOnTime = 0.08f;
    public float tapBlinkOffTime = 0.08f;
    public int tapBlinkRepeat = 1;

    private GameObject star;
    private Color[] originalColors;

    void Awake()
    {
        // ���� �÷� ����
        originalColors = new Color[cellImages.Length];
        for (int i = 0; i < cellImages.Length; i++)
        {
            if (cellImages[i] != null)
                originalColors[i] = cellImages[i].color;
        }
    }

    void Start()
    {
        // �������� ������ ���� �� ������ 1�� ����
        if (starPrefab != null && cellAnchors != null && cellAnchors.Length >= 5 && cellAnchors[4] != null)
        {
            star = Instantiate(starPrefab, cellAnchors[4].position, Quaternion.identity);
            star.SetActive(false);
        }
    }

    // PatternRunner�� TEACH �ܰ迡�� ȣ��
    public void ShowStepVisual(Step s)
    {
        StopAllCoroutines();

        if (s.type == InputType.Tap)
        {
            int cellIdx = Dir9ToIndex(s.dir);
            if (cellIdx < 0 || cellIdx >= cellImages.Length || cellImages[cellIdx] == null) return;

            //  Tap: �ش� �� UI ��ü ������
            StartCoroutine(BlinkCell(cellIdx, tapBlinkRepeat, tapBlinkOnTime, tapBlinkOffTime, tapBlinkColor));
        }
        else if (s.type == InputType.Swipe)
        {
            //  Swipe: �� ������ �̵�(���͡�Ÿ��)
            int startIdx = 4; // �⺻: ���Ϳ��� ����
            int targetIdx = Dir9ToIndex(s.dir);
            if (star == null || cellAnchors[startIdx] == null || cellAnchors[targetIdx] == null) return;

            StartCoroutine(MoveStar(cellAnchors[startIdx].position, cellAnchors[targetIdx].position, swipePreviewTime));
        }
    }

    // === �ܺ�(���� ���� �� ��)���� ��� �����̱� ���� ���� ===
    public void FlashTapCell(int cellIdx, int repeat = 1)
    {
        if (cellIdx < 0 || cellIdx >= cellImages.Length || cellImages[cellIdx] == null) return;
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

    // Dir9 �� 3x3 �ε��� ���� (row0=���)
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
