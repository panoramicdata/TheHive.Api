using Refit;
using TheHive.Api.Interfaces;

namespace TheHive.Api;

/// <summary>Client for the TheHive 5 REST API.</summary>
public sealed class TheHiveClient : IDisposable
{
	private readonly HttpClient _httpClient;

	/// <summary>Creates a client.</summary>
	/// <param name="options">Connection options.</param>
	public TheHiveClient(TheHiveClientOptions options) : this(options, new HttpClientHandler())
	{
	}

	internal TheHiveClient(TheHiveClientOptions options, HttpMessageHandler inner)
	{
		options.Validate();
		var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
		_httpClient = new HttpClient(new Handlers.AuthRetryHandler(options) { InnerHandler = inner })
		{
			BaseAddress = new Uri(baseUrl),
			// The per-attempt timeout is applied inside AuthRetryHandler so retries and Retry-After waits are not cut short.
			Timeout = System.Threading.Timeout.InfiniteTimeSpan
		};
		Settings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(TheHiveJson.Options),
			// Interface paths are relative (no leading slash) so they append to a path-prefixed BaseUrl such as https://host/thehive/.
			UrlResolution = UrlResolutionMode.Rfc3986,
			// Boolean query parameters must be lowercase true/false, not Refit's True/False.
			UrlParameterFormatter = new TheHiveUrlParameterFormatter(),
			ExceptionFactory = response => new ValueTask<Exception?>(TheHiveErrorMapper.CreateAsync(response))
		};
		Cases = RestService.For<ICases>(_httpClient, Settings);
		Alerts = RestService.For<IAlerts>(_httpClient, Settings);
		Comments = RestService.For<IComments>(_httpClient, Settings);
		Tasks = RestService.For<ITasks>(_httpClient, Settings);
		TaskLogs = RestService.For<ITaskLogs>(_httpClient, Settings);
		Observables = RestService.For<IObservables>(_httpClient, Settings);
		ObservableTypes = RestService.For<IObservableTypes>(_httpClient, Settings);
		CaseTemplates = RestService.For<ICaseTemplates>(_httpClient, Settings);
		CustomFields = RestService.For<ICustomFields>(_httpClient, Settings);
		Dashboards = RestService.For<IDashboards>(_httpClient, Settings);
		Profiles = RestService.For<IProfiles>(_httpClient, Settings);
		Permissions = RestService.For<IPermissions>(_httpClient, Settings);
		Organisations = RestService.For<IOrganisations>(_httpClient, Settings);
		Users = RestService.For<IUsers>(_httpClient, Settings);
		Shares = RestService.For<IShares>(_httpClient, Settings);
		Tags = RestService.For<ITags>(_httpClient, Settings);
		Taxonomies = RestService.For<ITaxonomies>(_httpClient, Settings);
		Cortex = RestService.For<ICortex>(_httpClient, Settings);
		Misp = RestService.For<IMisp>(_httpClient, Settings);
		Functions = RestService.For<IFunctions>(_httpClient, Settings);
		AlertFeeders = RestService.For<IAlertFeeders>(_httpClient, Settings);
		EmailIntake = RestService.For<IEmailIntake>(_httpClient, Settings);
		Pages = RestService.For<IPages>(_httpClient, Settings);
		PageTemplates = RestService.For<IPageTemplates>(_httpClient, Settings);
		Timeline = RestService.For<ITimeline>(_httpClient, Settings);
		Views = RestService.For<IViews>(_httpClient, Settings);
		CaseReportTemplates = RestService.For<ICaseReportTemplates>(_httpClient, Settings);
		CaseReports = RestService.For<ICaseReports>(_httpClient, Settings);
		Query = RestService.For<IQuery>(_httpClient, Settings);
		Describe = RestService.For<IDescribe>(_httpClient, Settings);
		Config = RestService.For<IConfig>(_httpClient, Settings);
		Status = RestService.For<IStatus>(_httpClient, Settings);
		License = RestService.For<ILicense>(_httpClient, Settings);
		Audit = RestService.For<IAudit>(_httpClient, Settings);
		Admin = RestService.For<IAdmin>(_httpClient, Settings);
		CaseStatuses = RestService.For<ICaseStatuses>(_httpClient, Settings);
		AlertStatuses = RestService.For<IAlertStatuses>(_httpClient, Settings);
		Patterns = RestService.For<IPatterns>(_httpClient, Settings);
		Procedures = RestService.For<IProcedures>(_httpClient, Settings);
	}

	/// <summary>Case operations.</summary>
	public ICases Cases { get; }

	/// <summary>Alert operations.</summary>
	public IAlerts Alerts { get; }

	/// <summary>Comment operations.</summary>
	public IComments Comments { get; }

	/// <summary>Task operations.</summary>
	public ITasks Tasks { get; }

	/// <summary>Task log operations.</summary>
	public ITaskLogs TaskLogs { get; }

	/// <summary>Observable operations.</summary>
	public IObservables Observables { get; }

	/// <summary>Observable type operations.</summary>
	public IObservableTypes ObservableTypes { get; }

	/// <summary>Case template operations.</summary>
	public ICaseTemplates CaseTemplates { get; }

	/// <summary>Custom field operations.</summary>
	public ICustomFields CustomFields { get; }

	/// <summary>Dashboard operations.</summary>
	public IDashboards Dashboards { get; }

	/// <summary>Permission profile operations.</summary>
	public IProfiles Profiles { get; }

	/// <summary>Permission operations.</summary>
	public IPermissions Permissions { get; }

	/// <summary>Organization operations, including sharing links and organization-level files.</summary>
	public IOrganisations Organisations { get; }

	/// <summary>User account operations.</summary>
	public IUsers Users { get; }

	/// <summary>Case, task and observable sharing operations.</summary>
	public IShares Shares { get; }

	/// <summary>Tag operations.</summary>
	public ITags Tags { get; }

	/// <summary>Taxonomy operations.</summary>
	public ITaxonomies Taxonomies { get; }

	/// <summary>Cortex connector operations: analyzers, jobs, analyzer templates and responder actions.</summary>
	public ICortex Cortex { get; }

	/// <summary>MISP connector operations: status, event synchronization, and case import and export.</summary>
	public IMisp Misp { get; }

	/// <summary>Function operations: JavaScript functions that process data and call TheHive API.</summary>
	public IFunctions Functions { get; }

	/// <summary>Alert feeder operations: scheduled HTTP retrieval of data converted into alerts.</summary>
	public IAlertFeeders AlertFeeders { get; }

	/// <summary>Email intake operations: mailbox connections that turn emails into alerts.</summary>
	public IEmailIntake EmailIntake { get; }

	/// <summary>Knowledge Base and case page operations.</summary>
	public IPages Pages { get; }

	/// <summary>Page template operations.</summary>
	public IPageTemplates PageTemplates { get; }

	/// <summary>Case timeline custom event operations.</summary>
	public ITimeline Timeline { get; }

	/// <summary>Saved list view operations.</summary>
	public IViews Views { get; }

	/// <summary>Case report template operations, including their image attachments.</summary>
	public ICaseReportTemplates CaseReportTemplates { get; }

	/// <summary>Case report operations: generate, upload, download and preview.</summary>
	public ICaseReports CaseReports { get; }

	/// <summary>Query API operations (list, search, filter, sort, page and count any entity) and query exports; see <see cref="Querying.QueryBuilder"/>.</summary>
	public IQuery Query { get; }

	/// <summary>Entity model metadata: the fields of each model that queries can filter, sort and aggregate on.</summary>
	public IDescribe Describe { get; }

	/// <summary>Configuration items of the calling user.</summary>
	public IConfig Config { get; }

	/// <summary>Platform status operations.</summary>
	public IStatus Status { get; }

	/// <summary>License operations: list, add, activate, challenge and current status.</summary>
	public ILicense License { get; }

	/// <summary>Audit trail operations.</summary>
	public IAudit Audit { get; }

	/// <summary>Platform administration operations: runtime log levels.</summary>
	public IAdmin Admin { get; }

	/// <summary>Configurable case status operations.</summary>
	public ICaseStatuses CaseStatuses { get; }

	/// <summary>Configurable alert status operations.</summary>
	public IAlertStatuses AlertStatuses { get; }

	/// <summary>Attack technique (MITRE ATT&amp;CK pattern) and catalog operations.</summary>
	public IPatterns Patterns { get; }

	/// <summary>Procedure (TTP) operations: ATT&amp;CK techniques linked to cases and alerts.</summary>
	public IProcedures Procedures { get; }

	internal HttpClient HttpClient => _httpClient;

	internal RefitSettings Settings { get; }

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();
}
