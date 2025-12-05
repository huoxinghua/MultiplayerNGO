using System;

namespace _Project.Code.Core.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        // Quota System
        public float CurrentQuota;
        public float CurrentQuotaProgress;
        public float DaysQuotaProgress;
        public int CurrentDayOfQuota;
        public int QuotasPassed;
        public bool BeforeNewRun;

        // Economy
        public int PlayerMoney;

        // Meta
        public string LastSaveTime;
        public int SaveVersion;

        public const int CURRENT_SAVE_VERSION = 1;

        public static SaveData CreateDefault()
        {
            return new SaveData
            {
                CurrentQuota = 0,
                CurrentQuotaProgress = 0,
                DaysQuotaProgress = 0,
                CurrentDayOfQuota = 0,
                QuotasPassed = 0,
                BeforeNewRun = true,
                PlayerMoney = 100,
                LastSaveTime = DateTime.Now.ToString("O"),
                SaveVersion = CURRENT_SAVE_VERSION
            };
        }
    }
}
