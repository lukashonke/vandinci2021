using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Add Immediate Effect", menuName = "Gama/Module Stats Effect/Projectile Add Immediate Effect")]
    public class ProjectileAddImmediateEffect : ModuleStatsEffect
    {
        public List<ImmediateEffect> effectsToAdd;
        
        public EffectType effectType;

        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule && effectsToAdd != null)
            {
                foreach (var effect in effectsToAdd)
                {
                    if (effectType == EffectType.Default)
                    {
                        if (!offensiveModule.additionalEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalEffects.Add((source, effect));
                        }    
                    }
                    else if (effectType == EffectType.OnBulletDestroy)
                    {
                        if (!offensiveModule.additionalOnBulletDestroyEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalOnBulletDestroyEffects.Add((source, effect));
                        }
                    }
                    else if (effectType == EffectType.SelfOnShoot)
                    {
                        if (!offensiveModule.additionalShootEffectsSelf.Contains((source, effect)))
                        {
                            offensiveModule.additionalShootEffectsSelf.Add((source, effect));
                        }
                    }
                    else if (effectType == EffectType.OnPickupGold)
                    {
                        if (!offensiveModule.additionalOnPickupGoldEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalOnPickupGoldEffects.Add((source, effect));
                        }
                    }
                }
                
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                foreach (var effect in effectsToAdd)
                {
                    if (effectType == EffectType.Default)
                    {
                        if (offensiveModule.additionalEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalEffects.Remove((source, effect));
                        } 
                    }
                    else if (effectType == EffectType.OnBulletDestroy)
                    {
                        if (offensiveModule.additionalOnBulletDestroyEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalOnBulletDestroyEffects.Remove((source, effect));
                        }
                    }
                    else if (effectType == EffectType.SelfOnShoot)
                    {
                        if (offensiveModule.additionalShootEffectsSelf.Contains((source, effect)))
                        {
                            offensiveModule.additionalShootEffectsSelf.Remove((source, effect));
                        }
                    }
                    else if (effectType == EffectType.OnPickupGold)
                    {
                        if (offensiveModule.additionalOnPickupGoldEffects.Contains((source, effect)))
                        {
                            offensiveModule.additionalOnPickupGoldEffects.Remove((source, effect));
                        }
                    }
                }
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            var ret = new List<(string title, string value)>();

            foreach (var effect in effectsToAdd)
            {
                ret.Add(("Add Effect", effect.name));
            }

            return ret;
        }
    }

    public enum EffectType
    {
        Default,
        OnBulletDestroy,
        SelfOnShoot,
        OnPickupGold
    }
}