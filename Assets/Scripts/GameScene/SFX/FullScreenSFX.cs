using CardBattleEngine;
using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "CustomSFX/FullScreenSFX")]
public class FullScreenSFX : CustomSFX
{
    public GameObject FullScreenObject;
    public float AnimationTime;
    public override IEnumerator Routine(IGameAction action, ActionContext context)
    {
        if (FullScreenObject != null)
        {
            var muzzle = Instantiate(FullScreenObject, Vector3.zero, Quaternion.identity);
            Destroy(muzzle, 1f);
        }

        yield return new WaitForSecondsRealtime(AnimationTime);
    }
}