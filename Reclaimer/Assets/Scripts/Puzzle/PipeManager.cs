using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeManager : MonoBehaviour
{
    public static PipeManager Instance;

    [SerializeField] private GameObject _cellPrefab;
    [SerializeField] private Vector3 _backgroundOffset = new Vector3(0, 0, 5);
    [SerializeField] private RectTransform targetPart;  
    [SerializeField] private float removeDistance = 150f;
    [SerializeField] private float removeDuration = 0.8f;
    public Pipe externalPipePrefab;
    public int externalFixedType = 4;
    private Pipe externalPipe;
    private List<Pipe> externalPipes = new List<Pipe>();

    [SerializeField] private AudioClip placeSound;    
    [SerializeField] private AudioClip successSound;  
    [SerializeField] private AudioClip rotateSound;   
    private AudioSource audioSource;
    [SerializeField] private AudioClip combineSlotSound;   
    [SerializeField] private AudioClip combineSuccessSound;

    [SerializeField] private List<LevelData> levels;  
    private LevelData _level;                       
    private int currentLevelIndex = 0;
    private List<bool> levelCompleted;
    public int CurrentLevelIndex => currentLevelIndex;


    //public float cellSize = 1f;
    private bool hasGameFinished;
    private Pipe[,] pipes;
    private List<Pipe> startPipes;

    private bool[,] hasPipe;

    public PanelManager panelManager;

    public Animator cyberbodyAnimator;   
    public float introDuration = 2f;

    [SerializeField] private GameObject[] materialPrefabs; 
    private Transform resultSlot;    
    public Transform ResultSlot => resultSlot;
    [SerializeField] private GameObject combineSlotPrefab;
    [SerializeField] private GameObject combineSlotLabelPrefab;

    private void Awake()
    {
        Instance = this;
        hasGameFinished = false;
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (levels != null && levels.Count > 0)
        {
            currentLevelIndex = 0;
            _level = levels[currentLevelIndex];
        }
    }
    private void Start()
    {
        //levelCompleted = new List<bool>();
        //for (int i = 0; i < levels.Count; i++)
        //    levelCompleted.Add(false);
        levelCompleted = new List<bool>(GameManager.Instance.LevelCompleted);

        currentLevelIndex = 0;
        hasGameFinished = false;
        _level = levels[currentLevelIndex];

        //Debug.Log($"[PipeManager] Start(): 初始化 levelCompleted → [{string.Join(",", levelCompleted)}]");
    }


    private void SpawnLevel()
    {
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

                //// Only row=0, col=3 can drag
                //if (i == _level.Row - 1 && j == 3)
                //    tempPipe.IsDraggable = true;
                //else
                //    tempPipe.IsDraggable = false;

                if (currentLevelIndex == 0)
                {
                    // 第一关：最后一行，第4列
                    tempPipe.IsDraggable = (i == _level.Row - 1 && j == 3);
                }
                else if (currentLevelIndex == 1)
                {
                    // 第二关：第4行，第4列 和 第4行，第3列
                    bool first = (i == _level.Row - 4 && j == 2);
                    bool second = (i == _level.Row - 2 && j == 3); 
                    tempPipe.IsDraggable = first || second;
                }
                else
                {
                    tempPipe.IsDraggable = false;
                }

                if (tempPipe.PipeType == 1)
                {
                    startPipes.Add(tempPipe);
                }
            }
        }
        //SpawnExternalPipe();

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
        //CheckFill();
        //CheckWin();
        StartCoroutine(DelayCheck());
        SpawnMaterials();

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
        if (hasGameFinished) return;
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
        //Debug.Log($"[PipeManager] GameFinished() at Level {currentLevelIndex}");
        yield return new WaitForSeconds(0.5f);
        //Debug.Log("Repair complete. Entering verification phase.");

        if (successSound != null)
            audioSource.PlayOneShot(successSound);

        levelCompleted[currentLevelIndex] = true;
        GameManager.Instance.LevelCompleted[currentLevelIndex] = true;
        //Debug.Log($"[PipeManager] Level {currentLevelIndex} 完成 → [{string.Join(",", levelCompleted)}]");
        hasGameFinished = true;

        if (levels != null && currentLevelIndex + 1 < levels.Count)
        {
            ClearBoard();
            currentLevelIndex++;
            _level = levels[currentLevelIndex];

            if (currentLevelIndex < levelCompleted.Count)
                levelCompleted[currentLevelIndex] = false;

            //yield return new WaitForSeconds(0.3f);
            hasGameFinished = false;
            SpawnLevel();
        }
        else
        {
            bool level1Done = IsLevel1Completed();
            bool level2Done = IsLevel2Completed();

            if (level1Done && level2Done)
            {
                ClearBoard();
                EnterVerification();
                if (panelManager != null)
                    panelManager.UpdateSceneState();
            }
        }
    }

    public void SpawnExternalPipe(int index = 0)
    {
        float offsetX = _level.Column + 2f + index * 2f;
        float spawnY = _level.Row * 0.5f;
        //float spawnX = _level.Column * cellSize + 2f * cellSize;
        //float spawnY = _level.Row * cellSize * 0.5f;

        Vector2 spawnPos = new Vector2(offsetX, spawnY);

        Pipe pipe = Instantiate(externalPipePrefab, spawnPos, Quaternion.identity);
        int pipeType = (index == 0) ? 4 : 6; 
        pipe.Init(pipeType);
        pipe.IsDraggable = true;
        pipe.gameObject.tag = "ExternalPipe";
        externalPipes.Add(pipe);
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

    //  供 PipeDragHandler 等使用的新主逻辑
    public bool TryPlacePipeAt(Pipe dragged, int row, int col)
    {
        if (!IsInsideBoard(row, col))
            return false;

        if (hasPipe[row, col])
            return false;

        bool canPlace = false;
        int type = dragged.PipeType;
        int level = currentLevelIndex;

        if (dragged.CompareTag("ExternalPipe"))
        {
            if (level == 0)
            {
                if (type == 4)
                    canPlace = (row == _level.Row - 1 && col == 3);
                else
                    canPlace = false;
            }
            else if (level == 1)
            {
                if (type == 4)
                    canPlace = (row == _level.Row - 2 && col == 3);
                else if (type == 6)
                    canPlace = (row == _level.Row - 4 && col == 2);
            }
        }
        else
        {
            canPlace = IsEmptyAt(row, col);
        }

        if (!canPlace)
            return false;

        DoPlace(dragged, row, col);
        return true;
    }

    private void DoPlace(Pipe dragged, int row, int col)
    {
        dragged.transform.position = GetCellCenter(row, col);
        dragged.IsDraggable = true;
        pipes[row, col] = dragged;
        hasPipe[row, col] = true;

        if (isActiveAndEnabled)
            _ = StartCoroutine(ShowHintWrapper());
    }

    public void PlacePipeAt(Pipe dragged, int row, int col)
    {
        TryPlacePipeAt(dragged, row, col);
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

        foreach (var mat in GameObject.FindGameObjectsWithTag("Material"))
            Destroy(mat);

        foreach (var slot in GameObject.FindGameObjectsWithTag("CombineSlot"))
            Destroy(slot);

        foreach (var ext in GameObject.FindGameObjectsWithTag("ExternalPipe"))
            Destroy(ext);

        resultSlot = null;
        externalPipes.Clear();

    }
    public void OnScrewRemoved()
    {
        removedCount++;
        //Debug.Log($"Screws removed: {removedCount}/{totalScrews}");

  

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
        if (levels != null && levels.Count > 0)
        {
            _level = levels[currentLevelIndex];
        }
        GameManager.Instance.HasPlayedPuzzle = true;

        SpawnLevel();
    }

    private void ClearBoard()
    {
        if (pipes != null)
        {
            int rows = pipes.GetLength(0);
            int cols = pipes.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var p = pipes[i, j];
                    if (p != null)
                    {
                        var cellGO = p.transform.parent ? p.transform.parent.gameObject : p.gameObject;
                        Destroy(cellGO);
                        pipes[i, j] = null;
                    }
                }
            }
        }

        hasPipe = null;

        foreach (var p in FindObjectsByType<Pipe>(FindObjectsSortMode.None))
            if (p != null) Destroy(p.gameObject);

        foreach (var sr in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            if (sr.gameObject.name.Contains("Cell")) Destroy(sr.gameObject);

        if (externalPipe != null) { Destroy(externalPipe.gameObject); externalPipe = null; }

        //Debug.Log("Board cleared");
    }

    public void PlayPipePlaceSound()
    {
        if (placeSound != null)
            audioSource.PlayOneShot(placeSound);
    }

    public void PlayRotateSound()
    {
        if (rotateSound != null)
            audioSource.PlayOneShot(rotateSound);
    }

    public void PlayCombineSlotSound()
    {
        if (combineSlotSound != null)
            audioSource.PlayOneShot(combineSlotSound);
    }

    public void PlayCombineSuccessSound()
    {
        if (combineSuccessSound != null)
            audioSource.PlayOneShot(combineSuccessSound);
    }

    public bool IsLevel1Completed() => GameManager.Instance.LevelCompleted.Length > 0 && GameManager.Instance.LevelCompleted[0];
    public bool IsLevel2Completed() => GameManager.Instance.LevelCompleted.Length > 1 && GameManager.Instance.LevelCompleted[1];


    private IEnumerator DelayCheck()
    {
        yield return new WaitForSeconds(0.5f);  
        CheckFill();

        if (!AllCellsAlreadyFilled())
            CheckWin();
    }

    private bool AllCellsAlreadyFilled()
    {
        for (int i = 0; i < _level.Row; i++)
            for (int j = 0; j < _level.Column; j++)
            {
                var pipe = pipes[i, j];
                if (pipe == null || !pipe.IsFilled)
                    return false;
            }
        return true;
    }

    public void SpawnMaterials()
    {
        if (materialPrefabs == null || materialPrefabs.Length == 0 || _level == null)
            return;

        float boardWidth = _level.Column;
        float boardHeight = _level.Row;

        Vector3 boardCenter = new Vector3(boardWidth * 0.5f, boardHeight * 0.5f, 0f);

        float startX = boardCenter.x - boardWidth / 2f - 6f;  
        float startY = boardCenter.y + boardHeight / 2f + 2f;
        float xSpacing = 1.8f;

        foreach (var old in GameObject.FindGameObjectsWithTag("Material"))
            Destroy(old);

        for (int i = 0; i < materialPrefabs.Length; i++)
        {
            Vector3 spawnPos = new Vector3(startX + i * xSpacing, startY, 0);
            GameObject mat = Instantiate(materialPrefabs[i], spawnPos, Quaternion.identity);
            mat.tag = "Material";
            mat.transform.localScale = Vector3.one * 4f;   
        }

        float slotX = boardCenter.x - boardWidth / 2f - 5f;
        float slotY = boardCenter.y - boardHeight / 2f - 1.2f;

        if (combineSlotPrefab != null)
        {
            GameObject slot = Instantiate(combineSlotPrefab, new Vector3(slotX, slotY, 0), Quaternion.identity);
            resultSlot = slot.transform;

            if (slot.GetComponent<Collider2D>() == null)
            {
                var col = slot.AddComponent<BoxCollider2D>();
                col.isTrigger = false;
                col.size = new Vector2(1.2f, 1.2f);
            }

            slot.tag = "CombineSlot";
            if (combineSlotLabelPrefab != null)
            {
                GameObject label = Instantiate(combineSlotLabelPrefab, slot.transform);

                label.transform.localPosition = new Vector3(0, 1.5f, 0f);
                label.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
            }
        }
        else
        {
        }
    }

}
