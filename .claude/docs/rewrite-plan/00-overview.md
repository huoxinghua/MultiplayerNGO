# MultiplayerNGO - Structured Rewrite Plan

**Timeline:** 1-2 Weeks (Sprint-based)
**Team Size:** 3 students
**Approach:** AI-assisted ground-up rebuild
**Goal:** Working multiplayer game with correct Unity NGO architecture

---

## Why Rewrite Instead of Refactor?

### Current State Analysis
- **75% of NetworkVariables missing** - Most game state doesn't sync
- **Critical systems not networked** - EnemySpawnManager, WalletBankton, Health
- **Architectural confusion** - Abandoned MVC attempt, 3 singleton patterns
- **Estimated refactor time** - 8-12 weeks (full semester)

### Rewrite Benefits
- ✅ **1-2 weeks** vs 8-12 weeks
- ✅ Learn correct patterns immediately
- ✅ Modern AI-assisted workflow
- ✅ Clean architecture from start
- ✅ Higher student motivation
- ✅ Better portfolio piece

---

## What We're Salvaging

### Keep (Already Good)
- ✅ Art assets (models, textures, animations)
- ✅ ScriptableObjects (EnemySO, ItemSO, SpawnDataSO)
- ✅ UI prefabs (just need to rewire to NetworkVariables)
- ✅ Sound effects and AudioManager
- ✅ Level generation logic (just needs NetworkBehaviour)
- ✅ Game design (enemy types, item types, mechanics)

### Rebuild (Fundamentally Broken)
- ❌ All NetworkBehaviour scripts (missing NetworkVariables)
- ❌ Enemy spawning (spawns on all clients)
- ❌ Player health (not networked)
- ❌ Item system (race conditions)
- ❌ Economy (WalletBankton is local-only)
- ❌ All singletons (3 different patterns, none correct)

---

## Team Structure

### Recommended 3-Person Split

**Student A - Player Systems Lead**
- Player controller (movement, input)
- Player health/death
- Player inventory
- Player IK/animations

**Student B - Enemy Systems Lead**
- Enemy spawning (server authority)
- Enemy AI state machines
- Enemy health
- Enemy animations

**Student C - Game Systems Lead**
- Economy (WalletBankton)
- Level generation sync
- Item pickups (world items)
- Combat damage system

**Everyone:**
- Daily standups (15 min)
- Code reviews (pair review each PR)
- Integration testing (all 3 test together)
- Documentation (what you learned)

---

## Sprint Breakdown

### Sprint 1: Core Foundation (Days 1-3)
**Goal:** Basic multiplayer works - players spawn, move, see each other

**Student A:** Networked player controller
- Movement with server validation
- NetworkVariable for health
- Spawning works for all clients

**Student B:** Basic enemy spawning
- EnemySpawnManager as NetworkBehaviour
- Server-only spawning
- Enemies visible on all clients

**Student C:** Architecture setup
- NetworkSingleton<T> base class
- EventBus integration
- Project structure cleanup

**Integration Test:** 3 clients connect, players move, one enemy spawns

---

### Sprint 2: Core Gameplay (Days 4-6)
**Goal:** Combat works - players can damage enemies, enemies can damage players

**Student A:** Player combat
- Attack input
- Damage via ServerRpc
- Death state with NetworkVariable

**Student B:** Enemy AI
- State machine with NetworkVariable<int> for state
- Chase/attack behavior
- Health system

**Student C:** Item system
- Basic item pickup
- NetworkList inventory
- Drop/equip logic

**Integration Test:** Player attacks enemy, enemy dies, enemy attacks player, player dies

---

### Sprint 3: Game Systems (Days 7-9)
**Goal:** Full game loop - money, items, progression

**Student A:** Advanced inventory
- IK hand positions (NetworkVariables)
- Item switching
- Multiple item types

**Student B:** Advanced enemies
- Multiple enemy types
- Spawning waves
- Different behaviors

**Student C:** Economy & progression
- WalletBankton as NetworkSingleton
- Money synced via NetworkVariable
- Research progress
- Shop integration

**Integration Test:** Full game loop - spawn, fight, earn money, buy items

---

