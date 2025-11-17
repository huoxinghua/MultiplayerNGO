# MultiplayerNGO Refactoring Documentation

**Created:** 2025-11-07
**Status:** Planning Complete, Ready to Execute

Welcome to the comprehensive refactoring documentation for the MultiplayerNGO project. This folder contains everything you need to audit, plan, and execute a complete refactoring of the Unity Netcode for GameObjects codebase.

---

## 📋 Quick Start

1. **Read the audit first:** Start with [`docs/audit-report.md`](docs/audit-report.md) to understand all issues
2. **Review the plan:** Read [`CLAUDE.md`](CLAUDE.md) for the complete refactoring roadmap
3. **Learn the patterns:** Study [`docs/networking-patterns.md`](docs/networking-patterns.md) before coding
4. **Follow phase checklists:** Use [`progress/phase-*-checklist.md`](progress/) to track work

---

## 📁 Folder Structure

```
.claude/
├── CLAUDE.md                      # 📘 Main refactoring plan (START HERE)
├── README.md                      # 📄 This file
│
├── docs/                          # 📚 Documentation
│   ├── audit-report.md           # 🔍 Detailed audit findings
│   ├── networking-patterns.md    # 🎯 Standard code patterns
│   ├── architecture-overview.md  # 🏗️ Target architecture
│   ├── authority-model.md        # 🔐 Network ownership rules
│   ├── testing-guide.md          # ✅ Multiplayer testing procedures
│   └── migration-guides/         # 📖 Step-by-step guides
│       ├── singletons-to-network.md
│       ├── item-pickup-refactor.md
│       └── state-machine-sync.md
│
├── commands/                      # ⚡ Claude Code slash commands
│   ├── audit-network.md          # Audit networking code
│   ├── check-authority.md        # Check authority patterns
│   └── find-duplicates.md        # Find duplicate code
│
├── templates/                     # 💻 Code templates
│   ├── network-singleton.cs      # Template for networked singletons
│   ├── networked-item.cs         # Template for items
│   └── enemy-state-machine.cs    # Template for enemy AI
│
└── progress/                      # ✓ Progress tracking
    ├── phase-1-checklist.md      # Phase 1 tasks
    ├── phase-2-checklist.md      # Phase 2 tasks
    └── (more checklists as needed)
```

---

## 🎯 The Plan

The refactoring is divided into **5 phases** over **8-12 weeks**:

| Phase | Duration | Focus | Status |
|-------|----------|-------|--------|
| **Phase 1** | Week 1-2 | Foundation & Cleanup | ⏸️ Not Started |
| **Phase 2** | Week 3-4 | Singleton & Service Architecture | ⏸️ Not Started |
| **Phase 3** | Week 5-7 | Network Synchronization | ⏸️ Not Started |
| **Phase 4** | Week 8-9 | Authority & Validation | ⏸️ Not Started |
| **Phase 5** | Week 10-12 | Optimization & Polish | ⏸️ Not Started |

**See [`CLAUDE.md`](CLAUDE.md) for full details.**

---

## 🔥 Critical Issues Found

The audit identified **15 issues** across 4 severity levels:

### ⛔ CRITICAL (Project-Breaking)
1. **EnemySpawnManager not networked** - Spawns enemies on each client
2. **WalletBankton money not synced** - Economy doesn't work in multiplayer
3. **BruteHealth.OnHit has no network checks** - Damage only local
4. **Duplicate player controller scripts** - Will cause conflicts

### 🔴 HIGH (Causes Desync)
5. **State machines only run on server** - Clients see frozen enemies
6. **Item pickup ownership race conditions** - Items can duplicate
7. **LevelNetworkSync timing issues** - Generation can fail
8. **CurrentPlayers singleton not networked** - Inconsistent lists

### 🟡 MEDIUM (Code Quality)
9-12. Mixed singleton patterns, unused ServiceLocator, refactor artifacts, commented code

