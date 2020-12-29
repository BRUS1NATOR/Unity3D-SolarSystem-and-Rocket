using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using UnityEngine.PlayerLoop;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(CelestiaBodiesManager))]
[ExecuteInEditMode]
public class OrbitDisplay : MonoBehaviour
{
    public bool drawOrbits = true;
    public Material lineRendererMaterial;
    public bool visualizeBuild = false;
    [Range(0.1f, 10)]
    public float lineWidth = 1;
    [Range(0, 3)]
    public float simplifyLine = 1;

    [Range(1, 30000)]
    public int numSteps = 1000;
    [Range(0.1f, 100000)]
    public float timeStep = 1f;
    public bool usePhysicsTimeStep;

    public bool relativeToBody;
    public CelestialBody centralBody;

    [SerializeField]
    public Distance[] distances;

    [SerializeField]
    public Vector3[][] drawPoints;

    public VirtualBody[] virtualBodies;

    private CelestiaBodiesManager manager;
    private CelestialBody[] bodies
    {
        get
        {
            return manager.celestialBodies;
        }
    }

    public PlaceRocket rocket;

    public LineRenderer rocketLine;
   // private PhantomRocket phantomRocket;

    [Range(1, 10000)]
    public float visualScale = 1000f;
    [Range(1, 2500)]
    public float maxScale = 1000f;

    public bool needsToUpdateOrbits
    {
        get
        {
            if (bodies.Any(x => x.needToUpdate))
            {
                bodies.ToList().ForEach(b => b.needToUpdate = false);

                return true;
            }
            return false;
        }
    }

    EditorCoroutine drawLineEditor = null;
    Coroutine drawLine = null;

    private bool needScaling = false;

    void OnValidate()
    {
        manager = GetComponent<CelestiaBodiesManager>();

        if (!needScaling)
        {
            needScaling = true;
            SetScale();
        }
        needScaling = false;
    }

