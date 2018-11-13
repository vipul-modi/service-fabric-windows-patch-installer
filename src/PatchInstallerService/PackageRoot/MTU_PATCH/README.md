---
services: service-fabric
platforms: dotnet, windows
author: ninzavivek
---

## About this Patch
The purpose of this patch is to ensure that the MTU size of each IPv4 interface on a cluster node is set correctly based on the type of container network it is used for.

For most interfaces, the MTU size should be set to 1500 bytes to match the MTU size of an interface programmed inside a Windows Container (NAT Networking mode). MTU size mismatch leads to fragmentation, which could cause choppy connections in some cases. An interface that belongs to an overlay network should have its MTU size set to 1450 because an overlay network packet has an additional 50 bytes in its header.
