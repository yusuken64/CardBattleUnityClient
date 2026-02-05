using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public GameObject staticBG;
    public List<ScrollingBackgroundCtrl> backgrounds;
    private ScrollingBackgroundCtrl _selected;

    public void Start()
    {
        ActivateBackgroundByName(GameManager.BackgroundName);
        GameManager.BackgroundName = null;
    }

    public void ActivateBackgroundByName(string id)
    {
        // Always disable everything first
        foreach (var bg in backgrounds)
            bg.gameObject.SetActive(false);

        // Find the requested one
        _selected = backgrounds.FirstOrDefault(x => x.BackgroundId == id);

        if (_selected == null)
        {
            // Fallback to static background
            staticBG.SetActive(true);
            return;
        }

        // Activate scrolling background
        staticBG.SetActive(false);
        _selected.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_selected != null)
        {
            _selected.MoveValue += _selected.MoveSpeed * Time.deltaTime;
        }
    }

    public void StopScrolling()
    {
        _selected = null;
    }

    [ContextMenu("Test Beach")]
    public void TestBeach()
    {
        ActivateBackgroundByName("Beach");
    }
}