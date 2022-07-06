using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    
    public Vector2 minMaxIntensity = new Vector2(3,6);
    public float flickerTime = 0.2f;
    private float flickedTime = 0f;
    private Light myLight;
    // Start is called before the first frame update
    void Start()
    {
        myLight = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        flickedTime += Time.deltaTime;
        if(flickedTime >= flickerTime)
        {
            flickedTime = 0;
            myLight.intensity = Random.Range(minMaxIntensity.x, minMaxIntensity.y);
        }
    }
}
