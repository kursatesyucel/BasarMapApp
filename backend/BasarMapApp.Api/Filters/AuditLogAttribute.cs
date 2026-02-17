using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BasarMapApp.Api.Models.Logs;
using BasarMapApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BasarMapApp.Api.Filters
{
    /// <summary>
    /// Action filter attribute for automatic audit logging of controller actions.
    /// Implements Aspect-Oriented Programming (AOP) pattern for cross-cutting concerns.
    /// Usage: [AuditLog(entityName: "Camera", action: "Create")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuditLogAttribute : ActionFilterAttribute
    {
        private readonly string _entityName;
        private readonly string _action;

        /// <summary>
        /// Creates an audit log attribute
        /// </summary>
        /// <param name="entityName">Name of the entity being affected (e.g., "Camera", "MapPoint")</param>
        /// <param name="action">Action being performed (e.g., "Create", "Update", "Delete")</param>
        public AuditLogAttribute(string entityName, string action)
        {
            _entityName = entityName;
            _action = action;
        }

        /// <summary>
        /// Executes after the action method completes successfully
        /// </summary>
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Execute the action
            var executedContext = await next();

            // Only log if the action succeeded (no exception)
            if (executedContext.Exception == null)
            {
                try
                {
                    // Manuel DI resolution - Attribute'larda constructor injection çalışmaz
                    var logService = context.HttpContext.RequestServices.GetService<ILogService>();
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<AuditLogAttribute>>();

                    if (logService == null)
                    {
                        logger?.LogWarning("ILogService is not available. Audit log will not be created.");
                        return;
                    }

                    // Extract user information from claims
                    var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
                    int? userId = null;
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }

                    // Extract IP address
                    var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();

                    // Try to extract entity ID from route values or result
                    string? entityId = null;
                    
                    // First, try to get ID from route values
                    if (context.RouteData.Values.TryGetValue("id", out var routeId))
                    {
                        entityId = routeId?.ToString();
                    }

                    // If not in route, try to extract from action arguments
                    if (string.IsNullOrEmpty(entityId) && context.ActionArguments.Count > 0)
                    {
                        // Look for common ID property names in the arguments
                        foreach (var arg in context.ActionArguments.Values)
                        {
                            if (arg == null) continue;

                            var argType = arg.GetType();
                            var idProperty = argType.GetProperty("Id") ?? argType.GetProperty("id");
                            
                            if (idProperty != null)
                            {
                                var idValue = idProperty.GetValue(arg);
                                entityId = idValue?.ToString();
                                break;
                            }
                        }
                    }

                    // Serialize action arguments as the new values with circular reference handling
                    object? newValues = null;
                    try
                    {
                        // Only serialize if there are arguments
                        if (context.ActionArguments.Count > 0)
                        {
                            // Remove large or sensitive data before serialization
                            var sanitizedArgs = new Dictionary<string, object?>();
                            foreach (var kvp in context.ActionArguments)
                            {
                                // Skip sensitive or binary data
                                if (kvp.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                                    kvp.Key.Contains("token", StringComparison.OrdinalIgnoreCase))
                                {
                                    sanitizedArgs[kvp.Key] = "[REDACTED]";
                                }
                                else if (kvp.Value != null)
                                {
                                    // Serialize with circular reference handling
                                    try
                                    {
                                        var jsonOptions = new JsonSerializerOptions
                                        {
                                            ReferenceHandler = ReferenceHandler.IgnoreCycles,
                                            WriteIndented = false,
                                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                            MaxDepth = 32 // Limit depth to prevent stack overflow
                                        };

                                        // Serialize to JSON string first, then deserialize to object
                                        var jsonString = JsonSerializer.Serialize(kvp.Value, jsonOptions);
                                        sanitizedArgs[kvp.Key] = JsonSerializer.Deserialize<object>(jsonString);
                                    }
                                    catch
                                    {
                                        // If serialization fails, store type name
                                        sanitizedArgs[kvp.Key] = $"[{kvp.Value.GetType().Name}]";
                                    }
                                }
                                else
                                {
                                    sanitizedArgs[kvp.Key] = null;
                                }
                            }

                            newValues = sanitizedArgs;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Failed to serialize action arguments for audit log");
                        newValues = new { error = "Failed to serialize", type = "SerializationError" };
                    }

                    // Create and log the audit entry
                    var auditLog = new AuditLog
                    {
                        UserId = userId,
                        Action = _action,
                        EntityName = _entityName,
                        EntityId = entityId,
                        Timestamp = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        NewValues = newValues
                    };

                    // Fire-and-forget: Don't wait for logging to complete
                    await logService.CreateAuditLogAsync(auditLog);
                    
                    logger?.LogDebug("Audit log queued: {Action} on {EntityName} by User {UserId}", 
                        _action, _entityName, userId);
                }
                catch (Exception ex)
                {
                    // Swallow any exceptions - audit logging should never break the main flow
                    var logger = context.HttpContext.RequestServices.GetService<ILogger<AuditLogAttribute>>();
                    logger?.LogError(ex, "Error creating audit log for {Action} on {EntityName}. Error is ignored.",
                        _action, _entityName);
                }
            }
        }
    }
}
