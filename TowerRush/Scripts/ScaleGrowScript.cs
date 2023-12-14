using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleGrowScript : MonoBehaviour
{
    float speed = 2f;
    Vector3 temp;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        temp = transform.localScale;
        temp.x += (Time.deltaTime * 0.01f);
        temp.y += (Time.deltaTime * 0.01f);
        transform.localScale = temp;
    }
}
