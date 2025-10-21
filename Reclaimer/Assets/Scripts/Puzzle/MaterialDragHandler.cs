using System.Collections.Generic;
using UnityEngine;

public class MaterialDragHandler : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    [HideInInspector] public Vector3 initialWorldPos;

    void Start()
    {
        initialWorldPos = transform.position;
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(null);
        transform.position = initialWorldPos;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isDragging = true;
                offset = transform.position - mouseWorld;
                transform.SetParent(null);
            }
        }

        if (isDragging && Input.GetMouseButton(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mouseWorld + offset;
        }

        if (isDragging && Input.GetMouseButtonUp(1))
        {
            isDragging = false;
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.5f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("CombineSlot"))
                {
                    transform.position = hit.transform.position;
                    transform.SetParent(hit.transform);
                    TryCombineIfAllReady();
                    return;
                }
            }
        }
    }

    private void TryCombineIfAllReady()
    {
        var mgr = PipeManager.Instance;
        if (mgr == null || mgr.ResultSlot == null) return;

        Transform slot = mgr.ResultSlot;
        int count = slot.childCount;
        if (count < 3) return; 

        List<string> ids = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var idComp = slot.GetChild(i).GetComponent<MaterialID>();
            if (idComp != null && !string.IsNullOrEmpty(idComp.id))
                ids.Add(idComp.id);
        }

        HashSet<string> unique = new HashSet<string>(ids);
        if (unique.Count < 3)
        {
            ResetAll(slot);
            return;
        }

        bool group1 = ContainsAll(ids, "A", "C", "F");
        bool group2 = ContainsAll(ids, "B", "D", "G");

        if (group1 || group2)
        {
            DestroyAll(slot);
            mgr.SpawnExternalPipe(group1 ? 0 : 1);
        }
        else
        {
            ResetAll(slot);
        }
    }

    private void DestroyAll(Transform slot)
    {
        for (int i = slot.childCount - 1; i >= 0; i--)
            Destroy(slot.GetChild(i).gameObject);
    }

    private void ResetAll(Transform slot)
    {
        for (int i = slot.childCount - 1; i >= 0; i--)
        {
            Transform t = slot.GetChild(i);
            var drag = t.GetComponent<MaterialDragHandler>();
            if (drag != null) drag.ReturnToOrigin();
            else t.SetParent(null);
        }
    }

    private bool ContainsAll(List<string> list, params string[] targets)
    {
        foreach (string t in targets)
        {
            bool found = false;
            foreach (string n in list)
            {
                if (n.Contains(t)) { found = true; break; }
            }
            if (!found) return false;
        }
        return true;
    }
}
