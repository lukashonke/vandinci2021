using System;
using _Chi.Scripts.Mono.Modules;
using _Chi.Scripts.Scriptables.Dtos;

namespace _Chi.Scripts.Mono.Ui
{
    public class AddingUiItem
    {
        public PrefabItem prefab;

        public Module prefabModule;

        public int level = 1;

        public Action finishCallback;

        public Action abortCallback;

        public AddingModuleInfoType type;
    }

    public enum AddingModuleInfoType
    {
        Add,
        Move
    }
}