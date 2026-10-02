namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The values of <see cref="CaseReportWidget.Kind"/> that TheHive 5.8 defines (the spec's <c>Kind</c>). The server may add more, so the property is a string.</summary>
public static class CaseReportWidgetKinds
{
	/// <summary>A Mustache text block.</summary>
	public const string Text = "Text";

	/// <summary>An image from a template attachment.</summary>
	public const string Image = "Image";

	/// <summary>The case custom fields, as a table.</summary>
	public const string CustomFields = "CustomFields";

	/// <summary>The case custom fields, as a list.</summary>
	public const string CustomFieldsList = "CustomFieldsList";

	/// <summary>A table of alerts.</summary>
	public const string AlertTable = "AlertTable";

	/// <summary>A list of alerts.</summary>
	public const string AlertList = "AlertList";

	/// <summary>A table of observables.</summary>
	public const string ObservableTable = "ObservableTable";

	/// <summary>A list of observables.</summary>
	public const string ObservableList = "ObservableList";

	/// <summary>A table of tasks.</summary>
	public const string TaskTable = "TaskTable";

	/// <summary>A list of tasks, optionally with their logs.</summary>
	public const string TaskList = "TaskList";

	/// <summary>A table of TTPs.</summary>
	public const string TTPTable = "TTPTable";

	/// <summary>A list of TTPs.</summary>
	public const string TTPList = "TTPList";

	/// <summary>The case timeline.</summary>
	public const string Timeline = "Timeline";

	/// <summary>The case comments.</summary>
	public const string Comments = "Comments";

	/// <summary>The case pages.</summary>
	public const string Pages = "Pages";
}
