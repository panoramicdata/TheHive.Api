namespace TheHive.Api.Data.Views;

/// <summary>The list a view applies to (the spec's <c>InputListViewEntity</c>); the wire value is the member name.</summary>
public enum ViewEntity
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The alert list.</summary>
	Alert,

	/// <summary>The observable list of an alert.</summary>
	AlertObservable,

	/// <summary>The TTP list of an alert.</summary>
	AlertTTP,

	/// <summary>The history list of an alert.</summary>
	AlertHistory,

	/// <summary>The similar-cases list of an alert.</summary>
	AlertSimilarCases,

	/// <summary>The similar-alerts list of an alert.</summary>
	AlertSimilarAlerts,

	/// <summary>The case list.</summary>
	Case,

	/// <summary>The task list of a case.</summary>
	CaseTask,

	/// <summary>The observable list of a case.</summary>
	CaseObservable,

	/// <summary>The TTP list of a case.</summary>
	CaseTTP,

	/// <summary>The history list of a case.</summary>
	CaseHistory,

	/// <summary>The similar-cases list of a case.</summary>
	CaseSimilarCases,

	/// <summary>The similar-alerts list of a case.</summary>
	CaseSimilarAlerts,

	/// <summary>The similar-linked-alerts list of a case.</summary>
	CaseSimilarLinkedAlerts,

	/// <summary>The dashboard list.</summary>
	Dashboard,

	/// <summary>The organization list.</summary>
	Organisation,

	/// <summary>The user list of an organization.</summary>
	OrganisationUsers,

	/// <summary>The case template list of an organization.</summary>
	OrganisationTemplateCase,

	/// <summary>The report template list of an organization.</summary>
	OrganisationTemplateReport,

	/// <summary>The page template list of an organization.</summary>
	OrganisationTemplatePages,

	/// <summary>The custom tag list of an organization.</summary>
	OrganisationCustomTags,

	/// <summary>The function list of an organization.</summary>
	OrganisationFunctions,

	/// <summary>The user list.</summary>
	User,

	/// <summary>The profile list.</summary>
	Profiles,

	/// <summary>The custom field list.</summary>
	CustomFields,

	/// <summary>The observable type list.</summary>
	ObservableTypes,

	/// <summary>The taxonomy list.</summary>
	Taxonomies,

	/// <summary>The task list.</summary>
	Task,

	/// <summary>The attack pattern list.</summary>
	AttackPatterns
}
