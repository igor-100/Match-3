using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour, IBoard
{
    private IResourceManager ResourceManager;

    private ICell selectedCell;

    private ICell[,] boardItems;
    private BoardProperties boardProperties;
    private ChipsProperties chipsProperties;

    public Transform Transform { get => transform; set => Transform = value; }

    private void Awake()
    {
        ResourceManager = CompositionRoot.GetResourceManager();

        boardProperties = CompositionRoot.GetConfiguration().GetBoardProperties();
        chipsProperties = CompositionRoot.GetConfiguration().GetChipsProperties();
    }

    void Start()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        transform.position = boardProperties.SpawnPoint;

        int xSize = boardProperties.XSize;
        int ySize = boardProperties.YSize;

        boardItems = new ICell[xSize, ySize];

        float startX = transform.position.x;
        float startY = transform.position.y;

        Vector2 offset = boardProperties.CellSize;

        List<ChipProperties> chips = chipsProperties.Chips;

        AssignBlockedCells(boardItems);

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                var currentCell = boardItems[x, y];

                if (currentCell == null)
                {
                    currentCell = CreateCell(startX, startY, offset, x, y);

                    var possibleChipsId = chips.Select(chip => chip.Id).ToList();
                    possibleChipsId.Remove(CheckDuplicationsToLeft(x, y));
                    possibleChipsId.Remove(CheckDuplicationsBelow(x, y));

                    int randomId = Random.Range(0, possibleChipsId.Count);
                    int chipId = possibleChipsId[randomId];

                    var chipType = chips[chipId].Type;

                    GameObject chipGO = ResourceManager.CreatePrefabInstance(chipType);

                    var chipComponent = chipGO.GetComponent<IChip>();
                    chipComponent.Id = chipId;
                    chipComponent.Type = chipType;

                    currentCell.SetChip(chipGO);
                    boardItems[x, y] = currentCell;
                }
                else if (currentCell != null && currentCell.IsBlocked)
                {
                    currentCell.Transform.position = new Vector2(startX + (offset.x * x), startY + (offset.y * y));
                }
            }
        }
    }

    private ICell CreateCell(float startX, float startY, Vector2 offset, int x, int y)
    {
        GameObject currentCellGO = ResourceManager.CreatePrefabInstance<EComponents>(EComponents.Cell);
        currentCellGO.transform.position = new Vector2(startX + (offset.x * x), startY + (offset.y * y));
        var currentCell = currentCellGO.GetComponent<ICell>();
        currentCell.BoardIndex = new BoardIndex(x, y);
        currentCell.Clicked += OnCellClicked;
        return currentCell;
    }

    private void OnCellClicked(ICell cell)
    {
        if (!cell.IsBlocked && cell.Chip != null)
        {
            if (selectedCell != null)
            {
                if (cell.IsSelected)
                {
                    cell.Deselect();
                    selectedCell = null;
                }
                else if (AreAdjacentCells(selectedCell, cell))
                {
                    selectedCell.Deselect();
                    SwapChips(selectedCell, cell);
                    selectedCell = null;

                    CheckBoard();
                }
                else
                {
                    selectedCell.Deselect();
                    selectedCell = cell;
                    cell.Select();
                }
            }
            else
            {
                selectedCell = cell;
                cell.Select();
            }
        }
    }

    private void CheckBoard()
    {
        VerticalRemoval();
        HorizontalRemoval();
        AddNewChips();
    }

    private void VerticalRemoval()
    {
        for (int x = 0; x < boardProperties.XSize; x++)
        {
            int verticalCount = 0;
            int previousChipId = -1;
            for (int y = 0; y < boardProperties.YSize; y++)
            {
                var cell = boardItems[x, y];
                if (cell.Chip != null && !cell.IsBlocked)
                {
                    if (cell.Chip.Id.Equals(previousChipId) && previousChipId != -1)
                    {
                        verticalCount++;
                        if (verticalCount > 1)
                        {
                            if (y + 1 >= boardProperties.YSize)
                            {
                                RemoveChipsVertically(x, y - verticalCount, y);
                                break;
                            }
                            else if (boardItems[x, y + 1].IsBlocked || boardItems[x, y + 1].Chip == null)
                            {
                                RemoveChipsVertically(x, y - verticalCount, y);
                                previousChipId = -1;
                                verticalCount = 0;
                            }
                            else if (boardItems[x, y + 1].Chip != null && !previousChipId.Equals(boardItems[x, y + 1].Chip.Id))
                            {
                                RemoveChipsVertically(x, y - verticalCount, y);
                            }
                        }
                    }
                    else
                    {
                        verticalCount = 0;
                        previousChipId = cell.Chip.Id;
                    }
                }
                else
                {
                    verticalCount = 0;
                    previousChipId = -1;
                }
            }
        }
    }

    private void HorizontalRemoval()
    {
        for (int y = 0; y < boardProperties.YSize; y++)
        {
            int horizontalCount = 0;
            int previousChipId = -1;
            for (int x = 0; x < boardProperties.XSize; x++)
            {
                var cell = boardItems[x, y];
                if (cell.Chip != null && !cell.IsBlocked)
                {
                    if (cell.Chip.Id.Equals(previousChipId) && previousChipId != -1)
                    {
                        horizontalCount++;
                        if (horizontalCount > 1)
                        {
                            if (x + 1 >= boardProperties.XSize)
                            {
                                RemoveChipsHorizontally(y, x - horizontalCount, x);
                                break;
                            }
                            else if (boardItems[x + 1, y].IsBlocked || boardItems[x + 1, y].Chip == null)
                            {
                                RemoveChipsHorizontally(y, x - horizontalCount, x);
                                previousChipId = -1;
                                horizontalCount = 0;
                            }
                            else if (boardItems[x + 1, y].Chip != null && !previousChipId.Equals(boardItems[x + 1, y].Chip.Id))
                            {
                                RemoveChipsHorizontally(y, x - horizontalCount, x);
                            }
                        }
                    }
                    else
                    {
                        horizontalCount = 0;
                        previousChipId = cell.Chip.Id;
                    }
                }
                else
                {
                    horizontalCount = 0;
                    previousChipId = -1;
                }
            }
        }
    }

    // start - inclusive, end - inclusive
    private void RemoveChipsVertically(int xRow, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            boardItems[xRow, i].DestroyChip();
        }
    }

    // start - inclusive, end - inclusive
    private void RemoveChipsHorizontally(int yColumn, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            boardItems[i, yColumn].DestroyChip();
        }
    }

    private void AddNewChips()
    {
        for (int x = 0; x < boardProperties.XSize; x++)
        {
            for (int y = boardProperties.YSize - 2; y >= 0; y--)
            {
                var cell = boardItems[x, y];
                if (cell.Chip != null)
                {
                    TryToMoveUp(cell, x, y);
                }
            }
        }
    }

    private void TryToMoveUp(ICell fromCell, int x, int y)
    {
        ICell topCell = null;
        for (int i = y; i < boardProperties.YSize; i++)
        {
            if (boardItems[x, i].IsBlocked)
            {
                break;
            }
            else if (boardItems[x, i].Chip == null)
            {
                topCell = boardItems[x, i];
            }
        }
        if (topCell != null)
        {
            Debug.Log("Move: " + fromCell.BoardIndex + " to " + topCell.BoardIndex);
            MoveChip(fromCell, topCell);
        }
    }

    private void GenerateNewChip(int x)
    {
        var chips = chipsProperties.Chips;
        int chipId = Random.Range(0, chips.Count);

        var chipType = chips[chipId].Type;

        GameObject chipGO = ResourceManager.CreatePrefabInstance(chipType);

        var chipComponent = chipGO.GetComponent<IChip>();
        chipComponent.Id = chipId;
        chipComponent.Type = chipType;

        boardItems[x, 0].SetChip(chipGO);
    }

    private void MoveChip(ICell fromCell, ICell toCell)
    {
        GameObject fromCellGO = fromCell.Chip.Transform.gameObject;
        toCell.SetChip(fromCellGO);
        fromCell.RemoveChip();
    }

    private void SwapChips(ICell firstCell, ICell secondCell)
    {
        GameObject secondChipGO = secondCell.Chip.Transform.gameObject;
        GameObject firstChipGO = firstCell.Chip.Transform.gameObject;
        firstCell.SetChip(secondChipGO);
        secondCell.SetChip(firstChipGO);
    }

    private bool AreAdjacentCells(ICell firstCell, ICell secondCell)
    {
        int xDifference = Mathf.Abs(firstCell.BoardIndex.X - secondCell.BoardIndex.X);
        int yDifference = Mathf.Abs(firstCell.BoardIndex.Y - secondCell.BoardIndex.Y);
        return (xDifference == 1 && yDifference == 0) || (yDifference == 1 && xDifference == 0);
    }
    //
    // Summary:
    //     Returns -1 if no duplications were found. Other values of chip ID if duplications were found.
    private int CheckDuplicationsToLeft(int x, int y)
    {
        if (x > 1)
        {
            var firstCell = boardItems[--x, y];
            var secondCell = boardItems[--x, y];

            if (!firstCell.IsBlocked && !secondCell.IsBlocked)
            {
                var firstCellChipId = firstCell.Chip.Id;
                var secondCellChipId = secondCell.Chip.Id;
                if (firstCellChipId == secondCellChipId)
                {
                    return firstCellChipId;
                }
            }
        }

        return -1;
    }

    //
    // Summary:
    //     Returns -1 if no duplications were found. Other values of chip ID if duplications were found.
    private int CheckDuplicationsBelow(int x, int y)
    {
        if (y > 1)
        {
            var firstCell = boardItems[x, --y];
            var secondCell = boardItems[x, --y];

            if (!firstCell.IsBlocked && !secondCell.IsBlocked)
            {
                var firstCellChipId = firstCell.Chip.Id;
                var secondCellChipId = secondCell.Chip.Id;
                if (firstCellChipId == secondCellChipId)
                {
                    return firstCellChipId;
                }
            }
        }

        return -1;
    }

    private void AssignBlockedCells(ICell[,] boardItems)
    {
        for (int i = 0; i < boardProperties.EmptyCellsNumber;)
        {
            int xValue = Random.Range(0, boardProperties.XSize);
            int yValue = Random.Range(0, boardProperties.YSize);
            if (boardItems[xValue, yValue] == null)
            {
                GameObject cellGO = ResourceManager.CreatePrefabInstance<EComponents>(EComponents.Cell);
                var cell = cellGO.GetComponent<ICell>();
                cell.IsBlocked = true;
                boardItems[xValue, yValue] = cell;
                i++;
            }
        }
    }

    private void Update()
    {

    }
}
