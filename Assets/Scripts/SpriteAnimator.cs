using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SpriteAnimationClip
{
    public string name;
    public List<Sprite> frames;
    public float frameRate = 12f;
    public bool loop = true;
}

public class SpriteAnimator : MonoBehaviour
{
    [Header("动画设置")]
    public List<SpriteAnimationClip> animations;
    [SerializeField] private Vector3 initialPosition = Vector3.zero; // 新增初始位置设置
    [SerializeField] private bool useMainCameraAsReference = false; // 是否使用主相机作为参考

    [Header("组件引用")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // 运行时变量
    private int currentFrame;
    private float timer;
    private SpriteAnimationClip currentClip;
    private bool isPlaying = true;

    void Awake()
    {
        EnsureSpriteRenderer();
        InitializePosition(); // 新增位置初始化
    }

    void Start()
    {
        // 默认播放第一个动画
        if (animations != null && animations.Count > 0)
        {
            PlayAnimation(animations[0].name);
        }
    }

    // 新增位置初始化方法
    private void InitializePosition()
    {
        if (useMainCameraAsReference)
        {
            // 获取主相机的位置
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.position = mainCam.transform.position + initialPosition;
            }
            else
            {
                Debug.LogWarning("主相机未找到，使用默认位置");
                transform.position = initialPosition;
            }
        }
        else
        {
            // 直接使用设置的初始位置
            transform.position = initialPosition;
        }
    }

    // 确保 SpriteRenderer 组件存在
    private void EnsureSpriteRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.LogWarning($"自动添加 SpriteRenderer 到 {gameObject.name}");
        }
    }

    void Update()
    {
        if (!isPlaying || currentClip == null) return;
        if (currentClip.frames == null || currentClip.frames.Count == 0) return;

        if (spriteRenderer == null)
        {
            EnsureSpriteRenderer();
            return;
        }

        float frameDuration = 1f / currentClip.frameRate;
        timer += Time.deltaTime;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            NextFrame();
        }
    }

    void NextFrame()
    {
        currentFrame++;

        if (currentFrame >= currentClip.frames.Count)
        {
            if (currentClip.loop)
            {
                currentFrame = 0;
            }
            else
            {
                currentFrame = currentClip.frames.Count - 1;
                isPlaying = false;
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = currentClip.frames[currentFrame];
        }
        else
        {
            Debug.LogError($"SpriteRenderer 缺失于 {gameObject.name}！");
            isPlaying = false;
        }
    }

    // ====== 公共方法 ======
    public void PlayAnimation(string clipName)
    {
        if (animations == null)
        {
            Debug.LogWarning("动画列表为空！");
            return;
        }

        SpriteAnimationClip clip = animations.Find(a => a.name == clipName);

        if (clip != null)
        {
            currentClip = clip;
            currentFrame = 0;
            timer = 0;
            isPlaying = true;

            if (clip.frames != null && clip.frames.Count > 0)
                spriteRenderer.sprite = clip.frames[0];
        }
        else
        {
            Debug.LogWarning($"找不到动画剪辑: {clipName}");
        }
    }

    public void Pause() => isPlaying = false;
    public void Resume() => isPlaying = true;

    public void Stop()
    {
        isPlaying = false;
        currentFrame = 0;
        if (currentClip != null && currentClip.frames != null && currentClip.frames.Count > 0)
            spriteRenderer.sprite = currentClip.frames[0];
    }

    // 新增方法：设置初始位置（可在运行时调用）
    public void SetInitialPosition(Vector3 newPosition)
    {
        initialPosition = newPosition;
        transform.position = useMainCameraAsReference && Camera.main != null ?
            Camera.main.transform.position + newPosition :
            newPosition;
    }
}