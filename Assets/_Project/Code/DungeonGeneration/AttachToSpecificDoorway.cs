using DunGen;
using UnityEngine;

public class AttachToSpecificDoorway : MonoBehaviour
{
    // The dungeon generator to use for generating the new dungeon. Set in inspector
    public RuntimeDungeon RuntimeDungeon;

    // The existing doorway component to attach to. Set in inspector
    public Doorway SpecificDoorway;

    public void Start()
    {
        GenerateAttached();
    }

    public void GenerateAttached()
    {
        RuntimeDungeon.Generator.AttachmentSettings = new DungeonAttachmentSettings(SpecificDoorway);
        RuntimeDungeon.Generate();
    }
}
