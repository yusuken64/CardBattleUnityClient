using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

public static class CardDefinitionNetworkExtensions
{
	// Match the engine's definition format, excluding client-only presentation data.
	private static readonly JsonSerializerSettings WireSettings = new JsonSerializerSettings
	{
		TypeNameHandling = TypeNameHandling.Auto,
		NullValueHandling = NullValueHandling.Ignore,
		ContractResolver = new WireContractResolver()
	};

	public static string ToWireDefinitionJson(this CardDefinition definition, CardBattleEngine.Card runtimeCard)
	{
		CardBattleEngine.CardDefinition wireDefinition = runtimeCard switch
		{
			CardBattleEngine.MinionCard m => CardBattleEngine.CardDatabase.ToMinionCardDefinition(m, definition.ID),
			CardBattleEngine.SpellCard s => CardBattleEngine.CardDatabase.ToSpellCardDefinition(s, definition.ID),
			CardBattleEngine.WeaponCard w => CardBattleEngine.CardDatabase.ToWeaponCardDefinition(w, definition.ID),
			_ => null
		};
		if (wireDefinition == null) return null;
		var serializer = JsonSerializer.Create(WireSettings);
		var payload = JObject.FromObject(wireDefinition, serializer);
		if (runtimeCard is CardBattleEngine.SpellCard spell && spell.CustomSFX is CustomSFX effect)
			payload[nameof(CardBattleEngine.SpellCard.PresentationEffectId)] = effect.PresentationEffectId;
		if (runtimeCard is CardBattleEngine.MinionCard minion)
		{
			// The bundled engine DLL predates this server definition field.
			payload["MinionTriggeredEffects"] = JArray.FromObject(minion.MinionTriggeredEffects, serializer);
		}
		return payload.ToString(Formatting.None);
	}

	private sealed class WireContractResolver : DefaultContractResolver
	{
		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			if (member.Name == nameof(CardBattleEngine.IGameAction.PresentationEffectId))
				property.ValueProvider = new EffectIdProvider(property.ValueProvider);
			if (member.Name == nameof(CardBattleEngine.IGameAction.CustomSFX) &&
				(typeof(CardBattleEngine.IGameAction).IsAssignableFrom(member.DeclaringType) ||
				 typeof(CardBattleEngine.SpellCard).IsAssignableFrom(member.DeclaringType)))
			{
				property.Ignored = true;
			}
			// Embedded summon/gain-card templates expose a read-only alias to themselves.
			if (member.Name == nameof(CardBattleEngine.Card.Entity) &&
				typeof(CardBattleEngine.ITriggerSource).IsAssignableFrom(member.DeclaringType))
			{
				property.Ignored = true;
			}
			if (member.Name == nameof(CardBattleEngine.IGameEntity.AttackBehavior) &&
				typeof(CardBattleEngine.IGameEntity).IsAssignableFrom(member.DeclaringType))
			{
				property.Ignored = true;
			}
			return property;
		}
	}

	private sealed class EffectIdProvider : IValueProvider
	{
		private readonly IValueProvider _inner;
		public EffectIdProvider(IValueProvider inner) { _inner = inner; }
		public object GetValue(object target)
		{
			var effect = (target as CardBattleEngine.IGameAction)?.CustomSFX as CustomSFX
				?? (target as CardBattleEngine.SpellCard)?.CustomSFX as CustomSFX;
			return effect != null ? effect.PresentationEffectId : _inner.GetValue(target);
		}
		public void SetValue(object target, object value) => _inner.SetValue(target, value);
	}
}
