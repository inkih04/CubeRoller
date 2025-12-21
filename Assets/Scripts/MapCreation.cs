using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMapAnimator : MonoBehaviour
{
    private class TileData
    {
        public Transform transform;
        public Vector3 originalPosition;
    }

    private List<TileData> tiles = new List<TileData>();

    void Awake()
    {
        foreach (Transform child in transform)
        {
            tiles.Add(new TileData { transform = child, originalPosition = child.position });
        }
    }


    private void ShuffleTiles()
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            TileData temp = tiles[i];
            int randomIndex = Random.Range(i, tiles.Count);
            tiles[i] = tiles[randomIndex];
            tiles[randomIndex] = temp;
        }
    }

    public void HideLevelInSky(float height)
    {
        foreach (var tile in tiles)
        {
            if (tile.transform != null)
                tile.transform.position = tile.originalPosition + Vector3.up * height;
        }
    }

    public IEnumerator AnimateMapFall(float speed)
    {
        ShuffleTiles(); 
        int activeAnimations = 0;

        foreach (var tile in tiles)
        {
            if (tile.transform != null)
            {
                activeAnimations++;

                StartCoroutine(MoveTileToTarget(tile, tile.originalPosition, speed, () => activeAnimations--));


                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
        }


        while (activeAnimations > 0)
        {
            yield return null;
        }
    }

    public IEnumerator AnimateMapDrop(float dropDepth, float speed)
    {
        ShuffleTiles(); 
        int activeAnimations = 0;

        float targetY = -dropDepth;

        foreach (var tile in tiles)
        {
            if (tile.transform != null)
            {
                activeAnimations++;

                Vector3 targetPos = new Vector3(tile.originalPosition.x, targetY, tile.originalPosition.z);

                StartCoroutine(MoveTileToTarget(tile, targetPos, speed, () => activeAnimations--));


                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
        }

        while (activeAnimations > 0)
        {
            yield return null;
        }
    }


    private IEnumerator MoveTileToTarget(TileData tile, Vector3 target, float speed, System.Action onComplete)
    {
        while (tile.transform != null && Vector3.Distance(tile.transform.position, target) > 0.01f)
        {
            tile.transform.position = Vector3.MoveTowards(tile.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        if (tile.transform != null)
        {
            tile.transform.position = target;
        }
        onComplete?.Invoke();
    }
}