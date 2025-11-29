using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CostFilter : MonoBehaviour, IFilterRule
{
    public List<FilterCostButton> CostButtons;

	public Action FilterChangedCallBack { get; set; }

	private void Start()
	{
        CostButtons.ForEach(x => x.FilterChangedCallBack = FilterChangedCallBack);

    }

	public Func<CardDefinition, bool> GetPredicate()
    {
        var activePredicates = CostButtons
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
