namespace _Chi.Scripts.Statistics
{
    public class ProjectileInstanceStats
    {
        public int piercedEnemies = 0;
        public bool active;

        public void Reset()
        {
            piercedEnemies = 0;
            active = true;
        }
    }
}