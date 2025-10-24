using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_CollectionDialogue : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas targetCanvas;              
    public RectTransform bubbleRoot;         
    public TextMeshProUGUI textLabel;        
    public CanvasGroup canvasGroup;          

    [Header("Typing")]
    [Range(1, 120)] public float charsPerSecond = 25f;  
    public bool clickToCompleteLine = true;             
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); 

    [Header("E-Hint")]
    public GameObject eHintRoot;             
    public Image eHintIcon;                  
    public TextMeshProUGUI eHintLabel;       
    public bool canPress = false;           
    public bool followPlayerFlipX = false;   

    [Header("Input Guard")]
    [SerializeField] private float inputIgnoreDuration = 0.1f; 
    private float inputAllowedAt = 0f;
    
    [Header("Voice / Blip Settings")]
    public AudioSource voiceSource;          
    public AudioClip[] blipClips;            
    [Tooltip("blip 触发间隔（秒），防止过密；留空则按速度自动算")]
    public float blipInterval = -1f;         
    [Range(0f, 1f)] public float blipVolume = 0.9f;
    [Tooltip("避免对空格与常见标点触发 blip")]
    public string muteChars = " \n\r\t.,;:!?，。；：！？…";
    [Tooltip("音高随机范围")]
    public Vector2 pitchRandom = new Vector2(0.96f, 1.04f);
    [Tooltip("每 N 个字符才触发一次 blip（1 = 每个合格字符都触发）")]
    public int blipEveryNChars = 1;
    [Header("Blip Audio Pool")]
    [SerializeField] private int blipPoolSize = 6;   
    private AudioSource[] blipPool;
    private int blipPoolCursor = 0;
    
    private List<string> lines = new List<string>();
    private int index = -1;
    private Coroutine typingRoutine;
    private bool isShowing = false;
    private bool isTyping = false;
    private float nextBlipTime = 0f;
    private int typedCountOnLine = 0;

    private Transform followTarget;          
    private Transform playerTransForFlip;    
    private Player cachedPlayer;             
    private Camera cam;
    
    public static UI_CollectionDialogue Instance { get; private set; }
    public bool IsShowing => isShowing;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (!bubbleRoot) bubbleRoot = GetComponent<RectTransform>();
        cam = Camera.main;
        
        if (!voiceSource) voiceSource = GetComponent<AudioSource>();
        if (!voiceSource) voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f; 

        HideImmediate();
        blipPool = new AudioSource[blipPoolSize];
        for (int i = 0; i < blipPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;   
            src.dopplerLevel = 0f;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.volume = 1f;         
            blipPool[i] = src;
        }
    }

    void LateUpdate()
    {
        if (!isShowing || followTarget == null || targetCanvas == null || bubbleRoot == null)
        {
            ApplyEHintActive();
            return;
        }
        
        var worldPos = followTarget.position + worldOffset;

        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            bubbleRoot.position = worldPos;
            bubbleRoot.rotation = Quaternion.identity;
        }
        else
        {
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam ? cam : Camera.main, worldPos);
            RectTransform canvasRect = targetCanvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screen,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 local
            );

            if (bubbleRoot.parent != canvasRect)
                bubbleRoot.SetParent(canvasRect, worldPositionStays: false);

            bubbleRoot.anchoredPosition = local;
            bubbleRoot.localRotation = Quaternion.identity;
        }
        
        if (Time.time >= inputAllowedAt && Input.GetKeyDown(KeyCode.E))
        {
            if (isTyping && clickToCompleteLine)
            {
                CompleteTypingInstant();        
            }
            else
            {
                Next();                         
            }
        }
        
        ApplyEHintActive();

        if (followPlayerFlipX && playerTransForFlip && eHintRoot)
        {
            var rt = eHintRoot.transform as RectTransform;
            if (rt)
            {
                Vector3 ls = rt.localScale;
                ls.x = Mathf.Sign(-playerTransForFlip.localScale.x) * Mathf.Abs(ls.x);
                rt.localScale = ls;
            }
        }
    }
    
    public void StartDialogue(IEnumerable<string> content, Transform follow, Player player)
    {
        lines.Clear();
        lines.AddRange(content);
        followTarget = follow;
        cachedPlayer = player ? player : FindFirstObjectByType<Player>();
        index = -1;
        
        if (cachedPlayer) cachedPlayer.canMove = false;
        if (cachedPlayer) playerTransForFlip = cachedPlayer.transform;
        
        isShowing = true;
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        bubbleRoot.gameObject.SetActive(true);
        
        canPress = true;
        UpdateEHintTypingState(true);
        ApplyEHintActive();
        
        Next();
        
        inputAllowedAt = Time.time + inputIgnoreDuration;
    }
    
    public void Next()
    {
        if (!isShowing) return;

        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }
    
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        UpdateEHintTypingState(true);      
        textLabel.text = "";
        typedCountOnLine = 0;

        float perCharDelay = 1f / Mathf.Max(1f, charsPerSecond);
        float interval = (blipInterval > 0f) ? blipInterval : (perCharDelay * 0.6f); 
        nextBlipTime = 0f; 

        foreach (char c in line)
        {
            textLabel.text += c;
            typedCountOnLine++;

            TryPlayBlip(c, interval);

            yield return new WaitForSeconds(perCharDelay);
        }

        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     
    }

    private void TryPlayBlip(char c, float interval)
    {
        if (blipClips == null || blipClips.Length == 0) return;
        
        if (muteChars.Contains(c.ToString())) return;
        
        if (Time.time < nextBlipTime) return;
        if (blipEveryNChars > 1 && (typedCountOnLine % blipEveryNChars) != 0) return;
        
        AudioSource src = null;
        for (int k = 0; k < blipPoolSize; k++)
        {
            int idxPool = (blipPoolCursor + k) % blipPoolSize;
            if (!blipPool[idxPool].isPlaying) { src = blipPool[idxPool]; blipPoolCursor = (idxPool + 1) % blipPoolSize; break; }
        }
        if (src == null)
        {
            src = blipPool[blipPoolCursor];
            blipPoolCursor = (blipPoolCursor + 1) % blipPoolSize;
        }
        
        int idx = Random.Range(0, blipClips.Length);
        var clip = blipClips[idx];

        src.clip = clip;
        src.pitch = Random.Range(pitchRandom.x, pitchRandom.y);
        src.volume = blipVolume;
        
        src.Play();
        
        nextBlipTime = Time.time + (interval > 0f ? interval : 0.04f);
    }
    
    private void CompleteTypingInstant()
    {
        if (!isTyping) return;
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        textLabel.text = lines[index];
        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false); 
    }
    
    public void EndDialogue()
    {
        isShowing = false;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        bubbleRoot.gameObject.SetActive(false);

        if (cachedPlayer) cachedPlayer.canMove = true;
        
        lines.Clear();
        index = -1;
        followTarget = null;
        playerTransForFlip = null;
        cachedPlayer = null;
        
        canPress = false;
        ApplyEHintActive();
    }

    private void HideImmediate()
    {
        if (bubbleRoot) bubbleRoot.gameObject.SetActive(false);
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        canPress = false;
        ApplyEHintActive();
    }
    
    private void ApplyEHintActive()
    {
        if (eHintRoot) eHintRoot.SetActive(canPress);
    }

    private void UpdateEHintTypingState(bool typing)
    {
        if (!eHintLabel) return;

        if (typing)
        {
            eHintLabel.text = "按 E 补完";
        }
        else
        {
            bool isLast = (index >= 0 && index == lines.Count - 1);
            eHintLabel.text = isLast ? "按 E 结束" : "按 E 下一页";
        }
    }
}
