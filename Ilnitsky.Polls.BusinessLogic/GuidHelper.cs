using System;
using UUIDNext;

namespace Ilnitsky.Polls.BusinessLogic;

public static class GuidHelper
{
    public static Guid CreateGuidV7() => Uuid.NewSequential();
}
