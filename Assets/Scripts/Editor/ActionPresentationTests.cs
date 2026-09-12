using System;
using System.Collections.Generic;
using CardBattleEngine;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace MonsterGirl.Tests
{
    public class ActionPresentationTests
    {
        [TestCase(typeof(MinionCardDefinition))]
        [TestCase(typeof(SpellCardDefinition))]
        [TestCase(typeof(WeaponCardDefinition))]
        public void LocalCardArtworkIdentitySurvivesPlaybackSnapshot(Type definitionType)
        {
            var definition = (CardDefinition)ScriptableObject.CreateInstance(definitionType);
            try
            {
                definition.ID = "artwork-catalog-id";
                definition.CardName = "Different display name";
                if (definition is WeaponCardDefinition weapon) weapon.WeaponName = definition.CardName;
                var owner = new CardBattleEngine.Player("Self");
                var other = new CardBattleEngine.Player("Other");
                var card = definition.CreateCard();
                card.Owner = owner;
                owner.Hand.Add(card);
                var state = new GameState(owner, other, new XorShiftRNG(1), new List<CardBattleEngine.Card>());
                var frame = CardBattleEngine.View.PlayerViewBuilder.BuildPlayback(state, owner,
                    new SpendManaAction { Amount = 0 }, new ActionContext { SourcePlayer = owner, SourceCard = card }, 1);
                var wire = JObject.FromObject(frame).ToObject<PlaybackEventView>();
                Assert.AreEqual(definition.ID, wire.After.Self.Hand[0].CardId);
                Assert.AreEqual(definition.ID, wire.SourceCard.Card.CardId);
            }
            finally { UnityEngine.Object.DestroyImmediate(definition); }
        }

        [Test]
        public void LeaderExportMatchesLocalHeroPowerWithoutAddingLeaderToDeck()
        {
            var leader = ScriptableObject.CreateInstance<MinionCardDefinition>();
            try
            {
                leader.ID = "leader-export-test"; leader.CardName = "Leader"; leader.Cost = 2;
                leader.MinionTriggeredEffects.Add(new TriggeredEffectWrapper
                {
                    EffectTrigger = EffectTrigger.Battlecry,
                    AffectedEntitySelectorWrapper = new ContextSelectorWrapper { IncludeSummonedMinion = true },
                    GameActions = new List<IGameActionWrapperBase>
                    {
                        new GainArmorActionWrapper { Amount = new ConstantValueWrapper { Number = 4 } },
                    },
                });
                var owner = new CardBattleEngine.Player("Alice");
                var local = HeroPowerDefinition.CreateHeroPowerFromHeroCard(leader, owner);
                Assert.AreSame(owner, local.LeaderCard.Owner);
                var request = new Deck { HeroCard = leader }.ToDecklistRequest("Alice");
                var definition = (CardBattleEngine.MinionCardDefinition)CardDatabase.LoadCardFromJson(request.LeaderDefinition);
                var effect = definition.MinionTriggeredEffects[0];
                var restored = new MinionCard(definition.Name, definition.Cost, definition.Attack, definition.Health);
                restored.MinionTriggeredEffects.Add(effect);
                var network = CardBattleEngine.HeroPower.FromLeader(restored);
                Assert.AreEqual(local.Name, network.Name);
                Assert.AreEqual(local.ManaCost, network.ManaCost);
                Assert.AreEqual(0, request.Minions.Count);
                Assert.AreEqual(0, request.CustomMinions.Count);
                Assert.IsTrue(((ContextSelector)network.AffectedEntitySelector).IncludeSourcePlayer);
                Assert.IsFalse(((ContextSelector)effect.AffectedEntitySelector).IncludeSourcePlayer,
                    "Converting the leader must preserve its original battlecry selector.");
                Assert.IsFalse(request.LeaderDefinition.Contains("UnityEngine"));
            }
            finally { UnityEngine.Object.DestroyImmediate(leader); }
        }

        [Test]
        public void CustomEffectRegistryAndExport_PreserveAssetIdentity()
        {
            var registry = Resources.Load<PresentationEffectRegistry>("PresentationEffectRegistry");
            Assert.IsNotNull(registry);
            Assert.Greater(registry.Entries.Count, 0);
            foreach (var entry in registry.Entries)
            {
                Assert.IsNotNull(entry.Effect);
                Assert.AreEqual(entry.Id, entry.Effect.PresentationEffectId);
                Assert.AreSame(entry.Effect, PresentationEffectRegistry.Resolve(entry.Id));
            }
            var definition = ScriptableObject.CreateInstance<SpellCardDefinition>();
            try
            {
                definition.ID = "effect-export-test";
                var effect = registry.Entries[0];
                var card = new SpellCard("Effect test", 1) { CustomSFX = effect.Effect };
                card.SpellCastEffects.Add(new SpellCastEffect { GameActions = new List<IGameAction> { new DamageAction { Damage = (Value)2, CustomSFX = effect.Effect } } });
                var json = definition.ToWireDefinitionJson(card);
                var restored = (CardBattleEngine.SpellCardDefinition)CardDatabase.LoadCardFromJson(json);
                Assert.AreEqual(effect.Id, restored.PresentationEffectId);
                Assert.AreEqual(effect.Id, restored.SpellCastEffects[0].GameActions[0].PresentationEffectId);
                Assert.IsFalse(json.Contains("CustomSFX"));
                Assert.IsFalse(json.Contains("UnityEngine"));
                Assert.AreSame(effect.Effect, card.CustomSFX);
            }
            finally { UnityEngine.Object.DestroyImmediate(definition); }
        }
        [Test]
        public void CapturedValues_DoNotFollowLiveEngineMutations()
        {
            var owner = new CardBattleEngine.Player("Self") { Mana = 3, Armor = 2 };
            var other = new CardBattleEngine.Player("Other");
            var state = new GameState(owner, other, new XorShiftRNG(1), new List<CardBattleEngine.Card>());
            var view = CardBattleEngine.View.PlayerViewBuilder.BuildPlayback(state, owner,
                new SpendManaAction { Amount = 2 }, new ActionContext { SourcePlayer = owner }, 1);
            var presentation = new ActionPresentation(JObject.FromObject(view).ToObject<PlaybackEventView>(), state);
            owner.Mana = 8;
            owner.Armor = 9;
            Assert.AreEqual(3, presentation.SourcePlayer.Mana);
            Assert.AreEqual(2, presentation.SourcePlayer.Armor);
            Assert.AreNotSame(owner, presentation.SourcePlayer);
        }

        [TestCase(typeof(SpendManaAnimation))]
        [TestCase(typeof(GainArmorActionAnimation))]
        [TestCase(typeof(RefillManaAnimation))]
        [TestCase(typeof(IncreaseMaxManaAnimation))]
        [TestCase(typeof(DrawCardFromDeckAnimation))]
        [TestCase(typeof(PromptMulliganGameActionAnimation))]
        public void ReplayingStateOnlyAnimation_DoesNotChangeTheEngineOrRequestInput(Type type)
        {
            var owner = new CardBattleEngine.Player("Self") { Mana = 3, MaxMana = 5, Armor = 2 };
            var other = new CardBattleEngine.Player("Other");
            var state = new GameState(owner, other, new XorShiftRNG(1), new List<CardBattleEngine.Card>());
            var view = CardBattleEngine.View.PlayerViewBuilder.BuildPlayback(state, owner,
                new SpendManaAction { Amount = 2 }, new ActionContext { SourcePlayer = owner }, 1);
            var presentation = new ActionPresentation(JObject.FromObject(view).ToObject<PlaybackEventView>(), state);
            var go = new GameObject("Animation isolation test");
            try
            {
                var animation = (GameActionAnimationBase)go.AddComponent(type);
                // No GameManager exists: playback must not depend on game progression or input services.
                animation.Init(null, presentation);
                for (int i = 0; i < 2; i++)
                {
                    var routine = animation.Play();
                    while (routine.MoveNext()) { }
                }
                Assert.AreEqual(3, owner.Mana);
                Assert.AreEqual(5, owner.MaxMana);
                Assert.AreEqual(2, owner.Armor);
                Assert.AreEqual(0, state.History.Count);
                Assert.AreSame(owner, state.CurrentPlayer);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
