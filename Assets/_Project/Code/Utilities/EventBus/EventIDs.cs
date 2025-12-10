using UnityEngine;

    public enum EventIDs
    {
        Default,

        #region EnemySounds

        EnemyBruteFootsteps,
        EnemyBruteAlert,
        EnemyBruteAttack,
        EnemyBruteIdleBreath,
        EnemyBruteHurtIdleBreath,
        EnemyDollGiggle,
        EnemyDollFootsteps,
        EnemyDollAlert,
        EnemyBeetleFootsteps,
        EnemyBeetleSqueak,
        EnemyBeetleBugNoise,

        #endregion

        #region PlayerSounds

        PlayerFootsteps,
        PlayerHurt,
        PlayerLanding,

        #endregion

        #region ItemSounds

        ItemFlashlightToggle,
        ItemMeleeStartAttack,
        ItemBaseballBatHit,
        ItemSledgehammerHit,
        ItemMacheteHit,
        ItemTranqGunShot,
        ItemTranqGunHit,
        ItemTestTubeCollect,

        #endregion

        #region EnviromentSounds

        EnvDoorSwing,
        EnvFluorescentLightSpawned,
        EnvEnterInterior,
        EnvExitInterior,
        EnvTruckDoorSwing,
        EnvDeliveryTruckStartDropping,
        EnvDeliveryTruckSpawn,
        EnvItemCollect

        #endregion
    }
