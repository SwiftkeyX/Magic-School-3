namespace MagicSchool.Heroes
{

    public class AttackCooldown
    {
        private float _remaining;
        public bool IsReady => _remaining <= 0f;

        // update cooldown
        public void Tick(float deltaTime)
        {
            // stop at ready - a long walk shouldn't bank up attacks to spend on arrival
            if (IsReady) return;

            _remaining -= deltaTime;
        }

        // to attack
        public void Spend(float attackSpeed) => _remaining += 1f / attackSpeed;
    }
}
