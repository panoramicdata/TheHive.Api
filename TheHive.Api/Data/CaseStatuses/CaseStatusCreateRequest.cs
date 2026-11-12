using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.CaseStatuses;

/// <summary>The body of a create-case-status request (the spec's <c>InputCreateCaseStatus</c>). Unset properties are omitted. <c>value</c> and <c>stage</c> cannot be changed after creation.</summary>
public sealed class CaseStatusCreateRequest : StatusCreateRequestBase<CaseStage>;
