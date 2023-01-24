using System;
using System.Collections.Generic;

namespace _Chi.Scripts.Mono.Ui
{
    public class ActionsPanel
    {
        public object source;
        public List<ActionsPanelButton> buttons;

        public Action abortFunction;
    }

    public class ActionsPanelButton
    {
        public ActionsPanelButtonType buttonType;
        public string label;
        public Action action;

        public ActionsPanelButton(string label, Action action, ActionsPanelButtonType buttonType=ActionsPanelButtonType.Default)
        {
            this.label = label;
            this.action = action;
            this.buttonType = buttonType;
        }
    }

    public enum ActionsPanelButtonType
    {
        Default
    }
}