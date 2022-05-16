using System.Collections.Generic;
using UnityEngine;

public class Configuration : IConfiguration
{
    private readonly ChipsProperties chipsProperties;
    private readonly BoardProperties boardProperties;

    public Configuration()
    {
        boardProperties = new BoardProperties
        {
            SpawnPoint = new Vector2(-3f, -3f),
            CellSize = new Vector2(1f, 1f),
            XSize = 6,
            YSize = 6,
            EmptyCellsNumber = 3
        };

        chipsProperties = new ChipsProperties
        {
            Chips = new List<ChipProperties>
            {
                new ChipProperties
                {
                    Id = 0,
                    Type = EComponents.Red_Chip
                },
                new ChipProperties
                {
                    Id = 1,
                    Type = EComponents.Blue_Chip
                },
                new ChipProperties
                {
                    Id = 2,
                    Type = EComponents.Green_Chip
                },
                new ChipProperties
                {
                    Id = 3,
                    Type = EComponents.Yellow_Chip
                },
            }
        };
    }

    public ChipsProperties GetChipsProperties() => chipsProperties;
    public BoardProperties GetBoardProperties() => boardProperties;
}
