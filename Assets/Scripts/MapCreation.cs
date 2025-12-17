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

    // Función auxiliar para desordenar la lista (Shuffle)
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
        ShuffleTiles(); // Desordenamos las tiles para que caigan aleatoriamente
        int activeAnimations = 0;

        foreach (var tile in tiles)
        {
            if (tile.transform != null)
            {
                activeAnimations++;
                // Iniciamos la caída individual de esta tile
                StartCoroutine(MoveTileToTarget(tile, tile.originalPosition, speed, () => activeAnimations--));

                // Esperamos un tiempo aleatorio muy breve antes de lanzar la siguiente
                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
        }

        // Esperamos a que todas hayan terminado
        while (activeAnimations > 0)
        {
            yield return null;
        }
    }

    public IEnumerator AnimateMapDrop(float dropDepth, float speed)
    {
        ShuffleTiles(); // Desordenamos para que caigan al vacío de forma caótica
        int activeAnimations = 0;
        // Usamos una profundidad mucho mayor si es necesario, calculada desde su origen
        float targetY = -dropDepth;

        foreach (var tile in tiles)
        {
            if (tile.transform != null)
            {
                activeAnimations++;
                // Calculamos el destino muy abajo
                Vector3 targetPos = new Vector3(tile.originalPosition.x, targetY, tile.originalPosition.z);

                StartCoroutine(MoveTileToTarget(tile, targetPos, speed, () => activeAnimations--));

                // Retraso aleatorio entre caídas
                yield return new WaitForSeconds(Random.Range(0.005f, 0.02f));
            }
        }

        while (activeAnimations > 0)
        {
            yield return null;
        }
    }

    // Corrutina individual para mover una sola pieza
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