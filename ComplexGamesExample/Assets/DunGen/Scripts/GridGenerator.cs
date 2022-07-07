using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    //public Vector2Int GridExtents = new Vector2Int(5,5);
    
    public Vector2Int extents = new Vector2Int(5, 5);
    public int cellSize;
    private float PrivateCellSize;

    public bool DrawOnlyExtents = true;

    private Vector3 offset => new Vector3()
    {
        x = PrivateCellSize / 2,
        y = PrivateCellSize / 2,
        z = PrivateCellSize / 2,
    };

    public BoundsInt CellBounds
    {
        get => new BoundsInt()
        {
            position = transform.position.ToInt() /*- (extents / 2)*/,
            size =  new Vector3Int(extents.x,1,extents.y)
        };
    }

    //returns cell indecies reletive to ceneter of array
    public Vector3Int GetCellIndices(Vector3 position)
    {
        Vector3Int cell = GetCellIndicesNotClamped(position);
        //if (!CellBounds.Contains(cell))
        //{
        //    return Vector3Int.zero;
        //}

        return cell;
    }
    //returns worldspace position of center of cell in given indeces
    public Vector3 GetCellPos(Vector3Int indices)
    {
        //clamp indeces
        //return the clamped vertion * cell size + offset
        Vector3 toReturn = new Vector3
        {
            x = Mathf.Clamp(indices.x, CellBounds.xMin, CellBounds.xMax),
            y = Mathf.Clamp(indices.y, CellBounds.yMin, CellBounds.yMax),
            z = Mathf.Clamp(indices.z, CellBounds.zMin, CellBounds.zMax)
        };
        toReturn *= PrivateCellSize;

        return toReturn + offset;
    }
    public Vector3Int GetCellIndicesNotClamped(Vector3 position)
    {
        //Debug.LogError("NOT YET IMPLEMENTED / NEEDS FIXING");
        Vector3Int cell = (position - CellBounds.position).ToInt();
        if (position.x < 0 || position.y < 0 || position.z < 0)
            cell = new Vector3Int(-10, -10, -10);
        return cell / (int)PrivateCellSize;
    }
    public Vector3 GetCellPosNotClamped(Vector3Int indices)
    {
        return ((Vector3)indices * PrivateCellSize) + offset;
    }

    public Vector3 GetRandCellPos()
    {
        Vector3 toReturn = new Vector3(Random.Range(CellBounds.xMin, CellBounds.xMax),
                                       Random.Range(CellBounds.yMin, CellBounds.yMax),
                                       Random.Range(CellBounds.zMin, CellBounds.zMax));
        
        return (toReturn * PrivateCellSize) + offset;
    }
    //return if worldspace position is in the grid
    public bool CellInGrid(Vector3 pos)
    {
        //return CellBounds.Contains((pos/cellSize).ToInt());
        return CellBounds.Contains(GetCellIndicesNotClamped(pos));
    }
    //returns if indicies is in the grid
    public bool CellInGrid(Vector3Int indicies)
    {
        return CellBounds.Contains(indicies);
    }

    private void OnValidate()
    {
        PrivateCellSize = cellSize;
    }

    //[System.Serializable]


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(CellBounds.center * PrivateCellSize, (Vector3)CellBounds.size * PrivateCellSize);
        //Gizmos.DrawSphere(CellBounds.position, 5);
        
        if (DrawOnlyExtents)
            return;

        for(int x = 0; x < extents.x; ++x)
        {
            for (int y = 0; y < extents.y; ++y)
            {

                Vector3 localPos = new Vector3(x, 0, y);
                localPos *= PrivateCellSize;

                Gizmos.DrawWireCube(CellBounds.position + localPos + offset, Vector3.one * PrivateCellSize);
            }
            
        }
    }
}
public class Cell
{
    public Vector3Int position;
    public Vector3Int previous;
    public List<Cell> neighbors;
    public bool hasRoom = false;

    public int gScore;
    public int hScore = 0;
    public int FScore
    {
        get => gScore + hScore;
    }
}
public static class Vector3Extensions
{
    public static Vector3Int ToInt(this Vector3 vector)
    {
        return new Vector3Int((int)vector.x, (int)vector.y, (int)vector.z);
    }
}
