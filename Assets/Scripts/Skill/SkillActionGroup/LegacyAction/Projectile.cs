
// using UnityEngine;

// /// <summary>
// /// Projectile was aim toward the "target", what determined which recipients get hit is based on the projectile type.
// /// </summary>
// public class Projectile : LegacyAction
// {
//     // normally, destroy upon projectile impact
//     // But here is also, fallback lifetime, in case something go wrong 
//     private float _lifetime = 0.5f;
//     private float _speed = 8f;

//     // how to shoot projectile?
//     // 1) source it was shoot from
//     // 2) the direction it was shoot at (aim)
//     // 3) resolve how projectile will hit target
//     private void Shoot()
//     {

//     }

//     // ======================================== protected ========================================
//     // source = where projectile spawn
//     // aim = where projectile shoot at
//     protected override Vector3 ResolveAimTarget(ActionSourceEnum source, AimTargetEnum aimTarget, Hero caster)
//     {
//         // ---------------------- resolve how source work ----------------------
//         Vector3 prefabSpawnLocation;
//         // spawn projectile at self
//         if (source == ActionSourceEnum.Self)
//         {
//             prefabSpawnLocation = this.transform.position;
//         }

//         // else if ()
//         // ...

//         // ---------------------- resolve how aim work ----------------------
//         Vector3 aimDirection;
//         // aim skill at self
//         if (aimTarget == AimTargetEnum.Self)
//         {
//             aimDirection = caster.transform.position;
//         }

//         // aim skill at current target
//         else if (aimTarget == AimTargetEnum.Current)
//         {
//             Hero target = caster.Blackboard.FindNearestEnemy();
//             return target != null ? target.transform.position : caster.transform.position;
//         }

//         return Vector3.zero;
//     }
// }