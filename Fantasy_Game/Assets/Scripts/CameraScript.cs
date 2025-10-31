using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System;

public class CameraScript : MonoBehaviour
{

    [SerializeField] private GameObject Player;
    [SerializeField] private GameObject ground;
    [SerializeField] private CinemachineOrbitalFollow follow;
    private float groundY;

    void Update()
    {
        float camDistance = Player.transform.position.y - groundY;
        var orbits = follow.Orbits;

        GroundHeight();

        if (camDistance > 3)
        {
            //smoothly increase look angle
            orbits.Bottom.Height = Mathf.Lerp(orbits.Bottom.Height, -2, Time.deltaTime * 2f);
            follow.Orbits = orbits;
        }

        else
        {
            //smoothly decreases look angle
            orbits.Bottom.Height = Mathf.Lerp(orbits.Bottom.Height, 0, Time.deltaTime * 2f);
            follow.Orbits = orbits;
        }

    }


    void GroundHeight()
    {
        foreach (Transform child in ground.transform)
        {
            Terrain terrain = child.GetComponent<Terrain>();
            if (terrain != null)
            {

                Vector3 terrainPos = terrain.GetPosition();
                Vector3 terrainSize = terrain.terrainData.size;


                Vector3 pos = Player.transform.position;

                //makes sure that the only terrain it looks at is the one the player is standing on
                if (pos.x >= terrainPos.x && pos.x <= terrainPos.x + terrainSize.x && pos.z >= terrainPos.z && pos.z <= terrainPos.z + terrainSize.z)
                {
                    float y = terrain.SampleHeight(pos) + terrainPos.y;
                    groundY = y;
                    break;
                }


            }
        }
    }
}
