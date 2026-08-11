using System;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Not cadence => apply effect when hitbox collide (OnTriggerEnter())
    /// cadence but start only initial collision => e.g. teemo's mushroom (after OnTriggerEnter(), start coroutine for the poison damage)
    /// cadence but not need initial collision => e.g. garen's E (OnTriggerEnter() trigger every interval)
    /// </summary>
    [Serializable]
    public class Cadence
    {
        public bool isCadence = false;

        // neither is read unless isCadence, so neither is shown unless isCadence
        [ShowIf(nameof(isCadence))] public float cadenceInterval = 0.5f;    // the interval of time effect is re-apply
        [ShowIf(nameof(isCadence))] public float cadenceDuration = 3f;      // total duration of the effect
    }
}
