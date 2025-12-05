using System;
using System.IO;
using _Project.Code.Core.Patterns;
using _Project.Code.Gameplay.Market.Quota;
using _Project.Code.Utilities.Singletons;
using UnityEngine;

namespace _Project.Code.Core.SaveSystem
{
    public class SaveManager : Singleton<SaveManager>
    {
        private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        protected override bool PersistBetweenScenes => true;

        public bool HasSave()
        {
            return File.Exists(SavePath);
        }

        public void Save()
        {
            try
            {
                var quotaManager = QuotaManager.Instance;
                var wallet = WalletBankton.Instance;

                if (quotaManager == null)
                {
                    Debug.LogWarning("[SaveManager] QuotaManager not found, cannot save.");
                    return;
                }

                var data = new SaveData
                {
                    CurrentQuota = quotaManager.GetCurrentQuota(),
                    CurrentQuotaProgress = quotaManager.GetCurrentQuotaProgress(),
                    DaysQuotaProgress = quotaManager.GetDaysQuotaProgress(),
                    CurrentDayOfQuota = quotaManager.GetCurrentDayOfQuota(),
                    QuotasPassed = quotaManager.GetQuotasPassed(),
                    BeforeNewRun = quotaManager.GetBeforeNewRun(),
                    PlayerMoney = wallet != null ? wallet.GetMoney() : 100,
                    LastSaveTime = DateTime.Now.ToString("O"),
                    SaveVersion = SaveData.CURRENT_SAVE_VERSION
                };

                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(SavePath, json);
                Debug.Log($"[SaveManager] Game saved to {SavePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {e.Message}");
            }
        }

        public SaveData Load()
        {
            if (!HasSave())
            {
                Debug.Log("[SaveManager] No save file found.");
                return null;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                if (data == null)
                {
                    Debug.LogWarning("[SaveManager] Failed to parse save file.");
                    return null;
                }

                if (data.SaveVersion != SaveData.CURRENT_SAVE_VERSION)
                {
                    Debug.LogWarning($"[SaveManager] Save version mismatch. Expected {SaveData.CURRENT_SAVE_VERSION}, got {data.SaveVersion}. Starting fresh.");
                    return null;
                }

                Debug.Log($"[SaveManager] Game loaded from {SavePath}");
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load game: {e.Message}");
                return null;
            }
        }

        public void DeleteSave()
        {
            if (HasSave())
            {
                try
                {
                    File.Delete(SavePath);
                    Debug.Log("[SaveManager] Save file deleted.");
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to delete save: {e.Message}");
                }
            }
        }
    }
}
