using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScannerTool : MonoBehaviour
{
    public static ScannerTool Instance;

    public RectTransform scannerIcon;   
    public GameObject hintPrefab;       
    public GameObject[] scanPoints;     
    public int brokenPartIndex = 3;     

    public float detectionRadius = 40f;
    public KeyCode actionKey = KeyCode.F;   

    private bool detected = false;     

    private Vector2 originalPos;

    private void Awake()
    {
        Instance = this;
        originalPos = scannerIcon.anchoredPosition;
        if (PipeManager.Instance != null &&
    PipeManager.Instance.CurrentStage == PipeManager.GameStage.Verification)
        {
            SetModeVerification();
        }

    }



    public void TryScanAtCursor()
    {
        if (detected) return;

        for (int i = 0; i < scanPoints.Length; i++)
        {
            RectTransform rt = scanPoints[i].GetComponent<RectTransform>();
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, rt.position);
            float dist = Vector2.Distance(screenPos, Input.mousePosition);

            if (dist < detectionRadius)
            {
                if (mode == ScanMode.Diagnosis && i == brokenPartIndex)
                {
                    Debug.Log("Abnormal part detected!");
                    detected = true;
                    //StartCoroutine(ShowError(scanPoints[i]));
                    ShowError(scanPoints[i]);


                    break;
                }

                if (mode == ScanMode.Verification)
                {
                    Debug.Log("Repair successful!");
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
        GameObject hint = Instantiate(hintPrefab, point.transform.parent);
        hint.GetComponentInChildren<TextMeshProUGUI>().text = "Error";
        hint.transform.position = point.transform.position;

        Image img = point.GetComponent<Image>();
        if (img != null)
        {
            var outline = point.GetComponent<Outline>();
            if (outline == null)
                outline = point.AddComponent<Outline>();

            outline.effectColor = Color.red;
            outline.effectDistance = new Vector2(4, 4);
        }
    }

    private IEnumerator ShowSuccess(GameObject point)
    {
        GameObject hint = Instantiate(hintPrefab, point.transform.parent);
        hint.GetComponentInChildren<TextMeshProUGUI>().text = "Success";
        hint.transform.position = point.transform.position;

        Image img = point.GetComponent<Image>();
        if (img == null) yield break;

        Color baseColor = img.color;
        Color flashColor = Color.green;

        for (int i = 0; i < 6; i++)
        {
            img.color = (i % 2 == 0) ? flashColor : baseColor;
            yield return new WaitForSeconds(0.2f);
        }
        img.color = baseColor;
    }

    public enum ScanMode { Diagnosis, Verification }
    public ScanMode mode = ScanMode.Diagnosis;

    public void SetModeVerification()
    {
        mode = ScanMode.Verification;
        Debug.Log("Switch the scanner to verification mode");
    }

    public void SetModeDiagnosis()
    {
        mode = ScanMode.Diagnosis;
        Debug.Log("Switch the scanner to diagnosis mode");
    }
    public void ResetScanner()
    {
        detected = false;
        scannerIcon.anchoredPosition = originalPos;
    }



}
