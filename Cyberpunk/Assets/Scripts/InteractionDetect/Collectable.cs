using UnityEngine;

public class Collectable : MonoBehaviour
{
    private int score = 1;                 // �������Ҫ�ӷֿɸ�Ϊ 0 ��ɾ���������
    private bool collide = true;
    private bool playerInRange = false;

    public InteractionDetect otherScript;  // ������ϵ� InteractionDetect��Inspector ���룩
    public Transform focusPoint;           // QTE��ͷ��UI�Խ��㣬��������������

    private Player cachedPlayer;

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
