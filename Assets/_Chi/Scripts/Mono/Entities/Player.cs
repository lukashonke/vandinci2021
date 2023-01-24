using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Statistics;
using UnityEngine;

namespace _Chi.Scripts.Mono.Entities
{
    public class Player : Entity
    {
        public PlayerStats stats;

        [NonSerialized] public PlayerBody body;
        [NonSerialized] public List<ModuleSlot> slots;

        public float nearestEnemiesDetectorRange = 15f;
        
        private Collider2D[] buffer = new Collider2D[4096];

        [NonSerialized] public List<Entity> nearestEnemies;

        public override void Awake()
        {
            base.Awake();

            body = GetComponentInChildren<PlayerBody>();
            slots = new List<ModuleSlot>();
            
            InitializeBody();
            
            nearestEnemies = new List<Entity>();
            buffer = new Collider2D[4096];
        }

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();

            StartCoroutine(UpdateNearbyEnemies());
        }

        // Update is called once per frame
        public override void DoUpdate()
        {
            base.DoUpdate();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
        
            if (CanMove())
            {
                SetRotation(EntityExtensions.RotateTowards(GetPosition(), rotationTarget.Value, rb.transform.rotation, stats.rotationSpeed.GetValue()));
            }
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);

            Debug.Log("trigger");

            var entity = other.gameObject.GetEntity();

            if (entity is Npc monster && this.AreEnemies(monster))
            {
                ReceiveDamage(monster.CalculateMonsterContactDamage(this), monster);
            }
        }

        private IEnumerator UpdateNearbyEnemies()
        {
            var waiter = new WaitForSeconds(0.2f);
            while (isAlive)
            {
                var count = EntityExtensions.GetNearest(this, GetPosition(), nearestEnemiesDetectorRange, TargetType.EnemyOnly, buffer);
                
                nearestEnemies.Clear();
                
                for (int i = 0; i < count; i++)
                {
                    var col = buffer[i];

                    var entity = col.gameObject.GetEntity();
                    if (entity is Npc npc && npc.AreEnemies(this))
                    {
                        nearestEnemies.Add(entity);
                    }
                }

                yield return waiter;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public void SetBody(GameObject bodyPrefab)
        {
            if (body != null)
            {
                Destroy(body.gameObject);
            }
            var newBody = Instantiate(bodyPrefab, GetPosition(), Quaternion.identity, this.transform);
            body = newBody.GetComponent<PlayerBody>();
            
            InitializeBody();
        }

        public void InitializeBody()
        {
            slots = body.GetComponentsInChildren<ModuleSlot>().ToList();
        }

        public ModuleSlot GetSlotById(int slotId)
        {
            return slots.FirstOrDefault(s => s.slotId == slotId);
        }

        public void SetModuleInSlot(ModuleSlot slot, GameObject modulePrefab, int level)
        {
            if (slot.currentModule != null)
            {
                slot.SetModuleInSlot(null, destroyCurrent: true);
            }

            var moduleGo = Instantiate(modulePrefab, slot.GetModulePosition(), Quaternion.identity, slot.transform);
            var module = moduleGo.GetComponent<Module>();

            module.level = level;
            
            slot.SetModuleInSlot(module);
        }
    }
}
