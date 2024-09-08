using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;  // For click detection

public class GameManager : MonoBehaviour
{
    public List<Sprite> images;  // List of your 10 sprites
    public GameObject imagePrefab;  // Prefab of the UI Image
    public RectTransform canvasRectTransform;  // The Canvas RectTransform
    public float spacing = 150f;  // Spacing between images
    public GameObject movingImageParent;  // GameObject to spawn and move images
    public float moveSpeed = 200f;  // Speed for the moving images
    private int numberOfImagesToSpawn = 3;  // Number of images to spawn initially
    private int numberOfMovingImages = 60;  // Total number of moving images
    private float spawnInterval = 0.5f;  // Interval for spawning new images

    private List<GameObject> spawnedImages = new List<GameObject>();  // List to hold the spawned images
    private GameObject highlightedImage;  // The image that is highlighted

    void Start()
    {
        // Step 1: Spawn 3 random images
        SpawnRandomImages();

        // Step 2: Highlight one of the 3 random images
        HighlightRandomImage();

        // Step 3: Start spawning moving images from the 3 spawned images
        StartCoroutine(SpawnMovingImages());
    }

    void SpawnRandomImages()
    {
        // Get the width of the Canvas
        float canvasWidth = canvasRectTransform.rect.width;

        // Calculate the starting X position for equal spacing
        float totalWidth = (numberOfImagesToSpawn - 1) * spacing;
        float startX = -totalWidth / 2;

        // List to store random images to avoid duplicates
        List<Sprite> selectedImages = new List<Sprite>();

        // Spawn 3 random images
        for (int i = 0; i < numberOfImagesToSpawn; i++)
        {
            // Get a random image that hasn't been used yet
            Sprite randomImage = null;
            do
            {
                randomImage = images[Random.Range(0, images.Count)];
            } while (selectedImages.Contains(randomImage));

            selectedImages.Add(randomImage);

            // Instantiate the image prefab
            GameObject newImage = Instantiate(imagePrefab, transform);

            // Set the sprite of the image
            newImage.GetComponent<Image>().sprite = randomImage;

            // Get the RectTransform of the new image
            RectTransform imageRect = newImage.GetComponent<RectTransform>();

            // Set the anchored position of the new image
            imageRect.anchoredPosition = new Vector2(startX + i * spacing, imageRect.rect.height / 2);

            // Add the new image to the spawnedImages list
            spawnedImages.Add(newImage);
        }
    }

    void HighlightRandomImage()
    {
        // Select a random image from the spawned images
        int randomIndex = Random.Range(0, spawnedImages.Count);
        highlightedImage = spawnedImages[randomIndex];

        // Add an Outline component to highlight the image
        Outline outline = highlightedImage.AddComponent<Outline>();
        outline.effectColor = Color.red;  // You can change the highlight color here
        outline.effectDistance = new Vector2(5, 5);  // Set the thickness of the outline
    }

    IEnumerator SpawnMovingImages()
    {
        // X position to start spawning the moving images
        float spawnPositionX = -canvasRectTransform.rect.width / 2;

        // Loop to spawn 30 images
        for (int i = 0; i < numberOfMovingImages; i++)
        {
            // Randomly select one of the 3 spawned images, but don't inherit highlight
            int randomIndex = Random.Range(0, spawnedImages.Count);
            Sprite selectedSprite = spawnedImages[randomIndex].GetComponent<Image>().sprite;

            // Instantiate a new image prefab (without outline)
            GameObject movingImage = Instantiate(imagePrefab, movingImageParent.transform);
            movingImage.GetComponent<Image>().sprite = selectedSprite;  // Set sprite only

            // Remove any Outline component if present (just in case)
            Outline outline = movingImage.GetComponent<Outline>();
            if (outline != null)
            {
                Destroy(outline);
            }

            // Set position for new moving image at spawnPositionX (one after another)
            RectTransform movingRect = movingImage.GetComponent<RectTransform>();
            movingRect.anchoredPosition = new Vector2(spawnPositionX, movingRect.rect.height / 2);

            // Add Button component and onClick listener
            Button button = movingImage.AddComponent<Button>();
            button.onClick.AddListener(() => OnImageClicked(movingImage));  // Add onClick handler

            // Start moving the image from left to right
            StartCoroutine(MoveImage(movingImage));

            // Wait for 0.5 seconds before spawning the next image
            yield return new WaitForSeconds(spawnInterval);

            // Increment spawnPositionX so the next image spawns after the previous one
            spawnPositionX += movingRect.rect.width + spacing;
        }
    }

    IEnumerator MoveImage(GameObject movingImage)
    {
        // Get the RectTransform of the moving image
        RectTransform movingRect = movingImage.GetComponent<RectTransform>();

        // Move the image from left to right
        while (movingImage != null)
        {
            // Move the image in one direction (left to right)
            movingRect.anchoredPosition += new Vector2(moveSpeed * Time.deltaTime, 0);

            // If the image goes beyond the right edge of the canvas, destroy it
            if (movingRect.anchoredPosition.x > canvasRectTransform.rect.width / 2)
            {
                Destroy(movingImage);  // Destroy the moving image GameObject
                yield break;  // Exit the coroutine to stop it from running
            }

            yield return null;
        }
    }

    void OnImageClicked(GameObject clickedImage)
    {
        // Get the sprite of the clicked image
        Sprite clickedSprite = clickedImage.GetComponent<Image>().sprite;

        // Get the sprite of the highlighted image
        Sprite highlightedSprite = highlightedImage.GetComponent<Image>().sprite;

        // Compare the clicked image's sprite with the highlighted image's sprite
        if (clickedSprite == highlightedSprite)
        {
            // If it's the same as the highlighted image, destroy the clicked image
            Destroy(clickedImage);
            Debug.Log("Correct! Image destroyed.");
        }
        else
        {
            Debug.Log("Incorrect! This is not the highlighted image.");
        }
    }
}
