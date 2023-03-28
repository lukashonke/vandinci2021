namespace _Chi.Scripts.Mono.Common
{
    public enum Rarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
    
    public static class RarityExtensions
    {
        public static string GetColor(this Rarity rarity)
        {
            return rarity switch
            {
                Rarity.Common => "#B3B3B3", // light grey
                Rarity.Rare => "#FFD700", // yellow
                Rarity.Epic => "#A020F0", // blue-purple
                Rarity.Legendary => "#FFA500", // orange
                _ => "#FFFFFF"
            };
        }
    }
}