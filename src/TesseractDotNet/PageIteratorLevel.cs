namespace TesseractDotNet;

public enum PageIteratorLevel
{
    /// <summary>
    /// Block of text/image/separator line.
    /// </summary>
    Block = 0,

    /// <summary>
    /// Paragraph within a block.
    /// </summary>
    Paragraph,

    /// <summary>
    /// Line within a paragraph.
    /// </summary>
    Textline,

    /// <summary>
    /// Word within a text line.
    /// </summary>
    Word,

    /// <summary>
    /// Symbol/character within a word.
    /// </summary>
    Symbol
}