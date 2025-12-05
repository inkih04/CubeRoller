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
        // Guardar posiciones iniciales
        foreach (Transform child in transform)
        {
            tiles.Add(new TileData { transform = child, originalPosition = child.position });
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

    // Animación de entrada (Cae del cielo al sitio)
    public IEnumerator AnimateMapFall(float speed)
    {
        bool allTilesLanded = false;
        while (!allTilesLanded)
        {
            allTilesLanded = true;
            foreach (var tile in tiles)
            {
                if (tile.transform == null) continue;
                if (Vector3.Distance(tile.transform.position, tile.originalPosition) > 0.01f)
                {
                    tile.transform.position = Vector3.MoveTowards(tile.transform.position, tile.originalPosition, speed * Time.deltaTime);
                    allTilesLanded = false;
                }
                else
                {
                    tile.transform.position = tile.originalPosition;
                }
            }
            yield return null;
        }
    }

    // --- NUEVO: Animación de salida (Cae del sitio al abismo) ---
    public IEnumerator AnimateMapDrop(float dropDepth, float speed)
    {
        bool allTilesGone = false;
        // Destino: muy abajo
        float targetY = -dropDepth;

        while (!allTilesGone)
        {
            allTilesGone = true;
            foreach (var tile in tiles)
            {
                if (tile.transform == null) continue;

                // Movemos hacia abajo
                Vector3 targetPos = new Vector3(tile.originalPosition.x, targetY, tile.originalPosition.z);

                if (tile.transform.position.y > targetY + 0.1f)
                {
                    tile.transform.position = Vector3.MoveTowards(tile.transform.position, targetPos, speed * Time.deltaTime);
                    allTilesGone = false;
                }
            }
            yield return null;
        }
    }
}