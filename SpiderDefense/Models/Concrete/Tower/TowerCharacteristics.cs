using System;
using Microsoft.Xna.Framework;

namespace SpiderDefense
{
    //this class holds a collection of all upgradeable tower characteristics
    //each tower holds a reference to it's own characteristics, so they can 
    //be saved and reloaded at the beginning of a level
    class TowerCharacteristics : ICloneable
    {
        private float range = 0.0f;
        private float damage = 0.0f;
        private float arrowSpeed = 0.0f;
        private float shootFrequency = 0.0f;
        private int currentLevel = 1;
        private int upgradeFee = 2;

        #region Properties
        public float Range
        {
            get { return range; }
            set { range = value; }
        }

        public float Damage
        {
            get { return damage; }
            set { damage = value; }
        }

        public float ArrowSpeed
        {
            get { return arrowSpeed; }
            set { arrowSpeed = value; }
        }

        public float ShootFrequency
        {
            get { return shootFrequency; }
            set { shootFrequency = value; }
        }

        public int CurrentLevel
        {
            get { return currentLevel; }
            set { currentLevel = value; }
        }

        public int UpgradeFee
        {
            get { return upgradeFee; }
        }

        #endregion

        public TowerCharacteristics()
        {
            Init();
        }

        private void Init()
        {
            float factor = (currentLevel - 1) / 4f;
            float rangeFactor = 40f;
            float damageFactor = 8f;
            float arrowSpeedFactor = .9f;
            float shootFrequencyFactor = 2000f;

            range = rangeFactor + rangeFactor * (factor / 4);
            damage = damageFactor + damageFactor * factor;
            arrowSpeed = arrowSpeedFactor + arrowSpeedFactor * (factor / 2);
            shootFrequency = shootFrequencyFactor - shootFrequencyFactor * (factor / 7);

            shootFrequency = MathHelper.Clamp(shootFrequency, 1, float.MaxValue);
        }

        public void Upgrade()
        {
            currentLevel++;
            Init();
        }

        #region ICloneable Members

        public object Clone()
        {
            TowerCharacteristics newCharacteristics = new TowerCharacteristics();
            newCharacteristics.Range = this.Range;
            newCharacteristics.Damage = this.Damage;
            newCharacteristics.ArrowSpeed = this.ArrowSpeed;
            newCharacteristics.ShootFrequency = this.ShootFrequency;
            newCharacteristics.CurrentLevel = this.CurrentLevel;
            return newCharacteristics;
        }

        #endregion
    }
}
