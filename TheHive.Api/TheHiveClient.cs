using Refit;
using System.Diagnostics.CodeAnalysis;
using TheHive.Api.Interfaces;

namespace TheHive.Api;

/// <summary>Client for the TheHive 5 REST API.</summary>
public sealed class TheHiveClient : IDisposable
{
	private readonly HttpClient _httpClient;

	/// <summary>Creates a client.</summary>
	/// <param name="options">Connection options.</param>
	public TheHiveClient(TheHiveClientOptions options) : this(options, CreateDefaultHandler(options))
	{
	}

	internal static HttpClientHandler CreateDefaultHandler(TheHiveClientOptions options)
	{
		ArgumentNullException.ThrowIfNull(options);
		var handler = new HttpClientHandler();
		if (options.IgnoreCertificateErrors)
		{
			handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
		}

		return handler;
	}

	internal TheHiveClient(TheHiveClientOptions options, HttpMessageHandler inner)
	{
		options.Validate();
		_httpClient = CreateHttpClient(options, inner);
		Settings = CreateSettings();
		CreateCaseGroups();
		CreateIntegrationGroups();
		CreatePlatformGroups();
	}

	private static HttpClient CreateHttpClient(TheHiveClientOptions options, HttpMessageHandler inner)
	{
		var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
		return new HttpClient(new Handlers.AuthRetryHandler(options) { InnerHandler = inner })
		{
			BaseAddress = new Uri(baseUrl),
			// The per-attempt timeout is applied inside AuthRetryHandler so retries and Retry-After waits are not cut short.
			Timeout = System.Threading.Timeout.InfiniteTimeSpan
		};
	}

	private static RefitSettings CreateSettings() => new()
	{
		ContentSerializer = new SystemTextJsonContentSerializer(TheHiveJson.Options),
		// Interface paths are relative (no leading slash) so they append to a path-prefixed BaseUrl such as https://host/thehive/.
		UrlResolution = UrlResolutionMode.Rfc3986,
		// Boolean query parameters must be lowercase true/false, not Refit's True/False.
		UrlParameterFormatter = new TheHiveUrlParameterFormatter(),
		ExceptionFactory = response => new ValueTask<Exception?>(TheHiveErrorMapper.CreateAsync(response))
	};

	private T For<T>() => RestService.For<T>(_httpClient, Settings);

	[MemberNotNull(nameof(Cases), nameof(Alerts), nameof(Comments), nameof(Tasks), nameof(TaskLogs), nameof(Observables),
		nameof(ObservableTypes), nameof(CaseTemplates), nameof(CustomFields), nameof(Dashboards), nameof(Shares), nameof(Tags),
		nameof(Taxonomies), nameof(Pages))]
	private void CreateCaseGroups()
	{
		Cases = For<ICases>();
		Alerts = For<IAlerts>();
		Comments = For<IComments>();
		Tasks = For<ITasks>();
		TaskLogs = For<ITaskLogs>();
		Observables = For<IObservables>();
		ObservableTypes = For<IObservableTypes>();
		CaseTemplates = For<ICaseTemplates>();
		CustomFields = For<ICustomFields>();
		Dashboards = For<IDashboards>();
		Shares = For<IShares>();
		Tags = For<ITags>();
		Taxonomies = For<ITaxonomies>();
		Pages = For<IPages>();
	}

	[MemberNotNull(nameof(Cortex), nameof(Misp), nameof(Functions), nameof(AlertFeeders), nameof(EmailIntake), nameof(PageTemplates),
		nameof(Timeline), nameof(Views), nameof(CaseReportTemplates), nameof(CaseReports), nameof(Query), nameof(Describe),
		nameof(Patterns), nameof(Procedures))]
	private void CreateIntegrationGroups()
	{
		Cortex = For<ICortex>();
		Misp = For<IMisp>();
		Functions = For<IFunctions>();
		AlertFeeders = For<IAlertFeeders>();
		EmailIntake = For<IEmailIntake>();
		PageTemplates = For<IPageTemplates>();
		Timeline = For<ITimeline>();
		Views = For<IViews>();
		CaseReportTemplates = For<ICaseReportTemplates>();
		CaseReports = For<ICaseReports>();
		Query = For<IQuery>();
		Describe = For<IDescribe>();
		Patterns = For<IPatterns>();
		Procedures = For<IProcedures>();
	}

	[MemberNotNull(nameof(Profiles), nameof(Permissions), nameof(Organisations), nameof(Users), nameof(Authentication), nameof(Branding),
		nameof(Config), nameof(Status), nameof(License), nameof(Audit), nameof(Admin), nameof(CaseStatuses), nameof(AlertStatuses))]
	private void CreatePlatformGroups()
	{
		Profiles = For<IProfiles>();
		Permissions = For<IPermissions>();
		Organisations = For<IOrganisations>();
		Users = For<IUsers>();
		Authentication = For<IAuthentication>();
		Branding = For<IBranding>();
		Config = For<IConfig>();
		Status = For<IStatus>();
		License = For<ILicense>();
		Audit = For<IAudit>();
		Admin = For<IAdmin>();
		CaseStatuses = For<ICaseStatuses>();
		AlertStatuses = For<IAlertStatuses>();
	}

