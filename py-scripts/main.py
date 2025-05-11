def imageCompression(path: str = "image.jpg", quality: int = 50):
    from PIL import Image

    # Open an image file
    with Image.open(path) as img:
        # Save the image with the specified quality
        img.save("compressed_image.jpg", "JPEG", quality=quality)
        print(f"Image saved as compressed_image.jpg with quality={quality}")


def main():
    imageCompression("./logo.png", 50)


if __name__ == "__main__":
    main()
