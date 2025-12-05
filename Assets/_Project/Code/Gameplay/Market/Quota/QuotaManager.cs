using _Project.Code.Core.Patterns;
using _Project.Code.Core.SaveSystem;
using _Project.Code.Gameplay.Player.MiscPlayer;
using _Project.Code.Utilities.EventBus;
using Unity.Netcode;
using UnityEngine;

namespace _Project.Code.Gameplay.Market.Quota
{
    public class QuotaManager : NetworkSingleton<QuotaManager>
    {
        protected override bool AutoSpawn => false;

        //Current quota. (Player Goal)
        private NetworkVariable<float> CurrentQuota = new NetworkVariable<float>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        //Current Quota Progress. (Player Progress)
        private NetworkVariable<float> CurrentQuotaProgress = new();

        //How much players have progressed in a day. Only adds to progress if day is "successful"
        private NetworkVariable<float> DaysQuotaProgress = new (0);

        //Current Day of Quota. Resets after quota timelimit
        private NetworkVariable<int> CurrentDayOfQuota = new(0);

        //How many quotas players have succesfully completed without fail
        private NetworkVariable<int> QuotasPassed = new(0);

        private NetworkVariable<bool> BeforeNewRun = new (true);
        //SO for quotas. Must assign in inspector
        [field: SerializeField] public QuotaDataSO QuotaSO { get; private set; }

        //Quickly gets if the players have passed the quota requirements
        public bool HasReachedQuota => CurrentQuotaProgress.Value >= CurrentQuota.Value;

        //Gets the player progress as a percentage. Players only see progress this way
        public float QuotaProgressPercentage => CurrentQuotaProgress.Value / CurrentQuota.Value;
        public float DayProgressPercentage => DaysQuotaProgress.Value / CurrentQuota.Value;

        //Gets total days. Current days of quota + days from previous quotas
        public int TotalDays => CurrentDayOfQuota.Value + (QuotasPassed.Value * (int)QuotaSO.DaysInAQuota);

        #region Respond To NetVar Changes

        public void HandleQuotaChanged(float oldAmount, float newAmount)
        {

        }

        public void HandleResearchProgressChanged(float oldAmount, float newAmount)
        {

        }

        public void HandleDaysProgressChanged(float oldAmount, float newAmount)
        {

        }

        public void HandleDaysChanged(int oldAmount, int newAmount)
        {

        }

        public void HandleQuotasPassedChanged(int oldAmount, int newAmount)
        {

        }

        #endregion

        #region Initialization

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            CurrentQuota.OnValueChanged += HandleQuotaChanged;
            CurrentQuotaProgress.OnValueChanged += HandleResearchProgressChanged;
            DaysQuotaProgress.OnValueChanged += HandleDaysProgressChanged;
            CurrentDayOfQuota.OnValueChanged += HandleDaysChanged;
            QuotasPassed.OnValueChanged += HandleQuotasPassedChanged;
            if (IsServer)
            {
                // Force the object to NOT be destroyed when the scene unloads.
                NetworkObject.DestroyWithScene = false;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            CurrentQuota.OnValueChanged -= HandleQuotaChanged;
            CurrentQuotaProgress.OnValueChanged -= HandleResearchProgressChanged;
            DaysQuotaProgress.OnValueChanged -= HandleDaysProgressChanged;
            CurrentDayOfQuota.OnValueChanged -= HandleDaysChanged;
            QuotasPassed.OnValueChanged -= HandleQuotasPassedChanged;
        }

        protected override void Awake()
        {
            base.Awake();

        }

        private void OnEnable()
        {
            EventBus.Instance.Subscribe<DayStartEvent>(this, HandleDayStarted);
            EventBus.Instance.Subscribe<SuccessfulDayEvent>(this, HandleSuccessfulDay);
            EventBus.Instance.Subscribe<OnEnterHubEvent>(this, HandleEnterHub);
        }

        private void OnDisable()
        {
            EventBus.Instance?.Unsubscribe<DayStartEvent>(this);
            EventBus.Instance?.Unsubscribe<SuccessfulDayEvent>(this);
            EventBus.Instance?.Unsubscribe<OnEnterHubEvent>(this);
        }

        #endregion

        #region New Days/Quotas/Hub Scene

        public void HandleDayStarted(DayStartEvent dayStartEvent)
        {
            //Increase days elapsed
            if (!IsServer) return;
            RequestResetDayProgressServerRpc();
            RequestIncreaseDaysServerRpc();
        }

        public void HandleSuccessfulDay(SuccessfulDayEvent dayStartEvent)
        {
            //Add progress towards quota
            if (!IsServer) return;
            RequestAddResearchProgressServerRpc(DaysQuotaProgress.Value);
        }

        public void HandleEnterHub(OnEnterHubEvent onEnterHubEvent)
        {
            //When loading hub, should get if its before a new run (no quota started whatsoever)
            //maybe check days passed instead of passing through
            if (!IsServer) return;
            if (BeforeNewRun.Value)
            {
                HandleNewQuota(true);
                RequestChangeBeforeRunStateServerRpc(false);
                return;
            }

            //if it isnt a new run (and therefore a new day) check if quota time limit reached 
            if (CurrentDayOfQuota.Value >= QuotaSO.DaysInAQuota)
            {
                QuotaTimeReached();
            }

            RequestResetDayProgressServerRpc();
        }

