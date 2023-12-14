using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharedReferences : MonoBehaviour
{


    public static SharedReferences Instance;















    private void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
