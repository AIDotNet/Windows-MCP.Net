using System.Drawing;

namespace WindowsMCP.Net.Services;

/// <summary>
/// Interface for OCR (Optical Character Recognition) services.
/// Provides methods for extracting text from images and screen regions.
/// </summary>
public interface IOcrService
{
    /// <summary>
    /// Extract text from a specific region of the screen.
    /// </summary>
    /// <param name="x">X coordinate of the region</param>
    /// <param name="y">Y coordinate of the region</param>
    /// <param name="width">Width of the region</param>
    /// <param name="height">Height of the region</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the extracted text and status code</returns>
    Task<(string Text, int Status)> ExtractTextFromRegionAsync(int x, int y, int width, int height, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract text from the entire screen.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the extracted text and status code</returns>
    Task<(string Text, int Status)> ExtractTextFromScreenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find specific text on the screen and return whether it was found.
    /// </summary>
    /// <param name="text">Text to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the search result and status code</returns>
    Task<(bool Found, int Status)> FindTextOnScreenAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the coordinates of specific text on the screen.
    /// </summary>
    /// <param name="text">Text to locate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the coordinates (or null if not found) and status code</returns>
    Task<(Point? Coordinates, int Status)> GetTextCoordinatesAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract text from an image stream.
    /// </summary>
    /// <param name="imageStream">Image stream</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A tuple containing the extracted text and status code</returns>
    Task<(string Text, int Status)> ExtractTextFromImageAsync(Stream imageStream, CancellationToken cancellationToken = default);
}