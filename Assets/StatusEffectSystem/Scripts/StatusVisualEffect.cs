using UnityEngine;

[CreateAssetMenu(fileName = "Visual Effect", menuName = "Status Effects/Visual")]
public class StatusVisualEffect : StatusEffect
{
    public GameObject particlePrefab;

    public Vector3 offset;

    private GameObject visualEffect;


    public override void OnStart(GameObject target)
    {
        if (particlePrefab == null)
        {
            return;
        }


        visualEffect = Instantiate(particlePrefab, target.transform);

        visualEffect.transform.localPosition = offset;
    }

    public override void OnEnd(GameObject target)
    {
        Destroy(visualEffect);
    }
}