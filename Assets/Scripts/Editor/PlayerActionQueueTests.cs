using System;
using NUnit.Framework;

namespace MonsterGirl.Tests
{
    public class PlayerActionQueueTests
    {
        [Test]
        public void OwnTurnCanAcceptInputWhilePresentationIsBehindOrRequestIsPending()
        {
            var self = Guid.NewGuid();
            var displayed = new PlayerGameView { CurrentPlayerId = self, Turn = 3, StateRevision = 10 };
            var latest = new PlayerGameView { CurrentPlayerId = self, Turn = 3, StateRevision = 12, PromptVersion = null };
            Assert.IsTrue(GameManager.CanQueueNetworkTurn(latest, displayed, self));
            latest.CurrentPlayerId = Guid.NewGuid();
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(latest, displayed, self));
            latest.CurrentPlayerId = self;
            latest.Turn = 5;
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(latest, displayed, self));
        }

        [Test]
        public void ChoicesAndGameEndBlockNormalActionQueue()
        {
            var self = Guid.NewGuid();
            var view = new PlayerGameView { CurrentPlayerId = self, Turn = 3 };
            view.PendingChoice = new PendingChoiceView { SourcePlayerId = self };
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(view, view, self));
            view.PendingChoice = null;
            view.OpponentIsChoosing = true;
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(view, view, self));
            view.OpponentIsChoosing = false;
            view.IsGameOver = true;
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(view, view, self));
            Assert.IsFalse(GameManager.CanQueueNetworkTurn(view, null, self));
        }

        [Test]
        public void QueuedActionUsesFreshIndexAndNeverRetargetsWhenOriginalTargetDisappears()
        {
            var source = Guid.NewGuid();
            var target = Guid.NewGuid();
            var other = new LegalActionView { Index = 2, ActionType = "AttackAction", SourceEntityId = source, TargetEntityId = Guid.NewGuid() };
            var wanted = new LegalActionView { Index = 17, ActionType = "AttackAction", SourceEntityId = source, TargetEntityId = target };
            Assert.AreSame(wanted, GameManager.FindQueuedNetworkAction(new[] { other, wanted }, "AttackAction", source, target));
            wanted.Index = 4;
            Assert.AreEqual(4, GameManager.FindQueuedNetworkAction(new[] { wanted, other }, "AttackAction", source, target).Index);
            Assert.IsNull(GameManager.FindQueuedNetworkAction(new[] { other }, "AttackAction", source, target));
            Assert.IsNull(GameManager.FindQueuedNetworkAction(new[] { wanted }, "PlayCardAction", source, target));
        }
    }
}
