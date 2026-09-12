using CardBattleEngine;
using System;
using System.Collections;
using UnityEngine;

public abstract class CustomSFX : ScriptableObject
{
	public string PresentationEffectId
	{
		get
		{
#if UNITY_EDITOR
			return UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(this));
#else
			return PresentationEffectRegistry.IdFor(this);
#endif
		}
	}
	abstract public IEnumerator Routine(ActionPresentation context);
}
