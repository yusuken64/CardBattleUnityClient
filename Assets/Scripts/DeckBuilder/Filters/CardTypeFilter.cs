using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardTypeFilter : MonoBehaviour, IFilterRule
{
    public List<FilterCardTypeButton> CardTypeButtons;

    public Action FilterChangedCallBack { get; set; }

    private void Start()
    {
        CardTypeButtons.ForEach(x => x.FilterChangedCallBack = FilterChangedCallBack);
    }

    public Func<CardDefinition, bool> GetPredicate()
    {
        var activePredicates = CardTypeButtons
            .Where(b => b.FilterActive)
            .Select(b => b.GetPredicate())
            .ToList();

        if (activePredicates.Count == 0)
            return card => true;

        return card =>
        {
            foreach (var pred in activePredicates)
            {
                if (pred(card))
                    return true;
            }
            return false;
        };
    }
}
