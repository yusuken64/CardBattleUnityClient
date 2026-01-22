using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "CustomSFX/Projectile")]
public class ProjectileSFX : CustomSFX
{
    public GameObject MuzzleObject;
    public GameObject ProjectileObject;
    public GameObject ImpactObject;
    public float ProjectileDuration = 0.5f; // time for projectile to reach target

    public override IEnumerator Routine(IGameAction action, ActionContext context)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        GameObject source = gameManager.GetObjectFor(context.Source);
        if (source == null)
		{
            source = gameManager.GetObjectFor(context.SourcePlayer);
        }

		List<IGameEntity> targets = context.AffectedEntities.Select(x => x.Item1).ToList();
        if (!targets.Any())
        {
            targets = new() { context.Target };
        }

        foreach (var targetData in targets)
        {
            GameObject target = gameManager.GetObjectFor(targetData);

            //if (source == null && target == null)
            //    yield break;

            if (MuzzleObject != null && source != null)
            {
                var muzzle = Instantiate(MuzzleObject, source.transform.position, Quaternion.identity);
                Destroy(muzzle, 1f);
            }

            if (ProjectileObject == null || target == null)
                yield break;

            var projectileInstance = Instantiate(ProjectileObject, source.transform.position, Quaternion.identity);

            Tween moveTween = projectileInstance.transform.DOMove(target.transform.position, ProjectileDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (ImpactObject != null)
                    {
                        var impact = Instantiate(ImpactObject, target.transform.position, Quaternion.identity);
                        Destroy(impact, 1f);
                    }

                    Destroy(projectileInstance);
                });

            yield return moveTween.WaitForCompletion();
        }
    }
}
