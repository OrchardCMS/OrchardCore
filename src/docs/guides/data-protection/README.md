# Data Protection in Orchard Core

This guide provides information about implementing data protection in Orchard Core applications, particularly in multi-tenant and load-balanced environments.

## Overview

Data Protection is a critical security feature in ASP.NET Core that Orchard Core leverages to protect sensitive data such as authentication cookies, anti-forgery tokens, persisted secrets that need to be decrypted (e.g. SMTP passwords but not user passwords), and temporary data. In a multi-tenant or load-balanced environment, it's essential to ensure that data protection keys are shared across all nodes and persisted properly.

Orchard Core provides several options for persisting data protection keys:

- Local storage in `App_Data` folder's tenant-specific folder (like `App_Data/Sites/Default/DataProtection-Keys`)
- [Azure Blob Storage](../../reference/modules/DataProtection.Azure/README.md)
- [Redis](../../reference/modules/Redis/README.md#redis-data-protection)

This guide will focus on the distributed options, since the local storage one just works out of the box without any configuration.

## Why Distributed Data Protection Matters

In a single-server deployment, data protection keys are stored locally by default. However, this approach creates problems in:

1. **Load-balanced environments**: Each server has its own keys, causing authentication failures when requests are routed to different servers
2. **Multi-tenant setups**: Each tenant needs isolated but persistent key storage
3. **Application restarts**: Locally stored keys may be lost, invalidating existing cookies and tokens

Distributed data protection solves these issues by storing keys in one or more shared locations accessible to all application instances.

## Implementation Options

### Azure Blob Storage

For detailed information about implementing data protection with Azure Blob Storage, including configuration options and Liquid templating, see:

[Data Protection (Azure Storage)](../../reference/modules/DataProtection.Azure/README.md)

### Redis

For detailed information about implementing data protection with Redis, including configuration options and persistence considerations, see:

[Redis Data Protection](../../reference/modules/Redis/README.md#redis-data-protection)