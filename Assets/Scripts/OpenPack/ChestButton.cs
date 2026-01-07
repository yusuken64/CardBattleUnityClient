using UnityEngine;

public class ChestButton : MonoBehaviour, IButtonSoundState
{
	public OpenPackScene OpenPackScene;
	public bool IsActive => OpenPackScene.CanOpenPack();
}
