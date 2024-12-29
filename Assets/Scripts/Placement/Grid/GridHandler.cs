using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GridHandler : MonoBehaviour
{
    List<GameObject> gridCells = new List<GameObject>();
    public float cellSize = 1;

    [Header("Generation")]
    public Vector2 size = new Vector2(10, 10);
    public float noiseFrequency = 2f;
    public float waterThreshold = 0.1f;
    public float natureThreshold = 0.4f;
    [Header("Generation References")]
    public Transform gridCellHolder;
    public GameObject gridCellPrefab;
    public List<Buildable> natureBuildables = new List<Buildable>();

    void Start()
    {
        Invoke(nameof(CreateGrid), 0);
    }

    void CreateGrid() {

        for(int i = 0; i < size.x; i++) {

            for(int j = 0; j < size.y; j++) {

                Vector3 pos = GetHexPos(i, j);

                float seed = Random.Range(0, 1000000);
                float waterValue = Mathf.PerlinNoise((pos.x + seed) / noiseFrequency, (pos.z + seed) / noiseFrequency);
                float falloffValue = GetFalloff(i, j);
                float combinedValue = waterValue * falloffValue;

                if(combinedValue < waterThreshold) {
                    continue;
                }

                GridTile cell = CreateGridCell(pos);

                float natureValue = Mathf.PerlinNoise((pos.x + seed * 2) / noiseFrequency, (pos.z + seed * 2) / noiseFrequency);

                if(natureValue < natureThreshold) {
                    cell.Initialize(natureBuildables[Random.Range(0, natureBuildables.Count)]);
                    continue;
                }
                cell.Initialize();

            }

        }

        Debug.Log("Done");

    }

    private GridTile CreateGridCell(Vector3 pos) {
        float animationTime = 0.5f;
        float animationTimeOffset = Random.Range(0, 0.2f);

        GameObject cell = Instantiate(gridCellPrefab, gridCellHolder);
        cell.transform.localScale = Vector3.zero;
        cell.transform.position = pos;

        cell.transform.DOScale(Vector3.one, animationTime + animationTimeOffset).SetEase(Ease.OutBounce);

        gridCells.Add(cell);
        cell.name = $"Grid Cell {gridCells.IndexOf(cell)}";

        return cell.GetComponent<GridTile>();
    }
    
    private Vector3 GetHexPos(int x, int y) {
        float xPos = x * cellSize + ((y % 2 == 1) ? (cellSize / 2) : 0);
        float yPos = y * cellSize * Mathf.Cos(Mathf.Deg2Rad * 30);
        return new Vector3(xPos, 1.2f, yPos);
    }

    private float GetFalloff(float x, float y) {
        float halfWidth = size.x / 2f;
        float halfHeight = size.y / 2f;

        float nx = (x - halfWidth) / halfWidth;
        float ny = (y - halfHeight) / halfHeight;

        float distance = Mathf.Sqrt(nx * nx + ny * ny); 
        float falloff = Mathf.Clamp01(1f - distance); 

        return falloff;
    }

}
