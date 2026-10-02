# Task 13a report

Created TheHive.Api.Test.Integration (xunit.v3 MTP, Xunit.Microsoft.DependencyInjection, user-secrets Config section, failSkips=false).
Tests: ReadOnlyTests (current user, permissions), CaseTests (lifecycle with Optional-null clear and 404 check; case with task/observable/comment), AlertAndCustomFieldTests (alert lifecycle, custom field create/list/delete; CustomFields has no Get so list is used).
Skips: BaseAddress/ApiKey missing, or BaseAddress host ends with example.com, via Assert.SkipWhen in the lazy Client property.
Verification: integration build 0 warnings; dotnet test -> 6 skipped, exit 0; API build green; unit tests 534 passed, coverage line-rate 1.
CI untouched (tests only TheHive.Api.Test); .codacy.yaml already excludes the project. Live tests never run; server-side behaviour (e.g. clearing Summary, custom field naming) unverified.
