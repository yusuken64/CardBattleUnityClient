using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

namespace BattleBGCtrlTest
{
    [System.Serializable]
    /// </summary>
    //This script is used for '2DBattleBackgound' demo play.
    //Used in the editor only.
    //This is a sample script and stability and optimization are not guaranteed.
    //The standard resolution of the demo scene is 1080x1920 pixels
    /// </summary>

    public class BattleBGCtrl : MonoBehaviour
    {
        //UI for testing
        public GameObject TestUI;

        //On/off of test (sample) UI
        public GameObject TestUIOn;
        public GameObject TestUIOff;

        //Brightness value of background image
        public Slider _BGBrightness;

        //Name of currently applied background texture
        public Text TxtBGName;

        //background theme & image count UI
        public Text TxtThemeCount;
        public Text TxtBGCount;
        public Text ThemeNameTxt;
        public Text BGNameTxt;

        //Button for switching background image
        public GameObject Btn_UPArrow, Btn_DownArrow, Btn_LArrow, Btn_RArrow;

        //Background mesh objects and textures and MeshRenderer
        public Transform BG_Object;
        Texture2D BGtexture = null;
        private MeshRenderer RenBG;

        //Background image path 
        public Object BGImageDir;
        private string BGDirPath;
        private string ThemeDirName;

        //background texture <list>
        public List<string> ThemeNameStr;
        public List<string> BGNameStr;

        //Theme & BGImage Count & Set-Value
        private int ThemeNum;
        private int BGImageNum;
        public int ThemeCount;
        public int BGImageCount;

        //Length of all background images
        public int BGImageLength = 0;

        //BG_Grid
        public GameObject BGGrid_Object;

        //Sample_Characters
        public List<GameObject> CharPos = new List<GameObject>();
        public Toggle ToggleViewChar;

        //Brightness value of Sample_Characters
        public Slider _CharBrightness;


        //Set keyboard input time interval
        bool IsBGChange;
        public float BGChangeBtnSpeed = 2.0f;
        private float CurBGChangeBtnSpeed = 0.0f;

        //Auto Play
        public bool _IsAutoPlay;


#if UNITY_EDITOR
        // Start is called before the first frame update
        void Start()
        {
            //Get the background mesh renderer
            RenBG = BG_Object.GetComponent<MeshRenderer>();

            //Reset
            IsBGChange = false;
            BGNameStr.Clear();

            //Adds a listener to the main slider and invokes a method when the value changes.
            //Background image brightness
            _BGBrightness.onValueChanged.AddListener(delegate { BGBrightnessCheck(); });

            //Brightness value of Sample_Characters
            _CharBrightness.onValueChanged.AddListener(delegate { CharBrightnessCheck(); });

            //Get sample characters(Prefab name = "CharPos")
            GameObject[] temp = FindObjectsOfType<GameObject>();
            for (int i = 0; i < temp.Length; i++)
            {
                if (temp[i].name == "CharPos")
                    CharPos.Add(temp[i]);
            }

            //Sample character display toggle value
            ToggleViewChar.onValueChanged.AddListener(delegate
            {
                ToggleViewCharSet(ToggleViewChar);
            });

            //Test UI state setting (visible)
            TestUISetting(true);

            //Background image settings
            ThemeGetting();
        }

        //Settings according to test UI on/off
        public void TestUISetting(bool _IsOn)
        {
            TestUI.SetActive(_IsOn);
            BGGrid_Object.SetActive(_IsOn);
            TestUIOn.SetActive(!_IsOn);
            TestUIOff.SetActive(_IsOn);
        }

        //Specify the background folder path and retrieve theme information
        public void ThemeGetting()
        {
            //Specify background folder path
            BGDirPath = UnityEditor.AssetDatabase.GetAssetPath(BGImageDir);

            //Get the names (=themes) of subfolders from the background folder
            if (System.IO.Directory.Exists(BGDirPath))
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(BGDirPath);
                foreach (System.IO.DirectoryInfo Dir in di.GetDirectories())
                {
                    //Get name of subfolder
                    ThemeDirName = Dir.Name;

                    //Add theme names to the list
                    ThemeNameStr.Add(ThemeDirName);
                }

                //Number of themes 
                ThemeCount = ThemeNameStr.Count - 1;

                //theme settings
                ThemeNumSet(0);
            }
            else
            {
                Debug.Log("!!Need to check if 'Background' folder is link to 'BGDirPath'!!");
            }
        }



