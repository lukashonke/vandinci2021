using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Entities;
using UnityEngine;

namespace _Chi.Scripts.Mono.System
{
    public class KillEffectManager : MonoBehaviour
    {
        public Material disolveMaterial;
        public Material defaultSpriteMaterial;

        [NonSerialized] public List<Npc> dissolves;

        private int fadePropertyId;
        private int colorPropertyId;
        private int posPropertyId;

        public void Awake()
        {
            dissolves = new();
            
            fadePropertyId = Shader.PropertyToID("_FullAlphaDissolveFade");
            colorPropertyId = Shader.PropertyToID("_Color");
            posPropertyId = Shader.PropertyToID("_SourceAlphaDissolvePosition");
        }

        public void StartDissolve(Npc npc)
        {
            npc.deathDirection = (npc.GetPosition() - Gamesystem.instance.objects.currentPlayer.GetPosition()).normalized;
            if (npc.hasRenderer)
            {
                npc.renderer.material = disolveMaterial;
            }
            dissolves.Add(npc);
        }

        public void Update()
        {
            bool anyFinished = false;
            
            for (var index = 0; index < dissolves.Count; index++)
            {
                var npc = dissolves[index];
                npc.currentDissolveProcess -= Time.deltaTime * npc.dissolveSpeed;
                if (npc.currentDissolveProcess < 0)
                {
                    npc.OnFinishedDissolve();
                    anyFinished = true;
                    continue;
                }
                
                npc.Move(npc.deathDirection * (Time.deltaTime * 500));
                if (npc.hasRenderer)
                {
                    npc.renderer.material.SetFloat(fadePropertyId, npc.currentDissolveProcess);
                    npc.renderer.material.SetColor(colorPropertyId, Color.Lerp(Color.black, Color.white, npc.currentDissolveProcess));
                }
                //npc.renderer.material.SetVector(colorPropertyId, npc.rotationTarget ?? Vector3.zero);
            }

            if (anyFinished)
            {
                dissolves.RemoveAll(f => f.currentDissolveProcess < 0);
            }
        }
    }
}