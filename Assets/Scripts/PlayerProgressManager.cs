using System.Collections.Generic;
using UnityEngine;

public class PlayerProgressManager : MonoBehaviour
{
    private Dictionary<string, bool> progress = new Dictionary<string, bool>();

    private void Awake()
    {
        progress["true"] = true;
    }

    public bool checkProgress(string key) {  return progress.ContainsKey(key) && progress[key]; }
    public void makeProgress(string key) { progress[key] = true; }
}
