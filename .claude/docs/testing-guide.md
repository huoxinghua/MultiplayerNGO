# Multiplayer Testing Guide

**Version:** 1.0
**Last Updated:** 2025-11-07
**Project:** MultiplayerNGO - Unity Netcode for GameObjects

This guide provides comprehensive testing procedures for validating multiplayer functionality during and after the refactoring process.

---

## Table of Contents

1. [Test Environment Setup](#test-environment-setup)
2. [Basic Network Tests](#basic-network-tests)
3. [System-Specific Tests](#system-specific-tests)
4. [Integration Tests](#integration-tests)
5. [Stress Tests](#stress-tests)
6. [Regression Testing](#regression-testing)
7. [Test Automation](#test-automation)

---

## Test Environment Setup

### Option 1: Local Multiplayer (Recommended for Development)

**Build two standalone builds:**

1. **Create Development Build:**
   - File → Build Settings
   - Check "Development Build"
   - Check "Autoconnect Profiler" (optional)
   - Build to `Builds/TestBuild1/`

2. **Run Multiple Instances:**
   - Open `TestBuild1.exe`
   - Open `TestBuild1.exe` again (second window)
   - One acts as Host, one as Client

**Advantages:**
- Fast iteration
- Easy debugging
- No network latency issues

**Disadvantages:**
- Doesn't test real network conditions
- Performance impact running multiple instances

---

### Option 2: Editor + Build (Faster Iteration)

**Setup:**
1. Build standalone executable
2. Run build as Client
3. Run Unity Editor as Host

**Steps:**
1. File → Build Settings → Build
2. Run build exe
3. In Unity Editor: Play
4. Editor: Start Host
5. Build: Start Client and join

**Advantages:**
- Fast iteration (no rebuilding for changes)
- Easy debugging in Editor
- Can use Unity Editor tools

**Disadvantages:**
- Performance differences between Editor and Build
- May miss build-specific bugs

---

### Option 3: Multiple Machines (Most Realistic)

**Setup:**
1. Build game
2. Copy to second computer on same network
3. Run Host on Computer 1
4. Run Client on Computer 2

**Advantages:**
- Real network conditions
- True multiplayer experience
- Performance accurate

**Disadvantages:**
- Slow iteration
- Harder to debug
- Requires second machine

---

### Network Conditions Simulation

**For realistic testing, simulate poor network conditions:**

**Unity Profiler:**
- Window → Analysis → Profiler
- Network Messages tab
- Can see traffic in real-time

**Third-Party Tools:**
- Clumsy (Windows): http://jagt.github.io/clumsy/
- Network Link Conditioner (Mac)
- Linux: tc (traffic control)

**Test with:**
- 50ms latency
- 1% packet loss
- 500kbps bandwidth limit

---

## Basic Network Tests

### Test 1: Connection Flow

**Purpose:** Verify basic connection/disconnection

**Steps:**
1. Launch Host instance
2. Click "Host" button
3. Verify: Console shows "Server started"
4. Verify: NetworkManager shows "IsServer: True"
5. Launch Client instance
6. Click "Join" button
7. Verify: Client connects successfully
8. Verify: Host sees "Client Connected" message
9. Client: Click "Disconnect"
10. Verify: Host sees "Client Disconnected"
11. Host: Click "Stop Host"
12. Verify: Clean shutdown, no errors

**Expected Results:**
- ✅ Clean connection
- ✅ Both show connected state
- ✅ Clean disconnection
- ✅ No console errors

**Common Issues:**
- ❌ "Connection Failed" - Check firewall, ports
- ❌ Timeout - Check IP address, relay setup

---

### Test 2: Player Spawning

**Purpose:** Verify players spawn correctly for all clients

**Steps:**
1. Start Host
2. Verify: Host player spawns
3. Verify: Can control host player (WASD movement)
4. Start Client
5. Verify: Client player spawns
6. Verify: Can control client player
7. **Host perspective:** Verify host sees 2 players (self + client)
8. **Client perspective:** Verify client sees 2 players (self + host)
9. Start second Client
10. Verify: All 3 clients see 3 players

**Expected Results:**
- ✅ Each client spawns own player
- ✅ Each client sees all other players
- ✅ Player positions sync in real-time
- ✅ Player name tags display correctly

**Common Issues:**
- ❌ Only see own player - Check NetworkObject spawn
- ❌ Duplicate players - Check spawning logic
- ❌ Players not moving - Check NetworkTransform

---

### Test 3: Basic Movement Sync

**Purpose:** Verify player movement syncs across clients

**Steps:**
1. Start Host and Client
2. **Host:** Move forward 10 units (W key)
3. **Client:** Verify host player moves on client's screen
4. **Client:** Move left 5 units (A key)
5. **Host:** Verify client player moves on host's screen
6. **Both:** Run in circles
7. **Both:** Verify smooth movement (no stuttering)
8. **Both:** Jump simultaneously
9. **Both:** Verify jumps appear on both screens

**Expected Results:**
- ✅ Movement syncs within 100ms
- ✅ Smooth interpolation
- ✅ No rubber-banding
- ✅ Jump animations sync

**Common Issues:**
- ❌ Jittery movement - Check NetworkTransform settings
- ❌ Delayed movement - Check tick rate, latency
- ❌ Teleporting - Check interpolation settings

---

## System-Specific Tests

### Economy System Tests

#### Test 4: Money Synchronization

**Purpose:** Verify WalletBankton syncs money across all clients

**Precondition:** WalletBankton migrated to NetworkSingleton (Phase 2)

**Steps:**
1. Start Host and Client
2. **Note:** Both start with $100 (default)
3. **Host:** Verify UI shows $100
4. **Client:** Verify UI shows $100
5. **Host:** Earn money (kill enemy, complete objective, etc.)
6. **Host:** Verify money increases
7. **Client:** Verify money increases on client's UI
8. **Client:** Spend money (buy item, etc.)
9. **Client:** Verify money decreases
10. **Host:** Verify money decreases on host's UI
11. **Both:** Verify final money matches

**Expected Results:**
- ✅ Money syncs within 1 second
- ✅ All clients show same value
- ✅ No money duplication
- ✅ EventBus triggers UI updates

**Common Issues:**
- ❌ Money different on each client - NetworkVariable not syncing
- ❌ Money only updates on server - OnValueChanged not subscribed

---

### Enemy System Tests

#### Test 5: Enemy Spawning

**Purpose:** Verify enemies spawn only once (server authority)

**Precondition:** EnemySpawnManager fixed (Phase 1)

**Steps:**
1. Start Host (alone)
2. Trigger enemy spawn (enter area, start wave, etc.)
3. **Host:** Count enemies spawned
4. Start Client
5. **Client:** Count enemies visible
6. Verify: Same number on both
7. **Host:** Trigger another spawn wave
8. **Both:** Count total enemies
9. Verify: Counts match

**Expected Results:**
- ✅ Enemies spawn once (not duplicated per client)
- ✅ All clients see same enemies
- ✅ Enemy NetworkObjects have valid IDs
- ✅ No spawn errors in console

**Common Issues:**
- ❌ Duplicate enemies - Check IsServer guard
- ❌ Enemies only on host - Check NetworkObject.Spawn()
- ❌ Late-join client sees no enemies - Check spawn timing

---

#### Test 6: Enemy AI Synchronization

**Purpose:** Verify enemy AI state/animations sync

**Precondition:** State machines synced (Phase 3)

**Steps:**
1. Start Host and Client
2. Spawn beetle enemy
3. **Both:** Observe beetle wandering
4. **Client:** Verify beetle is animated (not T-pose)
5. **Host:** Approach beetle
6. **Host:** Verify beetle follows
7. **Client:** Verify beetle follows host
8. **Client:** Verify running animation plays
9. **Host:** Run away from beetle
10. **Client:** Verify beetle stops following
11. **Client:** Verify returns to wander state
12. **Either:** Kill beetle
13. **Both:** Verify death animation plays

**Expected Results:**
- ✅ All clients see same enemy state
- ✅ Animations play on all clients
- ✅ State transitions sync within 200ms
- ✅ No frozen/T-pose enemies

**Common Issues:**
- ❌ Frozen enemies - State machine not synced
- ❌ Different states on each client - NetworkVariable not updating
- ❌ No animations - UpdateAnimation not called

---

#### Test 7: Enemy Combat

**Purpose:** Verify enemy health syncs and damage applies correctly

**Steps:**
1. Start Host and Client
2. Spawn enemy with 100 HP
3. **Host:** Deal 25 damage to enemy
4. **Host:** Verify enemy HP = 75
5. **Client:** Verify enemy HP = 75 (check health bar)
6. **Client:** Deal 25 damage to same enemy
7. **Client:** Verify enemy HP = 50
8. **Host:** Verify enemy HP = 50
9. **Both:** Attack simultaneously until dead
10. **Both:** Verify enemy dies at HP = 0 (not negative)
11. **Both:** Verify death animation plays
12. **Both:** Verify enemy despawns

**Expected Results:**
- ✅ Health syncs across all clients
- ✅ Damage applies regardless of who attacks
- ✅ No negative health values
- ✅ Death triggers on all clients

**Common Issues:**
- ❌ Health desync - Missing ServerRpc
- ❌ Double damage - Check RPC pattern
- ❌ Negative health - Missing validation

---

### Item System Tests

#### Test 8: Item Pickup

**Purpose:** Verify item pickup with no duplication or race conditions

**Precondition:** Item pickup refactored (Phase 3)

**Steps:**
1. Start Host and Client
2. Place item in world
3. **Host:** Approach item, press E
4. **Host:** Verify item disappears
5. **Host:** Verify item appears in inventory
6. **Client:** Verify item disappeared on their screen
7. **Client:** Verify item in host's inventory (if visible)
8. **Now test Client pickup:**
9. Place another item
10. **Client:** Approach and pickup
11. **Client:** Verify item in own inventory
12. **Host:** Verify item disappeared on host's screen

**Expected Results:**
- ✅ Item disappears from world on all clients
- ✅ Item appears in correct player's inventory
- ✅ No duplication
- ✅ Visual feedback (sound, UI message)

**Common Issues:**
- ❌ Item duplicated - Race condition in pickup
- ❌ Item disappears but not in inventory - Sync issue
- ❌ Can pickup from far away - Missing distance check

---

#### Test 9: Item Pickup Race Condition

**Purpose:** Verify no duplication when both players grab simultaneously

**This is a CRITICAL test!**

**Steps:**
1. Start Host and Client
2. Place item between both players
3. **Both:** Stand next to item (equal distance)
4. **On count of 3, BOTH press E simultaneously:**
   - 1... 2... 3... PRESS!
5. **Verify:** Only ONE player gets item
6. **Verify:** Other player sees "already picked up" or similar
7. **Verify:** Item count = 1 (not 2)
8. **Repeat test 5 times to ensure consistency**

**Expected Results:**
- ✅ Only one player gets item
- ✅ No duplication
- ✅ Server resolves race correctly
- ✅ Feedback to losing player

**Common Issues:**
- ❌ Both players get item - Missing atomic check
- ❌ Neither gets item - Overly strict validation
- ❌ Intermittent duplication - Race condition

---

#### Test 10: Item Usage

**Purpose:** Verify item usage syncs (weapon swing, flashlight toggle, etc.)

**Steps:**
1. Start Host and Client
2. **Host:** Pick up baseball bat
3. **Host:** Swing bat (left click)
4. **Host:** Verify swing animation plays locally
5. **Client:** Verify sees host swing bat
6. **Client:** Verify swing animation/sound
7. **Host:** Hit enemy with bat
8. **Client:** Verify enemy takes damage
9. **Client:** Pick up flashlight
10. **Client:** Toggle flashlight on
11. **Host:** Verify sees client's flashlight light
12. **Client:** Toggle off
13. **Host:** Verify light disappears

**Expected Results:**
- ✅ Usage triggers on all clients
- ✅ Animations sync
- ✅ Effects sync (damage, light, etc.)
- ✅ Durability decreases properly

**Common Issues:**
- ❌ Only user sees action - Missing ClientRpc
- ❌ Delayed action - High latency or missing prediction
- ❌ Duplicate effects - ClientRpc called on owner

---

### Level Generation Tests

#### Test 11: Dungeon Sync

**Purpose:** Verify level generates identically on all clients

**Precondition:** LevelNetworkSync fixed (Phase 3)

**Steps:**
1. Start Host
2. Host generates level (procedural dungeon)
3. **Host:** Note layout (room positions, connections)
4. Start Client
5. **Client:** Wait for level sync
6. **Client:** Compare layout to host
7. Walk through level on both
8. Verify: Walls, floors, doors match
9. **Host:** Open a door
10. **Client:** Verify same door opens
11. Place marker in specific room on host
12. **Client:** Navigate to same room
13. Verify: Marker in same location

**Expected Results:**
- ✅ Level layout identical on all clients
- ✅ No missing walls/floors
- ✅ Nav mesh synced
- ✅ Interactive objects (doors) work

**Common Issues:**
- ❌ Different layouts - Seed not synced
- ❌ Missing geometry - Data not fully synced
- ❌ Can walk through walls - Colliders not synced

---

## Integration Tests

### Test 12: Full Gameplay Loop

**Purpose:** Verify complete gameplay from start to finish

**Steps:**
1. Start Host and Client
2. Both spawn in lobby
3. Host starts game
4. Both teleport to dungeon
5. Dungeon generates and syncs
6. Both explore dungeon
7. Encounter enemies
8. Defeat enemies (test combat sync)
9. Collect loot (test item pickup)
10. Earn money (test economy sync)
11. Buy items from shop
12. Complete objectives
13. Return to lobby
14. Verify: Stats synced, progression saved

**Expected Results:**
- ✅ Smooth flow from start to finish
- ✅ No disconnects or errors
- ✅ All systems work together
- ✅ Progress tracked correctly

---

### Test 13: Late-Join Scenario

**Purpose:** Verify late-joining client sees correct state

**Steps:**
1. Start Host
2. Host plays for 5 minutes:
   - Generates level
   - Spawns enemies
   - Picks up items
   - Earns money
3. **Now:** Start Client and join
4. **Client:** Verify sees current level
5. **Client:** Verify sees existing enemies
6. **Client:** Verify sees host's inventory items
7. **Client:** Check money matches
8. **Client:** Play normally
9. Verify: Client can interact with everything

**Expected Results:**
- ✅ Late joiner syncs to current state
- ✅ All NetworkVariables sync
- ✅ Level appears correctly
- ✅ Can interact normally

**Common Issues:**
- ❌ Missing level - Sync not triggered
- ❌ Wrong money value - NetworkVariable not synced
- ❌ Invisible enemies - Spawned before client joined

---

## Stress Tests

### Test 14: Multiple Clients

**Purpose:** Test with maximum expected player count

**Steps:**
1. Start Host
2. Start Client 1
3. Start Client 2
4. Start Client 3
5. (Add up to max players, e.g., 4-8)
6. All clients move simultaneously
7. Spawn enemies (stress enemy system)
8. All clients attack same enemy
9. Drop/pickup items rapidly
10. Monitor performance and errors

**Expected Results:**
- ✅ No disconnects
- ✅ Acceptable FPS on all clients
- ✅ No desync issues
- ✅ Correct state on all clients

**Monitor:**
- FPS (should stay above 30)
- Network traffic (check Profiler)
- Console errors
- Memory usage

---

### Test 15: Rapid State Changes

**Purpose:** Stress test NetworkVariable updates

**Steps:**
1. Start Host and Client
2. **Host:** Rapidly change money (script or debug command):
   - Add $10, 50 times per second
3. **Client:** Monitor money UI
4. Verify: Updates smoothly, no errors
5. Spawn 10 enemies
6. Change all enemy states rapidly
7. Verify: No errors, animations keep up

**Expected Results:**
- ✅ System handles rapid updates
- ✅ No packet overflow errors
- ✅ No visual glitches

---

## Regression Testing

### After Each Phase

Run this checklist after completing each refactoring phase:

**Phase 1 Checklist:**
- [ ] Connection/disconnection works
- [ ] Player spawning works
- [ ] Enemy spawning works (no duplicates)
- [ ] Basic combat works
- [ ] No commented code remains

**Phase 2 Checklist:**
- [ ] All Phase 1 tests pass
- [ ] Money syncs across clients
- [ ] Research progress syncs
- [ ] Singletons accessible
- [ ] No "instance is null" errors

**Phase 3 Checklist:**
- [ ] All Phase 1-2 tests pass
- [ ] Item pickup works (no duplication)
- [ ] Enemy AI visible on all clients
- [ ] Animations play correctly
- [ ] Level generation syncs

**Phase 4 Checklist:**
- [ ] All previous tests pass
- [ ] Distance checks work (can't pickup from far)
- [ ] Cooldowns enforced
- [ ] Speed hacking prevented
- [ ] No invalid operations allowed

**Phase 5 Checklist:**
- [ ] All previous tests pass
- [ ] Performance acceptable
- [ ] No Debug.Log spam
- [ ] Clean, documented code

---

## Test Automation

### Unity Test Framework

**Create automated tests:**

**File:** `Assets/_Project/Tests/NetworkTests.cs`

```csharp
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class NetworkTests
{
    [UnityTest]
    public IEnumerator TestMoneySync()
    {
        // Start server
        NetworkManager.Singleton.StartHost();
        yield return new WaitForSeconds(1f);

        // Add money on server
        int startMoney = WalletBankton.Instance.TotalMoney;
        WalletBankton.Instance.AddSubMoneyServerRpc(50);

        yield return new WaitForSeconds(0.5f);

        // Verify money increased
        Assert.AreEqual(startMoney + 50, WalletBankton.Instance.TotalMoney);

        // Cleanup
        NetworkManager.Singleton.Shutdown();
    }

    [UnityTest]
    public IEnumerator TestEnemySpawnOnlyOnce()
    {
        // Start server
        NetworkManager.Singleton.StartHost();
        yield return new WaitForSeconds(1f);

        // Spawn enemy
        GameObject enemy = GameObject.Instantiate(enemyPrefab);
        enemy.GetComponent<NetworkObject>().Spawn();

        yield return new WaitForSeconds(0.5f);

        // Count enemies in scene
        int enemyCount = GameObject.FindObjectsOfType<EnemyAI>().Length;
        Assert.AreEqual(1, enemyCount, "Enemy should spawn only once");

        // Cleanup
        NetworkManager.Singleton.Shutdown();
    }
}
```

---

## Test Results Documentation

### Create Test Report Template

**File:** `.claude/progress/test-report-template.md`

```markdown
# Test Report - [Phase X] - [Date]

## Test Environment
- Unity Version:
- Netcode Version:
- Platform: Windows/Mac/Linux
- Number of Clients Tested:

## Test Results Summary
- Total Tests: X
- Passed: X
- Failed: X
- Skipped: X

## Detailed Results

### Test 1: [Name]
**Status:** PASS / FAIL
**Notes:** ...

### Test 2: [Name]
**Status:** PASS / FAIL
**Notes:** ...

## Issues Found
1. [Issue description]
   - **Severity:** Critical / High / Medium / Low
   - **Reproduction Steps:** ...
   - **Expected:** ...
   - **Actual:** ...

## Conclusion
[Summary of testing phase]
```

---

## Continuous Testing

**Best Practices:**

1. **Test after every major change**
2. **Test with 2+ clients minimum**
3. **Test late-join scenarios regularly**
4. **Document all failures**
5. **Retest after fixes**

**Daily Testing Routine:**
- Morning: Run basic connection tests
- After changes: Run affected system tests
- Before commit: Run regression suite
- Weekly: Run full integration tests

---

**Last Updated:** 2025-11-07

For more details, see:
- `.claude/CLAUDE.md` - Refactoring phases
- `.claude/docs/networking-patterns.md` - Expected behavior
- `.claude/docs/audit-report.md` - Known issues
