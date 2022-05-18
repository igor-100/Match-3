using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour, IBoard
{
    private IResourceManager ResourceManager;

    private ICell selectedCell;

    private bool anyChipsMovedUp;
    private bool newChipsAdded;
    private bool anySequenceRemoved;

    private ICell[,] boardItems;
    private BoardProperties boardProperties;
    private ChipsProperties chipsProperties;

    public event Action StartedProcessingActions = () => { };
    public event Action StopedProcessingActions = () => { };
    public event Action<int> ChipsRemoved = (numberOfChips) => { };

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

                    int randomId = UnityEngine.Random.Range(0, possibleChipsId.Count);
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
                    StartCoroutine(TrySwappingChips(cell));
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

    private IEnumerator TrySwappingChips(ICell cell)
    {
        selectedCell.Deselect();

        yield return StartCoroutine(SwapChipsOverTime(cell, selectedCell));

        if (CheckSequencesAround(cell) || CheckSequencesAround(selectedCell))
        {
            StartCoroutine(CheckBoard());
        }
        else
        {
            yield return StartCoroutine(SwapChipsOverTime(cell, selectedCell));
        }

        selectedCell = null;
    }


    //To split on states
    private IEnumerator CheckBoard()
    {
        Debug.Log("Checking Board");
        StartedProcessingActions();
        RemoveSequences();
        if (anySequenceRemoved)
        {
            Debug.Log("Moving chips up");
            yield return StartCoroutine(MoveChipsUp());
            Debug.Log("Adding new chips");
            yield return AddNewChips();
            if (newChipsAdded)
            {
                Debug.Log("Checking Board again");
                yield return StartCoroutine(CheckBoard());
            }
            else
            {
                Debug.Log("No chips added");
            }
        }
        else
        {
            Debug.Log("No sequence removed");
        }
        StopedProcessingActions();
    }

    private void RemoveSequences()
    {
        anySequenceRemoved = false;
        VerticalRemoval();
        HorizontalRemoval();
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
            anySequenceRemoved = true;
        }
        ChipsRemoved(end - start + 1);
    }

    // start - inclusive, end - inclusive
    private void RemoveChipsHorizontally(int yColumn, int start, int end)
    {
        for (int i = start; i <= end; i++)
        {
            boardItems[i, yColumn].DestroyChip();
            anySequenceRemoved = true;
        }
        ChipsRemoved(end - start + 1);
    }

    private IEnumerator MoveChipsUp()
    {
        anyChipsMovedUp = false;
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
        if (anyChipsMovedUp)
        {
            yield return new WaitForSeconds(1.1f);
        }
        else
        {
            yield return null;
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
            MoveChip(fromCell, topCell);
            anyChipsMovedUp = true;
        }
    }

    private void MoveChip(ICell fromCell, ICell toCell)
    {
        StartCoroutine(MoveChipOverTime(fromCell, toCell));
    }

    private IEnumerator MoveChipOverTime(ICell fromCell, ICell toCell)
    {
        var chipGO = fromCell.Chip.Transform.gameObject;
        var chip = fromCell.Chip;
        chip.MoveToTarget(toCell.Transform);
        toCell.SetChipReference(chip);
        fromCell.RemoveChip();
        yield return new WaitForSeconds(1f);
        toCell.SetChip(chipGO);
    }

    private IEnumerator SwapChipsOverTime(ICell firstCell, ICell secondCell)
    {
        StartedProcessingActions();
        var firstChipGO = firstCell.Chip.Transform.gameObject;
        var firstChip = firstCell.Chip;
        var secondChipGO = secondCell.Chip.Transform.gameObject;
        var secondChip = secondCell.Chip;
        firstChip.MoveToTarget(secondCell.Transform);
        secondChip.MoveToTarget(firstCell.Transform);
        firstCell.SetChipReference(secondChip);
        secondCell.SetChipReference(firstChip);
        yield return new WaitForSeconds(0.5f);
        firstCell.SetChip(secondChipGO);
        secondCell.SetChip(firstChipGO);
        StopedProcessingActions();
    }

    private IEnumerator AddNewChips()
    {
        newChipsAdded = false;
        for (int x = 0; x < boardProperties.XSize; x++)
        {
            int nullChipsCounter = 0;

            int currentBlockedCellYValue = -1;
            int currentNullChipsCounterAfterBlocked = 0;
            var blockedCellsWithNullsAfter = new Dictionary<int, int>();
            for (int y = 0; y < boardProperties.YSize; y++)
            {
                if (blockedCellsWithNullsAfter.Count == 0 && boardItems[x, y].Chip == null && !boardItems[x, y].IsBlocked)
                {
                    nullChipsCounter++;
                }
                else if (boardItems[x, y].IsBlocked)
                {
                    currentBlockedCellYValue = y;
                    currentNullChipsCounterAfterBlocked = 0;
                    blockedCellsWithNullsAfter.Add(currentBlockedCellYValue, currentNullChipsCounterAfterBlocked);
                }
                else if (blockedCellsWithNullsAfter.Count > 0 && boardItems[x, y].Chip == null && !boardItems[x, y].IsBlocked)
                {
                    currentNullChipsCounterAfterBlocked++;
                    blockedCellsWithNullsAfter[currentBlockedCellYValue] = currentNullChipsCounterAfterBlocked;
                }
            }

            if (nullChipsCounter > 0)
            {
                for (int i = nullChipsCounter - 1; i >= 0; i--)
                {
                    GenerateNewChip(x, 0);
                    if (i != 0)
                    {
                        MoveChip(boardItems[x, 0], boardItems[x, i]);
                    }
                    newChipsAdded = true;
                }
            }
            if (blockedCellsWithNullsAfter.Count > 0)
            {
                foreach (var entry in blockedCellsWithNullsAfter)
                {
                    int blockedCellY = entry.Key;
                    int nullChips = entry.Value;
                    for (int i = blockedCellY + nullChips; i > blockedCellY; i--)
                    {
                        GenerateNewChip(x, blockedCellY + 1);
                        if (i != blockedCellY + 1)
                        {
                            MoveChip(boardItems[x, blockedCellY + 1], boardItems[x, i]);
                        }
                        newChipsAdded = true;
                    }
                }
            }
        }
        if (newChipsAdded)
        {
            yield return new WaitForSeconds(1.1f);
        }
        else
        {
            yield return null;
        }
    }

    private void GenerateNewChip(int x, int y)
    {
        var chips = chipsProperties.Chips;
        int chipId = UnityEngine.Random.Range(0, chips.Count);

        var chipType = chips[chipId].Type;

        GameObject chipGO = ResourceManager.CreatePrefabInstance(chipType);

        var chipComponent = chipGO.GetComponent<IChip>();
        chipComponent.Id = chipId;
        chipComponent.Type = chipType;

        boardItems[x, y].SetChip(chipGO);
    }

    private IEnumerator SwapChips(ICell firstCell, ICell secondCell)
    {
        StartCoroutine(MoveChipOverTime(firstCell, secondCell));
        yield return new WaitForSeconds(1.1f);
        StartCoroutine(MoveChipOverTime(secondCell, firstCell));
        yield return new WaitForSeconds(1f);
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

    private bool CheckSequencesAround(ICell cell)
    {
        int x = cell.BoardIndex.X;
        int y = cell.BoardIndex.Y;
        var chipId = boardItems[x, y].Chip.Id;
        int horizontalNumber = 0;
        int verticalNumber = 0;

        if (y > 0)
        {
            var down1 = boardItems[x, y - 1];
            if (down1.Chip != null && !down1.IsBlocked && chipId.Equals(down1?.Chip.Id))
            {
                verticalNumber++;
                if (y > 1)
                {
                    var down2 = boardItems[x, y - 2];
                    if (down2.Chip != null && !down2.IsBlocked && chipId.Equals(down2?.Chip.Id))
                    {
                        verticalNumber++;
                    }
                }
            }
        }

        if (y < boardProperties.YSize - 1)
        {
            var up1 = boardItems[x, y + 1];
            if (up1.Chip != null && !up1.IsBlocked && chipId.Equals(up1?.Chip.Id))
            {
                verticalNumber++;
                if (y < boardProperties.YSize - 2)
                {
                    var up2 = boardItems[x, y + 2];
                    if (up2.Chip != null && !up2.IsBlocked && chipId.Equals(up2?.Chip.Id))
                    {
                        verticalNumber++;
                    }
                }
            }
        }

        if (x > 0)
        {
            var left1 = boardItems[x - 1, y];
            if (left1.Chip != null && !left1.IsBlocked && chipId.Equals(left1?.Chip.Id))
            {
                horizontalNumber++;
                if (x > 1)
                {
                    var left2 = boardItems[x - 2, y];
                    if (left2.Chip != null && !left2.IsBlocked && chipId.Equals(left2?.Chip.Id))
                    {
                        horizontalNumber++;
                    }
                }
            }
        }

        if (x < boardProperties.XSize - 1)
        {
            var right1 = boardItems[x + 1, y];
            if (right1.Chip != null && !right1.IsBlocked && chipId.Equals(right1?.Chip.Id))
            {
                horizontalNumber++;
                if (x < boardProperties.XSize - 2)
                {
                    var right2 = boardItems[x + 2, y];
                    if (right2.Chip != null && !right2.IsBlocked && chipId.Equals(right2?.Chip.Id))
                    {
                        horizontalNumber++;
                    }
                }
            }
        }

        return verticalNumber > 1 || horizontalNumber > 1;
    }

    private void AssignBlockedCells(ICell[,] boardItems)
    {
        for (int i = 0; i < boardProperties.EmptyCellsNumber;)
        {
            int xValue = UnityEngine.Random.Range(0, boardProperties.XSize);
            int yValue = UnityEngine.Random.Range(0, boardProperties.YSize);
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
