using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Ui;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public TextMeshProUGUI benchmarkStats;

    [Required] public VehicleSettingsWindow vehicleSettingsWindow; 
    
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
