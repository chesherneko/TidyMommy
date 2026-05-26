using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

public interface IBlockPool
{
    void EnqueueBlock(Block block);
}

public class BlockManager : MonoBehaviour, IBlockPool
{
    [Header("Components")]
    [SerializeField] TidyMommy tidyMommy;
    [SerializeField] private BlockBucket[] buckets;

    [Header("Block")]
    private readonly Queue<Block> blockQueue = new();
    [SerializeField] private Transform blockContainer;

    private Block selectedBlock;

    private readonly BlockType[] blockTypes = (BlockType[])Enum.GetValues(typeof(BlockType));

    #region UNITY METHOD
    private void Awake()
    {
        InitAndEnqueueBlocks();
    }

    private void InitAndEnqueueBlocks()
    {
        int blockCount = blockContainer.childCount;

        for (int i = 0; i < blockCount; i++)
        {
            Block block = blockContainer.GetChild(i).GetComponent<Block>();

            block.Initialize(this);
            EnqueueBlock(block);
        }
    }
    #endregion

    #region PRIVATE METHOD
    private BlockType GetNextSpawnType(List<BlockType> lastTwoTypes)
    {
        Dictionary<BlockType, int> counts = GetBlockCounts();
        Dictionary<BlockType, float> weights = BuildSpawnWeights(counts, out float totalWeight);

        BlockType candidate = GetBlockTypeByWeight(weights, totalWeight);

        DictionaryPool<BlockType, int>.Release(counts);
        DictionaryPool<BlockType, float>.Release(weights);

        if (WouldCreateTripleMatch(candidate, lastTwoTypes) == false)
            return candidate;

        return GetBlockTypeExcluding(candidate);
    }

    private Dictionary<BlockType, int> GetBlockCounts()
    {
        Dictionary<BlockType, int> counts = DictionaryPool<BlockType, int>.Get();

        Level level = tidyMommy.CurrentLevel;
        int maxSpawnTypeIdx = (int)GetMaxBlockType(level);

        for (int i = 0; i <= maxSpawnTypeIdx; i++)
            counts[blockTypes[i]] = 1; //기본적으로 생성 가능한 모든 블럭은 1의 가중치를 가짐

        for (int i = 0; i < buckets.Length; i++)
        {
            BlockBucket bucket = buckets[i];
            IReadOnlyList<Block> activeBlocks = bucket.ActiveBlocks;

            for (int j = 0; j < activeBlocks.Count; j++)
            {
                Block block = activeBlocks[j];
                BlockType blockType = block.Type;

                //Bomb 타입은 특수한 상황에서만 스폰 가능
                if (blockType == BlockType.Bomb) continue;

                counts[blockType]++;
            }
        }

        return counts;
    }

    private Dictionary<BlockType, float> BuildSpawnWeights(Dictionary<BlockType, int> counts, out float totalWeight)
    {
        totalWeight = 0f;
        Dictionary<BlockType, float> weights = DictionaryPool<BlockType, float>.Get();

        Level level = tidyMommy.CurrentLevel;
        float exponent = GetLevelExponent(level);

        foreach (var count in counts)
        {
            float weight = Mathf.Pow(count.Value, exponent);

            weights[count.Key] = weight;
            totalWeight += weight;
        }

        return weights;
    }

    private BlockType GetBlockTypeByWeight(Dictionary<BlockType, float> weights, float totalWeight)
    {
        float weightCum = 0f;
        float randWeight = Random.Range(0f, totalWeight);

        BlockType type = BlockType.Red;

        foreach (var weight in weights)
        {
            weightCum += weight.Value;

            if (randWeight <= weightCum)
            {
                type = weight.Key;
                break;
            }
        }

        return type;
    }

    private BlockType GetBlockTypeExcluding(BlockType exclude)
    {
        List<BlockType> types = ListPool<BlockType>.Get();

        int maxTypeIdx = (int)GetMaxBlockType(tidyMommy.CurrentLevel);

        for (int i = 0; i <= maxTypeIdx; i++)
            types.Add(blockTypes[i]); //사용 가능한 모든 타입 추가

        types.Remove(exclude); //제외할 타입 제거

        int randIdx = Random.Range(0, types.Count);
        BlockType type = types[randIdx];

        ListPool<BlockType>.Release(types);

        return type;
    }

    private BlockType GetMaxBlockType(Level level)
    {
        return level switch
        {
            Level.One => BlockType.Blue,
            Level.Two => BlockType.Yellow,
            Level.Three => BlockType.Green,
            Level.Four => BlockType.Purple,
            _ => BlockType.White,
        };
    }

    private float GetLevelExponent(Level level)
    {
        return level switch
        {
            Level.Six => 1.5f,
            Level.Seven => 1f,
            Level.Eight => 0f,
            Level.Nine => -0.25f,
            Level.Ten => -0.5f,
            _ => 2f
        };
    }

    private bool WouldCreateTripleMatch(BlockType type, List<BlockType> lastTwoTypes)
    {
        //이전 타입이 2개 미만일 경우, 해당 버킷에 나와있는 블럭이 하나 뿐이란 소리
        //그러므로 연속된 색이 3개가 올 수 없으니 유효로 즉시 반환
        if (lastTwoTypes.Count < 2) return false;

        return lastTwoTypes[0] == type 
            && lastTwoTypes[1] == type;
    }
    #endregion

    #region PUBLIC METHOD
    public void EnqueueBlock(Block block)
    {
        blockQueue.Enqueue(block);

#if UNITY_EDITOR
        if (block.transform.parent != blockContainer)
        {
            block.transform.SetParent(blockContainer);
            block.transform.localPosition = Vector3.zero;
        }
#endif
    }

    public void HandleSelectedBucket(BlockBucket selectedBucket)
    {
        //선택된 블록이 있을 경우
        if (selectedBlock != null)
        {
            //선택된 블록이 속한 버킷과 선택한 버킷이 같은 경우
            if (selectedBlock.CurrentBucket == selectedBucket)
            {
                DeselectSelectedBlock();
                return;
            }

            if (selectedBucket.HasAvailableSpace == false) return;

            selectedBlock.TransferBucketTo(selectedBucket);
            DeselectSelectedBlock();

            if (selectedBucket.CheckBlockMatch())
            {
                SpawnRandomBlocksToAllLines();

                tidyMommy.AdvanceLevelUpCount();
                GameManager.Instance.IncreaseScore();
            }
        }
        else //선택된 블록이 없을 경우
        {
            if (selectedBucket.IsSpaceEmpty) return;

            selectedBlock = selectedBucket.ActiveBlocks[0];
            selectedBlock.OnSelected();
        }

        void DeselectSelectedBlock()
        {
            selectedBlock.OnDeselected();
            selectedBlock = null;
        }
    }

    public void SpawnRandomBlocksToAllLines(int counts = 1)
    {
        for (int count = 0; count < counts; count++)
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                Block block = blockQueue.Dequeue();

                BlockBucket bucket = buckets[i];

                List<BlockType> lastTwoTypes = bucket.GetLastTwoBlockTypes();
                BlockType spwanType = GetNextSpawnType(lastTwoTypes);

                //블록을 소환할 수 없는 경우가 생기면 게임 오버
                if (block.TryActive(spwanType, bucket) == false)
                {
                    GameManager.Instance.GameOver();
                    return;
                }
            }
        }
    }
    #endregion
}
