using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.CaseStatuses;

/// <summary>A configurable case status (the spec's <c>OutputCaseStatus</c>).</summary>
public sealed class CaseStatus : StatusEntity<CaseStage>;
