---
services: service-fabric
platforms: dotnet, windows
author: ninzavivek
---

## About this Patch
The purpose of this patch is to workaround intermittent DNS resolution failures by adding a secondary entry for Fabric DNS Service with local host IP (127.0.0.1).