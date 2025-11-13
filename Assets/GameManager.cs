using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private GameObject cellPrefab;
    private Camera mainCamera;
    private bool isDragging = false;
    private Vector2 lastMousePosition = new Vector2();
    [SerializeField] private float timeInterval = 0.5f;

    [SerializeField] private Slider speedSlider;
    [SerializeField] private Button startButton;
    [SerializeField] private TMP_Text speedLabel;
    private bool isSimulating = false;
    private Vector2Int? lastCellPos = null; // store last modified cell


    private Dictionary<Vector2Int, Cell> grid = new Dictionary<Vector2Int, Cell>();

    void Start()
    {
        mainCamera = Camera.main;
        GenerateGrid();
        CenterCameraOnGrid();
        startButton.onClick.AddListener(() => {
            StopAllCoroutines();
            StartSimulation();
        });
        speedSlider.onValueChanged.AddListener((value) => {
            SetSpeedInterval(value);
            speedLabel.text = "Speed: " + value.ToString("F2") + "s";
        });
        SetSpeedInterval(speedSlider.value);
        speedLabel.text = "Speed: " + speedSlider.value.ToString("F2") + "s";
    }

    private void SetSpeedInterval(float value)
    {
        timeInterval = speedSlider.value;
    }
    private void Update()
    {
        HandleInput();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RunSimulationStep();
        }
        HandleCamera();
        HandleZoom();
    }
    private void StartSimulation()
    {
        if(isSimulating)
        {
            isSimulating = false;
            StopAllCoroutines();
        }
        else
        {
            isSimulating = true;
            StartCoroutine(Simulate());
        }

    }
    private IEnumerator Simulate()
    {
        RunSimulationStep();
        yield return new WaitForSeconds(timeInterval);
        StartCoroutine(Simulate());
    }

    private void HandleCamera()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 delta = mainCamera.ScreenToWorldPoint(lastMousePosition) - mainCamera.ScreenToWorldPoint(currentMousePosition);
            mainCamera.transform.position += new Vector3(delta.x, delta.y, 0);
            lastMousePosition = currentMousePosition;
        }
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0.0f)
        {
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - scroll * 5f, 2f, 100f);
        }
    }
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int position = new Vector2Int(x, y);
                GameObject cellObj = Instantiate(cellPrefab, new Vector3(x, y, 0), Quaternion.identity);
                Cell cell = cellObj.GetComponent<Cell>();
                grid[position] = cell;
            }
        }
    }

    private void CenterCameraOnGrid()
    {
        float centerX = (width - 1) / 2f;
        float centerY = (height - 1) / 2f;
        mainCamera.transform.position = new Vector3(centerX, centerY, -10);
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(mousePos.x), Mathf.RoundToInt(mousePos.y));

            // Only act if we're inside the grid and over a new cell
            if (grid.TryGetValue(gridPos, out Cell cell))
            {
                if (lastCellPos == null || lastCellPos != gridPos)
                {
                    cell.ToggleState();
                    lastCellPos = gridPos;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Reset when mouse released
            lastCellPos = null;
        }
    }

    private void RunSimulationStep()
    {
        Dictionary<Vector2Int, bool> newStates = new Dictionary<Vector2Int, bool>();

        foreach (KeyValuePair<Vector2Int, Cell> cellPair in grid)
        {
            Vector2Int pos = cellPair.Key;
            Cell cell = cellPair.Value;

            int activeNeighbors = CountActiveNeighbors(pos);

            if (cell.isActive)
            {
                newStates[pos] = (activeNeighbors == 2 || activeNeighbors == 3);
            }
            else
            {
                newStates[pos] = (activeNeighbors == 3);
            }
        }

        foreach (KeyValuePair<Vector2Int, bool> statePair in newStates)
        {
            grid[statePair.Key].SetState(statePair.Value);
        }
    }

    int CountActiveNeighbors(Vector2Int pos)
    {
        int count = 0;
        int[,] directions = {
        {-1, -1}, {-1, 0}, {-1, 1},
        { 0, -1},         { 0, 1},
        { 1, -1}, { 1, 0}, { 1, 1}
    };

        // Loop through each direction
        for (int i = 0; i < directions.GetLength(0); i++)
        {
            Vector2Int neighborPos = new Vector2Int(pos.x + directions[i, 0], pos.y + directions[i, 1]);
            if (grid.TryGetValue(neighborPos, out Cell neighbor) && neighbor.isActive)
            {
                count++;
            }
        }

        return count;
    }

}
