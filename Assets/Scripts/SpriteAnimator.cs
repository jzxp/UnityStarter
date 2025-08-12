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
    [Header("��������")]
    public List<SpriteAnimationClip> animations;
    [SerializeField] private Vector3 initialPosition = Vector3.zero; // ������ʼλ������
    [SerializeField] private bool useMainCameraAsReference = false; // �Ƿ�ʹ���������Ϊ�ο�

    [Header("�������")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    // ����ʱ����
    private int currentFrame;
    private float timer;
    private SpriteAnimationClip currentClip;
    private bool isPlaying = true;

    void Awake()
    {
        EnsureSpriteRenderer();
        InitializePosition(); // ����λ�ó�ʼ��
    }

    void Start()
    {
        // Ĭ�ϲ��ŵ�һ������
        if (animations != null && animations.Count > 0)
        {
            PlayAnimation(animations[0].name);
        }
    }

    // ����λ�ó�ʼ������
    private void InitializePosition()
    {
        if (useMainCameraAsReference)
        {
            // ��ȡ�������λ��
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                transform.position = mainCam.transform.position + initialPosition;
            }
            else
            {
                Debug.LogWarning("�����δ�ҵ���ʹ��Ĭ��λ��");
                transform.position = initialPosition;
            }
        }
        else
        {
            // ֱ��ʹ�����õĳ�ʼλ��
            transform.position = initialPosition;
        }
    }

    // ȷ�� SpriteRenderer �������
    private void EnsureSpriteRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            Debug.LogWarning($"�Զ���� SpriteRenderer �� {gameObject.name}");
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
            Debug.LogError($"SpriteRenderer ȱʧ�� {gameObject.name}��");
            isPlaying = false;
        }
    }

    // ====== �������� ======
    public void PlayAnimation(string clipName)
    {
        if (animations == null)
        {
            Debug.LogWarning("�����б�Ϊ�գ�");
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
            Debug.LogWarning($"�Ҳ�����������: {clipName}");
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

    // �������������ó�ʼλ�ã���������ʱ���ã�
    public void SetInitialPosition(Vector3 newPosition)
    {
        initialPosition = newPosition;
        transform.position = useMainCameraAsReference && Camera.main != null ?
            Camera.main.transform.position + newPosition :
            newPosition;
    }
}