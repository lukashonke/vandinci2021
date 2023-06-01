using System;
using System.Collections.Generic;
using System.Text;
using _Chi.Scripts.Mono.Common;
using _Chi.Scripts.Mono.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Chi.Scripts.Mono.Ui
{
    public class StatWindow : MonoBehaviour
    {
        public TextMeshProUGUI playerStats;
        public TextMeshProUGUI weaponStats;
        
        public void SetOpened(bool b)
        {
            gameObject.SetActive(b);
            
            if (b)
            {
                Gamesystem.instance.Pause();
            
                Initialise();    
            }
            else
            {
                Gamesystem.instance.Unpause();
            }
        }

        public bool Opened()
        {
            return gameObject.activeSelf;
        }
        
        public void Toggle()
        {
            if (Gamesystem.instance.uiManager.vehicleSettingsWindow.Opened())
            {
                return;
            }
            
            SetOpened(!gameObject.activeSelf);
        }

        public void Initialise()
        {
            StringBuilder sb = new StringBuilder();

            var player = Gamesystem.instance.objects.currentPlayer;
            var stats = player.stats;

            var listedStats = new List<(string name, string value)>()
            {
                ("Speed", $"{stats.speed.GetValue()}"), 
                ("Force Damage", $"{stats.velocityToDamageMul.GetValue()}"), 
                ("HP Regen",  $"{stats.hpRegenPerSecond.GetValue()} / sec"), 
                ("Skill Reuse", $"x{stats.skillReuseMul.GetValue()}"), 
                ("Skill Power", $"x{stats.skillPowerMul.GetValue()}"), 
                ("Base Crit Rate", $"{stats.baseCriticalRate.GetValue()}%"),
                ("Base Damage", $"x{stats.dealtDamageMul.GetValue()}"),
                ("Base Non-Crit Damage", $"x{stats.nonCriticalDamageMul.GetValue()}"),
                ("Base Crit Damage", $"x{stats.criticalDamageMul.GetValue()}"), 
                ("Received Damage", $"x{stats.receiveDamageMul.GetValue()}"), 
                ("Received Damage +", $"{stats.receiveDamageAdd.GetValue()}"), 
                ("Weapon Reload", $"x{stats.moduleReloadDurationMul.GetValue()}"), 
                ("Weapon Fire Rate", $"x{stats.moduleFireRateMul.GetValue()}"), 
                ("Pickup Range", $"{stats.pickupAttractRange.GetValue()}")
            };
            
            sb.AppendLine($"<size=150%>Player<size=100%>\n");

            foreach (var stat in listedStats)
            {
                sb.AppendLine($"<b>{stat.name}</b>: {stat.value}");
            }
            
            playerStats.text = sb.ToString();
            
            // ---
            
            sb = new StringBuilder();
            sb.AppendLine($"<size=150%>Weapons<size=100%>\n");


            foreach (var slot in player.slots)
            {
                if (slot.currentModule != null && slot.currentModule is OffensiveModule weapon)
                {
                    sb.AppendLine($"<size=125%>{slot.currentModule.name}<size=100%>\n");

                    var listedWeaponStats = new List<(string name, string value)>()
                    {
                        ("Projectiles", $"{weapon.stats.projectileCount.GetValue()} x {weapon.stats.projectileMultiplier.GetValue()}"),
                        ("Reload Duration", $"{weapon.stats.reloadDuration.GetValue()}s"),
                        ("Fire Rate", $"{(weapon.stats.fireRate.GetValue() > 0 && weapon.stats.magazineSize.GetValueInt() > 0 ? (1/weapon.stats.fireRate.GetValue()) : '-')}/s"),
                        ("Damage", $"{weapon.stats.projectileDamage.GetValue()}"),
                        //("Target Range", $"{weapon.stats.targetRange.GetValue()}"),
                        ("Magazine Size", $"{weapon.stats.magazineSize.GetValue()}"),
                        ("Critical Rate", $"{weapon.stats.projectileCriticalRate.GetValue()}%"),
                        ("Critical Damage", $"x{weapon.stats.projectileCriticalDamage.GetValue()}"),
                    };

                    float dmg = weapon.stats.projectileCount.GetValueInt() * weapon.stats.projectileMultiplier.GetValue() * weapon.stats.projectileDamage.GetValue();
                    float dmgPerMagazine = dmg * Math.Max(1, weapon.stats.magazineSize.GetValueInt());
                    float magazinePerMinute = 60f / (weapon.stats.reloadDuration.GetValue() + (weapon.stats.fireRate.GetValue() * Math.Max(1, weapon.stats.magazineSize.GetValueInt())));
                    float dpm = dmgPerMagazine * magazinePerMinute;
                    
                    listedWeaponStats.Add(("DPM", $"{dpm}"));
                    
                    foreach (var stat in listedWeaponStats)
                    {
                        sb.AppendLine($"<b>{stat.name}</b>: {stat.value}");
                    }
                    
                    sb.AppendLine("\n");
                }
            }
            
            weaponStats.text = sb.ToString();
        }
    }
}