        //Get BGimages(=background image texture)
        public void BGImageGetting()
        {
            //Get the address of the subfolder (=theme folder) of the 'background' folder
            DirectoryInfo BGDir = new DirectoryInfo(BGDirPath + "/" + ThemeNameStr[ThemeNum]);

            //Add BGImage names to the list
            BGNameStr.Clear();
            foreach (FileInfo File in BGDir.GetFiles("*.png"))
            {
                BGNameStr.Add($"{Path.GetFileNameWithoutExtension(File.Name)}");
            }

            //Number of BGimages
            BGImageCount = BGNameStr.Count - 1;

            //BGimage setting
            BGNumSet(0);
        }

        public void ThemeNumSetAdd(int AddNum)
        {
            if (!_IsAutoPlay)
            {
                ThemeNum += AddNum;
                ThemeNumSet(ThemeNum);
            }
        }

        //Theme setting
        public void ThemeNumSet(int Num)
        {
            ThemeNum = Num;

            if (ThemeNum > ThemeCount)
                ThemeNum = 0;

            if (ThemeNum < 0)
                ThemeNum = ThemeCount;

            ThemeNameTxt.text = ThemeNameStr[ThemeNum];
            TxtThemeCount.text = (ThemeNum + 1) + " / " + (ThemeCount + 1);
            BGImageNum = 0;
            BGImageGetting();
        }

        //Background image switching
        public void BGNumSetAdd(int AddNum)
        {
            if (!_IsAutoPlay)
            {
                BGNumSet(AddNum);
            }
        }

        //Background image Setting
        public void BGNumSet(int _AddNum)
        {
            BGImageNum += _AddNum;
            if (BGImageNum > BGImageCount)
                BGImageNum = 0;

            if (BGImageNum < 0)
                BGImageNum = BGImageCount;

            //Display UI
            BGNameTxt.text = BGNameStr[BGImageNum];
            TxtBGCount.text = (BGImageNum + 1) + " / " + (BGImageCount + 1);

            //Apply texture
            string CurPath = BGDirPath + "/" + ThemeNameStr[ThemeNum] + "/" + BGNameStr[BGImageNum] + ".png";
            byte[] byteTexture = System.IO.File.ReadAllBytes(CurPath);
            if (byteTexture.Length > 0)
            {
                BGtexture = new Texture2D(0, 0);
                BGtexture.LoadImage(byteTexture);
                RenBG.material.SetTexture("_MainTex", BGtexture);
            }
            IsBGChange = true;
        }

        //Settings according to automatic play on/off
        public void _IsAutoPlayModeSetting(bool _IsAutoPlayMode)
        {
            _IsAutoPlay = _IsAutoPlayMode;
            Btn_UPArrow.SetActive(!_IsAutoPlayMode);
            Btn_DownArrow.SetActive(!_IsAutoPlayMode);
            Btn_LArrow.SetActive(!_IsAutoPlayMode);
            Btn_RArrow.SetActive(!_IsAutoPlayMode);
        }

        // Update is called once per frame
        void Update()
        {
            //Theme & Background image switching
            if (!IsBGChange && !_IsAutoPlay)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    ThemeNum += 1;
                    ThemeNumSet(ThemeNum);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    ThemeNum -= 1;
                    ThemeNumSet(ThemeNum);
                }

                if (Input.GetKey(KeyCode.RightArrow))
                    BGNumSetAdd(1);

                if (Input.GetKey(KeyCode.LeftArrow))
                    BGNumSetAdd(-1);

                CurBGChangeBtnSpeed = 0.0f;
            }

            //Background image transition(switching time) interval
            if (IsBGChange)
            {
                CurBGChangeBtnSpeed += BGChangeBtnSpeed * Time.deltaTime;
                if (CurBGChangeBtnSpeed > 1.0f)
                {
                    CurBGChangeBtnSpeed = 0.0f;
                    IsBGChange = false;
                }
            }
        }

        // Invoked when the value of the slider changes.
        //Brightness value of background image
        public void BGBrightnessCheck()
        {
            RenBG.material.SetFloat("_Brightness", _BGBrightness.value);
        }

        //Brightness value of sample Char
        public void CharBrightnessCheck()
        {
            for (int i = 0; i < CharPos.Count; i++)
            {
                GameObject CurObj = CharPos[i].transform.GetChild(0).gameObject;
                GameObject CurChar = CurObj.transform.GetChild(0).gameObject;
                MeshRenderer CurCharRen = CurChar.GetComponent<MeshRenderer>();
                CurCharRen.material.SetFloat("_Brightness", _CharBrightness.value);
            }
        }

        //Sample_Characters On/Off
        public void ToggleViewCharSet(Toggle _CharPosSet)
        {
            for (int i = 0; i < CharPos.Count; i++)
            {
                CharPos[i].SetActive(ToggleViewChar.isOn);
            }
        }
#endif
    }

}
