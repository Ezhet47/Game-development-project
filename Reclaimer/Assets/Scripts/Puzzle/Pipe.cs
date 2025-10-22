using System.Collections.Generic;
using UnityEngine;

public class Pipe : MonoBehaviour
{
    [HideInInspector] public bool IsDraggable = false;

    [HideInInspector] public bool IsFilled;
    [HideInInspector] public int PipeType;

    [SerializeField] private Transform[] _pipePrefabs;

    private Transform currentPipe;
    private int rotation;

    private SpriteRenderer emptySprite;
    private SpriteRenderer filledSprite;
    private List<Transform> connectBoxes;

    private const int minRotation = 0;
    private const int maxRotation = 3;
    private const int rotationMultiplier = 90;

    public void Init(int pipe)
    {
        PipeType = pipe % 10;
        currentPipe = Instantiate(_pipePrefabs[PipeType], transform);
        currentPipe.transform.localPosition = Vector3.zero;
        if (PipeType == 1 || PipeType == 2)
        {
            rotation = pipe / 10;
        }
        else
        {
            rotation = Random.Range(minRotation, maxRotation + 1);
        }
        currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * rotationMultiplier);

        if (PipeType == 0 || PipeType == 1)
        {
            IsFilled = true;
        }

        if (PipeType == 0)
        {
            return;
        }
        if (PipeType == 7 || PipeType == 8)
        {
            emptySprite = currentPipe.GetChild(0).GetComponent<SpriteRenderer>();
            filledSprite = null;
            IsFilled = false;   
            return;
        }

        emptySprite = currentPipe.GetChild(0).GetComponent<SpriteRenderer>();
        emptySprite.gameObject.SetActive(!IsFilled);
        filledSprite = currentPipe.GetChild(1).GetComponent<SpriteRenderer>();
        filledSprite.gameObject.SetActive(IsFilled);

        connectBoxes = new List<Transform>();
        for (int i = 2; i < currentPipe.childCount; i++)
        {
            connectBoxes.Add(currentPipe.GetChild(i));
        }

        UpdateFilled();
    }

    public void UpdateInput()
    {
        if (PipeType == 0 || PipeType == 1 || PipeType == 2)
        {
            return;
        }

        rotation = (rotation + 1) % (maxRotation + 1);
        currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * rotationMultiplier);

        if (PipeManager.Instance != null)
            PipeManager.Instance.StartCoroutine(PipeManager.Instance.ShowHintWrapper());
    }

    public void UpdateFilled()
    {
        if (PipeType == 0 || PipeType == 7 || PipeType == 8) return;
        if (emptySprite == null || filledSprite == null) return;
        emptySprite.gameObject.SetActive(!IsFilled);
        filledSprite.gameObject.SetActive(IsFilled);
    }

    public bool[] GetOpenings()
    {
        bool[] openings = new bool[4];
        if (connectBoxes == null) return openings;

        foreach (var box in connectBoxes)
        {
            Vector2 d = (Vector2)(box.position - transform.position); 

            if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
            {
                if (d.x > 0.05f) openings[1] = true;   // Right
                else if (d.x < -0.05f) openings[3] = true; // Left
            }
            else
            {
                if (d.y > 0.05f) openings[0] = true;   // Top
                else if (d.y < -0.05f) openings[2] = true; // Bottom
            }
        }
        return openings;
    }

    public List<Pipe> ConnectedPipes()
    {
        List<Pipe> result = new List<Pipe>();
        PipeManager mgr = PipeManager.Instance;

        int row, col;
        mgr.WorldToCell(transform.position, out row, out col);

        bool[] myOpen = GetOpenings();

        // Top
        if (myOpen[0])
        {
            Pipe n = mgr.GetPipe(row + 1, col);
            if (n != null && n.GetOpenings()[2]) result.Add(n);
        }
        // Right
        if (myOpen[1])
        {
            Pipe n = mgr.GetPipe(row, col + 1);
            if (n != null && n.GetOpenings()[3]) result.Add(n);
        }
        // Bottom
        if (myOpen[2])
        {
            Pipe n = mgr.GetPipe(row - 1, col);
            if (n != null && n.GetOpenings()[0]) result.Add(n);
        }
        // Left
        if (myOpen[3])
        {
            Pipe n = mgr.GetPipe(row, col - 1);
            if (n != null && n.GetOpenings()[1]) result.Add(n);
        }

        return result;
    }


    public int GetRotationIndex() { return rotation; }

    public void SetRotationIndex(int rot)
    {
        rotation = ((rot % (maxRotation + 1)) + (maxRotation + 1)) % (maxRotation + 1);
        if (currentPipe != null)
            currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * rotationMultiplier);
    }

    public void RefreshInput()
    {
        if (currentPipe != null)
            currentPipe.transform.eulerAngles = new Vector3(0, 0, rotation * 90);
    }
}
