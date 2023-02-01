using System;
using _Chi.Scripts.Scriptables;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class SkillIndicatorUi : MonoBehaviour
    {
        public Image skillIcon;

        public RectTransform progressMask;
        
        private Skill currentSkill;

        public void SetSkill(Skill skill)
        {
            if (currentSkill == skill) return;

            currentSkill = skill;

            var db = Gamesystem.instance.prefabDatabase;

            foreach (var prefab in db.prefabs)
            {
                if (prefab.skill == skill)
                {
                    skillIcon.sprite = prefab.prefabUi.GetComponent<Image>().sprite;
                }
            }
        }

        public void SetReloadPercentage(float percent)
        {
            progressMask.localScale = new Vector3(1 - percent, 1, 0);
        }
    }
}