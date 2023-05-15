using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Chi.Scripts.Mono.Misc;
using _Chi.Scripts.Mono.Mission;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Mono.Ui.Dialogs;
using _Chi.Scripts.Mono.Ui.Tooltips;
using _Chi.Scripts.Scriptables;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Required] public GameObject fullscreenWindowBackground;
    [Required] public GameObject overlay;
    [Required] public GameObject windows;

    [Required] public ProgressBar goldProgressBar;

    [Required] public GameObject confirmDialogPrefab;
    private GameObject confirmDialog;

    [Required] public GameObject moduleTooltipPrefab;
    private GameObject currentTooltip;
    
    [Required] public GameObject actionsPanelPrefab;
    [NonSerialized] public GameObject currentActionsPanel;
    [NonSerialized] public ActionsPanel currentActionsPanelData;

    [Required] public GameObject defaultActionsPanelButton;
    
    [Required] public TextMeshProUGUI benchmarkStats;
    [Required] public TextMeshProUGUI level;
    [Required] public TextMeshProUGUI gold;
    [Required] public TextMeshProUGUI time;
    [Required] public TextMeshProUGUI killed;
    [FormerlySerializedAs("dropdown")] [Required] public TMPro.TMP_Dropdown missionSelector;
    [Required] public TMPro.TMP_Dropdown missionWaweSelector;
    [Required] public TextMeshProUGUI missionText;

    public SkillIndicatorUi[] skillIndicators;
    
    [Required] public VehicleSettingsWindow vehicleSettingsWindow;

    [NonSerialized] public AddingUiItem addingUiItem;

    void Awake()
    {
        Gamesystem.instance.uiManager = this;

        skillIndicators = GetComponentsInChildren<SkillIndicatorUi>();
        
        missionSelector.onValueChanged.AddListener(OnMissionSelectorChange);
        missionWaweSelector.onValueChanged.AddListener(OnMissionWaweSelectorChange);
    }

    // Start is called before the first frame update
    void Start()
    {
        missionSelector.options.Add(new TMP_Dropdown.OptionData()
        {
            text = "Change mission"
        });  
        
        StartCoroutine(UpdateCoroutine());
        StartCoroutine(LoadMissionWawes());
    }

    private IEnumerator LoadMissionWawes()
    {
        yield return null;
        
        foreach (var mission in Gamesystem.instance.missionDatabase.missions)
        {
            missionSelector.options.Add(new TMP_Dropdown.OptionData()
            {
                text = mission.name,
                
            });    
        }

        foreach (var go in Gamesystem.instance.missionManager.GetCurrentFirstMission().events)
        {
            if (!string.IsNullOrEmpty(go.eventName))
            {
                missionWaweSelector.options.Add(new TMP_Dropdown.OptionData()
                {
                    text = go.eventName
                });
            }
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        var waiter = new WaitForSecondsRealtime(.5f);
        while (true)
        {
            DoStatsUpdate();
            
            yield return waiter;
        }
    }

    // Update is called once per frame
    private void DoStatsUpdate()
    {
        var progressData = Gamesystem.instance.progress.progressData;
        
        benchmarkStats.text = $"objects: {Gamesystem.instance.objects.npcEntitiesList.Count}";

        level.text = $"Lv {progressData.level}";
        gold.text = $"{progressData.run.gold}";
        time.text = $"{TimeSpan.FromSeconds(Time.time - Gamesystem.instance.levelStartedTime).ToString(@"mm\:ss")}";
        killed.text = $"{progressData.run.killed}";
    }

    private void Update()
    {
        var player = Gamesystem.instance.objects.currentPlayer;

        for (int index = 0; index < skillIndicators.Length; index++)
        {
            var skillIndicator = skillIndicators[index];

            if (player.skills.Count <= index)
            {
                if (skillIndicator.gameObject.activeSelf)
                {
                    skillIndicator.gameObject.SetActive(false);
                }
            }
            else
            {
                if (!skillIndicator.gameObject.activeSelf)
                {
                    skillIndicator.gameObject.SetActive(true);
                }
                
                var skill = player.skills[index];
                var data = player.GetSkillData(skill);

                var reuseDelay = skill.GetReuseDelay(player);
                if (reuseDelay > 0)
                {
                    int maxExtraCharges = 0;
                    if (player.stats.skillExtraChargeCounts.TryGetValue(skill, out var stat))
                    {
                        maxExtraCharges = stat.GetValueInt();
                    }

                    
                    float reloadPercentage;
                    reloadPercentage = (Time.time - data.lastUse) / reuseDelay;
                    if (reloadPercentage > 1) reloadPercentage = 1;
                    
                    var wasReloaded = skillIndicators[index].IsReloaded();

                    skillIndicators[index].SetReloadPercentage(reloadPercentage);
                
                    var nowReloaded = skillIndicators[index].IsReloaded();
                    
                    if (nowReloaded && maxExtraCharges > 0 && skill.rechargeSingleCharges == ExtraChargesRechargeType.SingleCharges)
                    {
                        var currentCharges = player.GetExtraSkillCharges(skill);
                        
                        if (currentCharges < maxExtraCharges)
                        {
                            if (!wasReloaded)
                            {
                                data.lastExtraChargeUse = Time.time;
                            }
                            
                            if (data.lastExtraChargeUse + reuseDelay < Time.time)
                            {
                                player.AddExtraSkillCharges(skill, 1);
                                data.lastExtraChargeUse = Time.time;
                            }
                        }
                    }

                    if (!wasReloaded && nowReloaded)
                    {
                        if (skill.rechargeSingleCharges == ExtraChargesRechargeType.AllAtOnce)
                        {
                            player.ResetExtraSkillCharges(skill);
                        }
                        
                    }
                }
                else
                {
                    skillIndicators[index].SetReloadPercentage(1);
                }
            
                skillIndicators[index].SetSkill(skill);
                skillIndicators[index].SetChargesCount(player.GetExtraSkillCharges(skill));
            }
        }
    }

    public void SetMissionText(string text)
    {
        missionText.text = text;
    }

    public void OpenVehicleSettings()
    {
        vehicleSettingsWindow.Toggle(true);
    }

    public void OpenRewardSetWindow(List<TriggeredShopSet> rewardSet, string title, TriggeredShop triggeredShop)
    {
        vehicleSettingsWindow.OpenWindow(rewardSet, title, triggeredShop);
    }

    public void UpdateFullscreenOverlay()
    {
        if (vehicleSettingsWindow.gameObject.activeSelf)
        {
            fullscreenWindowBackground.SetActive(true);
            return;
        }
        
        fullscreenWindowBackground.SetActive(false);
    }

    public void RemoveActionsPanel()
    {
        if (currentActionsPanel != null)
        {
            if (currentActionsPanel != null)
            {
                currentActionsPanelData.abortFunction?.Invoke();
            }
            
            Destroy(currentActionsPanel);

            currentActionsPanelData = null;
        }
    }

    public void SetActionsPanel(ActionsPanel buttons, RectTransform panel)
    {
        RemoveActionsPanel();

        if (currentActionsPanelData != null && currentActionsPanelData.source == buttons.source)
        {
            currentActionsPanelData = null;
            return;
        }

        currentActionsPanel = Instantiate(actionsPanelPrefab, panel.position, Quaternion.identity, panel);
        currentActionsPanelData = buttons;
        currentActionsPanel.transform.position = Input.mousePosition;
        
        currentActionsPanel.transform.RemoveAllChildren();
        
        foreach (ActionsPanelButton button in buttons.buttons)
        {
            switch (button.buttonType)
            {
                default:
                {
                    var btnGo = Instantiate(defaultActionsPanelButton, currentActionsPanel.transform.position,
                        Quaternion.identity, currentActionsPanel.transform);

                    var btn = btnGo.GetComponent<Button>();
                    btn.onClick.AddListener(() => button.action());
                    var text = btn.GetComponentInChildren<TextMeshProUGUI>();
                    text.text = button.label;
                    break;
                }
            }
        }
        
        // set to this to show on top
        currentActionsPanel.transform.SetParent(this.transform, true);
    }

    public void SetAddingUiItem(AddingUiItem uiItem)
    {
        if (this.addingUiItem != null)
        {
            this.addingUiItem.abortCallback?.Invoke();
        }
        
        this.addingUiItem = uiItem;

        if (vehicleSettingsWindow.gameObject.activeSelf && vehicleSettingsWindow.ui != null)
        {
            foreach (var uiSlot in vehicleSettingsWindow.ui.slots)
            {
                if (addingUiItem != null)
                {
                    uiSlot.NotifyAddingModule(uiItem);
                }
                else
                {
                    uiSlot.NotifyAddingModule(null);
                }
            }
            
            foreach (var uiSlot in vehicleSettingsWindow.skillSlots)
            {
                if (addingUiItem != null)
                {
                    uiSlot.NotifyAddingItem(uiItem);
                }
                else
                {
                    uiSlot.NotifyAddingItem(null);
                }
            }
            
            foreach (var uiSlot in vehicleSettingsWindow.mutatorSlots)
            {
                if (addingUiItem != null)
                {
                    uiSlot.NotifyAddingItem(uiItem);
                }
                else
                {
                    uiSlot.NotifyAddingItem(null);
                }
            }
        }
    }

    public void OnClickToPanel()
    {
        RemoveActionsPanel();

        if (addingUiItem != null)
        {
            SetAddingUiItem(null);
        }
    }

    public void HideConfirmDialog()
    {
        if (confirmDialog != null)
        {
            Destroy(confirmDialog);
        }
    }

    public void ShowConfirmDialog(string title, string text, Action confirm, Action reject, Action abort, string confirmLabel = "OK", string rejectLabel = "Cancel")
    {
        HideConfirmDialog();

        confirmDialog = Instantiate(confirmDialogPrefab, this.transform);
        var dialog = confirmDialog.GetComponent<ConfirmDialog>();
        dialog.Initialise(title, text, confirmLabel, rejectLabel, confirm, reject, abort);
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
        }
    }

    public void ShowItemTooltip(RectTransform targetTransform, PrefabItem prefab, int level, TooltipAlign align = TooltipAlign.TopRight, TooltipType type = TooltipType.Default, List<UpgradeItem> upgrades = null, int? maxLevel = null)
    {
        var pos = targetTransform.position;

        if (align == TooltipAlign.TopRight)
        {
            pos += new Vector3(targetTransform.sizeDelta.x/2f, targetTransform.sizeDelta.y/2f, 0);
        }
        else if (align == TooltipAlign.BottomLeft)
        {
            pos += new Vector3(-targetTransform.sizeDelta.x/2f, -targetTransform.sizeDelta.y/2f, 0);
        }
        
        currentTooltip = Instantiate(moduleTooltipPrefab, pos, Quaternion.identity, targetTransform);
        currentTooltip.transform.SetParent(this.transform, true);
        var dialog = currentTooltip.GetComponent<ModuleTooltip>();
        dialog.Initialise(prefab, level, type, upgrades, maxLevel);
    }

    public void OnMissionSelectorChange(int index)
    {
        Gamesystem.instance.missionManager.ChangeMission(index - 1);
    }
    
    public void OnMissionWaweSelectorChange(int optionIndex)
    {
        var option = missionWaweSelector.options[optionIndex];
        var name = option.text;
        var wave = Gamesystem.instance.missionManager.GetCurrentFirstMission().events.First(e => e.eventName == name);
        var waveIndex = Gamesystem.instance.missionManager.GetCurrentFirstMission().events.IndexOf(wave);
        
        Gamesystem.instance.missionManager.ChangeMissionWave(waveIndex);
    }

    public void ResetRun()
    {
        Gamesystem.instance.progress.ResetRun();
    }

    public enum TooltipAlign
    {
        TopRight,
        BottomLeft
    }

    public enum TooltipType
    {
        Default,
        ExcludeTitleLogoDescription
    }
}
