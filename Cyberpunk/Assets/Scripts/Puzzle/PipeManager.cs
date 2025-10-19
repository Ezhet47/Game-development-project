using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public static PipeManager Instance;

    [SerializeField] private LevelData _level;
    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private GameObject _backgroundPrefab;
    [SerializeField] private Vector3 _backgroundOffset = new Vector3(0, 0, 5);
    [SerializeField] private RectTransform targetPart;  
    [SerializeField] private float removeDistance = 150f;
    [SerializeField] private float removeDuration = 0.8f;



    //public float cellSize = 1f;
    private bool hasGameFinished;
    private Pipe[,] pipes;
    private List<Pipe> startPipes;

    private bool[,] hasPipe;

    public PanelManager panelManager;

    public Animator cyberbodyAnimator;   
    public float introDuration = 1f;     

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;

    }

    private void SpawnLevel()
    {
        // Spawn background
        if (_backgroundPrefab != null)
        {
            GameObject bg = Instantiate(_backgroundPrefab);
            bg.transform.position = new Vector3(_level.Column * 0.5f, _level.Row * 0.5f, _backgroundOffset.z);
        }


        pipes = new Pipe[_level.Row, _level.Column];
        startPipes = new List<Pipe>();
        hasPipe = new bool[_level.Row, _level.Column];

        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Column; j++)
            {
                Vector2 spawnPos = new Vector2(j + 0.5f, i + 0.5f);
                GameObject cellGO = Instantiate(_cellPrefab, spawnPos, Quaternion.identity);
                //cellGO.transform.localScale = Vector3.one * cellSize;

                Pipe tempPipe = cellGO.GetComponentInChildren<Pipe>();
                tempPipe.Init(_level.Data[i * _level.Column + j]);

                pipes[i, j] = tempPipe;
                hasPipe[i, j] = true;

                // Only row=0, col=3 can drag
                if (i == _level.Row - 1 && j == 3)
                    tempPipe.IsDraggable = true;
                else
                    tempPipe.IsDraggable = false;

                if (tempPipe.PipeType == 1)
                {
                    startPipes.Add(tempPipe);
                }
            }
        }
        SpawnExternalPipe();

        Vector3 cameraPos = Camera.main.transform.position;

        //cameraPos.x = _level.Column * cellSize * 0.5f;
        //cameraPos.y = _level.Row * cellSize * 0.5f;
        //Camera.main.orthographicSize = Mathf.Max(_level.Row, _level.Column) * cellSize * 0.5f + 3f * cellSize;
        cameraPos.x = _level.Column * 0.5f;
        cameraPos.y = _level.Row * 0.5f;
        Camera.main.orthographicSize = Mathf.Max(_level.Row, _level.Column) * 0.5f + 3f;

        Camera.main.transform.position = cameraPos;


        StartCoroutine(ShowHint());

        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Column; j++)
            {
                Pipe tempPipe = pipes[i, j];
                if (tempPipe != null) tempPipe.RefreshInput();
            }
        }

        CheckFill();
        CheckWin();
    }

    private void Update()
    {
        if (hasGameFinished) return;

    }

    public IEnumerator ShowHintWrapper()
    {
        yield return StartCoroutine(ShowHint());
    }

    private IEnumerator ShowHint()
    {
        yield return new WaitForSeconds(0.1f);
        CheckFill();
        CheckWin();
    }

    private void CheckFill()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Column; j++)
            {
                Pipe tempPipe = pipes[i, j];
                if (tempPipe == null) continue;
                if (tempPipe.PipeType != 0)
                {
                    tempPipe.IsFilled = false;
                }
            }
        }

        Queue<Pipe> check = new Queue<Pipe>();
        HashSet<Pipe> finished = new HashSet<Pipe>();
        foreach (var pipe in startPipes)
        {
            check.Enqueue(pipe);
        }

        while (check.Count > 0)
        {
            Pipe pipe = check.Dequeue();
            if (pipe == null || finished.Contains(pipe)) continue;
            finished.Add(pipe);

            List<Pipe> connected = pipe.ConnectedPipes();
            foreach (var connectedPipe in connected)
            {
                if (connectedPipe != null && !finished.Contains(connectedPipe))
                    check.Enqueue(connectedPipe);
            }
        }

        foreach (var filled in finished)
        {
            if (filled != null) filled.IsFilled = true;

        }

        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Column; j++)
            {
                Pipe tempPipe = pipes[i, j];
                if (tempPipe != null) tempPipe.UpdateFilled();
            }
        }

    }

    private void CheckWin()
    {
        for (int i = 0; i < _level.Row; i++)
        {
            for (int j = 0; j < _level.Column; j++)
            {
                Pipe tempPipe = pipes[i, j];
                if (tempPipe == null || !tempPipe.IsFilled)
                    return;
            }
        }

        hasGameFinished = true;
        StartCoroutine(GameFinished());
    }

    private IEnumerator GameFinished()
    {
        yield return new WaitForSeconds(1.5f);
        //Debug.Log("Repair complete. Entering verification phase.");
        GameManager.Instance.PuzzleCompleted = true;

        ClearBoard();

        EnterVerification();

        // ÏÔÊ¾ panel_test
        if (panelManager != null)
        {
            panelManager.ShowPanelTest();
        }

    }

    public Pipe externalPipePrefab;  
    public int externalFixedType = 4;
    private Pipe externalPipe;

    public void SpawnExternalPipe()
    {
        float spawnX = _level.Column + 2f;
        float spawnY = _level.Row * 0.5f;
        //float spawnX = _level.Column * cellSize + 2f * cellSize;
        //float spawnY = _level.Row * cellSize * 0.5f;


        Vector2 spawnPos = new Vector2(spawnX, spawnY);

        externalPipe = Instantiate(externalPipePrefab, spawnPos, Quaternion.identity);

        int pipeType = Mathf.Clamp(externalFixedType, 0, 6); 
        externalPipe.Init(pipeType);
        //externalPipe.transform.localScale = Vector3.one * cellSize;

        externalPipe.IsDraggable = true;
    }

    public bool IsInsideBoard(int row, int col)
    {
        return row >= 0 && row < _level.Row && col >= 0 && col < _level.Column;
    }

    public bool IsEmptyAt(int row, int col)
    {
        if (!IsInsideBoard(row, col))
            return false;
        return !hasPipe[row, col];
    }

    public void PlacePipeAt(Pipe dragged, int row, int col)
    {
        if (!IsInsideBoard(row, col))
            return;

        dragged.transform.position = GetCellCenter(row, col);

        //dragged.transform.localScale = Vector3.one * cellSize;

        dragged.IsDraggable = true;

        pipes[row, col] = dragged;
        hasPipe[row, col] = true;

        if (isActiveAndEnabled)
            _ = StartCoroutine(ShowHintWrapper());
    }


    public void ClearPipeAt(int row, int col)
    {
        pipes[row, col] = null;
        hasPipe[row, col] = false;
    }

    public void WorldToCell(Vector3 worldPos, out int row, out int col)
    {
        row = Mathf.RoundToInt(worldPos.y - 0.5f);
        col = Mathf.RoundToInt(worldPos.x - 0.5f);
        //row = Mathf.RoundToInt((worldPos.y - cellSize / 2f) / cellSize);
        //col = Mathf.RoundToInt((worldPos.x - cellSize / 2f) / cellSize);

    }

    public Vector2 GetCellCenter(int row, int col)
    {
        return new Vector2(col + 0.5f, row + 0.5f);
        //return new Vector2(col * cellSize + cellSize / 2f,
        // row * cellSize + cellSize / 2f);

    }

    public Vector2 externalDropPos = new Vector2(12, -2);

    public Pipe GetPipe(int row, int col)
    {
        if (IsInsideBoard(row, col))
            return pipes[row, col];
        return null;
    }

    public LevelData Level => _level;

    public int totalScrews = 5;
    private int removedCount = 0;

    public enum GameStage
    {
        Diagnosis,   
        Repair,      
        Verification 
    }

    public GameStage CurrentStage = GameStage.Diagnosis;

    public void EnterVerification()
    {
        CurrentStage = GameStage.Verification;

    }
    public void OnScrewRemoved()
    {
        removedCount++;
        Debug.Log($"Screws removed: {removedCount}/{totalScrews}");

  

        if (removedCount >= totalScrews)
        {
            //Debug.Log("All screws removed. Accessing internal structure...");
            //CurrentStage = GameStage.Repair;

            //if (panelManager != null)
            //{
            //    panelManager.ShowBG();
            //}
            //SpawnLevel();

            StartCoroutine(PlayInternalAnimationThenPuzzle());
        }
    }

    private IEnumerator PlayInternalAnimationThenPuzzle()
    {
        if (targetPart != null)
        {
            Vector2 startPos = targetPart.anchoredPosition;
            Vector2 endPos = startPos + new Vector2(0f, -removeDistance);
            var img = targetPart.GetComponent<UnityEngine.UI.Image>();
            float t = 0f;
            Color c0 = img ? img.color : Color.white;

            while (t < 1f)
            {
                t += Time.deltaTime / removeDuration;
                targetPart.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                if (img)
                {
                    var c = c0; c.a = Mathf.Lerp(c0.a, 0f, t);
                    img.color = c;
                }
                yield return null;
            }

            targetPart.gameObject.SetActive(false);
        }

        if (cyberbodyAnimator != null)
        {
            cyberbodyAnimator.Rebind();
            cyberbodyAnimator.Update(0f);
            cyberbodyAnimator.Play("roboticArm_0", 0, 0f);
        }

        yield return new WaitForSeconds(introDuration);

        CurrentStage = GameStage.Repair;
        panelManager?.ShowBG();
        SpawnLevel();
    }

    

    private void ClearBoard()
    {
        if (pipes != null)
        {
            for (int i = 0; i < _level.Row; i++)
            {
                for (int j = 0; j < _level.Column; j++)
                {
                    var p = pipes[i, j];
                    if (p != null)
                    {
                        var cellGO = p.transform.parent ? p.transform.parent.gameObject : p.gameObject;
                        Destroy(cellGO);
                        pipes[i, j] = null;
                        hasPipe[i, j] = false;
                    }
                }
            }
        }

        Pipe[] loosePipes = FindObjectsByType<Pipe>(FindObjectsSortMode.None);
        foreach (var p in loosePipes)
        {
            if (p != null)
            {
                Destroy(p.gameObject);
            }
        }

        var cells = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var c in cells)
        {
            if (c.gameObject.name.Contains("Cell"))
                Destroy(c.gameObject);
        }

        if (externalPipe != null)
        {
            Destroy(externalPipe.gameObject);
            externalPipe = null;
        }

        //Debug.Log("Board cleared");
    }


}
