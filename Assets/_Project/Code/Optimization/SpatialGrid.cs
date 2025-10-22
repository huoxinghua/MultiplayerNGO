using System.Collections.Generic;
using UnityEngine;

public class SpatialGrid
{
    private Dictionary<Vector2Int, List<LightObject>> _grid = new();
    private float _cellSize;

    public SpatialGrid(float cellSize)
    {
        this._cellSize = cellSize;
    }

    private Vector2Int GetCellCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / _cellSize),
            Mathf.FloorToInt(position.z / _cellSize)
        );
    }

    public void AddLight(LightObject light)
    {
        var coord = GetCellCoord(light.transform.position);
        if (!_grid.ContainsKey(coord))
            _grid[coord] = new List<LightObject>();

        _grid[coord].Add(light);
    }

    public List<LightObject> GetNearbyLights(Vector3 position, int rangeInCells = 1)
    {
        List<LightObject> result = new();
        var center = GetCellCoord(position);

        for (int x = -rangeInCells; x <= rangeInCells; x++)
        {
            for (int z = -rangeInCells; z <= rangeInCells; z++)
            {
                var coord = center + new Vector2Int(x, z);
                if (_grid.TryGetValue(coord, out var lights))
                {
                    result.AddRange(lights);
                }
            }
        }

        return result;
    }
}
