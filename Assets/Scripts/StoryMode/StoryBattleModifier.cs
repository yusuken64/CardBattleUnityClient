using System;
using System.Collections.Generic;

[Serializable]
public class StoryBattleModifier
{
	public string ModifierText;
	public List<TriggeredEffectWrapper> TriggeredEffects;
}
