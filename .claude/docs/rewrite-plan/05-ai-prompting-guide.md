# AI-Assisted Development Guide for Unity NGO

**Goal:** Learn how to use AI (Claude, ChatGPT, etc.) effectively for multiplayer game development
**Not Goal:** Copy-paste without understanding

---

## Core Principle

> **AI is a pair programmer, not a code generator.**

You should understand every line of code the AI produces. If you don't understand it, ask the AI to explain it.

---

## The Prompting Framework

### 1. Context First, Request Second

**Bad prompt:**
```
make a player health script for unity
```

**Good prompt:**
```
I'm building a multiplayer game with Unity Netcode for GameObjects.

Current situation:
- I have a player GameObject that spawns for each client
- I need health to be synced across all clients
- Server should have authority over health changes

Requirements:
- Use NetworkVariable<float> for health value
- Server-only damage application
- OnValueChanged callback to update UI
- Death state should also be a NetworkVariable

I have this ScriptableObject for player stats:
[paste PlayerSO.cs if relevant]

Generate a PlayerHealth.cs script following Unity NGO best practices.
```

**Why it's better:**
- AI understands your architecture
- AI knows what patterns to use
- AI can suggest improvements
- Result is tailored to your project

---

## The Iterative Refinement Process

### Step 1: Get Initial Code
```
[Your detailed prompt with context]
```

### Step 2: Review and Question
```
Explain the following:
1. Why did you use NetworkVariable instead of just a regular variable?
2. What does RequireOwnership = false mean in the ServerRpc attribute?
3. Why subscribe to OnValueChanged only if IsClient?
```

### Step 3: Request Improvements
```
This is good, but I need:
- Add validation to prevent negative health
- Add a max health property
- Trigger a death event when health reaches 0
- Add XML documentation comments
```

### Step 4: Understand Tradeoffs
```
What are the pros and cons of syncing health via NetworkVariable vs
sending health updates via ClientRpc?
```

---

## Prompting Patterns for Unity NGO

### Pattern 1: Creating a Networked System

**Template:**
```
I'm implementing [SYSTEM_NAME] for Unity Netcode for GameObjects.

Context:
- [Current architecture overview]
- [What already exists]
- [What this system needs to interact with]

Requirements:
- [Must have feature 1]
- [Must have feature 2]
- [Must have feature 3]

Networking requirements:
- [What needs to sync across clients]
- [Who has authority (server/owner/client)]
- [What happens on server vs client]

Reference pattern:
[paste from .claude/docs/networking-patterns.md if relevant]

Generate [CLASS_NAME].cs following Unity NGO best practices.
```

**Example:**
```
I'm implementing enemy spawning for Unity Netcode for GameObjects.

Context:
- We have enemy prefabs with NetworkObject components
- We have spawn points distributed around the level
- Server should control when and where enemies spawn

Requirements:
- Only server spawns enemies
- Clients automatically see spawned enemies
- Random spawn point selection
- Timed spawning (every 30 seconds)
- Different enemy types with weighted random selection

Networking requirements:
- Server has full authority
- Uses NetworkObject.Spawn() to sync to clients
- No client-side spawning

Generate EnemySpawnManager.cs following Unity NGO best practices.
```

---

### Pattern 2: Understanding Existing Code

**Template:**
```
I'm looking at this [CODE_TYPE] from our Unity NGO project:

[paste code]

Questions:
1. What is this code doing?
2. What are the networking implications?
3. What could go wrong in multiplayer?
4. How would you improve it?
```

**Example:**
```
I'm looking at this enemy health script from our Unity NGO project:

[paste BeetleHealth.cs]

Questions:
1. I see it inherits from NetworkBehaviour but _currentHealth is not a
   NetworkVariable. What happens in multiplayer?
2. Why does OnHit check if (!IsServer) and call ServerRpc?
3. What happens on the host (who is both server and client)?
4. How should this be rewritten?
```

