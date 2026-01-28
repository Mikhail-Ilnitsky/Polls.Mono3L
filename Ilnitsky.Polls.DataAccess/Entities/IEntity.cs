using System;

namespace Ilnitsky.Polls.DataAccess.Entities;

public interface IEntity<TKey>
    where TKey : struct, IEquatable<TKey>
{
    TKey Id { get; }
}

public interface IEntity : IEntity<Guid>;
