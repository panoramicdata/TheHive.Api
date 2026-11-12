using System.Net;

namespace TheHive.Api.Test.Support;

internal static class ApiAssert
{
	public static async Task ForbiddenAsync(Func<Task> act)
		=> (await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
}
