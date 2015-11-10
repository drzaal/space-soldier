﻿using UnityEngine;

public class EnergyGun : Weapon
{
    public float energyCost;

    public override float Click(Transform transform)
    {
        if (CanFire())
        {
            nextFiringTime = Time.time + FiringDelay;
            GameObject orb = StackPool.Pop();
            orb.transform.position = transform.position;
            orb.transform.rotation = transform.rotation;

            Vector2 direction = VectorUtil.DirectionToMousePointer(transform);

            orb.SetActive(true);
            orb.GetComponent<Rigidbody2D>().velocity = direction * ProjectileSpeed;

            return energyCost;
        }

        return 0;
    }

    public override float GetEnergyRequirement()
    {
        return energyCost;
    }

    public override string GetName()
    {
        return "energyGun";
    }
}