using System;
using System.Collections;
using System.Collections.Generic;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Mono.Ui;
using _Chi.Scripts.Mono.Ui.Dialogs;
using _Chi.Scripts.Mono.Ui.Tooltips;
using _Chi.Scripts.Scriptables.Dtos;
using _Chi.Scripts.Utilities;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [Required] public GameObject fullscreenWindowBackground;
    [Required] public GameObject overlay;
    [Required] public GameObject windows;

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
    [Required] public TMPro.TMP_Dropdown dropdown;

    [Required] public VehicleSettingsWindow vehicleSettingsWindow;

    [NonSerialized] public AddingUiItem addingUiItem;

    void Awake()
    {
        Gamesystem.instance.uiManager = this;
        
        dropdown.onValueChanged.AddListener(OnMissionSelectorChange);
    }

    // Start is called before the first frame update
    void Start()
    {
        dropdown.options.Add(new TMP_Dropdown.OptionData()
        {
            text = "Change mission"
        });  
        
        foreach (var mission in Gamesystem.instance.missionDatabase.missions)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData()
            {
                text = mission.name
            });    
        }
        
        StartCoroutine(UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        var waiter = new WaitForSecondsRealtime(.5f);
        while (true)
        {
            DoUpdate();
            
            yield return waiter;
        }
    }

    // Update is called once per frame
    private void DoUpdate()
    {
        var progressData = Gamesystem.instance.progress.progressData;
        
        benchmarkStats.text = $"objects: {Gamesystem.instance.objects.npcEntitiesList.Count}";

        level.text = $"Lv {progressData.level}";
        gold.text = $"{progressData.run.gold}";
        time.text = $"{TimeSpan.FromSeconds(Time.time - Gamesystem.instance.levelStartedTime).ToString(@"mm\:ss")}";
        killed.text = $"{progressData.run.killed}";
    }

    public void OpenVehicleSettings()
    {
        vehicleSettingsWindow.Toggle();
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
        currentActionsPanel.transform.parent = this.transform;
    }

    public void SetAddingUiItem(AddingUiItem module)
    {
        if (this.addingUiItem != null)
        {
            this.addingUiItem.abortCallback?.Invoke();
        }
        
        this.addingUiItem = module;

        if (vehicleSettingsWindow.gameObject.activeSelf && vehicleSettingsWindow.ui != null)
        {
            foreach (var uiSlot in vehicleSettingsWindow.ui.slots)
            {
                if (addingUiItem != null)
                {
                    uiSlot.NotifyAddingModule(module);
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
                    uiSlot.NotifyAddingItem(module);
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
                    uiSlot.NotifyAddingItem(module);
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
        Debug.Log("on click to panel");

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

    public void ShowModuleTooltip(RectTransform targetTransform, PrefabItem prefab, int level)
    {
        var pos = targetTransform.position;
        pos += new Vector3(targetTransform.sizeDelta.x/2f, targetTransform.sizeDelta.y/2f, 0);
        
        currentTooltip = Instantiate(moduleTooltipPrefab, pos, Quaternion.identity, targetTransform);
        currentTooltip.transform.parent = this.transform;
        var dialog = currentTooltip.GetComponent<ModuleTooltip>();
        dialog.Initialise(prefab, level);
    }

    public void OnMissionSelectorChange(int index)
    {
        Gamesystem.instance.missionManager.ChangeMission(index - 1);
    }
}
