using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public void Close_Clicked()
    {
        Common.Instance.SaveManager.Save(Common.Instance.SaveData);
        this.gameObject.SetActive(false);
    }
}
