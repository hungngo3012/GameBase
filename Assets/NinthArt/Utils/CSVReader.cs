
using System.Text;
using System.Collections.Generic;
using UnityEngine;

//using UnityEngine;

internal static class CsvReader
{
	internal delegate void OnRowRead(List<string> cells);

	internal delegate void OnHeaderRead(Dictionary<string, int> columns);

	internal static void LoadFromResource(string file, OnHeaderRead onHeaderRead, OnRowRead onRowRead)
	{
		var csv = Resources.Load<TextAsset>(file).text;
		LoadFromString(csv, onHeaderRead, onRowRead);
	}

	internal static void LoadFromString(string fileContents, OnHeaderRead onHeaderRead, OnRowRead onRowRead)
	{
		var fileLength = fileContents.Length;
		// read char by char and when a , or \n, perform appropriate action
		var curFileIndex = 0; // index in the file
		var curLine = new List<string>(); // current line of data
		var curLineNumber = 0;
		var curItem = new StringBuilder("");
		var insideQuotes = false; // managing quotes
		var columns = new Dictionary<string, int>();
		while (curFileIndex < fileLength)
		{
			var c = fileContents[curFileIndex++];
			switch (c)
			{
				case '"':
					if (!insideQuotes)
					{
						insideQuotes = true;
					}
					else
					{
						if (curFileIndex == fileLength)
						{
							// end of file
							insideQuotes = false;
							goto case '\n';
						}

						if (fileContents[curFileIndex] == '"')
						{
							// double quote, save one
							curItem.Append("\"");
							curFileIndex++;
						}
						else
						{
							// leaving quotes section
							insideQuotes = false;
						}
					}

					break;
				case '\r':
					// ignore it completely
					break;
				case ',':
					goto case '\n';
				case '\n':
					if (insideQuotes)
					{
						// inside quotes, this characters must be included
						curItem.Append(c);
					}
					else
					{
						// end of current item
						curLine.Add(curItem.ToString());
						curItem.Length = 0;
						if (c == '\n' || curFileIndex == fileLength)
						{
							// also end of line, call line reader
							if (curLineNumber == 0)
							{
								for (var i = 0; i < curLine.Count; i++)
								{
									columns.Add(curLine[i], i);
								}

								onHeaderRead(columns);
							}
							else
							{
								onRowRead(curLine);
							}
							
							curLineNumber++;
							curLine.Clear();
						}
					}

					break;
				default:
					// other cases, add char
					curItem.Append(c);
					if (curFileIndex == fileLength)
					{
						curLine.Add(curItem.ToString());
						// also end of line, call line reader
						if (curLineNumber == 0)
						{
							for (var i = 0; i < curLine.Count; i++)
							{
								columns.Add(curLine[i], i);
							}

							onHeaderRead(columns);
						}
						else
						{
							onRowRead(curLine);
						}
							
						curLineNumber++;
						curLine.Clear();
					}

					break;
			}
		}
	}
}