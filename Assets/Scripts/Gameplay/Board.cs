using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour, IBoard
{
    private IResourceManager ResourceManager;

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
        CreateBoard(boardProperties, chipsProperties);
    }

    private void CreateBoard(BoardProperties boardProperties, ChipsProperties chipsProperties)
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
                    GameObject currentCellGO = ResourceManager.CreatePrefabInstance<EComponents>(EComponents.Cell);
                    currentCellGO.transform.position = new Vector2(startX + (offset.x * x), startY + (offset.y * y));
                    currentCell = currentCellGO.GetComponent<ICell>();

                    int chipId = Random.Range(0, chips.Capacity);

                    if (IsChipTheThird(x, y, chipId))
                    {
                        chipId = RandomExcept(0, chips.Capacity, chipId);
                    }

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

    private bool IsChipTheThird(int x, int y, int chipId)
    {
        if (x > 1)
        {
            for (int tempX = x - 1; tempX >= x - 2; tempX--)
            {
                var tempCell = boardItems[tempX, y];
                if (!tempCell.IsBlocked)
                {
                    if (tempCell.Chip.Id != chipId)
                    {
                        break;
                    }
                    else
                    {
                        if (tempX == x - 2)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        if (y > 1)
        {
            for (int tempY = y - 1; tempY >= y - 2; tempY--)
            {
                var tempCell = boardItems[x, tempY];
                if (!tempCell.IsBlocked)
                {
                    if (tempCell.Chip.Id != chipId)
                    {
                        break;
                    }
                    else
                    {
                        if (tempY == y - 2)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }

        return false;
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

    private int RandomExcept(int min, int max, int except)
    {
        int random = Random.Range(min, max);
        if (random >= except) random = (random + 1) % max;
        return random;
    }
}
