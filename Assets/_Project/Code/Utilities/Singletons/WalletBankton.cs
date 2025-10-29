using _Project.Code.Core.Patterns;
using _Project.Code.Utilities.EventBus;
using UnityEngine;

public class WalletBankton : Singleton<WalletBankton>
{
    public int TotalMoney { get; private set; } = 100;
    public float CurrentResearchProgress { get; private set; } = 0;
    public float ResearchQuota { get; private set; } = 250;
    public void AddSubMoney(int amount)
    {
        TotalMoney += amount;
        EventBus.Instance.Publish<WalletUpdate>(new WalletUpdate());
    }
    public void AddResearchProgress(float amount)
    {
        CurrentResearchProgress += amount;
    }
    public void ResetResearchProgress()
    {
        CurrentResearchProgress = 0;
    }
    public void SetQuota(float quota)
    {
        ResearchQuota = quota;
    }
    public void AddToQuota(float amount)
    {
        ResearchQuota += amount;
    }
}
