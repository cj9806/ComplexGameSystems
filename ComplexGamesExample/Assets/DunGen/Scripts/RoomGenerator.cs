using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [SerializeField] GridGenerator grid;
    [Header("Rooms")]
    public GameObject startRoomFab;
    public GameObject endRoomFab;
    public GameObject[] corriderRooms;
    public GameObject[] cornerRooms;

    [Header("Settings")]
    public int extraNodes;

    [SerializeField]
    public bool generateOnRuntime;
    public bool GORT { get => generateOnRuntime; }

    [HideInInspector]
    public bool manulallySelectNodes = false;
    [SerializeField]
    private HandPlacedNode[] required;

    //private under the hood things--------------------------------------
    [HideInInspector]
    public Transform startRoomPos;
    GameObject parent;
    private List<Vector3Int> path;

    private LinkedList<Room> ActiveRooms = new LinkedList<Room>();
    Dictionary<Vector3Int, Cell> cellLookup = new Dictionary<Vector3Int, Cell>();
    private bool exsitingDungeon = false;
    //--------------------------------------------------------------------
    private void Start()
    {
        if (generateOnRuntime)
            MakeDungeon();
    }

    void GenerateStartAndEnd()
    {
        ActiveRooms.AddFirst(Instantiate(startRoomFab, grid.GetRandCellPos(), Quaternion.identity).GetComponent<Room>());
        
        for (int i = 0; i < 100; ++i)
        {
            int yRot = Random.Range(0, 3);
            ActiveRooms.First.Value.transform.Rotate(0, yRot * 90, 0);
            if (CheckIfDoorsInGrid(ActiveRooms.First.Value))
                break;
        }
        ActiveRooms.First.Value.transform.SetParent(parent.transform);
        for (int i = 0; i < 100; ++i)
        {
            Vector3 endPos = grid.GetRandCellPos();
            if (endPos != ActiveRooms.First.Value.transform.position && endPos != ActiveRooms.First.Value.doors[0].position)
            {
                ActiveRooms.AddLast(Instantiate(endRoomFab, endPos, Quaternion.identity).GetComponent<Room>());
                for (int j = 0; j < 100; ++j)
                {
                    int yRot = Random.Range(0, 3);
                    ActiveRooms.Last.Value.transform.Rotate(0, yRot * 90, 0);
                    if (CheckIfDoorsInGrid(ActiveRooms.Last.Value) && ActiveRooms.Last.Value.doors[0].position != ActiveRooms.First.Value.doors[0].position && ActiveRooms.Last.Value.doors[0].position != ActiveRooms.First.Value.transform.position)
                    {
                        ActiveRooms.Last.Value.transform.SetParent(parent.transform);
                        break;
                    }
                }
                break;
            }
        }
        cellLookup[grid.GetCellIndices(ActiveRooms.First.Value.transform.position)].hasRoom = true;
        cellLookup[grid.GetCellIndices(ActiveRooms.Last.Value.transform.position)].hasRoom = true;
        startRoomPos = ActiveRooms.First.Value.transform;
    }
    
    void GenerateExtraNodes()
    {
        //for each extra node
        for(int i = 0; i < extraNodes; ++i)
        {
            //give the code 100 chances to do this right
            for (int j = 0; j < 100; ++j)
            {
                //get a random position in the grid
                Vector3 midPos = grid.GetRandCellPos();
                if (cellLookup[grid.GetCellIndices(midPos)].hasRoom)
                    continue;
                //spawn new room
                int cOrC = Random.Range((int)1, 3);
                GameObject[] objs = cOrC == 1 ? cornerRooms : corriderRooms;
                GameObject room = Instantiate(objs[Random.Range(0, objs.Length)], midPos, Quaternion.identity, parent.transform);
                //check to see if room spawned correctly
                if (CheckIfDoorsInGrid(room.GetComponent<Room>()) && !CheckIfDoorsInRoom(room.GetComponent<Room>()))
                {
                    //add to list of rooms
                    ActiveRooms.AddBefore(ActiveRooms.Last, room.GetComponent<Room>());
                    //tell dictionary cell is now occupied
                    cellLookup[grid.GetCellIndices(room.transform.position)].hasRoom = true;
                    break;
                }
                else
                {
                    if (generateOnRuntime)
                        Destroy(room);
                    else
                        DestroyImmediate(room);

                }
            }
        }
    }

    //generate list to path and outputs a list of cells
    List<Vector3Int> Pathfind(Vector3Int start, Vector3Int end)
    {
        
        List<Vector3Int> openList = new List<Vector3Int>();
        List<Vector3Int> closedList = new List<Vector3Int>();

        openList.Add(start);
        Vector3Int activeNode = openList[0];

        //while open list is not empty
        while (openList.Count > 0)
        {
            //set the active node to the node at the front of the open list
            activeNode = openList[0];
            //remove the first node from the open list
            openList.RemoveAt(0);
            //add the active node to the end of closed list
            closedList.Add(activeNode);
            //check to see if path is complete
            if (activeNode == end) 
                break; 

            
            cellLookup[activeNode].neighbors = new List<Cell>();
            //for each possible neighbor in list(i=0 i<4)
            for(int i = 0; i < 4; ++i) 
            {
                Vector3Int neighborPos = new Vector3Int();
                Vector3Int checkNeighbors = new Vector3Int();
                
                //check if neighbor is in graph
                switch (i)
                {
                    //north
                    case 0:
                        {
                            checkNeighbors = new Vector3Int(activeNode.x, activeNode.y, activeNode.z + 1);
                            break;
                        }
                    //east
                    case 1:
                        {
                             checkNeighbors = new Vector3Int(activeNode.x+1, activeNode.y, activeNode.z);
                            break;
                        }
                    //south
                    case 2:
                        {
                             checkNeighbors = new Vector3Int(activeNode.x, activeNode.y, activeNode.z-1);
                            break;
                        }
                    //west
                    case 3:
                        {
                            checkNeighbors = new Vector3Int(activeNode.x-1, activeNode.y, activeNode.z);
                            break;
                        }
                }
                bool isNeighbor = grid.CellInGrid(checkNeighbors);

                //add valid neighbors to open list
                if (isNeighbor)
                {
                    if (!cellLookup[checkNeighbors].hasRoom)
                        neighborPos = checkNeighbors;
                    else continue;                   
                }
                else
                {
                    continue;
                }

                bool inOpenList = false;
                foreach (Vector3Int cells in openList)
                {
                    if (neighborPos == cells)
                    {
                        inOpenList = true;
                        break;
                    }
                }

                //if neighbor is in open list
                if (inOpenList)
                {
                    //overwite gscore if it is lower
                    if (cellLookup[neighborPos].gScore > cellLookup[activeNode].gScore + 1)
                    {
                        cellLookup[neighborPos].gScore = cellLookup[activeNode].gScore + 1;
                        cellLookup[neighborPos].previous = activeNode;
                    }
                    continue;
                }

                //check to see if node is in closed list
                bool inClosedList = false;
                foreach (Vector3Int cells in closedList)
                {
                    if (neighborPos == cells)
                    {
                        inClosedList = true;
                        break;
                    }
                }

                //if the node is not already in the closed list
                if (!inClosedList)
                {
                    //add it to open list and update the G score
                    openList.Add(neighborPos);
                    cellLookup[neighborPos].gScore = cellLookup[activeNode].gScore + 1;
                    cellLookup[neighborPos].previous = activeNode;

                    cellLookup[neighborPos].hScore = Mathf.Abs(end.x - neighborPos.x) + Mathf.Abs(end.z - neighborPos.z);
                }
            }
            //sort the open list
            SortCells(openList);            
        }

        //if path failed
        if (openList.Count <= 0 && activeNode != end)
        {
            //return null
            return null;
        }
        List<Vector3Int> toReturn = new List<Vector3Int>();
        int safety = 0;
        Cell tempcell = cellLookup[end];
        while (tempcell.position != start && safety < closedList.Count)
        {
            toReturn.Add(tempcell.position);
            
            tempcell = cellLookup[tempcell.previous];
            ++safety;
        }
        toReturn.Add(tempcell.position);
        return toReturn;
    }

    void GeneratePath(List <Vector3Int> cells, LinkedListNode<Room> nextRoom)
    {
        cells.Reverse();
        cells.Add(grid.GetCellIndices(nextRoom.Value.transform.position));  
        cellLookup[cells[0]].previous = grid.GetCellIndices(nextRoom.Previous.Value.transform.position);//problomatic in current implementation
        for (int i = 0; i < cells.Count - 1; ++i)
        {
            Vector3Int prev = cellLookup[cells[i]].previous;
            Vector3Int pos = cellLookup[cells[i]].position;
            Vector3Int next = cellLookup[cells[i + 1]].position;
            GameObject room;
            
            
            //if straight
            if (prev.x == next.x)
            {
                //place with rotation 0 availible rooms[2]
                room = Instantiate(corriderRooms[Random.Range((int)0,corriderRooms.Length)], grid.GetCellPos(pos), Quaternion.identity);
                ActiveRooms.AddAfter(ActiveRooms.First, room.GetComponent<Room>());
            }
            else if (prev.z == next.z) 
            {
                //place with rotation 90
                room = Instantiate(corriderRooms[Random.Range((int)0, corriderRooms.Length)], grid.GetCellPos(pos), Quaternion.Euler(0, 90, 0));
                ActiveRooms.AddAfter(ActiveRooms.First, room.GetComponent<Room>());
            }
            //corner
            else
            {
                room = Instantiate(cornerRooms[Random.Range((int)0,cornerRooms.Length)], grid.GetCellPos(pos), Quaternion.identity);
                Room doors = room.GetComponent<Room>();
                for(int j = 0; j < 4; ++j)
                {
                    room.transform.rotation = Quaternion.Euler(0,j*90,0);

                    //see if (either door 1 or door 2 match with previous room) and (door room or door 2 match with next room)
                    if ((grid.GetCellIndices(doors.doors[0].position) == prev ||
                        grid.GetCellIndices(doors.doors[1].position) == prev)

                        &&
                        (grid.GetCellIndices(doors.doors[0].position) == next ||
                        grid.GetCellIndices(doors.doors[1].position) == next)) break;
                    
                }
            }
            room.transform.SetParent(parent.transform);
            cellLookup[pos].hasRoom = true;
        }
    }
    //------------------------------Utility Funtions---------------------------------------------\\
    void SortCells(List<Vector3Int> cells)
    {
        bool noSwap = false;

        while (!noSwap)
        {
            noSwap = true;

            for (int i = 0; i < cells.Count; ++i)
            {
                if (i == 0) continue;
                if(cellLookup[cells[i-1]].FScore > cellLookup[cells[i]].FScore)
                {
                    noSwap = false;
                    Vector3Int temp = cells[i];
                    cells[i] = cells[i - 1];
                    cells[i - 1] = temp;
                }
            }
        }
    }
    bool CheckIfDoorsInGrid(Room room)
    {
        foreach (Transform door in room.doors)
        {
            if (!grid.CellInGrid(door.position))
                return false;
        }
        return true;
    }
    bool CheckIfDoorsInRoom(Room room)
    {
        foreach(Transform door in room.doors)
        {
            if (cellLookup[grid.GetCellIndices(door.position)].hasRoom)
                return true;
        }
        return false;
    }
    void GenerateLookup()
    {
        cellLookup.Clear();
        for (int i = 0; i < grid.extents.x; ++i)
        {
            for (int j = 0; j < grid.extents.y; ++j)
            {
                //popultate dictionary
                cellLookup.Add(new Vector3Int(i, 0, j), new Cell());
                cellLookup[new Vector3Int(i, 0, j)].position = new Vector3Int(i, 0, j);
            }
        }
    }
    
    //-----------------------------------------------------------------------------------\\

    public void MakeDungeon()
    {
        //destroy current dungeon and make new varibles
        if (!generateOnRuntime && exsitingDungeon)
        {
            DestroyImmediate(parent.gameObject);
            ActiveRooms.Clear();
        }
        else if(exsitingDungeon)
        {
            Destroy(parent.gameObject);
            ActiveRooms.Clear();
        }
        GenerateLookup();
        

        for (int s = 0; s < 200; ++s)
        {
            parent = new GameObject();
            parent.name = "Dungeon";

            GenerateLookup();
            if (!manulallySelectNodes)
            {
                GenerateStartAndEnd();
                GenerateExtraNodes();
            }
            else
            {
                ActiveRooms.Clear();
                foreach(HandPlacedNode node in required)
                {
                    GameObject tempObj = Instantiate(node.roomType, grid.GetCellPos(node.gridPosition), Quaternion.Euler(0, node.rotation, 0));
                    ActiveRooms.AddLast(tempObj.GetComponent<Room>());
                    tempObj.transform.SetParent(parent.transform);
                    cellLookup[node.gridPosition].hasRoom = true;
                }
            }
            LinkedList<Room> temp = new LinkedList<Room>(ActiveRooms);
            for (LinkedListNode<Room> i = temp.First; i != temp.Last; i = i.Next)
            {
                if (i == temp.First)
                {
                    path = Pathfind(grid.GetCellIndices(i.Value.doors[0].position), grid.GetCellIndices(i.Next.Value.doors[0].position));
                }
                else
                    path = Pathfind(grid.GetCellIndices(i.Value.doors[1].position), grid.GetCellIndices(i.Next.Value.doors[0].position));
                if (path == null)
                {
                    ActiveRooms.Clear();
                    if (generateOnRuntime)
                    {
                        Destroy(parent.gameObject);
                    }
                    else
                        DestroyImmediate(parent.gameObject);
                    break;
                }
                GeneratePath(path, i.Next);
            }
            if (path != null)
            {
                if (grid.GetCellPos(path[path.Count - 1]) == ActiveRooms.Last.Value.transform.position)
                {
                    break;
                }
                break;
            }


            if (s == 199)
            {
                parent = new GameObject();
                parent.name = "Dungeon";
                GenerateLookup();
                GenerateStartAndEnd();
                path = Pathfind(grid.GetCellIndices(ActiveRooms.First.Value.doors[0].position), grid.GetCellIndices(ActiveRooms.Last.Value.doors[0].position));
                GeneratePath(path,ActiveRooms.Last);
            }
        }
        exsitingDungeon = true;
    }
    [System.Serializable]
    public struct HandPlacedNode
    {
        public GameObject roomType;
        public Vector3Int gridPosition;
        [Tooltip("only assign values 0,90,180,270")]
        public float rotation;        
    }
}

