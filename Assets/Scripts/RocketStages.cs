using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class RocketStages : IEnumerable<RocketStage>
{
    public float overallMass
    {
        get
        { 
            return stages.Where(stage => stage.dropped == false).Select(stage => stage.overallMass).Sum();
        }
    }

    [SerializeField]
    public List<RocketStage> stages = new List<RocketStage>();

    public void DropStage()
    {
        //stages.RemoveAt(stages.Count - 1);
    }

    public RocketStage Last()
    {
        RocketStage stage = stages.Where(s => s.dropped == false).Count() == 0 ? null : stages.Where(s => s.dropped == false).Last();
        return stage;
    }

    IEnumerator<RocketStage> IEnumerable<RocketStage>.GetEnumerator()
    {
        foreach (var stage in stages)
        {
            yield return stage;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return ((IEnumerable<RocketStage>)stages).GetEnumerator();
    }
}

[Serializable]
public class RocketStage
{
    public float overallMass
    {
        get
        {
            return stageMass + fuelMass;
        }
    }
    public float stageMass;
    public float fuelMass;
    public float fuelConsamptionPerSecond;
    [Range(100,5000)]
    public float u;

    public bool dropped
    {
        get
        {
            if (fuelMass <= 0)
            {
                return true;
            }
            return false;
        }
    }

    //  mass = 300 000
    //  mass at 100 second = 300 000-160 000 = 140 000
    // 3000 * 1600 / 140 000

    public Vector3d AccelReactive(float throttle, float rocketMassOverall, Vector3d direction)
    {
        // throttle - используемый процент мощности 0..1
        // u - максимальная скорость истечения газов
        // mu / mt - расход топлива в единицу времени, в данном случае в секунду
        if (fuelMass <= 0)
        {
            return Vector3d.zero;
        }

        return throttle * (-u) * (fuelConsamptionPerSecond / rocketMassOverall) * direction;
    }

    public Vector3d AccelReactive(float throttle, double rocketMassOverall, Vector3d direction)
    {
        // throttle - используемый процент мощности 0..1
        // u - максимальная скорость истечения газов
        // mu / mt - расход топлива в единицу времени, в данном случае в секунду
        if (fuelMass <= 0)
        {
            return Vector3d.zero;
        }

        return throttle * (-u) * (fuelConsamptionPerSecond / rocketMassOverall) * direction;
    }

    public void BurstFuel(float throttle, float dt)
    {
        if (fuelMass > 0)
        {
            fuelMass -= fuelConsamptionPerSecond * dt * throttle;
        }
        else
        {
            fuelMass = 0;
        }
    }
}