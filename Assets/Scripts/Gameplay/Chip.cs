using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour, IChip
{
	private static Chip previousSelected = null;

	private Color initialColor;
	private Color selectedColor;
	private SpriteRenderer render;
	private bool isSelected = false;

	private ICell cell;

	public int Id { get; set; }
    public EComponents Type { get; set; }

    private void Awake()
    {
		render = GetComponent<SpriteRenderer>();
		initialColor = render.color;
		selectedColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0.5f);
	}

    private void Start()
    {
		cell = transform.parent.GetComponent<ICell>();
	}

    private void Select()
	{
		Debug.Log(cell.BoardIndex);
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Chip>();
	}

	private void Deselect()
	{
		isSelected = false;
		render.color = initialColor;
		previousSelected = null;
	}

    private void OnMouseDown()
	{
		if (isSelected)
		{ // Is it already selected?
			Deselect();
		}
		else
		{
			if (previousSelected == null)
			{ // Is it the first tile selected?
				Select();
			}
			else
			{
				
			}
		}
	}
}
