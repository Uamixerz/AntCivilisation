using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private float count = 1f;
    public float getCountItem()
    {
        return count;
    }
}
