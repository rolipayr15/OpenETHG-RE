using System;
using UnityEngine;

[Serializable]
public class LevelCellData
{
	public Sprite sprite;

	public string elemName;

	public string elemTranslationKey;

	public ContentType contentType;

	public Level level;

	public Enemy enemy;

	public Mode mode;
}
