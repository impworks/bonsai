using System;

namespace Bonsai.Code.Utils;

/// <summary>
/// Exception during an admin operation.
/// </summary>
public class OperationException(string message) : Exception(message);