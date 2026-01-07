using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectionFilter : MonoBehaviour
{
    public List<GameObject> Filters;

    public event Action<Func<CardDefinition, bool>> FilterChanged;

    public Func<CardDefinition, bool> CurrentPredicate { get; private set; }
        = card => true;

	private void Start()
	{
        Filters.ForEach(x =>
        {
            var filterRule = x.GetComponent<IFilterRule>();
            filterRule.FilterChangedCallBack = NotifyFiltersUpdated;
        });
    }

	public void NotifyFiltersUpdated()
    {
        // Rebuild predicate based on UI
        CurrentPredicate = BuildPredicate();

        // Notify listeners
        FilterChanged?.Invoke(CurrentPredicate);
    }

    private Func<CardDefinition, bool> BuildPredicate()
    {
        List<Func<CardDefinition, bool>> activeFilters = new();

        foreach (var f in Filters)
        {
            if (!f.activeInHierarchy)
                continue;

            var comp = f.GetComponent<IFilterRule>();
            if (comp == null)
                continue;

            // Each filter returns its own predicate
            activeFilters.Add(comp.GetPredicate());
        }

        // If no filters, return pass-all
        if (activeFilters.Count == 0)
            return card => true;

        return card =>
        {
            foreach (var rule in activeFilters)
            {
                if (!rule(card))
                    return false;
            }
            return true;
        };
    }
}
internal interface IFilterRule
{
	Action FilterChangedCallBack { get; set; }

	Func<CardDefinition, bool> GetPredicate();
}
