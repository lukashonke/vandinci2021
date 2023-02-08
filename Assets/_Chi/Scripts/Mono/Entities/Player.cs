using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.System;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using DamageNumbersPro;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Chi.Scripts.Mono.Entities
{
    public class Player : Entity
    {
        
        public PlayerStats stats;

        public List<Skill> skills;
        
        public List<Mutator> mutators;

        [NonSerialized] public PlayerBody body;
        [NonSerialized] public List<ModuleSlot> slots;
        
        public float nearestEnemiesDetectorRange = 15f;
        
        private Collider2D[] buffer = new Collider2D[4096];

        [NonSerialized] public List<Entity> targetableEnemies;
        [NonSerialized] public HashSet<Entity> damagingEnemies;
        private List<Entity> damagingEnemiesToRemove;

        [ReadOnly] public int shieldCharges;
        
        private Dictionary<Skill, SkillData> skillDatas;
        
        private float nextRestoreShield = -1;

        [NonSerialized] public float lastSkillUseTime;

        public ImmediateEffect pushEffect;
        public GameObject damageEffect;
        public GameObject shieldEffectVfx;
        public GameObject shieldAfterSkillUseVfx;

        public override void Awake()
        {
            base.Awake();

            body = GetComponentInChildren<PlayerBody>();
            slots = new List<ModuleSlot>();
            
            InitializeBody();
            
            targetableEnemies = new List<Entity>();
            damagingEnemies = new();
            damagingEnemiesToRemove = new ();
            buffer = new Collider2D[4096];
            skillDatas = new Dictionary<Skill, SkillData>();

            foreach (var skill in skills)
            {
                AddSkill(skill);
            }
            
            foreach (var mutator in mutators)
            {
                AddMutator(mutator);
            }
        }

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();

            //StartCoroutine(UpdateNearbyEnemies());
            //StartCoroutine(DamageByNearbyCoroutine());
            StartCoroutine(CleanupJob());
            StartCoroutine(StatsJob());
        }

        private IEnumerator CleanupJob()
        {
            var waiter = new WaitForSeconds(0.1f);
            while (isAlive)
            {
                targetableEnemies.RemoveAll(e => e == null || !e.isAlive);

                yield return waiter;
            }   
        }
        
        private IEnumerator StatsJob()
        {
            var waiter = new WaitForSeconds(0.05f);

            float nextHeal = Time.time + 1f;
            
            while (isAlive)
            {
                if (nextHeal <= Time.time)
                {
                    nextHeal = Time.time + 1f;
                    this.Heal(stats.hpRegenPerSecond.GetValue());
                }

                if (nextRestoreShield < 0 && shieldCharges < stats.shieldChargesCount.GetValue())
                {
                    nextRestoreShield = Time.time + stats.singleShieldRechargeDelay.GetValue();
                }
                else if (nextRestoreShield > 0 && nextRestoreShield <= Time.time)
                {
                    if (shieldCharges < stats.shieldChargesCount.GetValue())
                    {
                        shieldCharges++;
                    }
                    nextRestoreShield = -1;
                }

                if (shieldCharges > 0)
                {
                    shieldEffectVfx.SetActive(true);
                }
                else
                {
                    shieldEffectVfx.SetActive(false);
                }

                if (shieldAfterSkillUseVfx != null)
                {
                    var skillUseReduceDamageDuration = stats.takeDamageFaterSkillUseDuration.GetValue();

                    if (lastSkillUseTime > 0 && skillUseReduceDamageDuration > 0 &&
                        (Time.time - lastSkillUseTime) < skillUseReduceDamageDuration)
                    {
                        shieldAfterSkillUseVfx.SetActive(true);
                    }
                    else
                    {
                        shieldAfterSkillUseVfx.SetActive(false);
                    }
                }
                

                yield return waiter;
            }   
        }

        private IEnumerator DamageByNearbyCoroutine()
        {
            var waiter = new WaitForSeconds(.1f);
            while (isAlive)
            {
                if (damagingEnemies.Any())
                {
                    foreach (Entity entity in damagingEnemies)
                    {
                        if (entity != null && entity is Npc monster && monster.nextDamageTime < Time.time)
                        {
                            var stillColliding = triggerCollider.OverlapPoint(entity.GetPosition()) || triggerCollider.IsTouching(entity.triggerCollider);
                            Debug.Log(stillColliding);
                            if (stillColliding)
                            {
                                ReceiveDamageByContact(monster, false);
                            }
                            else
                            {
                                damagingEnemiesToRemove.Add(monster);
                            }
                        }
                    }

                    damagingEnemies.RemoveWhere(d => d == null);

                    if (damagingEnemiesToRemove.Any())
                    {
                        foreach (var entity in damagingEnemiesToRemove) damagingEnemies.Remove(entity);
                        damagingEnemiesToRemove.Clear();
                    }
                }
                
                yield return waiter;
            }
        }
    
        // Update is called once per frame
        public override void DoUpdate()
        {
            base.DoUpdate();
        }

        public void Update()
        {
            Debug.DrawLine(GetPosition(), GetPosition() + Vector3.down, Color.cyan);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            if (CanMove() && rotationTarget.HasValue)
            {
                SetRotation(EntityExtensions.RotateTowards(GetPosition(), rotationTarget.Value, rb.transform.rotation, stats.rotationSpeed.GetValue()));
            }
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            
            var entity = other.gameObject.GetEntity();

            if (entity is Npc monster && this.AreEnemies(monster))
            {
                var velocity = rb.velocity.magnitude;

                if (velocity >= stats.minVelocityToDamage.GetValue())
                {
                    var damage = stats.velocityToDamageMul.GetValue() * velocity;

                    if (damage > 0 && monster.CanBePushed())
                    {
                        var dmgWithFlags = DamageExtensions.CalculateEffectDamage(damage, monster, this);
                        monster.ReceiveDamage(dmgWithFlags.damage, this, dmgWithFlags.flags);

                        if (pushEffect != null)
                        {
                            pushEffect.Apply(monster, this, null, null, velocity);
                        }
                    }
                }

                if (monster.isAlive)
                {
                    //ReceiveDamageByContact(monster, true);
                }
            }
        }

        public void ReceiveDamageByContact(Npc monster, bool addToDamagingEnemiesList)
        {
            if (Time.time > monster.nextDamageTime/* && monster.distanceToPlayer < Math.Pow(stats.maxDistanceToReceiveContactDamage.GetValue(), 2)*/)
            {
                var damage = monster.CalculateMonsterContactDamage(this);
                if (damage > 0)
                {
                    ReceiveDamage(damage, monster);
                    var effect = Gamesystem.instance.poolSystem.SpawnGo(damageEffect);
                    effect.transform.position = triggerCollider.bounds.ClosestPoint(monster.GetPosition());
                    effect.transform.parent = transform;
                    
                    Gamesystem.instance.Schedule(Time.time + 1f, () => Gamesystem.instance.poolSystem.DespawnGo(damageEffect, effect));
                }
                monster.nextDamageTime = Time.time + monster.stats.contactDamageInterval;

                if (addToDamagingEnemiesList)
                {
                    damagingEnemies.Add(monster);
                }
            }
        }

        public override bool CanReceiveDamage(float damage, Entity damager)
        {
            nextRestoreShield = Time.time + stats.singleShieldRechargeDelay.GetValue();
            
            if (shieldCharges > 0)
            {
                shieldCharges--;
                //TODO add vfx
                return false;
            }

            return true;
        }

        public void AddNearbyEnemy(Npc npc)
        {
            targetableEnemies.Add(npc);
        }

        public void RemoveNearbyEnemy(Npc npc)
        {
            targetableEnemies.Remove(npc);
        }

        public bool IsInNearbyDistance(float dist)
        {
            return dist < stats.nearbyEnemyRangeSqrt.GetValue();
        }

        /*private IEnumerator UpdateNearbyEnemies()
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
        }*/

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnDie(DieCause cause)
        {
            base.OnDie(cause);
            
            Gamesystem.instance.missionManager.OnPlayerDie();
        }

        public void SetBody(GameObject bodyPrefab)
        {
            if (body != null)
            {
                Destroy(body.gameObject);
            }
            var newBody = Instantiate(bodyPrefab, GetPosition(), Quaternion.identity, this.transform);
            body = newBody.GetComponent<PlayerBody>();
            
            body.transform.localRotation = Quaternion.identity;
            
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

        public void SetModuleInSlot(ModuleSlot slot, GameObject modulePrefab, int level, int rotation)
        {
            if (slot.currentModule != null)
            {
                slot.SetModuleInSlot(null, destroyCurrent: true);
            }

            var moduleGo = Instantiate(modulePrefab, slot.GetModulePosition(), Quaternion.identity, slot.transform);
            var module = moduleGo.GetComponent<Module>();

            module.level = level;
            module.SetOriginalRotation(rotation);
            
            slot.SetModuleInSlot(module);
        }
        
        public bool TryActivateSkill(int slot)
        {
            var skill = GetSkill(slot);

            if (skill == null) return false;

            return skill.Trigger(this);
        }

        public Skill GetSkill(int slot)
        {
            if (skills.Count - 1 >= slot)
            {
                return skills[slot];
            }

            return null;
        }

        public override SkillData GetSkillData(Skill skill)
        {
            return skillDatas.TryGetValue(skill, out var data) ? data : null;
        }

        public void AddMutator(Mutator mutator)
        {
            if (mutators.Contains(mutator)) return;
            
            mutators.Add(mutator);
            mutator.ApplyToPlayer(this);
        }

        public void RemoveMutator(Mutator mutator)
        {
            if (mutators.Remove(mutator))
            {
                mutator.RemoveFromPlayer(this);
            }
        }

        public void RemoveMutators()
        {
            foreach (var mutator in mutators.ToArray())
            {
                RemoveMutator(mutator);
            }
        } 

        public void AddSkill(Skill skill)
        {
            if (!skills.Contains(skill))
            {
                skills.Add(skill);
            }

            if (!skillDatas.ContainsKey(skill))
            {
                skillDatas[skill] = skill.CreateDefaultSkillData();
            }
        }

        public void RemoveSkills()
        {
            foreach (var skill in skills.ToArray())
            {
                RemoveSkill(skill);
            }
        }

        public void RemoveSkill(Skill skill)
        {
            skills.Remove(skill);
            skillDatas.Remove(skill);
        }

        public override void OnSkillUse()
        {
            var restoreHealth = stats.skillUseHealthPercent.GetValue();

            if (restoreHealth > 0)
            {
                var toRestore = entityStats.maxHp * restoreHealth;
                this.Heal(toRestore);
            }

            lastSkillUseTime = Time.time;
        }
    }
}
