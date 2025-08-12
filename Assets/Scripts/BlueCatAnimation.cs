using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlueCatAnimation : MonoBehaviour
{

    private Image ImageSource;
    private int mCurFrame = 0;
    private float mDelta = 0;
    public float FPS = 5;
    public List<Sprite> SpriteFrames;
    public bool IsPlaying = false;
    public bool Foward = true;
    public bool AutoPlay = false;
    public bool Loop = false;
    public int FrameCount
    {
        get { return SpriteFrames.Count; }
    }

    void Awake()
    {
        ImageSource = GetComponent<Image>();
    }


    void Start()
    {
        if (AutoPlay)
        {
            Play();
        }
        else
        {
            IsPlaying = false;
        }
    }

    private void SetSprite(int idx)
    {
        ImageSource.sprite = SpriteFrames[idx];
        ImageSource.SetNativeSize();//设置为原本大小
    }

    //正常播放
    public void Play()
    {
        IsPlaying = true;//播放
        Foward = false;//前进否
    }

    //倒放
    public void PlayReverse()
    {
        IsPlaying = true;//播放
        Foward = false;//前进否
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsPlaying || 0 == FrameCount)
        {
            return;
        }

        mDelta += Time.deltaTime;
        if (mDelta > 1 / FPS)
        {
            mDelta = 0;
            if (Foward)
            {
                mCurFrame++;
            }
            else
            {
                mCurFrame--;
            }
            if (mCurFrame >= FrameCount)
            {
                if (Loop)
                {
                    mCurFrame = 0;
                }
                else
                {
                    IsPlaying = false;
                    return;
                }
            }
            else if (mCurFrame < 0)
            {
                if (Loop)
                {
                    mCurFrame = FrameCount - 1;
                }
                else
                {
                    IsPlaying = false;
                    return;
                }
            }
            SetSprite(mCurFrame);
        }
    }


    //暂停
    public void Pause()
    {
        IsPlaying = false;
    }

    //继续
    public void Resume()
    {
        if (!IsPlaying)
        {
            IsPlaying = true;
        }
    }


    //停止
    public void Stop()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        IsPlaying = false;
    }


    //重新开始
    public void Rewind()
    {
        mCurFrame = 0;
        SetSprite(mCurFrame);
        Play();
    }

}
