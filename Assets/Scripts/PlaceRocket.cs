using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class PlaceRocket : MonoBehaviour
{
    public GameObject rocketMiniature;
    public Rocket realRocket;

    public CelestialBody planetNow;
    public Vector3 visualOffset;

    public Vector3 rayToPlanet;

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    public void UpdatePosition()
    {
        Debug.Log(rayToPlanet);
        if (planetNow != null)
        {
            Debug.DrawRay(planetNow.transform.position + rayToPlanet * planetNow.transform.localScale.x, -rayToPlanet * planetNow.transform.localScale.x, Color.red, 1f);
            RaycastHit[] hit = Physics.RaycastAll(planetNow.transform.position + rayToPlanet * planetNow.transform.localScale.x, -rayToPlanet * planetNow.transform.localScale.x,
                100000f, 1 << LayerMask.NameToLayer("MiniaturePlanets"));
            foreach(var h in hit)
            {
                if(h.transform.gameObject == planetNow.gameObject)
                {
                    SetPlanet(h.collider.gameObject.GetComponent<CelestialBody>(), h.point, ToSpherical(h.collider.transform.InverseTransformPoint(h.point)));
                }
            }
        }
    }


    private void OnScene(SceneView scene)
    {
        if (Selection.activeInstanceID == this.gameObject.GetInstanceID())
        {
            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 1 << LayerMask.NameToLayer("MiniaturePlanets")))
                {
                    SetPlanet(hit.collider.gameObject.GetComponent<CelestialBody>(), hit.point, ToSpherical(hit.collider.transform.InverseTransformPoint(hit.point)));
                }

                GUIUtility.hotControl = controlId;
                Event.current.Use();
            }
        }
    }

    public void SetPlanet(CelestialBody celestial, Vector3 point, Vector2 longLat)
    {
        planetNow = celestial;

        visualOffset = planetNow.visualOffset;
        rayToPlanet = (point - planetNow.transform.position).normalized;

        Debug.Log($"Ракета расположена на долготе {longLat.y} и широте {longLat.x}");

        rocketMiniature.transform.position = point;
        rocketMiniature.transform.rotation = Quaternion.FromToRotation(Vector3.up, rayToPlanet);

        realRocket.transform.position = celestial.reference.transform.position + (rayToPlanet * (celestial.reference.transform.localScale.x / 2f));
        realRocket.transform.rotation = rocketMiniature.transform.rotation;
        realRocket.attachedTo = celestial.reference;
        realRocket.rocketLatitude = longLat.y;
        // planet.CalculateRotationVelocity(i);
    }

    //stackoverflow
    public static Vector2 ToSpherical(Vector3 position)
    {
        // The vertical coordinate (y) varies as the sine of latitude, not the cosine.
        float lat = Mathf.Asin(position.y * 2) * Mathf.Rad2Deg;

        // Use the 2-argument arctangent, which will correctly handle all four quadrants.
        float lon = Mathf.Atan2(position.x, position.z) * Mathf.Rad2Deg;

        // I usually put longitude first because I associate vector.x with "horizontal."
        return new Vector2(lon, lat);
    }
}
