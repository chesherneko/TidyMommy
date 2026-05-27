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
    private const int BOMB_BLOCK_REMOVE_COUNT = 3;

    [Header("Components")]
    [SerializeField] private TidyMommy tidyMommy;
    [SerializeField] private BlockBucket[] buckets;
    [SerializeField] private CameraShake cameraShake;

    [Header("Settings")]
    [SerializeField] private float matchSpawnChance = 0.9f;

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
    private BlockType GetNextSpawnType(bool isGetLastTwoTypes, BlockType t1, BlockType t2)
    {
        Dictionary<BlockType, int> counts = GetBlockCounts();
        Dictionary<BlockType, float> weights = BuildSpawnWeights(counts, out float totalWeight);

        BlockType candidate = GetBlockTypeByWeight(weights, totalWeight);

        DictionaryPool<BlockType, int>.Release(counts);
        DictionaryPool<BlockType, float>.Release(weights);

        if (isGetLastTwoTypes == false) return candidate; //해당 버킷에 블럭이 2개 미만일 경우
        if (WouldCreateMatch(candidate, t1, t2) == false) return candidate;

        return GetBlockTypeExcluding(candidate);
    }

    private Dictionary<BlockType, int> GetBlockCounts(bool useDefaultCount = true)
    {
        Dictionary<BlockType, int> counts = DictionaryPool<BlockType, int>.Get();

        Level level = tidyMommy.CurrentLevel;
        Mode mode = tidyMommy.CurrentMode;
        int maxSpawnTypeIdx = (int)GetMaxBlockType(level, mode);

        for (int i = 0; i <= maxSpawnTypeIdx; i++)
            counts[blockTypes[i]] = useDefaultCount ? 1 : 0;

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

        Level level = tidyMommy.CurrentLevel;
        Mode mode = tidyMommy.CurrentMode;
        int maxTypeIdx = (int)GetMaxBlockType(level, mode);

        for (int i = 0; i <= maxTypeIdx; i++)
            types.Add(blockTypes[i]); //사용 가능한 모든 타입 추가

        types.Remove(exclude); //제외할 타입 제거

        int randIdx = Random.Range(0, types.Count);
        BlockType type = types[randIdx];

        ListPool<BlockType>.Release(types);

        return type;
    }

    private BlockType GetMaxBlockType(Level level, Mode mode)
    {
        if (mode == Mode.SuperFever) return BlockType.Blue;

        return level switch
        {
            Level.One => BlockType.Blue,
            Level.Two => BlockType.Yellow,
            Level.Three => BlockType.Green,
            Level.Four => BlockType.Purple,
            _ => BlockType.White,
        };
    }

    private void TrySpawnBlocksWhenMatched()
    {
        //기본적으로 스폰은 확률로, 그러나 매칭되는 블록이 없다면 강제 스폰
        float chance = Random.Range(0f, 1f);

        if (chance <= matchSpawnChance)
            SpawnRandomBlocksToAllLines();

        SpawnRandomBlocksIfNoMatches();
    }

    private void SpawnRandomBlocksIfNoMatches()
    {
        Dictionary<BlockType, int> counts = GetBlockCounts(false);

        bool isValid = false;

        foreach (var count in counts.Values)
        {
            if (count >= BlockBucket.MATCH_BLOCK_COUNT)
            {
                isValid = true;
                break;
            }
        }

        DictionaryPool<BlockType, int>.Release(counts);

        if (isValid) return;

        SpawnRandomBlocksToAllLines();
        SpawnRandomBlocksIfNoMatches();
    }

    private void OnBlockMatched()
    {
        TrySpawnBlocksWhenMatched();

        tidyMommy.OnBlockMatched();
        GameManager.Instance.IncreaseScore();

        cameraShake.Shake();
        SoundManager.Instance.PlayBlockMatchSFX(isBomb: false);
    }

    private void UseBombBlock(Block block)
    {
        if (block.Type != BlockType.Bomb) return;

        block.Inactive();
        RemoveBlocksFromAllLines(BOMB_BLOCK_REMOVE_COUNT);

        SpawnRandomBlocksToAllLines(Random.Range(1, 3)); //1~2줄 생성

        cameraShake.BigShake();
        SoundManager.Instance.PlayBlockMatchSFX(isBomb: true);
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

    private bool WouldCreateMatch(BlockType type, BlockType t1, BlockType t2)
    {
        return t1 == type 
            && t2 == type;
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
                SoundManager.Instance.PlayBlockDeselectSFX();
                return;
            }

            if (selectedBucket.HasAvailableSpace == false) return;

            if (selectedBlock.Type == BlockType.Bomb)
            {
                UseBombBlock(selectedBlock);
                selectedBlock = null;
                return;
            }

            selectedBlock.TransferBucketTo(selectedBucket);
            DeselectSelectedBlock();

            if (selectedBucket.CheckBlockMatch())
                OnBlockMatched();
        }
        else //선택된 블록이 없을 경우
        {
            if (selectedBucket.IsSpaceEmpty) return;

            selectedBlock = selectedBucket.ActiveBlocks[0];
            selectedBlock.OnSelected();

            SoundManager.Instance.PlayBlockSelectSFX();
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

                bool isGetLastTypes = bucket.TryGetLastTwoBlockTypes(out BlockType t1, out BlockType t2);
                BlockType spawnType = GetNextSpawnType(isGetLastTypes, t1, t2);

                //블록을 소환할 수 없는 경우가 생기면 게임 오버
                if (block.TryActive(spawnType, bucket) == false)
                {
                    GameManager.Instance.GameOver();
                    return;
                }
            }
        }

        SpawnRandomBlocksIfNoMatches();
    }

    public void SpawnBlocksToAllLines(BlockType type, int counts = 1)
    {
        for (int count = 0; count < counts; count++)
        {
            for (int i = 0; i < buckets.Length; i++)
            {
                BlockBucket bucket = buckets[i];
                Block block = blockQueue.Dequeue();

                block.Active(type, bucket);
            }
        }
    }

    public void RemoveBlocksFromAllLines(int counts)
    {
        if (counts <= 0) return;

        //전달 받은 counts에 따라 '가능한 만큼' 삭제
        for (int i = 0; i < buckets.Length; i++)
        {
            BlockBucket bucket = buckets[i];
            IReadOnlyList<Block> blocks = bucket.ActiveBlocks;

            for (int j = 0; j < counts; j++)
            {
                if (blocks.Count == 0) break;

                Block block = blocks[0];
                block.Inactive();
            }
        }
    }

    public bool TrySpawnBombBlock()
    {
        List<BlockBucket> availableBuckets = ListPool<BlockBucket>.Get();

        for (int i = 0; i < buckets.Length; i++)
        {
            BlockBucket bucket = buckets[i];

            if (bucket.HasAvailableSpace)
                availableBuckets.Add(bucket);
        }

        bool canAnySpawn = availableBuckets.Count > 0;

        if (canAnySpawn)
        {
            int randIdx = Random.Range(0, availableBuckets.Count);
            BlockBucket bucket = availableBuckets[randIdx];

            Block block = blockQueue.Dequeue();
            block.Active(BlockType.Bomb, bucket);
        }

        ListPool<BlockBucket>.Release(availableBuckets);
        return canAnySpawn;
    }
    #endregion
}
