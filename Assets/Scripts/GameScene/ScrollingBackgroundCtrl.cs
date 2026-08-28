using UnityEngine;

public class ScrollingBackgroundCtrl : MonoBehaviour
{
    //Background Layers
    public Transform[] Background;

    //Scrolling Speeds
    public float[] ScrollSpeed;

    //Renderer
    public MeshRenderer[] Ren;
    public MeshRenderer SkyRen;

    //Movement speed according to keyboard input
    public float MoveValue;
    public float MoveSpeed;

    //Scroll of the sky
    public float SkyMoveValue;
    public float SkyScrollSpeed;

    public string BackgroundId;

	void Start()
    {
        //Reset Values
        MoveValue = 0;
        SkyMoveValue = 0;

        //Get MeshRenderers
        for (int i = 0; i < Background.Length; i++)
            Ren[i] = Background[i].GetComponent<MeshRenderer>();
    }


    void Update()
    {
        //Material OffSet
        for (int i = 0; i < Background.Length; i++)
            Ren[i].material.mainTextureOffset = new Vector2(MoveValue * ScrollSpeed[i], 0);

        SkyRen.material.mainTextureOffset = new Vector2(SkyMoveValue += (Time.unscaledDeltaTime * -SkyScrollSpeed), 0);
    }
}