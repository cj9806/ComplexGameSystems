using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTMG;
using static RTMG.MapGenerator;
public class DoorwayPlacer : MonoBehaviour
{
    public GameObject doorwayPrefab;
    // Start is called before the first frame update
    public void OnGenerated(Map finishedMap)
    {
        foreach(DoorLocation dl in finishedMap.doorways)
        {
            GameObject newDoorway = Instantiate(doorwayPrefab, transform);
            newDoorway.transform.position = new Vector3(dl.position.x, 0, dl.position.y);
            newDoorway.transform.eulerAngles = new Vector3(0, dl.rotation, 0);
        }
    }
}
