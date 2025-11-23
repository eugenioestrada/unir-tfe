using System;

namespace GameTribunal.Application.Contracts;

/// <summary>
/// Provides deterministic access to the current UTC timestamp.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current time expressed in UTC.
    /// </summary>
    DateTime UtcNow { get; }
}