    public void Setup(bool onValidate)
    {
        if (onValidate)
        {
            if (usePhysicsTimeStep)
            {
                timeStep = Simulation.timeStep;
            }

            DrawOrbits(onValidate);
        }
        bodies.ToList().ForEach(b => b.lineRenderer.gameObject.SetActive(drawOrbits));
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private void OnScene(SceneView scene)
    {
        if (SceneView.lastActiveSceneView != null)
        {
            Vector3 camPos = SceneView.lastActiveSceneView.camera.transform.position;

            bodies.ToList().ForEach(b => b.lineRenderer.widthMultiplier = Vector3.Distance(b.transform.position, camPos) / 1000f * lineWidth);

            //if (rocketLine != null)
            //{
            //    rocketLine.widthMultiplier = Vector3.Distance(rocket.transform.position, camPos) / 1000f * lineWidth;
            //}
        }
    }

    void DrawOrbits(bool onValidate)
    {
        if (drawOrbits)
        {
            bodies.ToList().ForEach(x => CheckLineRenderer(x));

            if (needsToUpdateOrbits || onValidate)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    if (drawLine != null)
                    {
                        StopCoroutine(drawLine);
                        drawLine = null;
                    }

                    drawLine = StartCoroutine(CalculateOrbits());
                }
                else
                {
                    if (drawLineEditor != null)
                    {
                        EditorCoroutineUtility.StopCoroutine(drawLineEditor);
                        drawLineEditor = null;
                    }

                    drawLineEditor = EditorCoroutineUtility.StartCoroutine(CalculateOrbits(), this);
                }
            }
        }
    }

    public void SetScale()
    {
        foreach (var body in bodies)
        {
            body.SetScale(visualScale, maxScale);
        }

        foreach (var body in bodies)
        {
            body.CheckVisualOverlap();
        }

        rocket.transform.localScale =  Vector3.one * visualScale / 100;

        PlaceRocket rocketPlace = FindObjectOfType<PlaceRocket>();
        rocketPlace.UpdatePosition();
    }

    IEnumerator CalculateOrbits()
    {
        virtualBodies = new VirtualBody[bodies.Length];

        drawPoints = new Vector3[bodies.Length][];

        distances = new Distance[virtualBodies.Length];
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            distances[i].minDistance = double.MaxValue;
            distances[i].maxDistance = 0;
            distances[i].planetName = "";
        }

        int relativeBodyIndex = 0;
        Vector3d referenceBodyInitialPosition = Vector3d.zero;

        if (rocket != null)
        {
            //phantomRocket = new PhantomRocket(rocket.realRocket, (Vector3d)rocket.transform.position, (Vector3d)rocket.transform.up, timeStep);
            if (rocketLine == null)
            {
                GameObject line = new GameObject();
                line.transform.SetParent(this.transform);
                rocketLine = line.AddComponent<LineRenderer>();
                rocketLine.sharedMaterial = lineRendererMaterial;
            }
            rocketLine.positionCount = 0;
            rocketLine.positionCount = numSteps;
        }

        // Initialize virtual bodies
        for (int i = 0; i < virtualBodies.Length; i++)
        {
            bodies[i].lineRenderer.positionCount = numSteps;
            drawPoints[i] = new Vector3[numSteps];

            virtualBodies[i] = new VirtualBody(bodies[i]);

            if (bodies[i] == centralBody && relativeToBody)
            {
                relativeBodyIndex = i;
                referenceBodyInitialPosition = virtualBodies[i].position;
            }
        }

        // Simulate
        for (int step = 0; step < numSteps; step++)
        {
            Vector3d referenceBodyPosition = (relativeToBody) ? virtualBodies[relativeBodyIndex].position : Vector3d.zero;

           // phantomRocket.position += phantomRocket.CalculateAcceleration(virtualBodies.ToList()) / Simulation.unitMiniatureM;
         //   rocketLine.SetPosition(step, (Vector3)phantomRocket.position);
            // Update velocities
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                virtualBodies[i].velocity += CalculateAcceleration(i, virtualBodies) * timeStep / Simulation.unitMiniatureM;
            }


            // Update positions
            for (int i = 0; i < virtualBodies.Length; i++)
            {
                Vector3d newPos = virtualBodies[i].position + virtualBodies[i].velocity * timeStep;
                virtualBodies[i].position = newPos;
                distances[i].planetName = virtualBodies[i].name;

                if (relativeToBody && i == relativeBodyIndex)
                {
                    newPos = referenceBodyInitialPosition;
                }
                else
                {
                    double distanceToRelativeBody = Simulation.DistanceMiniature(newPos, referenceBodyPosition) / Math.Pow(10,9);
                    if (distances[i].maxDistance < distanceToRelativeBody)
                    {
                        distances[i].maxDistance = distanceToRelativeBody;
                        distances[i].maxDistanceAU = distanceToRelativeBody / Simulation.astronomicUnit;
                    }
                    if (distances[i].minDistance > distanceToRelativeBody)
                    {
                        distances[i].minDistance = distanceToRelativeBody;
                        distances[i].minDistanceAU = distanceToRelativeBody / Simulation.astronomicUnit;
                    }
                }
                if (relativeToBody)
                {
                    var referenceFrameOffset = referenceBodyPosition - referenceBodyInitialPosition;
                    newPos -= referenceFrameOffset;
                }

                drawPoints[i][step] = (Vector3)newPos;

                if (visualizeBuild)
                {
                    bodies[i].lineRenderer.SetPosition(step, drawPoints[i][step]);
                }
            }

            if (step % 100 == 0)
            {
                yield return new WaitForSecondsRealtime(0.025f);
            }
        }

        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].lineRenderer.SetPositions(drawPoints[i]);
            bodies[i].lineRenderer.Simplify(simplifyLine);
        }
        rocketLine.Simplify(simplifyLine);

        Debug.Log("Orbits have been built");
    }

    Vector3d CalculateAcceleration(int bodyID, VirtualBody[] virtualBodies)
    {
        Vector3d acceleration = Vector3d.zero;

        for (int j = 0; j < virtualBodies.Length; j++)
        {
            if (bodyID == j || virtualBodies[j].celestialType == CelesitalBodyType.Satellite)
            {
                continue;
            }

            acceleration += CelestialBody.CalculateVelocity(virtualBodies[bodyID].position, virtualBodies[j].position, virtualBodies[j].mass, WorldType.Miniature);
        }

        return acceleration;
    }

    public void CheckLineRenderer(CelestialBody body)
    {
        if (body.lineRenderer == null)
        {
            GameObject line = new GameObject("LineRenderer");
            line.transform.SetParent(this.gameObject.transform);
            body.lineRenderer = line.AddComponent<LineRenderer>();
            body.lineRenderer.gameObject.layer = LayerMask.NameToLayer("MiniaturePlanets");
        }

        body.lineRenderer.positionCount = 0;
        body.lineRenderer.startColor = body.orbitColor;
        body.lineRenderer.endColor = body.orbitColor;
        body.lineRenderer.sharedMaterial = lineRendererMaterial;
    }

    [Serializable]
    public struct Distance
    {
        public string planetName;

        public double maxDistance;
        public double maxDistanceAU;

        public double minDistance;
        public double minDistanceAU;
    }

    public class PositionAndMass
    {
        public string name;
        public Vector3d position;
        public double mass;

        public PositionAndMass(Vector3d pos, double m, string name)
        {
            position = pos;
            mass = m;
            this.name = name;
        }
    }

    public class VirtualBody
    {
        public string name;

        public CelesitalBodyType celestialType;
        public int planetOrbitId=0;

        public Vector3d position;
        public Vector3d velocity;

        public double mass;

        public VirtualBody(CelestialBody body)
        {
            name = body.planetName;
            //celestialType = body.celestialType;

            position = (Vector3d)(body.position);
            velocity = body.initialVelocity / Simulation.unitMiniatureM;

            mass = body.mass; 
        }
    }

    ////Расчет ракеты
    //public class PhantomRocket
    //{
    //    RocketStages stages;
    //    float massOverall
    //    {
    //        get
    //        {
    //            return massRocket + stages.overallMass;
    //        }
    //    }
    //    public Vector3d velocity;
    //    public Vector3d position;
    //    public Vector3d rocketSpeed;

    //    Vector3d direction;
    //    float massRocket;
    //    float timeStep;
    //    public PhantomRocket(Rocket rocket, Vector3d position, Vector3d dir, float timeStep)
    //    {
    //        stages = new RocketStages();
    //        massRocket = rocket.massRocket;
    //        foreach (RocketStage s in rocket.stages)
    //        {
    //            RocketStage stage = new RocketStage();
    //            stage.fuelMass = s.fuelMass;
    //            stage.fuelConsamptionPerSecond = s.fuelConsamptionPerSecond;
    //            stage.stageMass = s.stageMass;
    //            stage.u = s.u;
    //            stages.stages.Add(stage);
    //        }
    //        this.position = position;
    //        direction = dir;
    //        this.timeStep = timeStep;
    //        this.rocketSpeed = rocket.attachedTo.reference.initialVelocity + rocket.attachedTo.CalculateRotationVelocity(rocket.rocketLongtitude);
    //    }

    //    public Vector3d CalculateAcceleration(List<VirtualBody> bodies)
    //    {
    //        var targetList = bodies
    //            .Select(x => new PositionAndMass(x.position, x.mass, x.name))
    //            .ToList();

    //        Vector3d none = new Vector3d();
    //        Vector3d none1 = new Vector3d();
    //        Vector3d none2 = new Vector3d();

    //        // return Vector3d.one;
    //        return Rocket.CalculateAcceleration(timeStep, 1, direction, targetList, massOverall, stages, position, ref none, ref none1, ref none2, ref rocketSpeed);
    //    }
    //}
}
