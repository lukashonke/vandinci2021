using System;
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

	private float canPierceRoll;

	private float pierceRemainingDamage = 0;
	private int piercedEnemies = 0;
	[NonSerialized] public BulletReceiver[] collidedWith = new BulletReceiver[8];
	[NonSerialized] public Entity lastAffectedEnemy;

	private bool diedByCollision;

	public override void Awake()
	{
		base.Awake();
		
		trail = transform.GetChild(0).gameObject.GetComponent<TrailRenderer>();
	}

	// You can access this.bullet to get the parent bullet script.
	// After bullet's death, you can delay this script's death : use this.lifetimeAfterBulletDeath.

	// Use this for initialization (instead of Start)
	public override void OnBulletBirth ()
	{
		base.OnBulletBirth();

		ownerModule = bullet.emitter.gameObject.GetModule();
		
		trail.Clear();
		
		ApplyTrailParameters();

		piercedEnemies = 0;
		
		diedByCollision = false;

		canPierceRoll = Random.value;

		var canPierce = CanPierce();
		if (canPierce != PierceType.NoPierce)
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

	enum PierceType
	{
		NoPierce,
		FixedCount,
		UsingDamage
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

		if (!diedByCollision && ownerModule is OffensiveModule offensiveModule)
		{
			var effects = offensiveModule.onBulletDestroyEffects;
			if (effects != null)
			{
				for (var index = 0; index < effects.Count; index++)
				{
					var effect = effects[index];

					var flags = ImmediateEffectFlags.None;
					float strength = 1f;
					
					effect.ApplyWithChanceCheck(null, transform.position, ownerModule.parent, null, ownerModule, strength, new ImmediateEffectParams(), flags);
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
					
					effect.ApplyWithChanceCheck(null, transform.position, ownerModule.parent, null, ownerModule, strength, new ImmediateEffectParams(), flags);
				}
			}
		}

		// Your code here
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
		if (canPierce != PierceType.NoPierce && !CanCollide(br))
		{
			return;
		}
		
		var entity = br.gameObject.GetEntity();
		if (entity != null && entity.isAlive)
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

				var effects = offensiveModule.effects;
				
				for (var index = 0; index < effects.Count; index++)
				{
					var effect = effects[index];
					var prevHp = entity.GetHp();

					var flags = ImmediateEffectFlags.None;

					float strength = 1f;
					/*if (canPierce == PierceType.UsingDamage)
					{
						strength = pierceRemainingDamage;
					}*/
					
					effect.ApplyWithChanceCheck(entity, entity.GetPosition(), ownerModule.parent, null, ownerModule, strength, new ImmediateEffectParams(), flags);
					
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
							effect.ApplyWithChanceCheck(entity, entity.GetPosition(), ownerModule.parent, null, ownerModule, 1, new ImmediateEffectParams());
							list.Add(effect);
						} 
					}
					
					list.Clear();
					ListPool<ImmediateEffect>.Release(list);
				}

				lastAffectedEnemy = entity;
				
				bool deactivate = false;

				if (canPierce == PierceType.FixedCount)
				{
					piercedEnemies++;
					collidedWith[piercedEnemies] = br;
					if (piercedEnemies >= offensiveModule.stats.projectilePierceCount.GetValueInt())
					{
						deactivate = true;
					}
				}
				else if(canPierce == PierceType.UsingDamage)
				{
					collidedWith[piercedEnemies] = br;
					if (pierceRemainingDamage <= 0)
					{
						deactivate = true;
					}
				}
				else
				{
					deactivate = true;
				}
				
				offensiveModule.OnBulletEffectGiven(bullet, this, bulletWillDie: deactivate);

				if (deactivate)
				{
					diedByCollision = true;
					
					effects = offensiveModule.onBulletDestroyEffects;
					if (effects != null)
					{
						for (var index = 0; index < effects.Count; index++)
						{
							var effect = effects[index];

							var flags = ImmediateEffectFlags.None;
							float strength = 1f;
					
							effect.ApplyWithChanceCheck(entity, entity.GetPosition(), ownerModule.parent, null, ownerModule, strength, new ImmediateEffectParams(), flags);
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
					
							effect.ApplyWithChanceCheck(entity, entity.GetPosition(), ownerModule.parent, null, ownerModule, strength, new ImmediateEffectParams(), flags);
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
