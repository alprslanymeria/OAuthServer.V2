using OAuthServer.V2.Core.Services;

namespace OAuthServer.V2.API.ModelBinding;

/// <summary>
/// ADAPTER THAT WRAPS AN IFORMFILE INTO THE IFILEUPLOAD ABSTRACTION.
/// </summary>
public class FormFileUploadAdapter(IFormFile formFile) : IFileUpload
{
    private readonly IFormFile _formFile = formFile;

    public string FileName => _formFile.FileName;
    public string ContentType => _formFile.ContentType;
    public long Length => _formFile.Length;
    public Stream OpenReadStream() => _formFile.OpenReadStream();
}