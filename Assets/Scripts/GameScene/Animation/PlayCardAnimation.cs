using CardBattleEngine;
using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayCardAnimation : GameActionAnimation<PlayCardAction>
{
    public AudioClip PlayCardClip;
    public override IEnumerator Play()
    {
        Common.Instance.AudioManager.PlaySound(PlayCardClip);
        var player = GameManager.GetPlayerFor(Presentation.SourcePlayer);
        var data = Presentation.SourceCard;
        var playedCard = player.Hand.Cards.FirstOrDefault(x => x.Data?.Id == data?.Id);
        if (playedCard == null && Presentation.IsNetwork && player == GameManager.Opponent)
            playedCard = player.Hand.Cards.FirstOrDefault();
        if (playedCard == null) yield break;
        Presentation.Own(playedCard);
        player.Hand.Cards.Remove(playedCard);
        player.Hand.UpdateCardPositions();
        playedCard.Moving = false;
        playedCard.Dragging = true;
        if (player == GameManager.Opponent && data != null)
        {
            playedCard.Setup(data);
            UI ui = FindFirstObjectByType<UI>();
            yield return playedCard.transform.DOMove(ui.CardPreview.transform.position, 0.5f).SetId(Presentation).SetEase(Ease.OutQuad).WaitForCompletion();
            playedCard.FlippableCard.CanFlip = true;
            playedCard.FlippableCard.Flip();
            playedCard.ForceReveal = true;
            ui.PreviewStart(playedCard);
            Destroy(playedCard.gameObject);
            yield return new WaitForSecondsRealtime(2f);
            ui.PreviewEnd();
        }
        else Destroy(playedCard.gameObject);
    }
}