---

### Pattern 3: Debugging Multiplayer Issues

**Template:**
```
I'm experiencing this issue in my Unity NGO multiplayer game:

Observed behavior:
- [What you see on host]
- [What you see on client]
- [What you expected to see]

Relevant code:
[paste the code you think is related]

Network setup:
- [How many clients]
- [Who is host]
- [When does the issue occur]

What could be causing this?
```

**Example:**
```
I'm experiencing this issue in my Unity NGO multiplayer game:

Observed behavior:
- Host sees enemy at correct position
- Client sees enemy but it's frozen in place
- Enemy is moving on host's screen

Relevant code:
[paste BeetleStateMachine.cs]

Network setup:
- 1 host, 1 client
- Issue occurs as soon as enemy spawns
- Enemy has NetworkObject and NetworkTransform

What could be causing this?
```

---

### Pattern 4: Comparing Approaches

**Template:**
```
I need to [GOAL] in Unity NGO.

I'm considering two approaches:

Approach A: [Description]
[code sketch or explanation]

Approach B: [Description]
[code sketch or explanation]

Questions:
1. Which approach is better for multiplayer?
2. What are the bandwidth implications?
3. What are the sync implications for late-joiners?
4. Are there edge cases I'm missing?
```

**Example:**
```
I need to sync player inventory in Unity NGO.

I'm considering two approaches:

Approach A: NetworkList of NetworkObjectReferences
- Store actual item NetworkObjects in a NetworkList
- Items persist in world, just change ownership
- Inventory is references to those items

Approach B: NetworkList of item IDs
- Destroy items when picked up
- Store item IDs (ints) in NetworkList
- Spawn visual representations when equipped

Questions:
1. Which approach is better for multiplayer?
2. What happens if a player disconnects with items?
3. What are the bandwidth implications?
4. How does this affect late-joiners?
```

---

### Pattern 5: Converting from MonoBehaviour

**Template:**
```
I have this working MonoBehaviour script that I need to convert to
NetworkBehaviour for multiplayer:

[paste current code]

Requirements:
- [What needs to sync]
- [Who has authority]
- [What stays local]

Convert this to NetworkBehaviour and explain what you changed and why.
```

---

## Common Pitfalls to Ask AI About

### When Getting Code, Always Ask:

1. **"Where should I add the IsServer/IsOwner checks?"**
   - AI might forget authority checks
   - You need to understand where they go and why

2. **"What NetworkVariables do I need for this system?"**
   - AI might use regular variables that won't sync
   - Explicitly ask about state synchronization

3. **"How does this handle late-joiners?"**
   - AI might generate code that only works for initial clients
   - Late-join support is critical

4. **"What happens if two clients do this simultaneously?"**
   - Race conditions are easy to miss
   - AI should explain the conflict resolution

5. **"Show me the data flow: client → server → other clients"**
   - Understanding the flow is crucial
   - Ask AI to diagram it

---

## Effective Follow-Up Questions

### To Deepen Understanding:

**"Why did you choose [X] over [Y]?"**
- Understand the reasoning, not just the solution

**"What would break if I removed [this line]?"**
- Test your understanding of each part

**"Explain this to me like I'm a beginner"**
- If AI's explanation is too technical

**"What are the edge cases here?"**
- AI should consider error scenarios

**"How would a professional Unity NGO developer write this?"**
- Get industry-standard patterns

---

## Red Flags to Watch For

### AI-Generated Code That's Wrong:

❌ **No NetworkBehaviour inheritance**
```csharp
public class EnemySpawnManager : MonoBehaviour  // WRONG!
```
Ask: "Should this be NetworkBehaviour? What's syncing across clients?"

---

❌ **Regular variables for synced state**
```csharp
private float health = 100f;  // WRONG! Won't sync!
```
Ask: "Should health be a NetworkVariable? How does this sync?"

---

