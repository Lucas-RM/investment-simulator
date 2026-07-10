namespace InvestmentSimulator.Application.Export;

/// <summary>
/// Binary export payload ready for download or persistence (ERS section 25).
/// </summary>
public sealed class ExportDocument
{
    public ExportDocument(
        ExportFormat format,
        string fileName,
        string contentType,
        byte[] content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentNullException.ThrowIfNull(content);

        Format = format;
        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }

    /// <summary>Export format used to generate this document.</summary>
    public ExportFormat Format { get; }

    /// <summary>Suggested download file name including extension.</summary>
    public string FileName { get; }

    /// <summary>MIME content type of <see cref="Content"/>.</summary>
    public string ContentType { get; }

    /// <summary>Raw file bytes.</summary>
    public byte[] Content { get; }
}
