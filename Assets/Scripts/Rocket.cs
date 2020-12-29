using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static OrbitDisplay;

public class Rocket : MonoBehaviour
{
    public Vector3d[] forces;
    public double[] distances;
    public double[] forcesD;

    public Arrow distanceFirst;
    public Arrow distanceSecond;
    public Arrow accelerationArrow;
    public Arrow reactiveArrow;
    public Arrow gravityArrow;
    public Arrow velocityArrow;

    public Text fuelInfo;

    public Vector3d velocity = Vector3d.zero;
    public Vector3d acceleration;

    public Vector3d nextPos;

    public Vector3d additionalAngularVelocity = Vector3d.zero;
    public float rocketLongtitude = 0;
    public float rocketLatitude = 0;

    public Vector3 direction
    {
        get
        {
            return transform.up;
        }
    }

    public float massOverall
    {
        get
        {
            return massRocket + stages.overallMass;
        }
    }

    public float massRocket;

    [SerializeField]
    public RocketStages stages = new RocketStages();

    [Range(0, 1)]
    public float throttle;

    public Vector3d reactive;
    public Vector3d gravityForces;

    RaycastHit hitInfo;
    Ray ray;

    public static Rocket instance;
    public Planet attachedTo;

    public void Awake()
    {
        if (instance != null)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(this);
            instance = this;
        }
    }

    private void Start()
    {
        WorldManager.ResetWorldOffset(this.gameObject);
        transform.rotation = Quaternion.identity;
        if (attachedTo != null)
        {
            attachedTo = null;
            // attachedTo.AttachRocket(transform.position, this);
            // attachedTo.attached = this.gameObject;
        }

        forces = new Vector3d[CelestiaBodiesManager.instance.celestialBodies.Length];
        forcesD = new double[CelestiaBodiesManager.instance.celestialBodies.Length];
        distances = new double[CelestiaBodiesManager.instance.celestialBodies.Length];
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            throttle -= 0.05f;
            if (throttle < 0)
            {
                throttle = 0;
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            throttle += 0.05f;
            if (throttle > 1)
            {
                throttle = 1;
            }
        }
    }


    private void CalculateForces()
    {
        Planet[] planets = CelestiaBodiesManager.instance.celestialBodies.Select(x => x.reference).ToArray();

        for (int i = 0; i < planets.Length; i++)
        {
            forces[i] = CelestialBody.CalculateVelocity(WorldManager.worldOffset, CelestiaBodiesManager.instance.celestialBodies[i].position, CelestiaBodiesManager.instance.celestialBodies[i].mass, WorldType.Rocket);
            forcesD[i] = forces[i].magnitude;
            distances[i] = Simulation.DistanceToPlanet(transform.position, planets[i]);
        }

        int minDistIndex = 0;
        int minDistSecondIndex = 0;

        for (int i = 0; i < CelestiaBodiesManager.instance.celestialBodies.Length; i++)
        {
            if (distances[i] < distances[minDistIndex])
            {
                minDistSecondIndex = minDistIndex;
                minDistIndex = i;
            }
        }

        CelestialBody minDist = CelestiaBodiesManager.instance.celestialBodies[minDistIndex];
        CelestialBody secondMinDist = CelestiaBodiesManager.instance.celestialBodies[minDistSecondIndex];

        distanceFirst.Set(minDist.reference.transform.position,
            minDist.name + "\t" + Math.Round(distances[minDistIndex], 1) * Simulation.unitRocketKM + " km",
            minDist.GetComponent<MeshRenderer>().sharedMaterial);

        distanceSecond.Set(secondMinDist.reference.transform.position,
                    secondMinDist.name + "\t" + Math.Round(distances[minDistSecondIndex], 1) * Simulation.unitRocketKM + " km",
                    secondMinDist.GetComponent<MeshRenderer>().sharedMaterial);

        accelerationArrow.Set((Vector3)acceleration.normalized * 100,"Acceleration: \t" + Math.Round(acceleration.magnitude, 5) * Simulation.unitRocketM + " m/s");
        reactiveArrow.Set(-(Vector3)reactive.normalized * 100, "Reactive: \t" + Math.Round(reactive.magnitude, 5) * Simulation.unitRocketM + " m/s");
        gravityArrow.Set((Vector3)gravityForces.normalized * 100, "Gravitational: \t" + Math.Round(gravityForces.magnitude, 5) * Simulation.unitRocketM + " m/s");
        velocityArrow.Set((Vector3)velocity.normalized * 100, "Velocity: \t" + Math.Round(velocity.magnitude, 5) * Simulation.unitRocketM + " m/s");

        fuelInfo.text = "Throttle " + Math.Round(throttle * 100,3) + "%\n";
        int n = 1;
        foreach (RocketStage s in stages)
        {
            fuelInfo.text += ($"Stage №{n}, time left: \t{Math.Round(s.fuelMass / s.fuelConsamptionPerSecond / throttle,1)}\n");
            n++;
        }
    }


    public void UpdatePosition()
    {
        CalculateForces();

        var targetList = CelestiaBodiesManager.instance.celestialBodies
                 .Select(x => new PositionAndMass(x.reference.positionRelativeToRocket, x.mass, x.name))
                 .ToList();

        //LOCAL POSITION
        if (attachedTo != null)
        {
            //Если ракета покидает планету
            //5 - условная величина, около 5км над планетой)
            if (Simulation.DistanceToPlanet(transform.position, attachedTo) > 5)
            {
                attachedTo.UnAttachRocket();    //открепляем ракету
                additionalAngularVelocity = attachedTo.CalculateRotationVelocity(rocketLongtitude);

                Debug.Log(string.Format("Ракета покидает планету! Additive velocity: {0} m/s", attachedTo.velocity));
                Debug.Log(string.Format("Ракета покидает планету! Additive angular velocity latitude (): {0} m/s", additionalAngularVelocity));

                velocity += attachedTo.velocity + additionalAngularVelocity;
                rocketLatitude = 0;  rocketLongtitude = 0;

                nextPos = CalculateAcceleration(Simulation.dt, throttle, (Vector3d)direction, targetList, massOverall);

                WorldManager.ResetWorldOffset(this.gameObject);
                attachedTo.transform.position = (Vector3)(attachedTo.position + WorldManager.worldOffset);
                attachedTo = null;

                WorldManager.worldOffset -= nextPos;    //Ракета теперь центр мира, двигаем не ракету а мир

                return;
            }
            else
            {
                nextPos = CalculateAcceleration(Simulation.dt, throttle, (Vector3d)direction, targetList, massOverall);
                transform.position += (Vector3)(nextPos);

                if (transform.localPosition.y < 0)
                {
                    transform.localPosition = new Vector3(transform.localPosition.x, 0.001f, transform.localPosition.z);
                    velocity = Vector3d.zero;
                }
            }
        }

        //GLOBAL POSITION
        else if (attachedTo == null)
        {    
            //Коллизия
            if (UpdateAttachment())
            {
                return;
            }

            nextPos = CalculateAcceleration(Simulation.dt, throttle, (Vector3d)direction, targetList, massOverall);
            WorldManager.worldOffset -= nextPos;     //Ракета теперь центр мира, двигаем не ракету а мир
        }
    }

    //true если ракета прикреплена к планете
    private bool UpdateAttachment()
    {
        //Луч к планете
        ray = new Ray(transform.position + transform.up * 25f, -transform.up);
        if (Physics.Raycast(ray, out hitInfo, 26f, 1 << LayerMask.NameToLayer("Planets")))
        {
            Planet planet = hitInfo.collider.gameObject.GetComponent<Planet>();

            if (planet != null)
            {
                planet.AttachRocket(hitInfo.point, this);
                //Получаем долготу и широту
                Vector2 longLat = PlaceRocket.ToSpherical(hitInfo.collider.transform.InverseTransformPoint(hitInfo.point));

                rocketLatitude = longLat.x;
                rocketLongtitude = longLat.y;

                Debug.Log($"Ракета расположена на долготе {longLat.y} и широте {longLat.x}");
                transform.position = hitInfo.point;
                velocity = Vector3d.zero;
                nextPos = Vector3d.zero;
                return true;
            }
        }
        return false;
    }

    public Vector3d CalculateAcceleration(float dt, float throttle, Vector3d direction, List<PositionAndMass> planets, float overallMass)
    {
        //Текущий двигатель
        RocketStage stage = stages.Last();
        reactive = Vector3d.zero;
        if (stage != null)
        {
            reactive = stage.AccelReactive(throttle, overallMass, direction) / Simulation.unitRocketM;
            stage.BurstFuel(dt, throttle);
        }

        gravityForces = Vector3d.zero;
        foreach (var planet in planets)
        {
            gravityForces += CalculateGravity(planet, (Vector3d)transform.position, WorldType.Rocket);
        }
        gravityForces /= Simulation.unitRocketM;  //переводим в наш мир

        acceleration = (gravityForces - reactive);

        Vector3d newPos = velocity * dt + 0.5f * acceleration * dt * dt;

        //Новое ускорение
        Vector3d next_Reactive = Vector3d.zero;
        if (stages.Last() != null)
        {
            next_Reactive = stages.Last().AccelReactive(throttle, overallMass, direction) / Simulation.unitRocketM;
        }

        Vector3d gravity_next = Vector3d.zero;
        foreach (var planet in planets)
        {
            gravity_next += CalculateGravity(planet, (Vector3d)transform.position + newPos, WorldType.Rocket);
        }
        gravity_next /= Simulation.unitRocketM;

        Vector3d accel_next = (gravity_next - next_Reactive);

        velocity = velocity + 0.5f * (acceleration + accel_next) * dt;

        return newPos;
    }

    //public static Vector3d CalculateAcceleration(float dt, float throttle, Vector3d direction, List<PositionAndMass> planets, float overallMass, RocketStages stages, Vector3d pos,
    //    ref Vector3d reactive, ref Vector3d gravityForces, ref Vector3d acceleration, ref Vector3d rocketVelocity)
    //{
    //    //Текущий двигатель
    //    RocketStage stage = stages.Last();
    //    reactive = Vector3d.zero;
    //    if (stage != null)
    //    {
    //        reactive = stage.AccelReactive(throttle, overallMass, direction) / Simulation.unitMiniatureM;
    //        stage.BurstFuel(dt, throttle);
    //    }

    //    gravityForces = Vector3d.zero;
    //    foreach (var planet in planets)
    //    {
    //        gravityForces += CalculateGravity(planet, (Vector3d)pos, WorldType.Miniature);
    //    }
    //    gravityForces /= Simulation.unitMiniatureM;  //переводим в наш мир

    //    acceleration = (gravityForces - reactive);

    //    Vector3d newPos = rocketVelocity * dt + 0.5f * acceleration * dt * dt;

    //    //Новое ускорение
    //    Vector3d next_Reactive = Vector3d.zero;
    //    if (stages.Last() != null)
    //    {
    //        next_Reactive = stages.Last().AccelReactive(throttle, overallMass, direction) / Simulation.unitMiniatureM;
    //    }

    //    Vector3d gravity_next = Vector3d.zero;
    //    foreach (var planet in planets)
    //    {
    //        gravity_next += CalculateGravity(planet, (Vector3d)pos + newPos, WorldType.Miniature);
    //    }
    //    gravity_next /= Simulation.unitMiniatureM;

    //    Vector3d accel_next = (gravity_next - next_Reactive);

    //    rocketVelocity = rocketVelocity + 0.5f * (acceleration + accel_next) * dt;

    //    return newPos;
    //}

    public static Vector3d CalculateGravity(PositionAndMass planet, Vector3d position, WorldType worldType)
    {
        return CelestialBody.CalculateVelocity(position, planet.position, planet.mass, worldType);
    }

    public Vector3d GetVelocityRelativeToPlanet(Planet planet)
    {
        if (attachedTo == planet)
        {
            return velocity;
        }
        return velocity - planet.velocity - additionalAngularVelocity;
    }
}