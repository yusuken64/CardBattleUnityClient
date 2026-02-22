using UnityEngine;
using UnityEngine.UI;

namespace BattleBGCtrlTest
{
    [System.Serializable]
    /// </summary>
    //This script is used for '2DBattleBackgound' demo play.
    //Used in the editor only.
    //This is a sample script and stability and optimization are not guaranteed.
    //The standard resolution of the demo scene is 1080x1920 pixels
    /// </summary>

    public class BattleBGDemoPlayCtrl : MonoBehaviour
    {
        //Automatically switch background image
        private bool IsAutoPlay;
        public Toggle ToggleAutoPlay;

        //Set how many seconds each background images should be displayed
        private float CurTime = 0;
        public float AutoPlayShowTime = 1.2f;

        public BattleBGCtrl _BattleBGCtrl;

        //background image count
        private int CurThemeCount = 0;
        private int BGViewCount = 0;//background image count
        private int BGImageCount = 0;//Number of images per theme
        public int BGImageStep = 1;//Number of images skipped
        public int BGImageMax = 4;//Maximum number of background images per theme
        public bool _IsShowFullImage;//Show all background images

#if UNITY_EDITOR
        // Start is called before the first frame update
        void Start()
        {
            //Auto (Scrolling) play
            ToggleAutoPlay.onValueChanged.AddListener(delegate
            {
                ToggleAutoPlaySet(ToggleAutoPlay);
            });
        }

        // Update is called once per frame
        void Update()
        {
            if (IsAutoPlay)
            {
                CurTime += Time.unscaledDeltaTime;
                if (CurTime > AutoPlayShowTime)
                {
                    if (BGImageStep > 0)
                        BGViewCount += BGImageStep;
                    else
                        BGViewCount += 1;

                    CurTime = 0.0f;

                    if ((BGViewCount / BGImageStep) < BGImageCount)
                    {
                        if (BGViewCount < _BattleBGCtrl.BGImageCount)
                        { 
                           _BattleBGCtrl.BGNumSet(BGImageStep);
                        }

                        if (BGViewCount >= _BattleBGCtrl.BGImageCount)
                        {
                            ThemeCheck();
                        }
                    }

                    if (((BGViewCount / BGImageStep) >= BGImageCount) && IsAutoPlay)
                    {
                        ThemeCheck();
                    }
                }
            }
        }

        public void ThemeCheck()
        {
            CurThemeCount += 1;
            if (CurThemeCount <= _BattleBGCtrl.ThemeCount)
            {
                _BattleBGCtrl.ThemeNumSet(CurThemeCount);
                BGViewCount = 0;
                BGImageCountSetting();
            }
            if (CurThemeCount > _BattleBGCtrl.ThemeCount)
            {
                IsAutoPlay = false;
                ToggleAutoPlay.isOn = false;
            }
        }

        //Maximum number of background images per theme
        public void BGImageCountSetting()
        {
            if (_IsShowFullImage)
                BGImageCount = _BattleBGCtrl.BGImageCount;
            else
                BGImageCount = BGImageMax;

        }

        //Automatically shows background image during play
        public void ToggleAutoPlaySet(Toggle _IsAutoPlay)
        {
            IsAutoPlay = ToggleAutoPlay.isOn;
            _BattleBGCtrl._IsAutoPlayModeSetting(ToggleAutoPlay.isOn);
            BGImageCountSetting();

            if (ToggleAutoPlay.isOn)
            {
                BGViewCount = 0;
                _BattleBGCtrl.ThemeNumSet(0);
                _BattleBGCtrl._IsAutoPlayModeSetting(true);
            }

            if (!ToggleAutoPlay.isOn)
            {
                CurThemeCount = 0;
                CurTime = 0;
                _BattleBGCtrl._IsAutoPlayModeSetting(false);
            }
        }
#endif
    }
}
