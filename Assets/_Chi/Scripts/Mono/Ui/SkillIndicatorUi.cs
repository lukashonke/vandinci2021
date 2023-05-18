using System;
using _Chi.Scripts.Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Chi.Scripts.Mono.Ui
{
    public class SkillIndicatorUi : MonoBehaviour
    {
        public Image skillIcon;

        public RectTransform progressMask;

        public TextMeshProUGUI chargesCount;
        
        private Skill currentSkill;

        public void Start()
        {
            chargesCount.gameObject.SetActive(false);
        }

        public void SetChargesCount(int count)
        {
            if (count == 0)
            {
                chargesCount.gameObject.SetActive(false);
            }
            else
            {
                chargesCount.gameObject.SetActive(true);
                chargesCount.text = "+" + count.ToString();
            }
        }

        public void SetSkill(Skill skill)
        {
            if (currentSkill == skill) return;

            currentSkill = skill;

            var db = Gamesystem.instance.prefabDatabase;

            foreach (var prefab in db.prefabs)
            {
                if (prefab.skill == skill)
                {
                    skillIcon.sprite = prefab.prefabUiImage != null ? prefab.prefabUiImage : prefab.prefabUi.GetComponent<Image>().sprite;
                }
            }
        }

        private bool isReloaded;

        public bool IsReloaded()
        {
            return isReloaded;
        }

        public void SetReloadPercentage(float percent)
        {
            if (percent >= 1)
            {
                isReloaded = true;
            }
            else
            {
                isReloaded = false;
            }
            
            progressMask.localScale = new Vector3(1 - percent, 1, 0);
        }
    }
}