using UnityEngine;

[System.Serializable]
public class WordData
{
    public string text;
    public int points;
    public GameObject wordObject;
    [HideInInspector] public Vector3 startPosition;
}