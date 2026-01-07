using System;
using UnityEngine;

public class FilterCardTypeButton : MonoBehaviour, IFilterRule, IButtonSoundState
{
    public GameObject ActiveIndicator;
    public bool FilterActive { get; internal set; }
    public Action FilterChangedCallBack { get; set; }

    public bool IsActive => FilterActive;

    public CardTypeEnum Filter;

    public Func<CardDefinition, bool> GetPredicate()
    {
        switch (Filter)
        {
            case CardTypeEnum.Minion:
                return card => card is MinionCardDefinition;
            case CardTypeEnum.Spell:
                return card => card is SpellCardDefinition;
            case CardTypeEnum.Weapon:
                return card => card is WeaponCardDefinition;
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

public enum CardTypeEnum
{
    Minion,
    Spell,
    Weapon
}