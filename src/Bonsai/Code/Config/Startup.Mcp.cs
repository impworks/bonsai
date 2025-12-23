using System;
using Bonsai.Areas.Mcp.Logic.Auth;
using Bonsai.Areas.Mcp.Logic.Services;
using Bonsai.Data;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace Bonsai.Code.Config;

public partial class Startup
{
    /// <summary>
    /// Configures MCP (Model Context Protocol) services and OpenIddict for OAuth 2.0/2.1 authorization.
    /// </summary>
    private void ConfigureMcpServices(IServiceCollection services)
    {
        // OpenIddict for OAuth 2.0/2.1 (required by MCP spec)
        ConfigureOpenIddict(services);

        // MCP context and authorization
        services.AddScoped<McpUserContext>();
        services.AddScoped<McpToolAuthorizationService>();

        // MCP server with tools
        services.AddMcpServer(options =>
            {
                options.ServerInfo = new() { Name = "Bonsai Family Wiki", Version = "1.0.0" };
                options.ServerInstructions = GetServerInstructions();
            })
            .WithHttpTransport()
            .WithToolsFromAssembly(typeof(Startup).Assembly);
    }

    /// <summary>
    /// Configures OpenIddict for OAuth 2.0/2.1 authorization (used by MCP).
    /// </summary>
    private void ConfigureOpenIddict(IServiceCollection services)
    {
        services.AddOpenIddict()
            // Register the OpenIddict core components
            .AddCore(options =>
            {
                // Use Entity Framework Core stores
                options.UseEntityFrameworkCore()
                    .UseDbContext<AppDbContext>();
            })

            // Register the OpenIddict server components
            .AddServer(options =>
            {
                // Enable the authorization and token endpoints
                options.SetAuthorizationEndpointUris("/oauth/authorize")
                    .SetTokenEndpointUris("/oauth/token");

                // Enable authorization code flow with PKCE (required by MCP spec)
                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();

                // Also allow client credentials for machine-to-machine scenarios
                options.AllowClientCredentialsFlow();

                // Allow refresh tokens
                options.AllowRefreshTokenFlow();

                // Register scopes
                options.RegisterScopes(
                    OpenIddictConstants.Scopes.OpenId,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.Email,
                    "mcp"  // Custom scope for MCP access
                );

                // Use development encryption/signing keys in development
                // In production, you should use proper certificates
                if (Environment.EnvironmentName == "Development")
                {
                    options.AddDevelopmentEncryptionCertificate()
                        .AddDevelopmentSigningCertificate();
                }
                else
                {
                    // For production, use ephemeral keys (you should configure proper certificates)
                    options.AddEphemeralEncryptionKey()
                        .AddEphemeralSigningKey();
                }

                // Disable access token encryption for easier debugging
                // MCP clients expect plain JWT tokens
                options.DisableAccessTokenEncryption();

                // Register the ASP.NET Core host
                var aspNetCoreBuilder = options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableStatusCodePagesIntegration();

                // Allow HTTP in development (required for localhost testing)
                if (Environment.EnvironmentName == "Development")
                {
                    aspNetCoreBuilder.DisableTransportSecurityRequirement();
                }

                // Set token lifetimes
                options.SetAccessTokenLifetime(TimeSpan.FromHours(1))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(14));

                // Add custom handler to include registration_endpoint in discovery document
                // This is required for MCP clients that use Dynamic Client Registration
                options.AddEventHandler<OpenIddictServerEvents.HandleConfigurationRequestContext>(builder =>
                    builder.UseInlineHandler(context =>
                    {
                        // Add the registration endpoint to the discovery document
                        context.Metadata["registration_endpoint"] = context.BaseUri + "oauth/register";
                        return default;
                    })
                    .SetOrder(int.MaxValue)); // Run after the default handler
            })

