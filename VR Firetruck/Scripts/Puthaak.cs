using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puthaak : MonoBehaviour
{
    [SerializeField] private GameObject deksel;
    [SerializeField] private Transform puthaakParent;
    [SerializeField] private Transform dekselParent;
    [Space]
    [SerializeField] private Vector3 puthaakPosition;
    [SerializeField] private Vector3 puthaakRotation;
    [SerializeField] private Vector3 dekselPosition;
    [SerializeField] private Vector3 dekselRotation;

    

    public void SetNewParent(string name)
    {
        if (name.Equals("Puthaak"))
        {
            deksel.transform.SetParent(puthaakParent);
            deksel.transform.localPosition = puthaakPosition;
            deksel.transform.localRotation = Quaternion.Euler(puthaakRotation);
        }
        else if (name.Equals("Deksel"))
        {
            deksel.transform.SetParent(dekselParent);
            deksel.transform.localPosition = dekselPosition;
            deksel.transform.localRotation = Quaternion.Euler(dekselRotation);
        }
    }
}
