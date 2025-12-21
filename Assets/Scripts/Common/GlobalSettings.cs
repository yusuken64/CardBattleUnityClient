using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public void Close_Clicked()
    {
        Common.Instance.SaveManager.Save();
        this.gameObject.SetActive(false);
    }
}
