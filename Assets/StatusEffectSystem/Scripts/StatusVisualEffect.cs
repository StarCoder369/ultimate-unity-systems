using UnityEngine;

[CreateAssetMenu(fileName = "Visual Effect", menuName = "Status Effects/Visual")]
public class StatusVisualEffect : StatusEffect
{
    public GameObject particlePrefab;

    public Vector3 offset;


    public override void OnStart(GameObject target)
    {
        if (particlePrefab == null)
        {
            return;
        }


        GameObject particle = Instantiate(particlePrefab, target.transform);

        particle.transform.localPosition = offset;
    }
}