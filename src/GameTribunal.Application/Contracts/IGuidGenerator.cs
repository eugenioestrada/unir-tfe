using System;

namespace GameTribunal.Application.Contracts;

/// <summary>
/// Provides deterministic GUID generation.
/// </summary>
public interface IGuidGenerator
{
    /// <summary>
    /// Creates a new globally unique identifier.
    /// </summary>
    Guid Create();
}
