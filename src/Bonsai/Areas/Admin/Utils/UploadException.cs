using System;

namespace Bonsai.Areas.Admin.Utils;

/// <summary>
/// Exception that occurs during a file upload.
/// </summary>
public class UploadException(string message) : Exception(message);