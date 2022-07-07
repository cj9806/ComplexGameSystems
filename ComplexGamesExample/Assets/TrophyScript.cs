using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
public class TrophyScript : MonoBehaviour
{
    public float rotationspeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, rotationspeed, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        FindObjectOfType<GameFlowManager>().EndGame(true);
    }
}
