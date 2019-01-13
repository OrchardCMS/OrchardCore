# Health Check (OrchardCore.HealthChecks)

This module provides health check for the website.

## Liveness probes
Liveness probe says: The application has crashed. You should shut it down and restart.

Liveness probe can be used in Docker, which will be the most basic health check, and sufficient for many applications.

In Docker, we'd prefer the swarm to know that things aren't healthy and restart the container so we can serve traffic again. You can use a liveness probe-based system like Docker’s built in HEALTHCHECK directive.
So in the Dockerfile, you can write something like:
+ Linux container:
    Bake-in curl
    `RUN apk --update add curl`
    Add a Healthcheck instruction
    `HEALTHCHECK CMD curl --fail http://yoursite//health/live/ || exit 1`
+ Windows container:
    HEALTHCHECK CMD powershell -command  
        try {
        $response = iwr http://localhost;
        if ($response.StatusCode -eq 200) { return 0}
        else {return 1};
    } catch { return 1 }