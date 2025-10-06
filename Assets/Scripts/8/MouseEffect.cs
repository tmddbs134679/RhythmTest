using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEffect : MonoBehaviour
{
    SpriteRenderer _sr;
    public Color[] _colors;

    Vector2 _dir;
    public float _moveSpeed = 0.01f;
    public float _maxSize = 0.3f;
    public float _minSize = 0.1f;
    public float _sizeSpeed = 1;
    public float _colorSpeed = 1f;
    // Start is called before the first frame update
    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.color = _colors[Random.Range(0, _colors.Length)];


        _dir = new Vector2(Random.Range(-1.0f,1.0f),Random.Range(-1.0f,1.0f));  
        float size = Random.Range(_minSize, _maxSize);
        transform.localScale = new Vector2(size, size);
    }

    // Update is called once per frame
    void Update()
    {
       // transform.Translate(_dir * _moveSpeed);
        transform.localScale = Vector2.Lerp(transform.localScale, Vector2.zero, Time.deltaTime * _sizeSpeed);

        Color color = _sr.color;
        color.a = Mathf.Lerp(_sr.color.a, 0, Time.deltaTime * _colorSpeed);
        _sr.color = color;

        if (_sr.color.a <= 0.01f)
            Destroy(gameObject);
    }
}