### Sprint 4: Polish & Testing (Days 10-12)
**Goal:** Bug-free, polished multiplayer experience

**Everyone:**
- Bug fixes from integration testing
- Late-joiner testing
- Performance optimization
- Documentation
- Final code review

**Integration Test:**
- 3+ simultaneous clients
- Late-joiner scenarios
- Disconnect/reconnect
- Stress testing (many enemies)

---

## Success Criteria

### Must Have (Sprint 1-2)
- [ ] Players spawn for all clients
- [ ] Players can move and see each other move
- [ ] Enemies spawn on server only
- [ ] Enemies visible on all clients
- [ ] Combat works (player → enemy, enemy → player)
- [ ] Health syncs across clients
- [ ] Death states sync

### Should Have (Sprint 3)
- [ ] Item pickup works without duplication
- [ ] Inventory syncs across clients
- [ ] Money/economy syncs
- [ ] IK hand positions sync
- [ ] Multiple enemy types

### Nice to Have (Sprint 4)
- [ ] Late-joiner support works perfectly
- [ ] No NetworkVariable spam (optimized)
- [ ] Smooth animations across clients
- [ ] Full feature parity with original game

---

## Daily Workflow

### Each Student, Each Day:

**Morning (2 hours):**
1. Daily standup (15 min) - What did you do? What will you do? Blockers?
2. Review task for the day
3. Read relevant pattern docs from `.claude/docs/`
4. Start implementation with AI assistance

**Afternoon (3 hours):**
5. Continue implementation
6. Write tests (multiplayer testing)
7. Code review with teammate
8. Commit + push

**End of Day:**
9. Document what you learned
10. Update task status
11. Prepare for next day

---

## Learning Objectives

By the end of this rewrite, students will understand:

### Technical Skills
- ✅ Unity Netcode for GameObjects fundamentals
- ✅ NetworkVariable vs ServerRpc vs ClientRpc
- ✅ Server authority patterns
- ✅ NetworkObjectReference for object passing
- ✅ NetworkSingleton pattern
- ✅ State synchronization strategies

### Soft Skills
- ✅ AI-assisted development workflow
- ✅ Prompt engineering for code generation
- ✅ Code review processes
- ✅ Team collaboration on shared codebase
- ✅ Git workflow (branching, PRs, merges)
- ✅ Technical documentation

### Architecture Skills
- ✅ When to use NetworkVariable vs local state
- ✅ Client-server architecture principles
- ✅ Event-driven design (EventBus)
- ✅ Unity component patterns
- ✅ ScriptableObject for configuration

---

## Risk Mitigation

### Risk: Students don't understand AI-generated code
**Mitigation:**
- Mandatory code review sessions
- "Explain this line" quizzes
- Documentation requirement (write what you learned)

### Risk: Integration breaks at the end
**Mitigation:**
- Daily integration tests
- Early and frequent merging
- Clear API contracts between systems

### Risk: Falls behind schedule
**Mitigation:**
- Cut features, not quality
- Focus on must-haves first
- Professor reviews progress daily

### Risk: AI generates incorrect networking code
**Mitigation:**
- Provide reference patterns in `.claude/docs/`
- Code reviews catch issues early
- Multiplayer testing validates correctness

---

## Files in This Folder

- `00-overview.md` - This file (big picture)
- `01-sprint-1-tasks.md` - Detailed Day 1-3 tasks
- `02-sprint-2-tasks.md` - Detailed Day 4-6 tasks
- `03-sprint-3-tasks.md` - Detailed Day 7-9 tasks
- `04-sprint-4-tasks.md` - Detailed Day 10-12 tasks
- `05-ai-prompting-guide.md` - How to use AI effectively
- `06-testing-checklist.md` - What to test each day
- `07-code-review-checklist.md` - What to look for in reviews
- `08-salvage-checklist.md` - What to keep from old code

---

## Getting Started

1. **Read this overview** - Understand the plan
2. **Read `05-ai-prompting-guide.md`** - Learn how to use AI
3. **Review `.claude/docs/networking-patterns.md`** - Learn the patterns
4. **Start Sprint 1** - See `01-sprint-1-tasks.md`

---

**Last Updated:** 2025-11-08
**Status:** Ready to Execute