        public void HandleNewQuota(bool isFirstQuota)
        {
            if (!IsServer) return;
            //if its a brand new quota (failed, just started) set quota to a starting value
            //if not, just add to the quota
            if (isFirstQuota)
            {
                RequestSetQuotaServerRpc(QuotaSO.RandomStartQuota);
            }
            else
            {
                RequestAddToQuotaServerRpc(QuotaSO.RandomIncreaseQuota);
            }
        }

        #endregion

        [ServerRpc(RequireOwnership = false)]
        private void RequestChangeBeforeRunStateServerRpc(bool Before)
        {
            BeforeNewRun.Value = Before;
        }
        #region Quota Finished Logic

        private void HandleFailedQuota()
        {
            if (!IsServer) return;
            //fails get a new quota from start. Publish the fail for other systems. 
            //Resets QuotasPassed
            
            RequestResetQuotasPassedServerRpc();
            HandleNewQuota(true);
            EventBus.Instance.Publish<QuotaFailedEvent>(new QuotaFailedEvent());
        }

        private void HandleSuccessfulQuota()
        {
            if (!IsServer) return;
            //Add to quota, and increment quotas passed, inform other systems
            
            HandleNewQuota(false);
            RequestIncreaseQuotasPassedServerRpc();
            EventBus.Instance.Publish<QuotaSuccessEvent>(new QuotaSuccessEvent());
        }

        private void QuotaTimeReached()
        {
            if (!IsServer) return;
            //End of a quota regardless of success should reset progress and days;
            RequestResetDaysServerRpc();
            RequestResetResearchProgressServerRpc();

            //check if quota succeeds
            if (HasReachedQuota)
            {
                HandleSuccessfulQuota();
            }
            else
            {
                HandleFailedQuota();
            }
        }

        #endregion

        #region Research

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddResearchProgressServerRpc(float amount)
        {
            CurrentQuotaProgress.Value += amount;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetResearchProgressServerRpc()
        {
            CurrentQuotaProgress.Value = 0;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddDayProgressServerRpc(float amount)
        {
            DaysQuotaProgress.Value += amount;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetDayProgressServerRpc()
        {
            DaysQuotaProgress.Value = 0;
        }

        #endregion

        #region Quota

        [ServerRpc(RequireOwnership = false)]
        public void RequestSetQuotaServerRpc(float quota)
        {
            CurrentQuota.Value = quota;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestAddToQuotaServerRpc(float amount)
        {
            CurrentQuota.Value += amount;
        }

        #endregion

        #region Days/Quotas Elapsed

        [ServerRpc(RequireOwnership = false)]
        public void RequestIncreaseDaysServerRpc()
        {
            CurrentDayOfQuota.Value++;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetDaysServerRpc()
        {
            CurrentDayOfQuota.Value = 0;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestIncreaseQuotasPassedServerRpc()
        {
            QuotasPassed.Value++;
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestResetQuotasPassedServerRpc()
        {
            QuotasPassed.Value = 0;
        }

        #endregion

        #region Save System

        // Getters for SaveManager
        public float GetCurrentQuota() => CurrentQuota.Value;
        public float GetCurrentQuotaProgress() => CurrentQuotaProgress.Value;
        public float GetDaysQuotaProgress() => DaysQuotaProgress.Value;
        public int GetCurrentDayOfQuota() => CurrentDayOfQuota.Value;
        public int GetQuotasPassed() => QuotasPassed.Value;
        public bool GetBeforeNewRun() => BeforeNewRun.Value;

        // Load state from save data (server only)
        public void LoadFromSave(SaveData data)
        {
            if (!IsServer) return;
            CurrentQuota.Value = data.CurrentQuota;
            CurrentQuotaProgress.Value = data.CurrentQuotaProgress;
            DaysQuotaProgress.Value = data.DaysQuotaProgress;
            CurrentDayOfQuota.Value = data.CurrentDayOfQuota;
            QuotasPassed.Value = data.QuotasPassed;
            BeforeNewRun.Value = data.BeforeNewRun;
        }

        // Reset to fresh game state (server only)
        public void ResetToDefaults()
        {
            if (!IsServer) return;
            CurrentQuota.Value = 0;
            CurrentQuotaProgress.Value = 0;
            DaysQuotaProgress.Value = 0;
            CurrentDayOfQuota.Value = 0;
            QuotasPassed.Value = 0;
            BeforeNewRun.Value = true;
        }

        #endregion
    }

    #region Events In
    public struct DayStartEvent : IEvent
    {
    
    }

    public struct SuccessfulDayEvent : IEvent
    {
    
    }

    public struct OnEnterHubEvent : IEvent
    {
    }
    #endregion

    #region Events Out
    public struct QuotaFailedEvent: IEvent
    {

    }

    public struct QuotaSuccessEvent : IEvent
    {
    
    }
    #endregion
}