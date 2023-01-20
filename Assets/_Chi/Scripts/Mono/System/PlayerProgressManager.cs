using _Chi.Scripts.Persistence;
using Sirenix.OdinInspector;

namespace _Chi.Scripts.Mono.System
{
    public class PlayerProgressManager : SerializedMonoBehaviour
    {
        public PlayerProgressData progressData;
        
        public void Awake()
        {
            this.progressData = LoadFile();
            
            this.InitializeProgressData();
        }

        private PlayerProgressData LoadFile()
        {
            PlayerProgressData progressData = PersistenceUtils.LoadState(PersistenceUtils.GetDefaultSaveName());

            if (progressData == null)
            {
                progressData = new PlayerProgressData();
            }

            return progressData;
        }

        private void InitializeProgressData()
        {
            //TODO restore player data
        }

        [Button]
        public void Save()
        {
            PersistenceUtils.SaveState(PersistenceUtils.GetDefaultSaveName(), this.progressData);
        }

        [Button]
        public void Reset()
        {
            PersistenceUtils.ResetState(PersistenceUtils.GetDefaultSaveName());
            this.progressData = LoadFile();
            
            this.InitializeProgressData();
        }
    }
}