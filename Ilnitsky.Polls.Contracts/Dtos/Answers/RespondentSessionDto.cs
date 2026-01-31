using System;

namespace Ilnitsky.Polls.Contracts.Dtos.Answers;

public record RespondentSessionDto(
    Guid SessionId,
    Guid RespondentId,
    DateTime DateTime,
    bool IsMobile,
    string RemoteIpAddress,
    string UserAgent,
    string AcceptLanguage,
    string Platform,
    string Brand);
