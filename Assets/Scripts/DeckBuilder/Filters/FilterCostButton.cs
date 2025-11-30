using System;
using UnityEngine;

public class FilterCostButton : MonoBehaviour, IFilterRule, IButtonSoundState
{
    public GameObject ActiveIndicator;
	public bool FilterActive { get; internal set; }
	public Action FilterChangedCallBack { get; set; }

	public bool IsActive => FilterActive;

	public string Filter;

    public Func<CardDefinition, bool> GetPredicate()
    {
        if (string.IsNullOrWhiteSpace(Filter))
            return card => true;

        // Trim whitespace to be safe
        var f = Filter.Trim();

        // Check for the "X+" case (greater or equal)
        if (f.EndsWith("+"))
        {
            var numberPart = f.Substring(0, f.Length - 1);

            if (int.TryParse(numberPart, out int minCost))
            {
                return card => card.Cost >= minCost;
            }

            // Fail-safe: if parsing fails, return pass-all
            return card => true;
        }

        // Otherwise it must be an exact number
        if (int.TryParse(f, out int exactCost))
        {
            return card => card.Cost == exactCost;
        }

        // Fail-safe fallback
        return card => true;
    }

	private void Start()
	{
        FilterActive = false;
        ActiveIndicator.SetActive(FilterActive);
    }

	public void TogggleActive()
	{
        FilterActive = !FilterActive;
        ActiveIndicator.SetActive(FilterActive);
        FilterChangedCallBack?.Invoke();
    }
}
