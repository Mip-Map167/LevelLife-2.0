namespace LevelLife
{
    public class Player
    {
        public string Name { get; set; }
        public string Goal { get; set; } = "Стать лучше";
        public int Level { get; private set; }
        public int CurrentXP { get; private set; }
        public int TotalXP { get; private set; }

        public int XpToNextLevel => 100 * Level;

        public Player(string name)
        {
            Name = name;
            Level = 1;
            CurrentXP = 0;
            TotalXP = 0;
        }

        public void AddXP(int xp)
        {
            if (xp <= 0) return;
            CurrentXP += xp;
            TotalXP += xp;

            while (CurrentXP >= XpToNextLevel)
            {
                CurrentXP -= XpToNextLevel;
                Level++;
            }
        }

        public int GetProgressPercent()
        {
            if (XpToNextLevel == 0) return 100;
            return (int)((double)CurrentXP / XpToNextLevel * 100);
        }

        public int GetRemainingXP() => XpToNextLevel - CurrentXP;

        public void Reset()
        {
            Level = 1;
            CurrentXP = 0;
            TotalXP = 0;
        }
    }
}