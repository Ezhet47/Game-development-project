using UnityEngine;

public class Collectable : MonoBehaviour
{
    public int score = 1;                 // �������Ҫ�ӷֿɸ�Ϊ 0 ��ɾ���������
    private bool collide = true;
    private bool playerInRange = false;

    public InteractionDetect otherScript;  // ������ϵ� InteractionDetect��Inspector ���룩
    public Transform focusPoint;           // QTE��ͷ��UI�Խ��㣬��������������

    private Player cachedPlayer;

    [Header("GET Popup")]
    public Popup PopupPrefab;       // Ԥ���壨Inspector ���룩
    public Sprite getIcon;               // ��ʾ��ͼ�꣨Inspector ���룩
    public string getText = "GET";       // ��ʾ������
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

        // �á�ͷ���㡱��Ϊ��׼���� QTE һ�£�
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
            // ����������QTEǰ����ʾ+��һ�δ���
            collide = false;
            if (otherScript) otherScript.canpress = false;

            // ��������ƶ�
            if (cachedPlayer) cachedPlayer.canMove = false;

            // �趨�Խ���
            Transform focus = focusPoint ? focusPoint : transform;

            // ����QTE
            UI_QTE.Instance.StartSkillCheck(
                focus,
success: () =>
{
    // �ָ��ƶ�
    if (cachedPlayer) cachedPlayer.canMove = true;

    // ��ѡ���ӷ�
    if (ComponentCount.instance != null && score != 0)
    {
        ComponentCount.instance.totalComponents += score;
        ComponentCount.instance.UpdateTotalScore();
    }

    GameManager.Instance.HasCollected = true;

    // ? ���������Ч
    PlayRandomCollectSound();

    // �ɹ�������
    SpawnGetPopup(focusPoint != null ? focusPoint : transform);
    Destroy(gameObject);
    
},
                fail: () =>
                {
                    // ʧ�ܣ��ָ�������
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
