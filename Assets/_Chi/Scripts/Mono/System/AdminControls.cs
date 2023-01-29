using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Utilities;
using UnityEngine;

public class AdminControls : MonoBehaviour
{
    public List<GameObject> prefabsToSpawn;
    
    public List<int> prefabsToSpawnCounts;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.U))
        {
            Spawn(0, false);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.I))
        {
            Spawn(1);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.O))
        {
            Spawn(2);        
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.P))
        {
            Spawn(3);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            Spawn(4);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            Spawn(5);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            Spawn(6);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            Spawn(7);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            Spawn(8);
        }
        
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            Spawn(9);
        }
    }

    private void Spawn(int index, bool random = true)
    {
        for (int i = 0; i < prefabsToSpawnCounts[index]; i++)
        {
            var mousePos = Utils.GetMousePosition();
            var pos = Utils.GenerateRandomPositionAround(mousePos, random ? 1f : 0f, random ? 0.1f : 0f);
            
            var prefab = prefabsToSpawn[index].GetComponent<Npc>();
            var instance = prefab.SpawnPooledNpc(pos, Quaternion.Euler(0, 0, Random.Range(0, 360)));
        }
    }
}
