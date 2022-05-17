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
        if (!cell.IsBlocked)
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
