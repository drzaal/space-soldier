﻿using UnityEngine;

public class MachineGun : Weapon
{
    private float firingDelay = .1f;
    private float bulletSpeed = 50;
    private int energyCost = 0;

    private StackPool bulletPool;

    public MachineGun(SkillTree skillTree) : base(skillTree)
    {
        bulletPool = GameObject.Find("BulletPool").GetComponent<StackPool>();
    }

    public override int Click(Transform transform)
    {
        if (CanFire())
        {
            nextFiringTime = Time.time + firingDelay;
            GameObject bullet = bulletPool.Pop();
            bullet.transform.position = transform.position;
            bullet.transform.rotation = transform.rotation;

            // Fire bullet towards mouse pointer - TODO: move this into a utility.
            Vector3 mouse = Input.mousePosition;
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 direction = new Vector2(mouse.x - screenPoint.x, mouse.y - screenPoint.y).normalized;

            // this has to be done before setting velocity or it won't work.
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;

            return energyCost;
        }

        return 0;
    }

    public override int GetEnergyRequirement()
    {
        return energyCost;
    }

    public override string GetName()
    {
        return "machineGun";
    }
}
