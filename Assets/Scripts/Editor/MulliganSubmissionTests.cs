using System;
using NUnit.Framework;

namespace MonsterGirl.Tests
{
    public class MulliganSubmissionTests
    {
        [Test]
        public void ConfirmedSelectionWaitsForOwnPromptAndFinishedPlayback()
        {
            var local = Guid.NewGuid();
            var choice = new PendingChoiceView { ChoiceKind = "MulliganChoce", SourcePlayerId = Guid.NewGuid() };
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, false, choice, local, true));
            choice.SourcePlayerId = local;
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, false, choice, local, false));
            Assert.IsTrue(MulliganPrompt.CanSubmitStagedChoice(true, false, choice, local, true));
            // Repeated snapshots must not submit the same confirmed hand twice.
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, true, choice, local, true));
        }

        [Test]
        public void UnconfirmedOrUnrelatedChoicesNeverAutoSubmit()
        {
            var local = Guid.NewGuid();
            var choice = new PendingChoiceView { ChoiceKind = "MulliganChoce", SourcePlayerId = local };
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(false, false, choice, local, true));
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, false, null, local, true));
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, false, choice, null, true));
            choice.ChoiceKind = "DiscoverChoice";
            Assert.IsFalse(MulliganPrompt.CanSubmitStagedChoice(true, false, choice, local, true));
        }
    }
}
