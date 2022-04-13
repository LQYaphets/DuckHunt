using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace LQ
{
    /// <summary>
    /// 封装的序列帧动画实现 
    /// </summary>
    public class DuckHuntUMovie : MonoBehaviour
    {
        public string MovieName;
        /// <summary>
        /// The gif sprite data list  
        /// </summary>
        public List<Sprite> LSprites;
        public float FSep = 0.05f;
        public bool Loop;
        public bool PlayOnEnable;
        /// <summary>
        /// The event when animation is end
        /// </summary>
        public UnityEngine.Events.UnityEvent OnAnimationPlayEnd;
        public float showerWidth
        {
            get
            {
                if (shower == null)
                {
                    return 0;
                }
                return shower.rectTransform.rect.width;
            }
        }
        public float showerHeight
        {
            get
            {
                if (shower == null)
                {
                    return 0;
                }
                return shower.rectTransform.rect.height;
            }
        }

        private void OnEnable()
        {
            if (PlayOnEnable)
            {
                curFrame = 0;
                fDelta = 0;
                isPlay = true;
            }
        }

        void Awake()
        {
            shower = GetComponent<Image>();
            if (string.IsNullOrEmpty(MovieName))
            {
                MovieName = "movieName";
            }
        }

        void Start()
        {
            if (LSprites.Count != 0)
            {
                Play(curFrame);
            }
        }

        private void Play(int iFrame)
        {
            if (iFrame >= FrameCount)
            {
                iFrame = 0;
            }
            //没有Loop的情况，只会播放一次就结束
            if (!Loop)
                //Animation is end 
                if (iFrame + 1 == FrameCount)
                {
                    isPlay = false;
                    OnAnimationPlayEnd.Invoke();
                }
            shower.sprite = LSprites[iFrame];
            curFrame = iFrame;
        }

        /// <summary>
        /// re play
        /// </summary>
        public void ToPlay()
        {
            curFrame = 0;
            fDelta = 0;
            isPlay = true;
        }

        private void ChangeAnimationAndPlay()
        {
            Play(curFrame);
            isPlay = true;
        }

        private Image shower;
        private int curFrame = 0;
        public int FrameCount
        {
            get
            {
                return LSprites.Count;
            }
        }

        private float fDelta = 0;
        bool isPlay = false;
        void Update()
        {
            if (isPlay)
            {
                fDelta += Time.deltaTime;
                if (fDelta > FSep)
                {
                    fDelta = 0;
                    curFrame++;
                    Play(curFrame);
                }
            }
        }
    }
}