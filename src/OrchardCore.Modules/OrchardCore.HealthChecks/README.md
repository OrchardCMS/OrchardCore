# Health Check (OrchardCore.HealthChecks)

This module provides health check for the website.

## Purpose

Health checks are used by a container orchestrator or load balancer to quickly determine if a system is responding to requests normally. A container orchestrator might respond to a failing health check by halting a rolling deployment, or restarting a container. A load balancer might respond to a health check by routing traffic away from the failing instance of the service.


## Liveness probe
Liveness probe say: The application has crashed. You should shut it down and restart.
### Configuration in Docker
Liveness probe can be used in Docker, which will be the most basic health check, and sufficient for many applications.

In Docker, we'd prefer the swarm to know that things aren't healthy and restart the container so we can serve traffic again. You can use a liveness probe-based system like Docker’s built in HEALTHCHECK directive.
So in the Dockerfile, you can write something like:
+ Linux container:
    Bake-in curl
    ```RUN apk --update add curl```
    Add a Healthcheck instruction
    ```HEALTHCHECK CMD curl --fail http://yoursite//health/live/ || exit 1```
+ Windows container:
    ```HEALTHCHECK CMD powershell -command  
        try {
        $response = iwr http://localhost;
        if ($response.StatusCode -eq 200) { return 0}
        else {return 1};
    } catch { return 1 }```

## Readiness probe
A failed readiness probe says: The application is OK but not yet ready to serve traffic.

## Reference
<https://devblogs.microsoft.com/aspnet/asp-net-core-2-2-0-preview1-healthcheck/>
<https://howchoo.com/g/zwjhogrkywe/how-to-add-a-health-check-to-your-docker-container>