### 🟢 LOW (Quick Wins)
13-15. Typos, debug log spam, RequireOwnership overuse

**Full details in [`docs/audit-report.md`](docs/audit-report.md)**

---

## 📚 Key Documents

### For Everyone

| Document | Purpose | When to Read |
|----------|---------|--------------|
| **[CLAUDE.md](CLAUDE.md)** | Complete refactoring plan | First, before starting |
| **[audit-report.md](docs/audit-report.md)** | All issues found | First, to understand scope |
| **[networking-patterns.md](docs/networking-patterns.md)** | How to write network code | Before writing any code |

### For Developers

| Document | Purpose | When to Read |
|----------|---------|--------------|
| **[authority-model.md](docs/authority-model.md)** | Network ownership rules | When implementing features |
| **[architecture-overview.md](docs/architecture-overview.md)** | Target architecture | When planning systems |
| **[testing-guide.md](docs/testing-guide.md)** | How to test multiplayer | After implementing features |

### For Specific Tasks

| Task | Migration Guide |
|------|-----------------|
| Converting singletons | [singletons-to-network.md](docs/migration-guides/singletons-to-network.md) |
| Refactoring item pickup | [item-pickup-refactor.md](docs/migration-guides/item-pickup-refactor.md) |
| Syncing enemy AI | [state-machine-sync.md](docs/migration-guides/state-machine-sync.md) |

---

## ⚡ Quick Commands

Use these Claude Code slash commands to audit your code:

- `/audit-network` - Scan for network anti-patterns
- `/check-authority` - Verify authority implementation
- `/find-duplicates` - Find duplicate code

---

## 💻 Code Templates

Copy these templates for new network code:

- **`templates/network-singleton.cs`** - For managers (GameManager, WalletManager, etc.)
- **`templates/networked-item.cs`** - For items (weapons, tools, etc.)
- **`templates/enemy-state-machine.cs`** - For enemy AI

---

## ✓ Progress Tracking

Use the phase checklists to track your work:

1. **[Phase 1 Checklist](progress/phase-1-checklist.md)** - Foundation & Cleanup
2. **[Phase 2 Checklist](progress/phase-2-checklist.md)** - Singleton Architecture
3. (More checklists in `progress/` folder)

Each checklist includes:
- Detailed task breakdown
- Testing requirements
- Definition of done
- Notes section for blockers

---

## 🎓 Learning Resources

### Unity Netcode for GameObjects

**Official Docs:** https://docs-multiplayer.unity3d.com/netcode/current/about/

**Key Concepts to Understand:**
- NetworkBehaviour lifecycle
- NetworkVariable synchronization
- RPC (ServerRpc/ClientRpc) patterns
- NetworkObject ownership
- Client/Server authority model

### Internal Patterns

**Read these docs first:**
1. [Networking Patterns](docs/networking-patterns.md) - Standard patterns for this project
2. [Authority Model](docs/authority-model.md) - Who controls what
3. [Architecture Overview](docs/architecture-overview.md) - System design

---

## 👥 Team Coordination

### Recommended Split (2-3 Developers)

**Developer 1: Network Systems Lead**
- NetworkSingleton pattern
- Item pickup refactor
- Enemy AI sync
- Authority documentation

**Developer 2: Code Quality & Architecture**
- Deduplication
- Folder restructuring
- Documentation
- Testing infrastructure

**Developer 3: Gameplay Systems** (if available)
- Combat fixes
- Enemy spawning
- Level generation
- Validation & security

### Daily Workflow

1. **Morning:** Standup - Review progress, plan today's work
2. **During Day:** Work on assigned phase tasks
3. **After Changes:** Run relevant tests from testing-guide.md
4. **Before Commit:** Check phase checklist, mark completed tasks
5. **End of Day:** Update progress notes, commit work

---

## 🧪 Testing Strategy

