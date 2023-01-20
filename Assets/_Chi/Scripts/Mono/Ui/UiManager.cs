using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public TextMeshProUGUI benchmarkStats;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        benchmarkStats.text = $"objects: {Gamesystem.instance.objects.npcEntitiesList.Count}";
    }
}
