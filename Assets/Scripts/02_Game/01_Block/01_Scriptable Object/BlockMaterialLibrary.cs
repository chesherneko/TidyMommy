using UnityEngine;

public enum BlockMaterialType
{
    None,
    Outline
}

[CreateAssetMenu(fileName = "BlockMaterialLibrary", menuName = "Scriptable Objects/Block/Materials")]
public class BlockMaterialLibrary : ScriptableObject
{
    [SerializeField] Material[] materials;

    public Material GetMaterial(BlockMaterialType type)
        => materials[(int)type];
}
