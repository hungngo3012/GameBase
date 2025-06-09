using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GeneralCalculate
{
    public static List<int> GenerateRandomNumbersWithSum(int targetSum, int minValue, int maxValue)
    {
        List<int> result = new List<int>();
        int currentSum = 0;

        // Tiếp tục chia nhỏ cho đến khi gần đạt giá trị đầu vào
        while (currentSum < targetSum)
        {
            // Tạo giá trị ngẫu nhiên giữa minValue và maxValue
            int remainingSum = targetSum - currentSum;
            int randomValue = Random.Range(minValue, Mathf.Min(maxValue + 1, remainingSum + 1));
            if (remainingSum - randomValue < minValue)
                randomValue = remainingSum;

            result.Add(randomValue);
            currentSum += randomValue;
        }
        return result;
    }
    public static int CountFilesInFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            // Lấy tất cả file trong thư mục
            string[] files = Directory.GetFiles(folderPath);
            return files.Length / 2; //Không tính file meta
        }
        else
        {
            Debug.LogError("Thư mục không tồn tại: " + folderPath);
            return 0;
        }
    }
    public static string FormatTime(float secs)
    {
        int hours = Mathf.FloorToInt(secs / 3600);
        int minutes = Mathf.FloorToInt((secs % 3600) / 60);
        int seconds = Mathf.FloorToInt(secs - hours * 3600 - minutes * 60);

        if (hours > 0)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
        else
        {
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    public static string GenerateUsername(int length = 8)
    {
        System.Random random = new System.Random();
        string randomName = GlobalDefine.realNames[random.Next(GlobalDefine.realNames.Length)];

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Text.StringBuilder result = new System.Text.StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            char randomChar = characters[random.Next(characters.Length)];
            result.Append(randomChar);
        }

        return randomName + "_" + result.ToString(); // Trả về tên ngẫu nhiên kèm theo chuỗi ký tự ngẫu nhiên
    }
    public static string GetRandomChars(int length = 8)
    {
        System.Random random = new System.Random();
        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        System.Text.StringBuilder result = new System.Text.StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            char randomChar = characters[random.Next(characters.Length)];
            result.Append(randomChar);
        }

        return result.ToString();
    }

    public static void SaveToJSON(string data, string filePath)
    {
        File.WriteAllText(filePath, data);
    }
    public static string LoadFromJSON(string filePath)
    {
        if (File.Exists(filePath))
        {
            return File.ReadAllText(filePath);
        }
        return null;
    }
    public static string LoadJsonFromResource(string filePath)
    {
        TextAsset jsonData = Resources.Load<TextAsset>(filePath);
        if (jsonData != null)
            return jsonData.text;

        return null;
    }
    public static void ScrollToIndex(int index, ScrollRect scrollRect, GridLayoutGroup contentGrid)
    {
        Canvas.ForceUpdateCanvases();

        // Calculate the position to scroll to
        float contentHeight = scrollRect.content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;
        float cellHeight = contentGrid.cellSize.y + contentGrid.spacing.y;
        float targetPositionY = cellHeight * index;

        // Ensure target position is within bounds
        targetPositionY = Mathf.Clamp(targetPositionY, 0, contentHeight - viewportHeight);

        // Smoothly scroll to the target position
        Vector2 targetPosition = new Vector2(scrollRect.content.anchoredPosition.x, targetPositionY);
        scrollRect.content.DOAnchorPos(targetPosition, 0.3f).SetEase(Ease.InOutQuad);
    }
    public static string FormatPoints(float points, int type = 2)
    {
        if (points >= 1000000000)
        {
            return (points / 1000000000).ToString("0.#") + "B";
        }
        else if (points >= 1000000 && type >= 1)
        {
            return (points / 1000000).ToString("0.#") + "M";
        }
        else if (points >= 1000 && type >= 2)
        {
            return (points / 1000).ToString("0.#") + "k";
        }
        else
        {
            return points.ToString("0");
        }
    }
    public static void UiScaleWithScreenSize(RectTransform rectTransform)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = 1080f / 1920f; // Tỷ lệ mục tiêu (bạn có thể đổi thành tỷ lệ gốc của game bạn)

        float difference = screenAspect / targetAspect;

        rectTransform.sizeDelta = rectTransform.sizeDelta * difference * 1.1f;
    }
    public static float GetResolutionRatio()
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float targetAspect = 1080f / 1920f;

        return targetAspect / screenAspect;
    }
    public static string FormatStringWithLineBreaks(string input, int lineLength)
    {
        if (string.IsNullOrEmpty(input) || lineLength <= 0)
            return input;

        System.Text.StringBuilder formattedString = new System.Text.StringBuilder();

        for (int i = 0; i < input.Length; i += lineLength)
        {
            if (i + lineLength < input.Length)
                formattedString.AppendLine(input.Substring(i, lineLength)); // Lấy đoạn 50 ký tự và thêm xuống dòng
            else
                formattedString.Append(input.Substring(i)); // Thêm phần còn lại
        }

        return formattedString.ToString();
    }
}
