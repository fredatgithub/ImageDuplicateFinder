# ImageDuplicateFinder

This WPF application allows you to find duplicate images in a folder and its subfolders. It uses the `System.Drawing.Image` class to compare images pixel by pixel.

## Installation

You can download the latest release from the [releases page](https://github.com/fredatgithub/ImageDuplicateFinder/releases).

## Usage

1. **Open the Application**: Run the `ImageDuplicateFinder.exe` file to open the application.
2. **Select a Folder**: Click on the "Select Folder" button and choose the folder you want to scan for duplicate images.
3. **Start the Scan**: Click on the "Start Scan" button to begin the image comparison process.
4. **View Results**: Once the scan is complete, the application will display the results in a list. You can view the list of duplicate images and their similarity scores.

## How It Works

The application uses the `System.Drawing.Image` class to load and compare images. It compares each image with every other image in the folder and its subfolders, calculating the similarity score based on the number of matching pixels.

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/fredatgithub/ImageDuplicateFinder/blob/master/LICENSE.txt) file for more information.

## Example

![Here is an example:](
https://github.com/fredatgithub/ImageDuplicateFinder/blob/master/Example1.png)