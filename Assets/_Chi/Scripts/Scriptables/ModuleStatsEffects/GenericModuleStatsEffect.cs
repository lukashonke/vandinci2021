using System;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using UnityEngine;

namespace _Chi.Scripts.Scriptables.ModuleStatsEffects
{
    [CreateAssetMenu(fileName = "Generic Module Stats Effect", menuName = "Gama/Module Stats Effect/Generic Module Stats Effect")]
    public class GenericModuleStatsEffect : ModuleStatsEffect
    {
        public ModuleStatsEffectType effectType;

        public override bool Apply(Module target, object source, int level)
        {
            if (target is OffensiveModule offensiveModule)
            {
                switch (effectType)
                {
                    case ModuleStatsEffectType.MagazineSize:
                        offensiveModule.stats.magazineSize.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.AddPiercedEnemies:
                        break;
                    case ModuleStatsEffectType.ProjectileCount:
                        offensiveModule.stats.projectileCount.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileEnablePierce:
                        break;
                    case ModuleStatsEffectType.FireRate:
                        offensiveModule.stats.fireRate.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.StationaryFireRateBoost:
                        offensiveModule.stats.stationaryFireRateBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.MovingFireRateBoost:
                        offensiveModule.stats.movingFireRateBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.Damage:
                        break;
                    case ModuleStatsEffectType.ProjectileRange:
                        offensiveModule.stats.projectileRange.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ShotsPerShot:
                        break;
                    case ModuleStatsEffectType.Speed:
                        break;
                    case ModuleStatsEffectType.ReloadDuration:
                        offensiveModule.stats.reloadDuration.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.StationaryReloadDurationBoost:
                        offensiveModule.stats.stationaryReloadDurationBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.MovingReloadDurationBoost:
                        offensiveModule.stats.movingReloadDurationBoost.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.PierceChance:
                        offensiveModule.stats.projectilePierceChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.PiercedCount:
                        offensiveModule.stats.projectilePierceCount.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.PierceDeadChance:
                        offensiveModule.stats.projectilePierceDeadChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.PierceDeadCount:
                        offensiveModule.stats.projectilePierceDeadCount.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.CriticalDamage:
                        offensiveModule.stats.projectileCriticalDamage.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.CriticalRate:
                        offensiveModule.stats.projectileCriticalRate.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ArmorPiercing:
                        offensiveModule.stats.armorPiercing.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ArmoredDamageMul:
                        offensiveModule.stats.armoredDamageMul.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.NonArmoredDamageMul:
                        offensiveModule.stats.nonArmorDamageMul.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.InstantReloadChance:
                        offensiveModule.stats.instantReloadChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ConsumeNoAmmoChance:
                        offensiveModule.stats.consumeNoAmmoChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.StandingConsumeNoAmmoChance:
                        offensiveModule.stats.standingConsumeNoAmmoChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.MovingConsumeNoAmmoChance:
                        offensiveModule.stats.movingConsumeNoAmmoChance.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileRandomRotation:
                        offensiveModule.stats.projectileRandomRotation.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileSpread:
                        offensiveModule.stats.projectileSpreadAngle.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileSpeed:
                        offensiveModule.stats.projectileSpeed.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileKilledDoNotCountPierced:
                        offensiveModule.stats.projectilePierceCountIgnoreKilled.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    case ModuleStatsEffectType.ProjectileKilledDoNotCountIfLessThanHp:
                        offensiveModule.stats.projectilePierceCountIgnoreIfLessThanHp.AddModifier(new StatModifier(source, AddLevelValue(value, level), modifier, (short) order));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                return true;
            }
            
            return false;
        }

        public override bool Remove(Module target, object source)
        {
            if (target is OffensiveModule offensiveModule)
            {
                switch (effectType)
                {
                    case ModuleStatsEffectType.MagazineSize:
                        offensiveModule.stats.magazineSize.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.AddPiercedEnemies:
                        break;
                    case ModuleStatsEffectType.ProjectileCount:
                        offensiveModule.stats.projectileCount.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileEnablePierce:
                        break;
                    case ModuleStatsEffectType.FireRate:
                        offensiveModule.stats.fireRate.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.StationaryFireRateBoost:
                        offensiveModule.stats.stationaryFireRateBoost.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.MovingFireRateBoost:
                        offensiveModule.stats.movingFireRateBoost.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.Damage:
                        break;
                    case ModuleStatsEffectType.ProjectileRange:
                        offensiveModule.stats.projectileRange.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ShotsPerShot:
                        break;
                    case ModuleStatsEffectType.Speed:
                        break;
                    case ModuleStatsEffectType.ReloadDuration:
                        offensiveModule.stats.reloadDuration.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.StationaryReloadDurationBoost:
                        offensiveModule.stats.stationaryReloadDurationBoost.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.MovingReloadDurationBoost:
                        offensiveModule.stats.movingReloadDurationBoost.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.PierceChance:
                        offensiveModule.stats.projectilePierceChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.PiercedCount:
                        offensiveModule.stats.projectilePierceCount.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.PierceDeadChance:
                        offensiveModule.stats.projectilePierceDeadChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.PierceDeadCount:
                        offensiveModule.stats.projectilePierceDeadCount.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.CriticalDamage:
                        offensiveModule.stats.projectileCriticalDamage.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.CriticalRate:
                        offensiveModule.stats.projectileCriticalRate.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ArmorPiercing:
                        offensiveModule.stats.armorPiercing.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ArmoredDamageMul:
                        offensiveModule.stats.armoredDamageMul.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.NonArmoredDamageMul:
                        offensiveModule.stats.nonArmorDamageMul.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.InstantReloadChance:
                        offensiveModule.stats.instantReloadChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ConsumeNoAmmoChance:
                        offensiveModule.stats.consumeNoAmmoChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.StandingConsumeNoAmmoChance:
                        offensiveModule.stats.standingConsumeNoAmmoChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.MovingConsumeNoAmmoChance:
                        offensiveModule.stats.movingConsumeNoAmmoChance.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileRandomRotation:
                        offensiveModule.stats.projectileRandomRotation.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileSpread:
                        offensiveModule.stats.projectileSpreadAngle.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileSpeed:
                        offensiveModule.stats.projectileSpeed.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileKilledDoNotCountPierced:
                        offensiveModule.stats.projectilePierceCountIgnoreKilled.RemoveModifiersBySource(source);
                        break;
                    case ModuleStatsEffectType.ProjectileKilledDoNotCountIfLessThanHp:
                        offensiveModule.stats.projectilePierceCountIgnoreIfLessThanHp.RemoveModifiersBySource(source);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return true;
            }

            return false;
        }
        
        public override List<(string title, string value)> GetUiStats(int level)
        {
            (string, string) tuple = default;
                    
            switch (effectType)
            {
                case ModuleStatsEffectType.MagazineSize:
                    tuple = ("Magazine Size", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.AddPiercedEnemies:
                    break;
                case ModuleStatsEffectType.ProjectileCount:
                    tuple = ("Shot Projectiles", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileEnablePierce:
                    break;
                case ModuleStatsEffectType.FireRate:
                    tuple = ("Fire Rate", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.StationaryFireRateBoost:
                    tuple = ("Standing Fire Rate", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.MovingFireRateBoost:
                    tuple = ("Moving Fire Rate", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.Damage:
                    break;
                case ModuleStatsEffectType.ProjectileRange:
                    tuple = ("Range", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ShotsPerShot:
                    break;
                case ModuleStatsEffectType.Speed:
                    break;
                case ModuleStatsEffectType.ReloadDuration:
                    tuple = ("Reload Duration", $"{AddLevelValueUI(value, level)}");
                    break;  
                case ModuleStatsEffectType.StationaryReloadDurationBoost:
                    tuple = ("Stationary Reload Duration", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.MovingReloadDurationBoost:
                    tuple = ("Moving Reload Duration", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.PierceChance:
                    tuple = ("Pierce Chance", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.PiercedCount:
                    tuple = ("Pierced Enemies", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.PierceDeadChance:
                    tuple = ("Dead Pierce Chance", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.PierceDeadCount:
                    tuple = ("Dead Pierced Enemies", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.CriticalDamage:
                    tuple = ("Critical Damage", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.CriticalRate:
                    tuple = ("Critical Rate", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ArmorPiercing:
                    tuple = ("Armor Piercing", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ArmoredDamageMul:
                    tuple = ("Damage against armored", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.NonArmoredDamageMul:
                    tuple = ("Damage against non-armored", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.InstantReloadChance:
                    tuple = ("Instant Reload", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ConsumeNoAmmoChance:
                    tuple = ("Consume No Ammo Chance", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.StandingConsumeNoAmmoChance:
                    tuple = ("Standing Consume No Ammo Chance", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.MovingConsumeNoAmmoChance:
                    tuple = ("Moving Consume No Ammo Chance", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileRandomRotation:
                    tuple = ("Accuracy", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileSpread:
                    tuple = ("Spread", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileSpeed:
                    tuple = ("Projectile Speed", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileKilledDoNotCountPierced:
                    tuple = ("Auto Pierce Killed", $"{AddLevelValueUI(value, level)}");
                    break;
                case ModuleStatsEffectType.ProjectileKilledDoNotCountIfLessThanHp:
                    tuple = ("Ignore Low HP", $"{AddLevelValueUI(value, level)}");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return new List<(string title, string value)>()
            {
                tuple
            };
        }
    }

    public enum ModuleStatsEffectType
    {
        MagazineSize,
        AddPiercedEnemies,
        ProjectileCount,
        ProjectileEnablePierce,
        FireRate,
        StationaryFireRateBoost,
        MovingFireRateBoost,
        Damage,
        ProjectileRange,
        ShotsPerShot,
        Speed,
        ReloadDuration,
        StationaryReloadDurationBoost,
        MovingReloadDurationBoost,
        PierceChance,
        PiercedCount,
        CriticalRate,
        CriticalDamage,
        ArmorPiercing,
        InstantReloadChance,
        PierceDeadChance,
        PierceDeadCount,
        ArmoredDamageMul,
        NonArmoredDamageMul,
        ConsumeNoAmmoChance,
        StandingConsumeNoAmmoChance,
        MovingConsumeNoAmmoChance,
        ProjectileRandomRotation,
        ProjectileSpread,
        ProjectileSpeed,
        ProjectileKilledDoNotCountPierced,
        ProjectileKilledDoNotCountIfLessThanHp,
    }
}