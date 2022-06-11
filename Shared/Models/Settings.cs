﻿namespace FileFlows.Shared.Models;

/// <summary>
/// Settings for FileFlows
/// </summary>
public class Settings : FileFlowObject
{
    /// <summary>
    /// Gets or sets if plugins should automatically be updated when new version are available online
    /// </summary>
    public bool AutoUpdatePlugins { get; set; }

    /// <summary>
    /// Gets or sets if the server should automatically update when a new version is available online
    /// </summary>
    public bool AutoUpdate { get; set; }

    /// <summary>
    /// Gets or sets if nodes should be automatically updated when the server is updated
    /// </summary>
    public bool AutoUpdateNodes { get; set; }

    /// <summary>
    /// Gets or sets if telemetry should be disabled
    /// </summary>
    public bool DisableTelemetry { get; set; }
    
    private List<string> _PluginRepositoryUrls = new ();
    /// <summary>
    /// Gets or sets a list of available URLs to additional plugin repositories
    /// </summary>
    public List<string> PluginRepositoryUrls
    {
        get => _PluginRepositoryUrls;
        set => _PluginRepositoryUrls = value ?? new();
    }

    /// <summary>
    /// Gets or sets if the Queue messages should be logged
    /// </summary>
    public bool LogQueueMessages { get; set; }

    /// <summary>
    /// Gets or sets if this is running on Windows
    /// </summary>
    public bool IsWindows { get; set; }
    
    /// <summary>
    /// Gets or sets if this is running inside Docker
    /// </summary>
    public bool IsDocker { get; set; }
    
    /// <summary>
    /// Gets or sets the FileFlows version number
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets if the system is paused
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// Gets or sets the license email
    /// </summary>
    public string LicenseEmail { get; set; }

    /// <summary>
    /// Gets or sets the licensed key
    /// </summary>
    public string LicenseKey { get; set; }

    /// <summary>
    /// Gets or sets the license code
    /// </summary>
    public string LicenseCode { get; set; }

    /// <summary>
    /// Gets the license flags for the user
    /// </summary>
    public string LicenseFlags { get; set; }
    /// <summary>
    /// Gets the license expiry date for the user
    /// </summary>
    public DateTime LicenseExpiryDate { get; set; }
    /// <summary>
    /// Gets the licensed processing nodes for the user
    /// </summary>
    public int LicenseProcessingNodes { get; set; }
    /// <summary>
    /// Gets the licensed status for the user
    /// </summary>
    public string LicenseStatus { get; set; }

    /// <summary>
    /// Gets or sets the type of database to use
    /// </summary>
    public DatabaseType DbType { get; set; }
    
    /// <summary>
    /// Gets or sets the db server to use
    /// </summary>
    public string DbServer { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the database
    /// </summary>
    public string DbName { get; set; }
    
    /// <summary>
    /// Gets or sets the user used to connect to the database
    /// </summary>
    public string DbUser { get; set; }
    
    /// <summary>
    /// Gets or sets the password used to connect to the database
    /// </summary>
    public string DbPassword { get; set; }
}

/// <summary>
/// The types of Databases supported
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// SQLite Database
    /// </summary>
    Sqlite = 0,
    /// <summary>
    /// Microsoft SQL Server
    /// </summary>
    SqlServer = 1,
    /// <summary>
    /// MySql / MariaDB
    /// </summary>
    MySql = 2
}