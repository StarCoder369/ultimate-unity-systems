using UnityEngine;

[CreateAssetMenu(fileName = "Damage Effect", menuName = "Status Effects/Damage")]
public class DamageEffect : StatusEffect
{
    public float damage = 10f;
    public float tickRate = 1f;


    private float timer;


    public override void OnStart(GameObject target)
    {
        timer = 0f;
    }


    public override void OnTick(GameObject target)
    {
        timer -= Time.deltaTime;


        if (timer <= 0f)
        {
            timer = tickRate;


            IDamageable damageable = target.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}