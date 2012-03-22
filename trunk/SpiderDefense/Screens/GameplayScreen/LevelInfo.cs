using System;

namespace SpiderDefense
{
    class LevelInfo
    {
        private int currentGameLevel;
        private int playerPoints;
        private int missedEnemies;
        private int enemyCount;
        private TimeSpan timeUntilNextSwarm;
        private int maxMissedEnemies = 6;

        public int CurrentGameLevel
        {
            get { return currentGameLevel; }
            set { currentGameLevel = value; }
        }

        public int PlayerPoints
        {
            get { return playerPoints; }
            set { playerPoints = value; }
        }

        public int MissedEnemies
        {
            get { return missedEnemies; }
            set { missedEnemies = value; }
        }

        public int EnemyCount
        {
            get { return enemyCount; }
            set { enemyCount = value; }
        }

        public TimeSpan TimeToNextSwarm
        {
            get { return timeUntilNextSwarm; }
            set { timeUntilNextSwarm = value; }
        }

        public bool GameOver
        {
            get { return missedEnemies >= maxMissedEnemies; }
        }

        public int MaxMissedEnemies
        {
            get { return maxMissedEnemies; }
        }

        public void DeductPoints(int points)
        {
            if (playerPoints >= points)
                playerPoints -= points;
        }
    }
}
