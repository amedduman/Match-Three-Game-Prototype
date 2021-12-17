using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Drop : MonoBehaviour
{
    public Sprite[] DropSprites; // TODO: use sprite atlas to minimize draw calls
    
    [HideInInspector] public int SpriteIndex;
    [HideInInspector] public Sprite currentSprite;
    public SpriteRenderer dropSpriteRenderer;
    [SerializeField] protected Transform gfx;
    [SerializeField] [Range(.01f, 1)] protected float movingAnimationTime = .2f;
    [SerializeField] [Range(.01f, 1)] protected float scalingDownAnimationTime = .2f;
    [Range(0,1)] [SerializeField] protected float minScaleForMatchAnim = .5f;

    public void UpdateDropSprite() 
    {
        dropSpriteRenderer.sprite = currentSprite;
        gameObject.name = currentSprite.name;
        // find sprite index since we will use it for find out if there is a match.
        for (int i = 0; i < DropSprites.Length; i++)
        {
            if (currentSprite == DropSprites[i])
            {
                SpriteIndex = i;
            }
        }
    }

    void RandomizeDrop()
    {
        // randomize drop when to make it ready fall again
        currentSprite = DropSprites[Random.Range(0, DropSprites.Length)];
        UpdateDropSprite();
    }

    public IEnumerator MoveToTile(Tile targetTile)
    {
        yield return StartCoroutine(MoveAnim(targetTile.transform.position));
    }

    IEnumerator MoveAnim(Vector3 endPos)
    {
        Vector3 startPos = transform.position;
        float duration = movingAnimationTime;
        float passedTime = 0;

        while (passedTime < duration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, passedTime/duration);
            passedTime += Time.deltaTime;
            yield return null;
        }
    }
    
    public IEnumerator MatchProcess(Tile topTile)
    {
        Vector3 normalScale = gfx.localScale;
        yield return StartCoroutine(ScaleDownAnim());
    
        if (topTile.isSpawner)
        {
            RandomizeDrop();
            topTile.AddToDropPile(this);
            gfx.localScale = normalScale;
        }
        else
        {
            gameObject.SetActive(false);
        }
        
    }
    
    IEnumerator ScaleDownAnim()
    {
        Vector3 startScale = gfx.localScale;
        Vector3 minScale = Vector3.one * minScaleForMatchAnim;
        float passedTime = 0;
        while (passedTime < scalingDownAnimationTime)
        {
            gfx.localScale = Vector3.Lerp(startScale, minScale, passedTime/scalingDownAnimationTime);
            passedTime += Time.deltaTime;
            yield return null;
        }
    }
}