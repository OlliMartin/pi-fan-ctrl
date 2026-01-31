# CORS Fix for SignalR Connections

## Problem
When attempting to connect to the SignalR hub from a browser client, users encountered the following error:
```
[6:39:00 AM] Error connecting: Error: Failed to complete negotiation with the server: TypeError: Failed to fetch
```

This error occurred because the ASP.NET Core application did not have Cross-Origin Resource Sharing (CORS) configured, causing the browser to block requests from different origins.

## Root Cause
1. **Missing CORS Configuration**: The application had no CORS policy configured
2. **Browser Security**: Modern browsers block cross-origin requests by default for security
3. **SignalR Requirements**: SignalR requires specific CORS settings, including credential support for WebSocket connections

## Solution

### 1. CORS Configuration in Program.cs

Added CORS policy configuration:
```csharp
builder.Services.AddCors(options =>
{
  options.AddPolicy("SignalRCorsPolicy", policy =>
  {
    policy.SetIsOriginAllowed(_ => true) // Allow any origin in development
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials(); // Required for SignalR WebSocket connections
  });
});
```

Key points:
- `SetIsOriginAllowed(_ => true)`: Allows connections from any origin (suitable for development)
- `AllowCredentials()`: Required for SignalR to properly upgrade to WebSocket connections
- `AllowAnyHeader()` and `AllowAnyMethod()`: Allow all HTTP headers and methods

### 2. Enable CORS Middleware

Added CORS middleware to the request pipeline:
```csharp
app.UseCors("SignalRCorsPolicy");
```

**Important**: The middleware must be placed before `app.UseAntiforgery()` and other middleware that need CORS support.

### 3. Local SignalR Library

Included the SignalR JavaScript library locally (`wwwroot/signalr.min.js`) to avoid issues with:
- CDN availability
- Ad blockers blocking CDN resources
- Network restrictions

### 4. CORS Test Page

Created `cors-test.html` to quickly verify CORS configuration is working correctly.

## Testing

### Successful Connection Indicators
1. ✅ WebSocket connection established
2. ✅ SignalR protocol negotiation completes
3. ✅ Real-time updates are received (temperature and RPM)
4. ✅ Methods can be invoked on the hub

### Test Results
- **Node.js Client**: ✅ Connected successfully
- **HTML Test Client (same origin)**: ✅ Connected successfully  
- **HTML Test Client (different origin)**: ✅ Connected successfully with CORS
- **Temperature Simulation**: ✅ Working
- **Fan Speed Control**: ✅ Working
- **Real-time Push Notifications**: ✅ Working

## Production Considerations

For production deployment, consider:

1. **Restrict Origins**: Instead of `SetIsOriginAllowed(_ => true)`, specify allowed origins:
   ```csharp
   policy.WithOrigins("https://yourdomain.com", "https://app.yourdomain.com")
   ```

2. **Environment-Specific Configuration**: Use different CORS policies for development vs production:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       builder.Services.AddCors(options => 
       {
           options.AddPolicy("DevCorsPolicy", policy => 
               policy.SetIsOriginAllowed(_ => true)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials());
       });
   }
   else
   {
       builder.Services.AddCors(options =>
       {
           options.AddPolicy("ProdCorsPolicy", policy =>
               policy.WithOrigins("https://yourdomain.com")
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials());
       });
   }
   ```

3. **Security Audit**: Review CORS settings as part of security audits

## Files Modified

1. `PiFanCtrl/Program.cs` - Added CORS configuration
2. `PiFanCtrl/wwwroot/signalr-test.html` - Updated to use local SignalR library
3. `PiFanCtrl/wwwroot/signalr.min.js` - Added local copy of SignalR library
4. `PiFanCtrl/wwwroot/cors-test.html` - New CORS verification page

## References

- [ASP.NET Core CORS Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/cors)
- [SignalR CORS Configuration](https://learn.microsoft.com/en-us/aspnet/core/signalr/security#cross-origin-resource-sharing)