**Test after EVERY change:**
1. Run basic connection test (host + client)
2. Test the specific system you changed
3. Run regression tests from previous phases
4. Document any failures

**See [`docs/testing-guide.md`](docs/testing-guide.md) for detailed test procedures.**

---

## 📊 Success Metrics

### Must-Have (Phase 1-3)
- ✅ No duplicate scripts
- ✅ All game-critical state synchronized
- ✅ Enemy AI visible on all clients
- ✅ Item pickup works without duplication
- ✅ Money/economy synced
- ✅ Level generation works for all clients

### Should-Have (Phase 4)
- ✅ No commented code
- ✅ Single consistent singleton pattern
- ✅ All RPCs have authority checks
- ✅ Validation prevents cheating
- ✅ Clear authority documentation

### Nice-to-Have (Phase 5)
- ✅ Network testing suite
- ✅ Optimized RPC usage
- ✅ Developer debugging tools
- ✅ Comprehensive documentation
- ✅ Onboarding guide

---

## ⚠️ Important Notes

### Before You Start

- [ ] Read [`CLAUDE.md`](CLAUDE.md) completely
- [ ] Review [`audit-report.md`](docs/audit-report.md)
- [ ] Set up test environment (see [`testing-guide.md`](docs/testing-guide.md))
- [ ] Commit all current work to Git
- [ ] Brief team on plan

### During Refactoring

- ⚠️ **Test constantly** - Don't batch all testing to the end
- ⚠️ **Commit frequently** - Small, atomic commits
- ⚠️ **Document decisions** - Update notes in checklists
- ⚠️ **Ask for help** - Use team standups to resolve blockers

### After Each Phase

- [ ] All phase checklist tasks complete
- [ ] All tests passing
- [ ] Code committed to Git
- [ ] Team standup: Confirm completion
- [ ] Update status in this README

---

## 🆘 Getting Help

### Something Not Working?

1. **Check the docs:**
   - [Networking Patterns](docs/networking-patterns.md) - Common patterns
   - [Testing Guide](docs/testing-guide.md) - How to test
   - [Migration Guides](docs/migration-guides/) - Step-by-step instructions

2. **Common Issues:**
   - See "Common Issues & Solutions" in each migration guide
   - Check [audit-report.md](docs/audit-report.md) for known anti-patterns

3. **Ask Claude Code:**
   - Use `/audit-network` command to check your code
   - Use `/check-authority` to verify authority patterns

4. **Team Standup:**
   - Bring blockers to daily standup
   - Document decisions in phase checklist notes

---

## 📝 Document Status

| Document | Status | Last Updated |
|----------|--------|--------------|
| CLAUDE.md | ✅ Complete | 2025-11-07 |
| audit-report.md | ✅ Complete | 2025-11-07 |
| networking-patterns.md | ✅ Complete | 2025-11-07 |
| architecture-overview.md | ✅ Complete | 2025-11-07 |
| authority-model.md | ✅ Complete | 2025-11-07 |
| testing-guide.md | ✅ Complete | 2025-11-07 |
| Migration guides | ✅ Complete | 2025-11-07 |
| Templates | ✅ Complete | 2025-11-07 |
| Commands | ✅ Complete | 2025-11-07 |
| Phase checklists | ✅ Complete (1-2) | 2025-11-07 |

---

## 🚀 Next Steps

**To begin refactoring:**

1. ✅ Read [`CLAUDE.md`](CLAUDE.md) (Main Plan)
2. ✅ Read [`docs/audit-report.md`](docs/audit-report.md) (Understand Issues)
3. ✅ Set up testing environment (See [`docs/testing-guide.md`](docs/testing-guide.md))
4. ✅ Start [`progress/phase-1-checklist.md`](progress/phase-1-checklist.md)
5. ✅ Commit changes frequently
6. ✅ Test continuously

**Good luck! 🎮**

---

**Questions?** Review the documentation or ask during team standups.

**Found an issue with this documentation?** Update it and commit the changes!
