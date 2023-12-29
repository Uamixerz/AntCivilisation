using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConroller : MonoBehaviour
{
    public GameObject antPrefab;
    public Transform posAnt;
    public Vector3 positionAnt;
    public void CreateAnt()
    {
        antPrefab.transform.position = positionAnt;
        Instantiate(antPrefab,posAnt);
    }
}
