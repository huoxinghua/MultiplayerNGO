Find duplicate or very similar code files in the project.

Search for:

1. **Exact Duplicates:**
   - Files with identical or near-identical content
   - Same class names in different locations
   - Copy-pasted code with minor modifications

2. **Similar Scripts:**
   - Scripts with >80% similarity
   - Same functionality implemented differently
   - Refactoring artifacts (old vs new versions)

3. **Naming Issues:**
   - Files with similar names (e.g., PlayerController, PlayerControl, Player_Controller)
   - Typos in class names
   - Files with "Old", "New", "Backup", "Refactor" in names

4. **Legacy Code:**
   - Backup files (.backup, .old extensions)
   - Commented-out entire files
   - Unused folders (Assets/Network vs Assets/_Project/Code/Network)

For each duplicate/similarity found, report:
- Both file paths
- Similarity percentage
- Differences (if any)
- Recommendation (which to keep, which to delete)

Provide a prioritized cleanup plan:
1. Exact duplicates (safe to delete)
2. Legacy/backup files (safe to delete)
3. Similar files (requires review)
4. Naming issues (requires renaming)

Format as a markdown report with clear action items.