using System.Collections.Generic;
using UnityEngine;

namespace NinthArt
{
	internal static class LevelManager
	{
		private static readonly List<string> List = new();

		internal static List<string> Levels
		{
			get
			{
				if (List is not { Count: > 0 })
				{
					LoadLevels();
				}
				return List;
			}
		}
		internal static List<string> LevelsData
		{
			get
			{
				if (List is not { Count: > 0 })
				{
					LoadLevelsData();
				}
				return List;
			}
		}

		internal static void LoadLevels()
		{
			LoadCsv("Levels");
		}
		internal static void LoadLevelsData()
		{
			LoadCsv("LevelsData", "LevelData");
		}

		private static void LoadCsv(string csvFile, string colum = "Level")
		{
			List.Clear();
			var index = 0;
			CsvReader.LoadFromResource(csvFile,
				columns => index = columns[colum],
				cells => List.Add(cells[index])
			);
		}
	}
}