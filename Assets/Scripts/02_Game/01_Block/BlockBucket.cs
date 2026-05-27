using System.Collections.Generic;
using UnityEngine;

public class BlockBucket : MonoBehaviour
{
    public const int MATCH_BLOCK_COUNT = 3;
    public const int MAX_BLOCKS_PER_BUCKET = 12;

    [Header("Components")]
    [SerializeField] BlockManager blockManager;

    [Header("Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 blockInterval;
    private readonly Vector3[] blockPositions = new Vector3[MAX_BLOCKS_PER_BUCKET];

    private readonly List<Block> activeBlocks = new();

    public IReadOnlyList<Block> ActiveBlocks => activeBlocks;

    public bool IsSpaceEmpty => activeBlocks.Count == 0;
    public bool HasAvailableSpace => activeBlocks.Count < MAX_BLOCKS_PER_BUCKET;

    #region UNITY METHOD
    private void Awake()
    {
        InitBlockPositions();
    }

    private void InitBlockPositions()
    {
        Vector3 spawnPos = transform.Find("StartPoint").position;

        blockPositions[0] = spawnPos;

        for (int i = 1; i < MAX_BLOCKS_PER_BUCKET; i++)
        {
            Vector3 prevBlockPos = blockPositions[i - 1];
            blockPositions[i] = prevBlockPos + blockInterval;
        }
    }
    #endregion

    #region PRIVATE METHOD
    private void ReorderActiveBlockPositions()
    {
        int blockPosIdx = 0;
        for (int i = activeBlocks.Count - 1; i >= 0; i--)
        {
            Block block = activeBlocks[i];
            Vector3 movePos = blockPositions[blockPosIdx++];
            block.MoveTo(movePos);
        }
    }

    private bool IsLastThreeConsecutiveSame()
    {
        if (activeBlocks.Count < MATCH_BLOCK_COUNT) return false;

        BlockType refType = activeBlocks[0].Type;

        for (int i = 1; i < MATCH_BLOCK_COUNT; i++)
        {
            BlockType type = activeBlocks[i].Type;

            if (refType != type)
                return false;
        }
        return true;
    }
    #endregion

    #region PUBLIC METHOD
    public void AddActiveBlock(Block block)
    {
        block.transform.position = spawnPoint.position;

        activeBlocks.Add(block);
        ReorderActiveBlockPositions();
    }

    public void InsertActiveBlock(Block block)
    {
        activeBlocks.Insert(0, block);
        ReorderActiveBlockPositions();
    }

    public void RemoveActiveBlock(Block block) => activeBlocks.Remove(block);

    public bool CheckBlockMatch()
    {
        if (IsLastThreeConsecutiveSame() == false)
            return false;

        for (int i = MATCH_BLOCK_COUNT - 1; i >= 0; i--)
            activeBlocks[i].Inactive();

        return true;
    } 

    public bool TryGetLastTwoBlockTypes(out BlockType t1, out BlockType t2)
    {
        t1 = BlockType.Red;
        t2 = BlockType.Red;

        if (activeBlocks.Count < 2) return false;

        int idx = activeBlocks.Count - 1;

        t1 = activeBlocks[idx--].Type;
        t2 = activeBlocks[idx].Type;

        return true;
    }
    #endregion

    #region EVENT TRIGGER METHOD
    public void OnClicked() => blockManager.HandleSelectedBucket(this);
    #endregion
}
