using System;

namespace Bonsai.Areas.Admin.Logic.Validation;

/// <summary>
/// Information about contradictory facts.
/// <param name="Message">Detailed information about the inconsistency.</param>
/// <param name="PageIds">Related pages.</param>
/// </summary>
public record ConsistencyErrorInfo(string Message, params Guid[] PageIds);