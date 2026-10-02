namespace TheHive.Api.Querying;

/// <content>Typed starters for the common primary operations; use <see cref="List"/> or <see cref="Get"/> for the others.</content>
public sealed partial class QueryBuilder
{
	/// <summary>Starts with <c>listCase</c>: all cases accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListCases() => List("listCase");

	/// <summary>Starts with <c>listAlert</c>: all alerts accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListAlerts() => List("listAlert");

	/// <summary>Starts with <c>listTask</c>: all tasks accessible to the current user, including case-template tasks.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListTasks() => List("listTask");

	/// <summary>Starts with <c>listObservable</c>: all observables accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListObservables() => List("listObservable");

	/// <summary>Starts with <c>listLog</c>: all task logs accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListTaskLogs() => List("listLog");

	/// <summary>Starts with <c>listComment</c>: all comments accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListComments() => List("listComment");

	/// <summary>Starts with <c>listUser</c>: all users.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListUsers() => List("listUser");

	/// <summary>Starts with <c>listOrganisation</c>: all organizations.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListOrganisations() => List("listOrganisation");

	/// <summary>Starts with <c>listCaseTemplate</c>: all case templates.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListCaseTemplates() => List("listCaseTemplate");

	/// <summary>Starts with <c>listCustomField</c>: all custom fields.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListCustomFields() => List("listCustomField");

	/// <summary>Starts with <c>listProcedure</c>: all procedures (TTPs) accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListProcedures() => List("listProcedure");

	/// <summary>Starts with <c>listPage</c>: all Knowledge Base and case pages accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListPages() => List("listPage");

	/// <summary>Starts with <c>listDashboard</c>: all dashboards accessible to the current user.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListDashboards() => List("listDashboard");

	/// <summary>Starts with <c>listProfile</c>: all profiles.</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListProfiles() => List("listProfile");

	/// <summary>Starts with <c>listTag</c>: the custom tags of the current organization (not taxonomy tags).</summary>
	/// <returns>A new builder.</returns>
	public static QueryBuilder ListTags() => List("listTag");

	/// <summary>Starts with <c>getCase</c>: one case.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetCase(string idOrName) => Get("getCase", idOrName);

	/// <summary>Starts with <c>getAlert</c>: one alert.</summary>
	/// <param name="idOrName">The alert ID preceded by <c>~</c>, or the alert reference <c>type;source;sourceRef</c>.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetAlert(string idOrName) => Get("getAlert", idOrName);

	/// <summary>Starts with <c>getTask</c>: one task.</summary>
	/// <param name="idOrName">The task ID preceded by <c>~</c>.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetTask(string idOrName) => Get("getTask", idOrName);

	/// <summary>Starts with <c>getObservable</c>: one observable.</summary>
	/// <param name="idOrName">The observable ID preceded by <c>~</c>.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetObservable(string idOrName) => Get("getObservable", idOrName);

	/// <summary>Starts with <c>getLog</c>: one task log.</summary>
	/// <param name="idOrName">The task log ID preceded by <c>~</c>.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetTaskLog(string idOrName) => Get("getLog", idOrName);

	/// <summary>Starts with <c>getUser</c>: one user.</summary>
	/// <param name="idOrName">The user ID preceded by <c>~</c>, or the login.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetUser(string idOrName) => Get("getUser", idOrName);

	/// <summary>Starts with <c>getOrganisation</c>: one organization.</summary>
	/// <param name="idOrName">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <returns>A new builder.</returns>
	public static QueryBuilder GetOrganisation(string idOrName) => Get("getOrganisation", idOrName);
}
