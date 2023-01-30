using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Entities;
using _Chi.Scripts.Mono.Extensions;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;
using BulletPro;

// This script is supported by the BulletPro package for Unity.
// Template author : Simon Albou <albou.simon@gmail.com>

// This script is actually a MonoBehaviour for coding advanced things with Bullets.
public class BulletBehavior : BaseBulletBehaviour
{
	private Module ownerModule;

	private int piercedEnemies = 0;
	private BulletReceiver[] collidedWith = new BulletReceiver[8];
	
	// You can access this.bullet to get the parent bullet script.
	// After bullet's death, you can delay this script's death : use this.lifetimeAfterBulletDeath.

	// Use this for initialization (instead of Start)
	public override void OnBulletBirth ()
	{
		base.OnBulletBirth();
		
		ownerModule = bullet.emitter.gameObject.GetModule();

		piercedEnemies = 0;

		if (CanPierce())
		{
			for (var index = 0; index < collidedWith.Length; index++)
			{
				collidedWith[index] = null;
			}
		}
	}

	private bool CanPierce()
	{
		if (ownerModule is OffensiveModule offensiveModule)
		{
			if (offensiveModule.stats.canProjectilePierce > 0)
			{
				return true;
			}
		}

		return false;
	}
	
	// Update is (still) called once per frame
	public override void Update ()
	{
		base.Update();

		// Your code here
	}

	// This gets called when the bullet dies
	public override void OnBulletDeath()
	{
		base.OnBulletDeath();

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

		bool canPierce = CanPierce();
		if (canPierce && !CanCollide(br))
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
					effect.Apply(entity, ownerModule.parent, null, ownerModule);

					bool deactivate = false;

					if (canPierce)
					{
						piercedEnemies++;
						collidedWith[piercedEnemies] = br;
						if (piercedEnemies >= offensiveModule.stats.projectilePierceCount.GetValueInt())
						{
							deactivate = true;
						}
					}
					else
					{
						deactivate = true;
					}

					if (deactivate)
					{
						bullet.Die();
					}
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
