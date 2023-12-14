using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [HideInInspector]public GameObject enemySoldier = null;
    [HideInInspector] public GameObject enemyCastle = null;
    public static AttackManager Instance;
    [HideInInspector]public bool isSoldier = false;

    private void Awake()
    {
        Instance = this;
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (enemySoldier == null)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                isSoldier = true;
                enemySoldier = other.gameObject;
                UnitMovement.instance.stopMovement();
                UnitMovement.instance.startAttack();
            }
        }
        if (enemyCastle == null)
        {
            if (other.collider.gameObject.layer == LayerMask.NameToLayer("EnemyCastle"))
            {
                enemyCastle = other.gameObject;
                UnitMovement.instance.stopMovement();
                UnitMovement.instance.startAttack();
                isSoldier = false;
            }
        }
    }

    
}
