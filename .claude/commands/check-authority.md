Analyze the network authority implementation in the specified file or system.

For the file/system provided, check:

1. **Server Authority Checks:**
   - Are game state modifications protected by `if (!IsServer) return;`?
   - Are spawning operations only on server?
   - Are NetworkVariable writes only on server?

2. **Client Authority Checks:**
   - Is input handling protected by `if (!IsOwner) return;`?
   - Are visual-only operations correctly run on clients?
   - Are UI updates not modifying game state?

3. **RPC Usage:**
   - Are ServerRpcs used for client → server communication?
   - Are ClientRpcs used for server → clients communication?
   - Is RequireOwnership set appropriately?

4. **Validation:**
   - Do ServerRpcs validate all parameters?
   - Are distance checks performed?
   - Are cooldowns enforced?
   - Are bounds checked?

5. **Ownership:**
   - Is object ownership clear and documented?
   - Are ownership transfers done correctly?
   - Is ChangeOwnership only called on server?

Provide a report with:
- Authority flow diagram
- Issues found (with severity)
- Recommendations for improvements
- Code examples of correct patterns

If no file is specified, analyze the entire project and provide a summary of authority patterns used.
