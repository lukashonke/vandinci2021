using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Misc;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.System;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Persistence;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Statistics;
using BulletPro;
using Com.LuisPedroFonseca.ProCamera2D;
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
        
        public List<PlayerUpgradeItem> playerUpgradeItems;
        
        public List<SkillUpgradeItem> skillUpgradeItems;
        
        [NonSerialized] public PlayerBody body;
        [NonSerialized] public List<ModuleSlot> slots;
        [NonSerialized] public PlayerControls controls;

        public float nearestEnemiesDetectorRange = 15f;
        
        private Collider2D[] buffer = new Collider2D[4096];

        [NonSerialized] public List<Entity> targetableEnemies;
        [NonSerialized] public HashSet<Entity> damagingEnemies;
        private List<Entity> damagingEnemiesToRemove;

        [NonSerialized] private bool canDealPushDamage = true;

        [ReadOnly] public int shieldCharges;
        
        public List<ImmediateEffect> shieldEffects = new List<ImmediateEffect>();

        private Dictionary<Skill, SkillData> skillDatas;

        [NonSerialized] private float nextRestoreShield = 1;

        [NonSerialized] public float lastSkillUseTime;

        public ImmediateEffect pushEffect;
        public GameObject damageEffect;
        public GameObject shieldEffectVfx;
        public GameObject shieldAfterSkillUseVfx;

        [NonSerialized] public Dictionary<Skill, int> extraSkillCharges;
        [NonSerialized] public Dictionary<Skill, float> extraSkillChargesLoadProgress;
        [NonSerialized] public VisualItemSlot[] visualItemSlots;

        public Vector3 position;

        public override void Awake()
        {
            base.Awake();

            extraSkillCharges = new();
            extraSkillChargesLoadProgress = new();
            body = GetComponentInChildren<PlayerBody>();
            controls = GetComponent<PlayerControls>();
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
            
            foreach (var upgradeItem in playerUpgradeItems)
            {
                AddPlayerUpgradeItem(upgradeItem);
            }
            
            foreach (var upgradeItem in skillUpgradeItems)
            {
                AddSkillUpgradeItem(upgradeItem);
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
            const float refreshDelay = 0.05f;
            var waiter = new WaitForSeconds(refreshDelay);

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
                else if (nextRestoreShield > 0 && nextRestoreShield <= Time.time && shieldCharges < stats.shieldChargesCount.GetValue())
                {
                    shieldCharges++;
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

                /*var skillCharges = stats.skillExtraChargeCount.GetValueInt();
                if (skillCharges > 0)
                {
                    foreach (var charge in extraSkillCharges)
                    {
                        if (charge.Value < skillCharges)
                        {
                            extraSkillChargesLoadProgress[charge.Key] += refreshDelay;
                        }
                        
                        if(extraSkillChargesLoadProgress[charge.Key] >= charge.Key.GetReuseDelay(this))
                        {
                            extraSkillChargesLoadProgress[charge.Key] = 0;
                            extraSkillCharges[charge.Key]++;
                            break;
                        }
                    }
                }*/

                yield return waiter;
            }   
        }

        public void RestoreShield(int charges)
        {
            for (int i = 0; i < charges; i++)
            {
                if(shieldCharges < stats.shieldChargesCount.GetValue())
                {
                    shieldCharges++;
                }
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
            position = GetPosition();
            
            Debug.DrawLine(GetPosition(), GetPosition() + Vector3.down, Color.cyan);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            
            Gamesystem.instance.TrackPlayerPosition(this);
            
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

                if (velocity >= stats.minVelocityToDamage.GetValue() && canDealPushDamage)
                {
                    var damage = stats.velocityToDamageMul.GetValue() * velocity * rb.mass;

                    if (damage > 0 && monster.CanBePushed())
                    {
                        var dmgWithFlags = DamageExtensions.CalculateEffectDamage(damage, monster, this);
                        monster.ReceiveDamage(dmgWithFlags.damage, this, dmgWithFlags.flags);

                        if (pushEffect != null)
                        {
                            pushEffect.Apply(monster, monster.GetPosition(), this, null, null, velocity, new ImmediateEffectParams());
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

        public override bool ReceiveDamage(float damage, Entity damager, DamageFlags damageFlags = DamageFlags.None, Color? damageTextColor = null)
        {
            var ret = base.ReceiveDamage(damage, damager, damageFlags);

            if (ret && damage > 0)
            {
                ProCamera2DShake.Instance.Shake("PlayerDamage");
            }
            
            return ret;
        }

        private float nextReceiveDamage;

        public override bool CanReceiveDamage(float damage, Entity damager)
        {
            if (nextReceiveDamage > Time.time) return false;

            nextReceiveDamage = Time.time + stats.receiveDamageMinInterval.GetValue();
            
            if (shieldCharges > 0)
            {
                nextRestoreShield = Time.time + stats.singleShieldRechargeDelay.GetValue();
                shieldCharges--;
                ShieldPushAwayEnemies();
                //TODO add vfx
                return false;
            }

            return true;
        }

        private void ShieldPushAwayEnemies()
        {
            var count = EntityExtensions.GetNearest(this, GetPosition(), stats.shieldEffectsRadius.GetValue(), TargetType.EnemyOnly, buffer);
            for (int i = 0; i < count; i++)
            {
                var col = buffer[i];
                var entity = col.gameObject.GetEntity();
                if (entity is Npc npc && npc.CanBePushed() && this.AreEnemies(npc))
                {
                    foreach (var effect in shieldEffects)
                    {
                        effect.Apply(npc, npc.GetPosition(), this, null, null, stats.shieldEffectsStrength.GetValue(), new ImmediateEffectParams());
                    }
                }
            }
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
            visualItemSlots = GetComponentsInChildren<VisualItemSlot>();

            body.Initialise(this);
        }

        public ModuleSlot GetSlotById(int slotId)
        {
            return slots.FirstOrDefault(s => s.slotId == slotId);
        }

        public void SetModuleInSlot(ModuleSlot slot, GameObject modulePrefab, int level, int rotation, List<ModuleUpgradeItem> moduleUpgradeItems)
        {
            if (slot.currentModule != null)
            {
                slot.SetModuleInSlot(null, destroyCurrent: true);
            }

            var moduleGo = Instantiate(modulePrefab, slot.GetModulePosition(), Quaternion.identity, slot.transform);
            var module = moduleGo.GetComponent<Module>();

            module.level = level;
            module.SetOriginalRotation(rotation);
            module.upgrades = moduleUpgradeItems;
            
            slot.SetModuleInSlot(module);
        }
        
        public bool TryActivateSkill(int slot)
        {
            var skill = GetSkill(slot);

            if (skill == null) return false;

            return skill.Trigger(this);
        }

        public bool IsStillActivated(Skill skill)
        {
            return controls.IsActionPressed(GetSkillSlot(skill));
        }

        public Skill GetSkill(int slot)
        {
            if (skills.Count - 1 >= slot)
            {
                return skills[slot];
            }

            return null;
        }

        public int GetSkillSlot(Skill skill)
        {
            return skills.IndexOf(skill);
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

        public void AddPlayerUpgradeItem(PlayerUpgradeItem item)
        {
            if (playerUpgradeItems.Contains(item) && !item.canBeStacked) return;
            
            playerUpgradeItems.Add(item);
            item.ApplyToPlayer(this);
        }

        public void RemovePlayerUpgradeItem(PlayerUpgradeItem item)
        {
            if (playerUpgradeItems.Remove(item))
            {
                item.RemoveFromPlayer(this);
            }
        }

        public void RemovePlayerUpgradeItems()
        {
            foreach (var item in playerUpgradeItems.ToArray())
            {
                RemovePlayerUpgradeItem(item);
            }
        }
        
        public void AddSkillUpgradeItem(SkillUpgradeItem item)
        {
            if (skillUpgradeItems.Contains(item)) return;
            
            skillUpgradeItems.Add(item);
            item.ApplyToPlayer(this);
        }

        public void RemoveSkillUpgradeItem(SkillUpgradeItem item)
        {
            if (skillUpgradeItems.Remove(item))
            {
                item.RemoveFromPlayer(this);
            }
        }

        public void RemoveSkillUpgradeItems()
        {
            foreach (var item in skillUpgradeItems.ToArray())
            {
                RemoveSkillUpgradeItem(item);
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

            if (!extraSkillCharges.ContainsKey(skill))
            {
                extraSkillCharges[skill] = 0;
            }
            
            if(!extraSkillChargesLoadProgress.ContainsKey(skill))
            {
                extraSkillChargesLoadProgress[skill] = 0;
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
            extraSkillCharges.Remove(skill);
            extraSkillChargesLoadProgress.Remove(skill);
        }

        public override void OnSkillUse(Skill skill)
        {
            var restoreHealth = stats.skillUseHealthPercent.GetValue();

            if (restoreHealth > 0)
            {
                var toRestore = GetMaxHp() * restoreHealth;
                this.Heal(toRestore);
            }

            lastSkillUseTime = Time.time;

            foreach (var slot in slots)
            {
                if (slot.currentModule != null && slot.currentModule.subEmitters != null)
                {
                    foreach (var kp in slot.currentModule.subEmitters)
                    {
                        foreach (var subEmitter in kp.Value)
                        {
                            subEmitter.OnSkillUse(skill);
                        }
                    }
                }
            }
        }

        public override void OnAfterSkillUse(Skill skill)
        {
            base.OnAfterSkillUse(skill);
            
            foreach (var slot in slots)
            {
                if (slot.currentModule != null)
                {
                    slot.currentModule.OnAfterSkillUse(skill);
                }
            }
        }

        public void ResetExtraSkillCharges(Skill skill)
        {
            extraSkillCharges[skill] = stats.skillExtraChargeCount.GetValueInt();
        }

        public void AddExtraSkillCharges(Skill skill, int count)
        {
            extraSkillCharges[skill] += count;
        }
        
        public void RemoveExtraSkillCharges(Skill skill, int count)
        {
            extraSkillCharges[skill] -= count;
        }

        public void SetVisualItems(SetVisualItemSlot item, bool remove)
        {
            foreach (var slot in visualItemSlots)
            {
                if (slot.slotType == item.slotType)
                {
                    if (remove)
                    {
                        slot.SetContent(null);   
                    }
                    else
                    {
                        slot.SetContent(item.prefab);
                    }
                }
            }
        }
        
        public void OnHitByBullet(Bullet bullet, Vector3 pos)
        {
            var damage = bullet.moduleParameters.GetFloat("_Damage");
             
            ReceiveDamage(damage, null);
        }

        public void SetCanDealPushDamage(bool b)
        {
            canDealPushDamage = b;
        }

        public void OnPickupGold(int amount)
        {
            foreach (var slot in slots)
            {
                if(slot.currentModule != null)
                {
                    slot.currentModule.OnPickupGold(amount);
                }
            }
        }
    }
}