            // Register the OpenIddict validation components for validating tokens
            .AddValidation(options =>
            {
                // Import the configuration from the local OpenIddict server instance
                options.UseLocalServer();

                // Register the ASP.NET Core host
                options.UseAspNetCore();
            });
    }

    /// <summary>
    /// Returns documentation for AI agents on how to use the MCP server.
    /// </summary>
    private static string GetServerInstructions() => """
        # Bonsai Family Wiki MCP Server

        This MCP server provides access to a family wiki containing information about people, pets, locations, and events.

        ## Custom Markdown Syntax

        Page descriptions support standard Markdown plus custom tags:

        ### Page Links
        Use double brackets to link to other pages:
        - `[[Page Title]]` - Links to a page with that title
        - `[[Page Title|Custom Label]]` - Links with custom display text

        Examples:
        - `[[John Smith]]` renders as a link to John Smith's page
        - `[[John Smith|my grandfather]]` renders as "my grandfather" linking to John Smith

        ### Media Embeds
        Embed photos/videos in page descriptions:
        - `[[media:media-key]]` - Basic embed (large, left-aligned)
        - `[[media:media-key|size:small]]` - Size options: large, medium, small
        - `[[media:media-key|align:right]]` - Alignment options: left, right
        - `[[media:media-key|Caption text]]` - Add a caption
        - `[[media:media-key|size:medium|align:right|Caption]]` - Combine options

        ## Date Formats (FuzzyDate)

        Dates support approximate values using this format: `YYYY.MM.DD`

        Use `?` for unknown parts:
        - `2024.05.15` - Exact date (May 15, 2024)
        - `2024.05.??` - Year and month only (May 2024)
        - `2024.??.??` - Year only (2024)
        - `199?.??.??` - Decade only (1990s)
        - `????.05.15` - Day and month without year (useful for recurring events)

        Date fields that accept this format:
        - All date-related tool parameters (durationStart, durationEnd, date, etc.)
        - Birth dates, death dates, event dates
        - Relation durations (marriages, friendships, etc.)

        ## Page Facts JSON Format

        The `facts` parameter accepts a JSON object. Structure varies by page type:

        ### Person Facts
        ```json
        {
          "Main.Name": { "Values": [{ "FirstName": "John", "MiddleName": "Robert", "LastName": "Smith", "Duration": "1990.??.??-" }] },
          "Birth.Date": { "Value": "1985.03.15" },
          "Birth.Place": { "Value": "New York, USA" },
          "Death.Date": { "Value": "2024.01.20", "Cause": "natural" },
          "Death.Place": { "Value": "Los Angeles, USA" },
          "Death.Cause": { "Value": "Heart disease" },
          "Death.Burial": { "Value": "Forest Lawn Cemetery" },
          "Bio.Gender": { "IsMale": true },
          "Bio.Blood": { "Type": "A", "RhFactor": true },
          "Bio.Eyes": { "Value": "Brown" },
          "Bio.Hair": { "Value": "Black" },
          "Person.Language": { "Values": [{ "Name": "English", "Level": "Native" }, { "Name": "Spanish", "Level": "Intermediate" }] },
          "Person.Skill": { "Values": [{ "Name": "Piano", "Level": "Professional" }] },
          "Person.Profession": { "Values": ["Software Engineer", "Teacher"] },
          "Person.Religion": { "Values": ["Christian"] },
          "Meta.SocialProfiles": { "Values": [{ "Type": "Facebook", "Value": "john.smith" }] }
        }
        ```

        ### Pet Facts
        ```json
        {
          "Main.Name": { "Value": "Buddy" },
          "Birth.Date": { "Value": "2018.06.??" },
          "Death.Date": { "Value": null },
          "Bio.Gender": { "IsMale": true },
          "Bio.Species": { "Value": "Dog" },
          "Bio.Breed": { "Value": "Golden Retriever" },
          "Bio.Color": { "Value": "Golden" }
        }
        ```

        ### Location Facts
        ```json
        {
          "Main.Location": { "Address": "123 Main St, City, Country", "Latitude": 40.7128, "Longitude": -74.0060 },
          "Main.Opening": { "Value": "1995.??.??" },
          "Main.Shutdown": { "Value": null }
        }
        ```

        ### Event Facts
        ```json
        {
          "Main.Date": { "Value": "2020.07.15" }
        }
        ```

        ### Human Name with Duration
        For names that changed over time (e.g., maiden name → married name):
        ```json
        {
          "Main.Name": {
            "Values": [
              { "FirstName": "Jane", "LastName": "Doe", "Duration": "1980.??.??-2005.06.15" },
              { "FirstName": "Jane", "LastName": "Smith", "Duration": "2005.06.15-" }
            ]
          }
        }
        ```
        Duration format: `START-END` where dates use FuzzyDate format. Omit end for "ongoing".

        ## Relation Types

        When creating relations, use these types:
        - **Family**: Parent, Child, Spouse, StepParent, StepChild
        - **Social**: Friend, Colleague
        - **Pet ownership**: Owner (person→pet), Pet (pet→person)
        - **Location**: Location (person→place), LocationInhabitant (place→person)
        - **Events**: Event (person→event), EventVisitor (event→person)
        - **Other**: For any other connection

        Relations can have duration (durationStart/durationEnd) for time-bounded relationships like marriages.

        ## Role Permissions

        - **User**: Read-only access (search, list, read pages/media/relations)
        - **Editor**: Can create, update, and delete content
        - **Admin**: Full access including user management
        """;
}
