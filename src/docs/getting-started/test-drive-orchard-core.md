# Test drive Orchard Core

If you just want to test drive Orchard Core as a user (including an administrator), without creating a .NET application even, you have several options.

## Try Orchard Core

Go to [Try Orchard Core](https://try.orchardcore.net/) and create a demo site with two clicks. We recommend starting with the Agency recipe (the first one) that showcases a simple company/portfolio website, or the Blog recipe (the second one) for a simple blog.

!!! warning
    Try Orchard Core is really only for trying out Orchard Core. Your site will be automatically deleted the next Sunday.

!!! info
    Try Orchard Core, just as everything related to Orchard Core, is open source, and you can find its source [here](https://github.com/OrchardCMS/TryOrchardCore).

## DotNest

[DotNest](https://dotnest.com/) is an Orchard Core SaaS, operated by the company [Lombiq](https://lombiq.com). After signing up, you can create cloud-hosted Orchard Core sites for free.

These sites can be used in production, since unlike Try Orchard Core, they aren't deleted periodically.

## Docker

You can also run Orchard Core locally. The easiest is to use Docker:

```
docker run --name orchardcms -p 8080:80 orchardproject/orchardcore-cms-linux:latest
```

Docker images and parameters can be found at <https://hub.docker.com/u/orchardproject/>. See [our Docker documentation](../topics/docker/README.md) for more details, especially if you're new to Docker.

## The full source code

Of course, you can also clone and run the full source code of Orchard Core. See [the contribution docs](../guides/contributing/contributing-code.md) for this.
