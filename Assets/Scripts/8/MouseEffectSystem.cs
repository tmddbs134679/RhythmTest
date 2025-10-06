using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseEffectSystem : MonoBehaviour
{

    public GameObject _prefab;
    public float _defaultTime = 0.05f;
    float _spawnsTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0) && _spawnsTime >= _defaultTime)
        {
            StartCreate();
            _spawnsTime = 0;
        }
        _spawnsTime += Time.deltaTime;
    }

    void StartCreate()
    {
        Vector3 pos =  Camera.main.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;
        Instantiate(_prefab, pos, Quaternion.identity);
    }
}
