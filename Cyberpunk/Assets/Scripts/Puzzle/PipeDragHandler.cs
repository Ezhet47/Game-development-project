using Unity.VisualScripting;
using UnityEngine;

public class PipeDragHandler : MonoBehaviour
{
    private Pipe pipe;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 startPosition;
    private bool startedFromBoard = false;
    private bool clearedFromBoard = false;

    private Vector3 externalStartPos;

    private void Awake()
    {
        pipe = GetComponent<Pipe>();
    }

    private void Update()
    {
        if (!pipe.IsDraggable)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    pipe.UpdateInput();
                    if (PipeManager.Instance != null)
                        PipeManager.Instance.PlayRotateSound();
                    if (PipeManager.Instance != null && PipeManager.Instance.isActiveAndEnabled)
                        _ = PipeManager.Instance.StartCoroutine(PipeManager.Instance.ShowHintWrapper());
                }
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                pipe.UpdateInput();
                if (PipeManager.Instance != null)
                    PipeManager.Instance.PlayRotateSound();
                if (PipeManager.Instance != null && PipeManager.Instance.isActiveAndEnabled)
                    _ = PipeManager.Instance.StartCoroutine(PipeManager.Instance.ShowHintWrapper());
            }
        }


        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mouseWorld.x, mouseWorld.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                startPosition = transform.position;
                isDragging = true;
                offset = transform.position - mouseWorld;

                if (gameObject.CompareTag("ExternalPipe"))
                    externalStartPos = transform.position;

                PipeManager.Instance.WorldToCell(startPosition, out int row, out int col);
                startedFromBoard = PipeManager.Instance.IsInsideBoard(row, col);
                clearedFromBoard = false;


                if (startedFromBoard && !clearedFromBoard)
                {
                    PipeManager.Instance.ClearPipeAt(row, col);
                    clearedFromBoard = true;

                    pipe.IsFilled = false;
                    pipe.UpdateFilled();
                }

            }
        }


        if (isDragging && Input.GetMouseButton(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = mouseWorld + offset;

            //PipeManager.Instance.WorldToCell(mouseWorld, out int row, out int col);
            //if (PipeManager.Instance.IsInsideBoard(row, col))
            //{
            //    if (PipeManager.Instance != null && PipeManager.Instance.isActiveAndEnabled)
            //        _ = PipeManager.Instance.StartCoroutine(PipeManager.Instance.ShowHintWrapper());
            //}
        }


        if (isDragging && Input.GetMouseButtonUp(1))
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PipeManager.Instance.WorldToCell(mouseWorld, out int row, out int col);

            bool isExternal = gameObject.CompareTag("ExternalPipe");
            bool validCell = PipeManager.Instance.IsInsideBoard(row, col) &&
                             PipeManager.Instance.IsEmptyAt(row, col) &&
                             row == PipeManager.Instance.Level.Row - 1 &&
                             col == 3;

            if (isExternal)
            {
                if (validCell)
                {
                    PipeManager.Instance.PlacePipeAt(pipe, row, col);
                    PipeManager.Instance.PlayPipePlaceSound();
                    pipe.IsDraggable = false;
                    if (PipeManager.Instance != null && PipeManager.Instance.isActiveAndEnabled)
                        _ = PipeManager.Instance.StartCoroutine(PipeManager.Instance.ShowHintWrapper());
                }
                else
                {
                    transform.position = externalStartPos;
                }
            }
            else
            {
                if (PipeManager.Instance.IsInsideBoard(row, col) &&
                    PipeManager.Instance.IsEmptyAt(row, col))
                {
                    PipeManager.Instance.PlacePipeAt(pipe, row, col);
                    PipeManager.Instance.PlayPipePlaceSound();
                }
                else
                {
                }
            }

            isDragging = false;
        }
    }
}