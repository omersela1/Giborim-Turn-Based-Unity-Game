using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSizeAdapter : MonoBehaviour
{
    public SpriteRenderer spriteToResize;

    private float _currentSpriteWidth;
    public Image imageToResize;

    void OnEnable()
    {
        GameLogic.OnEnteredMyTeamScreen += ResizeImage;
        GameLogic.OnEnteredBattlegroundScreen += ResizeSprite;
    }

    void OnDisable()
    {
        GameLogic.OnEnteredMyTeamScreen -= ResizeImage;
        GameLogic.OnEnteredBattlegroundScreen -= ResizeSprite;
    }
    private void ResizeImage()
    {
        // Get the RectTransform of the UI Image
        RectTransform rectTransform = imageToResize.GetComponent<RectTransform>();
        
        // Calculate the new width based on screen width and the specified percentage
        float newWidth = Screen.width * 0.5f;
        
        // Set the new size while keeping the height the same
        rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
    }

    private void ResizeSprite()
    {
        // Get the width of the sprite in world units
        float spriteWidth = spriteToResize.bounds.size.x;

        // Calculate the desired width in world units based on screen width in world space
        float screenWidthInWorldUnits = Camera.main.orthographicSize * 2 * Screen.width / Screen.height;
        float newSpriteWidth = screenWidthInWorldUnits * 0.035f;

        if (_currentSpriteWidth != newSpriteWidth)
        {
            float scaleFactor = newSpriteWidth / spriteWidth;
            spriteToResize.transform.localScale = new Vector3(scaleFactor, spriteToResize.transform.localScale.y,
                spriteToResize.transform.localScale.z);
            _currentSpriteWidth = newSpriteWidth;
        }
    }
}
