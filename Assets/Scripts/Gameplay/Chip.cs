using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour, IChip
{
	private Color initialColor;
	private Color selectedColor;
	private SpriteRenderer render;

	private bool isMoving;
	private Transform target;

	private ICell cell;

    public Transform Transform { get => transform; set => Transform = value; }
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

    public void Select()
    {
		render.color = selectedColor;
	}

	public void Deselect()
	{
		render.color = initialColor;
	}

	public void MoveToTarget(Transform target)
    {
		this.target = target;
		isMoving = true;
    }

    private void Update()
    {
        if (isMoving)
        {
			transform.position = Vector2.MoveTowards(transform.position, target.position, 5f * Time.deltaTime);
            if (transform.position.Equals(target.position))
            {
				isMoving = false;
            }
		}
    }
}