❌ **Client-side spawning**
```csharp
void Start()
{
    GameObject enemy = Instantiate(enemyPrefab);  // WRONG! All clients spawn!
}
```
Ask: "Where's the IsServer check? Should only server spawn?"

---

❌ **Missing OnNetworkSpawn**
```csharp
private void Start()  // RISKY! NetworkObject might not be ready
{
    _health.OnValueChanged += OnHealthChanged;
}
```
Ask: "Should this be in OnNetworkSpawn instead of Start?"

---

❌ **Not unsubscribing from events**
```csharp
// No OnNetworkDespawn to unsubscribe!
```
Ask: "Do I need to unsubscribe in OnNetworkDespawn to prevent memory leaks?"

---

## Example: Full Conversation Flow

### You:
```
I'm implementing player health for Unity NGO. I have:
- Player prefab with NetworkObject
- PlayerSO ScriptableObject with maxHealth = 100
- UI HealthBar that needs to update

Requirements:
- Health syncs across all clients
- Server authority for damage
- Death state should trigger player respawn
- UI should update when health changes

Generate PlayerHealth.cs following NGO best practices.
```

### AI:
```csharp
[generates code]
```

### You (Review & Question):
```
Thanks! A few questions:

1. Why did you use NetworkVariable<float> instead of just float?
2. Explain line 23 - why do we check IsClient before subscribing?
3. What happens if a client tries to modify _health.Value directly?
4. How does this handle late-joiners (clients who join mid-game)?
```

### AI:
```
[explains each point]
```

### You (Request Improvements):
```
Great explanation! Can you add:
- Maximum health cap (can't heal above max)
- Damage validation (can't take negative damage to heal)
- Invincibility period after taking damage (0.5 seconds)
- XML documentation comments
```

### AI:
```csharp
[generates improved version]
```

### You (Verify Understanding):
```
Perfect. Now explain to me:
1. How would I test this in multiplayer (what should I verify)?
2. What would break if I removed the IsServer check in TakeDamageServerRpc?
3. Show me the data flow when Client 2 damages Client 1
```

### AI:
```
[explains testing strategy and data flow]
```

**Now you understand the code!** Copy it into your project.

---

## Learning Checklist

Before accepting AI-generated code, can you answer:

- [ ] What NetworkVariables are used and why?
- [ ] Where are the IsServer/IsOwner checks and why?
- [ ] What happens on Server vs Client vs Owner?
- [ ] How does this handle late-joiners?
- [ ] What could go wrong in multiplayer?
- [ ] What's the data flow for the main operations?
- [ ] Why was this approach chosen over alternatives?

**If you can't answer these, ask more questions!**

---

## Resources to Give AI

### Always Provide:
1. **Architecture context** - What exists, what you're building
2. **Networking requirements** - What syncs, who has authority
3. **Reference patterns** - Link to `.claude/docs/networking-patterns.md`
4. **Existing code** - Related scripts it needs to interact with

### Never Expect AI To:
1. Know your specific project structure
2. Remember previous conversations (always provide context)
3. Understand implicit requirements (be explicit!)
4. Debug without seeing the full error/code

---

## Final Tips

### DO:
- ✅ Start with context-rich prompts
- ✅ Ask "why" questions
- ✅ Request explanations
- ✅ Iterate and refine
- ✅ Verify understanding before coding
- ✅ Test in multiplayer immediately

### DON'T:
- ❌ Copy-paste without understanding
- ❌ Accept first output without review
- ❌ Skip testing in multiplayer
- ❌ Forget to ask about edge cases
- ❌ Ignore red flags in generated code
- ❌ Use AI as a replacement for learning

---

## Remember

**The goal is to learn Unity NGO with AI as a teacher, not to have AI do your homework.**

Every interaction should teach you something new about:
- Networking patterns
- Unity architecture
- Multiplayer game design
- Problem-solving strategies

If you're just copying code, you're missing the point!

---

**Last Updated:** 2025-11-08
