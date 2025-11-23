using System;
using GameTribunal.Application.Contracts;

namespace GameTribunal.Infrastructure.Services;

/// <summary>
/// Generates GUIDs using the platform's default provider.
/// </summary>
public sealed class GuidGenerator : IGuidGenerator
{
    /// <inheritdoc />
    public Guid Create()
    {
        return Guid.NewGuid();
    }
}
