using System;
using System.Linq;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables;
using UnityEngine;
using BulletPro;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public class BulletBehavior : BaseBulletBehaviour
{
	private Module ownerModule;
	private TrailRenderer trail;
	private SpriteRenderer effectRenderer;

	private float canPierceRoll;
	private float canPierceDeadRoll;

	private float pierceRemainingDamage = 0;
	[NonSerialized] public int piercedEnemies = 0;
	[NonSerialized] public int maxPiercedEnemies = 0;
	[NonSerialized] public int piercedDeadEnemies = 0;
	[NonSerialized] public int maxPiercedDeadEnemies = 0;
	[NonSerialized] public BulletReceiver[] collidedWith = new BulletReceiver[16];
	[NonSerialized] public Entity lastAffectedEnemy;

	private object ignoreTarget;

	private bool diedByCollision;

	public override void Awake()
	{
		base.Awake();
		
		trail = transform.GetChild(0).gameObject.GetComponent<TrailRenderer>();
		effectRenderer = transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>();
	}

	// You can access this.bullet to get the parent bullet script.
	// After bullet's death, you can delay this script's death : use this.lifetimeAfterBulletDeath.

	// Use this for initialization (instead of Start)
	public override void OnBulletBirth ()
	{
		base.OnBulletBirth();

		ownerModule = bullet.emitter.gameObject.GetModule();

		if (ownerModule is OffensiveModule offensiveModule2)
		{
			maxPiercedEnemies = offensiveModule2.stats.projectilePierceCount.GetValueInt();
			maxPiercedDeadEnemies = offensiveModule2.stats.projectilePierceDeadCount.GetValueInt();

			foreach (var parameter in bullet.moduleParameters.parameters)
			{
				switch (parameter.name)
				{
					case "Override_Penetrations":
						maxPiercedEnemies = parameter.intValue;
						break;
					case "Override_DeadPenetrations":
						maxPiercedDeadEnemies = parameter.intValue;
						break;
				}
			}
		}
		
		var ignoreTarget1 = this.bullet.moduleParameters.GetObjectReferenceSilent(BulletVariables.IgnoreTarget1);
		var ignoreTarget2 = this.bullet.subEmitter.moduleParameters.GetObjectReferenceSilent(BulletVariables.IgnoreTarget1);

		this.ignoreTarget = ignoreTarget1 ? ignoreTarget1 : ignoreTarget2;
		
		//var ignoreTarget2 = bullet.dynamicSolver.SolveDynamicObjectReference(BulletVariables.IgnoreTarget1, 1561651, ParameterOwner.Bullet);
		
		trail.Clear();
		
		ApplyTrailParameters();
		ApplySpriteEffectParameters();

		piercedEnemies = 0;
		
		diedByCollision = false;

		canPierceRoll = Random.value;
		canPierceDeadRoll = Random.value;

		var canPierce = CanPierce();
		var canPierceDead = CanPierceDead();
		if (canPierce != PierceType.NoPierce || canPierceDead != PierceDeadType.NoPierce)
		{
			if (canPierce == PierceType.UsingDamage)
			{
				if (ownerModule is OffensiveModule offensiveModule)
				{
					pierceRemainingDamage = offensiveModule.stats.projectileDamage.GetValue();
				}
			}
			
			for (var index = 0; index < collidedWith.Length; index++)
			{
				collidedWith[index] = null;
			}
		}
	}

	private void ApplyTrailParameters()
	{
		if (ownerModule is OffensiveModule offensiveModule)
		{
			if (offensiveModule.trailParameters != null && offensiveModule.trailParameters.useTrail)
			{
				trail.enabled = true;
				trail.material = offensiveModule.trailParameters.material;
				trail.time = offensiveModule.trailParameters.trailLengthTime;
			}
			else
			{
				trail.enabled = false;
			}
		}
	}

	private void ApplySpriteEffectParameters()
	{
		if (ownerModule is OffensiveModule offensiveModule)
		{
			if (offensiveModule.spriteEffectParameters != null && offensiveModule.spriteEffectParameters.enabled)
			{
				effectRenderer.enabled = true;
				effectRenderer.material = offensiveModule.spriteEffectParameters.material;
				effectRenderer.sprite = offensiveModule.spriteEffectParameters.sprite;
				Transform transform1;
				(transform1 = effectRenderer.transform).localRotation = Quaternion.Euler(0, 0, offensiveModule.spriteEffectParameters.rotation);
				transform1.localScale = offensiveModule.spriteEffectParameters.scale;
				transform1.localPosition = offensiveModule.spriteEffectParameters.offset;
			}
			else
			{
				effectRenderer.enabled = false;
			}
		}
	}

	private PierceType CanPierce()
	{
		if (ownerModule is OffensiveModule offensiveModule)
		{
			if (canPierceRoll < offensiveModule.stats.projectilePierceChance.GetValue())
			{
				if(offensiveModule.stats.canProjectilePierce > 0) return PierceType.FixedCount;
				if(offensiveModule.stats.canProjectilePierceUsingDamage > 0) return PierceType.UsingDamage;
			}
		}

		return PierceType.NoPierce;
	}
	
	private PierceDeadType CanPierceDead()
	{
		if (ownerModule is OffensiveModule offensiveModule)
		{
			if (canPierceRoll < offensiveModule.stats.projectilePierceDeadChance.GetValue())
			{
				return PierceDeadType.FixedCount;
			}
		}

		return PierceDeadType.NoPierce;
	}

	enum PierceType
	{
		NoPierce,
		FixedCount,
		UsingDamage,
	}

	enum PierceDeadType
	{
		NoPierce,
		FixedCount
	}
	
	// Update is (still) called once per frame
	/*public override void Update ()
	{
		base.Update();

		// Your code here
	}*/

	// This gets called when the bullet dies
	public override void OnBulletDeath()
	{
		base.OnBulletDeath();
		
		trail.enabled = false;

		if (ownerModule is OffensiveModule offensiveModule)
		{
			offensiveModule.OnBulletDeath(bullet, this);
			
			if (!diedByCollision)
			{
				var effects = offensiveModule.onBulletDestroyEffects;
				if (effects != null)
				{
					for (var index = 0; index < effects.Count; index++)
					{
						var effect = effects[index];

						var flags = ImmediateEffectFlags.None;
						float strength = 1f;
					
						var effectData = Gamesystem.instance.poolSystem.GetEffectData();
						effectData.target = null;
						effectData.targetPosition = transform.position;
						effectData.sourceEntity = ownerModule.parent;
						effectData.sourceModule = ownerModule;
						effectData.sourceBullet = this;
						effectData.sourceEmitter = bullet.emitter;
					
						effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams(), flags);
					
						Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
					}
				}
					
				var effects2 = offensiveModule.additionalOnBulletDestroyEffects;
				if (effects2 != null)
				{
					for (var index = 0; index < effects2.Count; index++)
					{
						var effect = effects2[index].Item2;

						var flags = ImmediateEffectFlags.None;
						float strength = 1f;
					
						var effectData = Gamesystem.instance.poolSystem.GetEffectData();
						effectData.target = null;
						effectData.targetPosition = transform.position;
						effectData.sourceEntity = ownerModule.parent;
						effectData.sourceModule = ownerModule;
						effectData.sourceBullet = this;
						effectData.sourceEmitter = bullet.emitter;
					
						effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams(), flags);
					
						Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
					}
				}
			}
		}
	}

	// This gets called after the bullet has died, it can be delayed.
	public override void OnBehaviourDeath()
	{
		base.OnBehaviourDeath();

		// Your code here
	}

	private bool CanCollide(BulletReceiver br)
	{
		for (int i = 0; i < collidedWith.Length; i++)
		{
			if (collidedWith[i] == br)
			{
				return false;
			}
		}

		return true;
	}

	// This gets called whenever the bullet collides with a BulletReceiver. The most common callback.
	public override void OnBulletCollision(BulletReceiver br, Vector3 collisionPoint)
	{
		base.OnBulletCollision(br, collisionPoint);
		
		var canPierce = CanPierce();
		if (canPierce != PierceType.NoPierce)
		{
			if (!CanCollide(br))
			{
				return;
			}
		}
		
		var canPierceDead = CanPierceDead();
		if (canPierceDead != PierceDeadType.NoPierce)
		{
			if (!CanCollide(br))
			{
				return;
			}
		}
		
		var entity = br.gameObject.GetEntity();
		if (entity != null && entity.isAlive && entity != ignoreTarget)
		{
			if (ownerModule is OffensiveModule offensiveModule)
			{
				if (offensiveModule.affectType == TargetType.EnemyOnly && !entity.AreEnemies(ownerModule.parent))
				{
					return;
				}

				if (offensiveModule.affectType == TargetType.FriendlyOnly && entity.AreEnemies(ownerModule.parent))
				{
					return;
				}

				if (!offensiveModule.CanTarget(entity))
				{
					return;
				}

				var entityHp = entity.entityStats.hp;
				var entityMaxHp = entity.entityStats.maxHp;

				var effects = offensiveModule.effects;
				
				for (var index = 0; index < effects.Count; index++)
				{
					var effect = effects[index];

					bool effectDisabled = false;
					foreach (var e in offensiveModule.disabledEffects)
					{
						if (e.Item2 == effect)
						{
							effectDisabled = true;
							break;
						}
					}

					if (effectDisabled) continue;
					
					var prevHp = entity.GetHp();

					var flags = ImmediateEffectFlags.None;

					float strength = 1f;
					/*if (canPierce == PierceType.UsingDamage)
					{
						strength = pierceRemainingDamage;
					}*/
					
					var effectData = Gamesystem.instance.poolSystem.GetEffectData();
					effectData.target = entity;
					effectData.targetPosition = entity.GetPosition();
					effectData.sourceEntity = ownerModule.parent;
					effectData.sourceModule = ownerModule;
					effectData.sourceBullet = this;
					effectData.sourceEmitter = bullet.emitter;
					
					effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams(), flags);
					
					Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
					
					var newHp = entity.GetHp();
					
					if (canPierce == PierceType.UsingDamage)
					{
						pierceRemainingDamage -= prevHp - newHp;
					}
				}
				
				var additionalEffects = offensiveModule.additionalEffects;

				if (additionalEffects.Count > 0)
				{
					ListPool<ImmediateEffect>.Get(out var list);

					for (var index = 0; index < additionalEffects.Count; index++)
					{
						var effect = additionalEffects[index].Item2;

						if (!list.Contains(effect))
						{
							var effectData = Gamesystem.instance.poolSystem.GetEffectData();
							effectData.target = entity;
							effectData.targetPosition = entity.GetPosition();
							effectData.sourceEntity = ownerModule.parent;
							effectData.sourceModule = ownerModule;
							effectData.sourceBullet = this;
							effectData.sourceEmitter = bullet.emitter;
							
							effect.ApplyWithChanceCheck(effectData, 1, new ImmediateEffectParams());
							
							Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
							
							list.Add(effect);
						} 
					}
					
					list.Clear();
					ListPool<ImmediateEffect>.Release(list);
				}

				lastAffectedEnemy = entity;
				
				bool deactivate = false;

				bool increasePiercedCount = true;

				if (offensiveModule.stats.projectilePierceCountIgnoreKilled.GetValueInt() > 0)
				{
					increasePiercedCount = entity.isAlive;
				}

				if (offensiveModule.stats.projectilePierceCountIgnoreIfLessThanHp.GetValue() >= entityHp)
				{
					increasePiercedCount = false;
				}

				if (offensiveModule.stats.projectileDamage.GetValue() * offensiveModule.stats.projectilePierceCountIgnoreIfLessThanProjectileDamagePortion.GetValue() >= entityMaxHp)
				{
					increasePiercedCount = false;
				}

				if (canPierce == PierceType.FixedCount)
				{
					if (increasePiercedCount)
					{
						piercedEnemies++;
						collidedWith[piercedEnemies] = br;
						if (piercedEnemies >= maxPiercedEnemies)
						{
							deactivate = true;
						}
					}
				}
				else if(canPierce == PierceType.UsingDamage)
				{
					if (increasePiercedCount)
					{
						collidedWith[piercedEnemies] = br;
						if (pierceRemainingDamage <= 0)
						{
							deactivate = true;
						}
					}
				}
				else
				{
					deactivate = true;
				}

				if (!entity.isAlive && canPierceDead == PierceDeadType.FixedCount)
				{
					piercedDeadEnemies++;
					collidedWith[piercedDeadEnemies] = br;
					if (piercedDeadEnemies >= maxPiercedDeadEnemies)
					{
						deactivate = true;
					}
					else
					{
						deactivate = false;
					}
				}
				
				offensiveModule.OnBulletEffectGiven(bullet, this, bulletWillDie: deactivate);

				if (deactivate)
				{
					diedByCollision = true;

					if (!offensiveModule.OnBulletBeforeDeactivated(bullet, this))
					{
						return;
					}
					
					effects = offensiveModule.onBulletDestroyEffects;
					if (effects != null)
					{
						for (var index = 0; index < effects.Count; index++)
						{
							var effect = effects[index];

							var flags = ImmediateEffectFlags.None;
							float strength = 1f;
							
							var effectData = Gamesystem.instance.poolSystem.GetEffectData();
							effectData.target = entity;
							effectData.targetPosition = entity.GetPosition();
							effectData.sourceEntity = ownerModule.parent;
							effectData.sourceModule = ownerModule;
							effectData.sourceBullet = this;
							effectData.sourceEmitter = bullet.emitter;
					
							effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams(), flags);
							
							Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
						}
					}
					
					var effects2 = offensiveModule.additionalOnBulletDestroyEffects;
					if (effects2 != null)
					{
						for (var index = 0; index < effects2.Count; index++)
						{
							var effect = effects2[index].Item2;

							var flags = ImmediateEffectFlags.None;
							float strength = 1f;
							
							var effectData = Gamesystem.instance.poolSystem.GetEffectData();
							effectData.target = entity;
							effectData.targetPosition = entity.GetPosition();
							effectData.sourceEntity = ownerModule.parent;
							effectData.sourceModule = ownerModule;
							effectData.sourceBullet = this;
							effectData.sourceEmitter = bullet.emitter;
					
							effect.ApplyWithChanceCheck(effectData, strength, new ImmediateEffectParams(), flags);
							
							Gamesystem.instance.poolSystem.ReturnEffectData(effectData);
						}
					}
					
					bullet.Die();
				}
			}
		}
	}

	// This gets called whenever the bullet collides with a BulletReceiver AND was not colliding during the previous frame.
	public override void OnBulletCollisionEnter(BulletReceiver br, Vector3 collisionPoint)
	{
		base.OnBulletCollisionEnter(br, collisionPoint);
		// Your code here
	}

	// This gets called whenever the bullet stops colliding with any BulletReceiver.
	public override void OnBulletCollisionExit()
	{
		base.OnBulletCollisionExit();

		// Your code here
	}

	// This gets called whenever the bullet shoots a pattern.
	public override void OnBulletShotAnotherBullet(int patternIndex)
	{
		base.OnBulletShotAnotherBullet(patternIndex);
		
		
		
		// Your code here
	}
}