	/// <summary>Case operations.</summary>
	public ICases Cases { get; private set; }

	/// <summary>Alert operations.</summary>
	public IAlerts Alerts { get; private set; }

	/// <summary>Comment operations.</summary>
	public IComments Comments { get; private set; }

	/// <summary>Task operations.</summary>
	public ITasks Tasks { get; private set; }

	/// <summary>Task log operations.</summary>
	public ITaskLogs TaskLogs { get; private set; }

	/// <summary>Observable operations.</summary>
	public IObservables Observables { get; private set; }

	/// <summary>Observable type operations.</summary>
	public IObservableTypes ObservableTypes { get; private set; }

	/// <summary>Case template operations.</summary>
	public ICaseTemplates CaseTemplates { get; private set; }

	/// <summary>Custom field operations.</summary>
	public ICustomFields CustomFields { get; private set; }

	/// <summary>Dashboard operations.</summary>
	public IDashboards Dashboards { get; private set; }

	/// <summary>Permission profile operations.</summary>
	public IProfiles Profiles { get; private set; }

	/// <summary>Permission operations.</summary>
	public IPermissions Permissions { get; private set; }

	/// <summary>Organization operations, including sharing links and organization-level files.</summary>
	public IOrganisations Organisations { get; private set; }

	/// <summary>User account operations.</summary>
	public IUsers Users { get; private set; }

	/// <summary>Case, task and observable sharing operations.</summary>
	public IShares Shares { get; private set; }

	/// <summary>Tag operations.</summary>
	public ITags Tags { get; private set; }

	/// <summary>Taxonomy operations.</summary>
	public ITaxonomies Taxonomies { get; private set; }

	/// <summary>Cortex connector operations: analyzers, jobs, analyzer templates and responder actions.</summary>
	public ICortex Cortex { get; private set; }

	/// <summary>MISP connector operations: status, event synchronization, and case import and export.</summary>
	public IMisp Misp { get; private set; }

	/// <summary>Function operations: JavaScript functions that process data and call TheHive API.</summary>
	public IFunctions Functions { get; private set; }

	/// <summary>Alert feeder operations: scheduled HTTP retrieval of data converted into alerts.</summary>
	public IAlertFeeders AlertFeeders { get; private set; }

	/// <summary>Email intake operations: mailbox connections that turn emails into alerts.</summary>
	public IEmailIntake EmailIntake { get; private set; }

	/// <summary>Knowledge Base and case page operations.</summary>
	public IPages Pages { get; private set; }

	/// <summary>Page template operations.</summary>
	public IPageTemplates PageTemplates { get; private set; }

	/// <summary>Case timeline custom event operations.</summary>
	public ITimeline Timeline { get; private set; }

	/// <summary>Saved list view operations.</summary>
	public IViews Views { get; private set; }

	/// <summary>Case report template operations, including their image attachments.</summary>
	public ICaseReportTemplates CaseReportTemplates { get; private set; }

	/// <summary>Case report operations: generate, upload, download and preview.</summary>
	public ICaseReports CaseReports { get; private set; }

	/// <summary>Query API operations (list, search, filter, sort, page and count any entity) and query exports; see <see cref="Querying.QueryBuilder"/>.</summary>
	public IQuery Query { get; private set; }

	/// <summary>Entity model metadata: the fields of each model that queries can filter, sort and aggregate on.</summary>
	public IDescribe Describe { get; private set; }

	/// <summary>Session login and logout, TOTP multifactor set-up and the local password policy.</summary>
	public IAuthentication Authentication { get; private set; }

	/// <summary>Branding operations: browser tab title, logos and favicon.</summary>
	public IBranding Branding { get; private set; }

	/// <summary>Configuration items of the calling user.</summary>
	public IConfig Config { get; private set; }

	/// <summary>Platform status operations.</summary>
	public IStatus Status { get; private set; }

	/// <summary>License operations: list, add, activate, challenge and current status.</summary>
	public ILicense License { get; private set; }

	/// <summary>Audit trail operations.</summary>
	public IAudit Audit { get; private set; }

	/// <summary>Platform administration operations: runtime log levels.</summary>
	public IAdmin Admin { get; private set; }

	/// <summary>Configurable case status operations.</summary>
	public ICaseStatuses CaseStatuses { get; private set; }

	/// <summary>Configurable alert status operations.</summary>
	public IAlertStatuses AlertStatuses { get; private set; }

	/// <summary>Attack technique (MITRE ATT&amp;CK pattern) and catalog operations.</summary>
	public IPatterns Patterns { get; private set; }

	/// <summary>Procedure (TTP) operations: ATT&amp;CK techniques linked to cases and alerts.</summary>
	public IProcedures Procedures { get; private set; }

	internal HttpClient HttpClient => _httpClient;

	internal RefitSettings Settings { get; }

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();
}
