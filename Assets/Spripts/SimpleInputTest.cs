using UnityEngine;

public class SimpleInputTest : MonoBehaviour
{
    private Vector2 startPos;
    private float minSwipeDist = 80f; // 스와이프 최소 거리(px)

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        MouseInput();
#else
        TouchInput();
#endif
    }

    void MouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            DetectSwipe((Vector2)Input.mousePosition - startPos);
        }
    }

    void TouchInput()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
        {
            startPos = t.position;
        }

        if (t.phase == TouchPhase.Ended)
        {
            DetectSwipe(t.position - startPos);
        }
    }

    void DetectSwipe(Vector2 delta)
    {
        if (delta.magnitude < minSwipeDist)
        {
            Debug.Log("?? Tap Detected");
            return;
        }

        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 8방향 분할 (45도씩)
        if (angle >= 337.5f || angle < 22.5f) Debug.Log("?? Right");
        else if (angle >= 22.5f && angle < 67.5f) Debug.Log("↗? Up-Right");
        else if (angle >= 67.5f && angle < 112.5f) Debug.Log("?? Up");
        else if (angle >= 112.5f && angle < 157.5f) Debug.Log("↖? Up-Left");
        else if (angle >= 157.5f && angle < 202.5f) Debug.Log("?? Left");
        else if (angle >= 202.5f && angle < 247.5f) Debug.Log("↙? Down-Left");
        else if (angle >= 247.5f && angle < 292.5f) Debug.Log("?? Down");
        else if (angle >= 292.5f && angle < 337.5f) Debug.Log("↘? Down-Right");
    }
}
