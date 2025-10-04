using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    public Text comboText;
    public Text scoreText;
    public Text gradeText;

    public void OnHit(int grade, int combo, int score)
    {
        if (gradeText) gradeText.text = grade == 2 ? "PERFECT!" : "GOOD";
        if (comboText) comboText.text = $"COMBO {combo}";
        if (scoreText) scoreText.text = $"SCORE {score}";
    }
    public void OnMiss()
    {
        if (gradeText) gradeText.text = "MISS";
        if (comboText) comboText.text = $"COMBO 0";
    }
}
