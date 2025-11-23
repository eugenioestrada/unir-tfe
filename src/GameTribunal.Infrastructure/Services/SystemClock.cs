using System;
using GameTribunal.Application.Contracts;

namespace GameTribunal.Infrastructure.Services;

/// <summary>
/// Provides UTC timestamps based on the system clock.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTime.UtcNow;
}
