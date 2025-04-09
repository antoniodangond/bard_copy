using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImagePool
{
    private Queue<Image> pool = new Queue<Image>(); // Pool of inactive images
    private Image prefab; // Prefab to instantiate
    private Transform parent; // Parent transform for the images

    public ImagePool(Image prefab, Transform parent, int initialSize)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Pre-instantiate objects and add them to the pool
        for (int i = 0; i < initialSize; i++)
        {
            Image image = CreateNewImage();
            image.gameObject.SetActive(false); // Deactivate the image
            pool.Enqueue(image);
        }
    }

    // Get an image from the pool
    public Image GetImage()
    {
        if (pool.Count > 0)
        {
            Image image = pool.Dequeue();
            image.gameObject.SetActive(true); // Activate the image
            return image;
        }
        else
        {
            // If the pool is empty, create a new image
            Debug.Log("Pool is empty! Creating a new image.");
            return CreateNewImage();
        }
    }

    // Return an image to the pool
    public void ReturnImage(Image image)
    {
        image.gameObject.SetActive(false); // Deactivate the image
        pool.Enqueue(image); // Add it back to the pool
    }

    // Create a new image
    private Image CreateNewImage()
    {
        Image image = Object.Instantiate(prefab, parent);
        return image;
    }
}