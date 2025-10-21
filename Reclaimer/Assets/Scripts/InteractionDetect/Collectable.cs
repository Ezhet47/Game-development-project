using UnityEngine;

public class Collectable : MonoBehaviour
{
    public int score = 1;                 // 如果不需要加分可改为 0 或删掉相关两行
    private bool collide = true;
    private bool playerInRange = false;

    public InteractionDetect otherScript;  // 玩家身上的 InteractionDetect（Inspector 拖入）
    public Transform focusPoint;           // QTE镜头与UI对焦点，空则用物体自身

    private Player cachedPlayer;

    [Header("GET Popup")]
    public Popup PopupPrefab;       // 预制体（Inspector 拖入）
    public Sprite getIcon;               // 显示的图标（Inspector 拖入）
    public string getText = "GET";       // 显示的文字
    [Header("Collectable Sounds")]
    public AudioSource audioSource;      
    public AudioClip[] collectClips;
    public AudioClip fail;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            cachedPlayer = collision.GetComponentInParent<Player>();
        }
    }
    private void SpawnGetPopup(Transform where)
    {
        if (PopupPrefab == null) return;

        Popup popup = Instantiate(PopupPrefab);

        // 用“头顶点”作为基准（和 QTE 一致）
        Transform target = where ? where : transform;
        Vector3 worldPos = GetTopOfBounds(target);

        popup.ShowAtWorld(worldPos, getIcon, getText, Camera.main);
    }

    private Vector3 GetTopOfBounds(Transform t)
    {
        if (!t) return transform.position;
        var sr = t.GetComponentInChildren<SpriteRenderer>();
        if (sr) { var b = sr.bounds; return new Vector3(b.center.x, b.max.y, t.position.z); }
        var col = t.GetComponentInChildren<Collider2D>();
        if (col) { var b = col.bounds; return new Vector3(b.center.x, b.max.y, t.position.z); }
        return t.position + new Vector3(0f, 0.5f, 0f);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (collision.GetComponentInParent<Player>() == cachedPlayer)
                cachedPlayer = null;
        }
    }

    private void Update()
    {
        if (playerInRange && collide && Input.GetKeyDown(KeyCode.E))
        {
            // 防抖：进入QTE前关提示+锁一次触发
            collide = false;
            if (otherScript) otherScript.canpress = false;

            // 冻结玩家移动
            if (cachedPlayer) cachedPlayer.canMove = false;

            // 设定对焦点
            Transform focus = focusPoint ? focusPoint : transform;

            // 启动QTE
            UI_QTE.Instance.StartSkillCheck(
                focus,
success: () =>
{
    // 恢复移动
    if (cachedPlayer) cachedPlayer.canMove = true;

    // 可选：加分
    if (ComponentCount.instance != null && score != 0)
    {
        ComponentCount.instance.totalComponents += score;
        ComponentCount.instance.UpdateTotalScore();
    }

    GameManager.Instance.HasCollected = true;

    // ? 播放随机音效
    PlayRandomCollectSound();

    // 成功：销毁
    SpawnGetPopup(focusPoint != null ? focusPoint : transform);
    Destroy(gameObject);
    
},
                fail: () =>
                {
                    // 失败：恢复可重试
                    if (cachedPlayer) cachedPlayer.canMove = true;
                    collide = true;
                    if (otherScript) otherScript.canpress = true;
                    PlayFailSound();
                }
                
            );
        }
    }
    private void PlayRandomCollectSound()
    {
        if (audioSource == null || collectClips == null || collectClips.Length == 0) return;

        int index = Random.Range(0, collectClips.Length);
        float volume = Random.Range(0.9f, 1.0f); 
        audioSource.PlayOneShot(collectClips[index], volume);
        
    }
    private void PlayFailSound()
    {
        if (audioSource == null || fail == null) return;
        audioSource.PlayOneShot(fail, 1f);
    }

}
