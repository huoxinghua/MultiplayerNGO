Audit the networking code in this Unity NGO project for common anti-patterns and issues.

Search for and report on:

1. **RPC Anti-patterns:**
   - Find all `[ServerRpc]` and `[ClientRpc]` implementations
   - Check for the "if (!IsServer) ServerRpc; else DirectCall" double-execution pattern
   - Look for `RequireOwnership = false` usage without comments explaining why
   - Find any RPCs that don't validate their parameters

2. **NetworkVariable Issues:**
   - Find NetworkVariable declarations without proper read/write permissions
   - Check for direct .Value modifications on clients (should use ServerRpc)
   - Look for missing OnValueChanged subscriptions/unsubscriptions

3. **Authority Issues:**
   - Find game logic that runs on clients without `if (!IsServer)` guards
   - Look for spawning code that doesn't check `IsServer`
   - Find NetworkObject.Spawn() calls on clients

4. **Race Conditions:**
   - Look for coroutines waiting for network state (WaitUntil, WaitForSeconds)
   - Find arbitrary delays in network code
   - Check for ownership changes without validation

5. **Memory Leaks:**
   - Find OnValueChanged subscriptions without corresponding unsubscriptions
   - Check OnNetworkSpawn without matching OnNetworkDespawn

For each issue found, report:
- File path and line number
- Code snippet
- Explanation of the issue
- Suggested fix

Format the output as a markdown report with severity levels (Critical, High, Medium, Low).
