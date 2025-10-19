using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScannerTool : MonoBehaviour
{
    public static ScannerTool Instance;

    public RectTransform scannerIcon;
    public Image scannerImage;               
    public Sprite normalSprite;               // Puzzle_scene_scanner_2
    public Sprite successSprite;              // Puzzle_scene_scanner_0
    public Sprite errorSprite;                // Puzzle_scene_scanner_1

    public GameObject hintPrefab;       
    public GameObject[] scanPoints;     
    public int brokenPartIndex = 3;     

    public float detectionRadius = 100f;
    public KeyCode actionKey = KeyCode.F;   

    private bool detected = false;     

    private Vector2 originalPos;

    private void Awake()
    {
        Instance = this;
        originalPos = scannerIcon.anchoredPosition;

        if (scannerImage != null && normalSprite != null)
            scannerImage.sprite = normalSprite;

        if (PipeManager.Instance != null &&
            PipeManager.Instance.CurrentStage == PipeManager.GameStage.Verification)
        {
            SetModeVerification();
        }
    }



    public void TryScanAtCursor()
    {
        //if (detected) return;

        float offset = 100f; 
        float angleRad = 30f * Mathf.Deg2Rad;
        Vector2 tipOffset = new Vector2(-Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * offset;
        Vector2 scanTip = scannerIcon.position + (Vector3)tipOffset;

        if (PipeManager.Instance != null && PipeManager.Instance.CurrentStage == PipeManager.GameStage.Verification)
        {
            GameObject cyberbody = GameObject.Find("cyberbody");
            if (cyberbody != null)
            {
                detected = true;
                StartCoroutine(ShowSuccess(cyberbody));
                return;
            }
        }

        for (int i = 0; i < scanPoints.Length; i++)
        {
            RectTransform rt = scanPoints[i].GetComponent<RectTransform>();
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
            //float dist = Vector2.Distance(screenPos, Input.mousePosition);
            float dist = Vector2.Distance(screenPos, scanTip);

            if (dist < detectionRadius)
            {
                if (mode == ScanMode.Diagnosis && i == brokenPartIndex)
                {
                    //Debug.Log("Abnormal part detected!");
                    detected = true;
                    //StartCoroutine(ShowError(scanPoints[i]));
                    ShowError(scanPoints[i]);


                    break;
                }

                if (mode == ScanMode.Verification)
                {
                    //Debug.Log("Repair successful!");
                    detected = true;
                    StartCoroutine(ShowSuccess(scanPoints[i]));
                    break;
                }
            }
        }
    }

    private void Update()
    {
   
        if (ToolManager.Instance.currentTool == ToolManager.ToolType.Scanner && Input.GetMouseButton(1))
        {
            TryScanAtCursor();  
        }
    }



    private void ShowError(GameObject point)
    {
        //GameObject hint = Instantiate(hintPrefab, point.transform.parent);
        //hint.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
        //hint.transform.position = point.transform.position;

        Image img = point.GetComponent<Image>();
        if (img != null)
        {
            var outline = point.GetComponent<Outline>();
            if (outline == null)
                outline = point.AddComponent<Outline>();

            outline.effectColor = Color.red;
            outline.effectDistance = new Vector2(6, 6);
        }

        if (scannerImage != null && errorSprite != null)
            scannerImage.sprite = errorSprite;

    }

    private IEnumerator ShowSuccess(GameObject point)
    {
        Image img = point.GetComponent<Image>();
        if (img != null)
        {
            var outline = point.GetComponent<Outline>();
            if (outline == null)
                outline = point.AddComponent<Outline>();

            outline.effectColor = Color.green;
            outline.effectDistance = new Vector2(8, 8);
        }

        if (scannerImage != null && successSprite != null)
            scannerImage.sprite = successSprite;

        yield return null;
    }

    public enum ScanMode { Diagnosis, Verification }
    public ScanMode mode = ScanMode.Diagnosis;

    public void SetModeVerification()
    {
        mode = ScanMode.Verification;
        //Debug.Log("Switch the scanner to verification mode");
    }

    public void SetModeDiagnosis()
    {
        mode = ScanMode.Diagnosis;
        //Debug.Log("Switch the scanner to diagnosis mode");
    }
    public void ResetScanner()
    {
        detected = false;
        scannerIcon.anchoredPosition = originalPos;
        if (scannerImage != null && normalSprite != null)
            scannerImage.sprite = normalSprite;
    }

}
