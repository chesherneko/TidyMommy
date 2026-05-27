using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BlockType
{
    Red, Blue, Yellow, 
    Green, Purple, White, 
    Bomb
}

public class Block : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private new SpriteRenderer renderer;

    [Header("Settings")]
    [SerializeField] private float moveDuration = 0.15f;

    [Header("Scriptable Objects")]
    [SerializeField] private BlockSpriteLibrary spriteLibrary;
    [SerializeField] private BlockMaterialLibrary materialLibrary;

    public BlockType Type { get; private set; } = BlockType.Red;
    public BlockBucket CurrentBucket { get; private set; }

    private IBlockPool pool;

    private Coroutine moveCoroutine;

    #region UNITY METHOD
    private void OnDisable()
    {
        OnDeselected();
    }
    #endregion

    #region PRIVATE METHOD
    private IEnumerator MoveToCoroutine(Vector3 endPos)
    {
        float time = 0f;
        Vector3 startPos = transform.position;

        while (true)
        {
            if (time > moveDuration) break;

            time += Time.deltaTime;

            float progress = time / moveDuration;
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);

            transform.position = currentPos;

            yield return null;
        }

        moveCoroutine = null;
    }

    private void SetRendererMaterial(BlockMaterialType type, bool isSetTexture)
    {
        Material mat = materialLibrary.GetMaterial(type);
        renderer.material = mat;

        if (isSetTexture) renderer.material.mainTexture = renderer.sprite.texture;
    }
    #endregion

    #region PUBLIC METHOD
    public void Initialize(IBlockPool pool)
    {
        this.pool = pool;
    }

    public void Active(BlockType type, BlockBucket bucket)
    {
        Type = type;
        CurrentBucket = bucket;

        renderer.sprite = spriteLibrary.GetSprite(type);
        gameObject.SetActive(true);

        bucket.AddActiveBlock(this);

#if UNITY_EDITOR
        transform.SetParent(bucket.transform);
#endif
    }

    public bool TryActive(BlockType type, BlockBucket bucket)
    {
        if (bucket.HasAvailableSpace == false)
            return false;

        Active(type, bucket);
        return true;
    }

    public void Inactive()
    {
        pool.EnqueueBlock(this);
        CurrentBucket.RemoveActiveBlock(this);

        gameObject.SetActive(false);
    }

    public void OnSelected()
    {
        BlockMaterialType type = BlockMaterialType.Outline;
        SetRendererMaterial(type, true);
    }

    public void OnDeselected()
    {
        BlockMaterialType type = BlockMaterialType.None;
        SetRendererMaterial(type, false);
    }

    public void TransferBucketTo(BlockBucket bucket)
    {
        BlockBucket prevBucket = CurrentBucket;
        prevBucket.RemoveActiveBlock(this);

        CurrentBucket = bucket;
        CurrentBucket.InsertActiveBlock(this);
    }

    public void MoveTo(Vector3 endPos)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(MoveToCoroutine(endPos));

        SoundManager.Instance.PlayBlockMoveSFX();
    }
    #endregion
}
