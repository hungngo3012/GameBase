using UnityEngine;
using UnityEngine.UI;
public class GridAutoChangeSize : MonoBehaviour
{
    public GridLayoutGroup gridLayoutGroup;
    void Awake()
    {
        AdjustGridCellSize();
    }

    void AdjustGridCellSize()
    {
		float screenAspect = (float)Screen.width / (float)Screen.height;
		float targetAspect = 1080f / 1920f; // Tỷ lệ mục tiêu (bạn có thể đổi thành tỷ lệ gốc của game bạn)

		float difference = screenAspect / targetAspect;

        if(difference < 1.0f)
        {
            gridLayoutGroup.cellSize = gridLayoutGroup.cellSize * difference * 1.2f;
        }
	}
}
