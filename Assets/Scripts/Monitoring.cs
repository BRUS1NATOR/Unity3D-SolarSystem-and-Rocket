using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Monitoring : MonoBehaviour
{
    public XCharts.BaseChart velocityChart;
    public XCharts.BaseChart accelerationChart;
    public XCharts.BaseChart heightChart;

    public Rocket rocket;
    public Planet planet;

    public float timeRemaining = 10;
    public float pauseAt;
    public int time = 0;

    private bool hidden = true;

    private void Start()
    {
        heightChart.ClearData();
        velocityChart.ClearData();
        accelerationChart.ClearData();
        pauseAt = timeRemaining;
        StartCoroutine(Monitor());
        heightChart.title.text = $"Высота относительно \"{planet.reference.planetName}\"";
    }

    public IEnumerator Monitor()
    {
        yield return new WaitForSecondsRealtime(1f);
        while (true)
        {
            time++;
            heightChart.AddData(0, time, (float)Math.Round(Simulation.DistanceToPlanet(rocket.transform.position, planet),1) * Simulation.unitRocketM);
            velocityChart.AddData(0, time, (float)(rocket.GetVelocityRelativeToPlanet(planet)).magnitude * Simulation.unitRocketM, null);
            accelerationChart.AddData(0, time, (float)rocket.acceleration.magnitude * Simulation.unitRocketM, null);
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.H))
        {
            hidden = !hidden;
            Hide(hidden);
        }

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = pauseAt;
            Debug.Break();
        }
    }

    void Hide(bool hide)
    {
        this.GetComponent<Canvas>().enabled = hide;
    }
}