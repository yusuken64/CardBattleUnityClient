using CardBattleEngine;
using System;
using System.Collections;
using UnityEngine;

public abstract class CustomSFX : ScriptableObject
{
	abstract public IEnumerator Routine(IGameAction action, ActionContext context);
}
