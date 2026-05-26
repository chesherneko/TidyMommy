using UnityEngine;

[CreateAssetMenu(fileName = "BlockSpriteLibrary", menuName = "Scriptable Objects/Block/Sprites")]
public class BlockSpriteLibrary : ScriptableObject
{
    [SerializeField] Sprite[] sprites;

    public Sprite GetSprite(BlockType type)
        => sprites[(int)type];
}
